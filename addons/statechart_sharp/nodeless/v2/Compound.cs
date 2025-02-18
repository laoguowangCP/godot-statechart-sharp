using System;
using System.Collections.Generic;
using System.Linq;

namespace LGWCP.Godot.StatechartSharp.Nodeless;

public partial class Statechart<TDuct, TEvent>
    where TDuct : StatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{

public class Compound : State
{
    public State _InitialState;
    public State _CurrentState;

    public Compound(
        Statechart<TDuct, TEvent> hostStatechart,
        Action<TDuct>[] enters,
        Action<TDuct>[] exits,
        State initialState) : base(hostStatechart)
    {
        _Enters = enters;
        _Exits = exits;
        _InitialState = initialState;
    }

    public override bool _IsValidState() => true;

    public override void _Setup(ref int parentOrderId, int substateIdx)
    {
        base._Setup(ref parentOrderId, substateIdx);

        // Init & collect states, transitions, actions
        // Get lower state and upper state
        State lastSubstate = null;
        for (int i = 0; i < _Comps.Count; ++i)
        {
            var comp = _Comps[i];
            if (comp is State s)
            {
                s._Setup(ref parentOrderId, _Substates.Count);
                _Substates.Add(s);

                if (_LowerState == null)
                {
                    _LowerState = s;
                }
                lastSubstate = s;
            }
            else if (comp is Transition t)
            {
                // Root state should not have transition
                if (_ParentState is null)
                {
                    continue;
                }
                t._Setup(ref parentOrderId);
            }
            else if (comp is Reaction a)
            {
                a._Setup(ref parentOrderId);
            }
        }

        if (lastSubstate is not null)
        {
            if (lastSubstate._UpperState is not null)
            {
                _UpperState = lastSubstate._UpperState;
            }
            else
            {
                _UpperState = lastSubstate;
            }
        }
        // Else state is atomic, lower and upper are both null.

        // Set initial state
        if (_InitialState == null
            || !_InitialState._IsValidState()
            || _InitialState._ParentState != this)
        {
            for (int i = 0; i < _Substates.Count; ++i)
            {
                var s = _Substates[i];
                if (s._IsValidState())
                {
                    _InitialState = s;
                    break;
                }
            }
        }
        _CurrentState = _InitialState;
    }

    public override void _SetupPost()
    {
        for (int i = 0; i < _Comps.Count; ++i)
        {
            var comp = _Comps[i];
            if (comp is State s)
            {
                s._SetupPost();
            }
            else if (comp is Transition t)
            {
                // Root state should not have transition
                if (_ParentState is null)
                {
                    continue;
                }

                t._SetupPost();
                if (t._IsAuto)
                {
                    _AutoTransitions.Add(t);
                    continue;
                }

                // Add transition
                var idx = _TransitionsKV.TryGet(t._Event, out var transitions);
                if (idx >= 0)
                {
                    transitions.Add(t);
                }
                else
                {
                    _TransitionsKV.DirectInsert(t._Event, new() { t });
                }
            }
            else if (comp is Reaction a)
            {
                a._SetupPost();

                // Add reaction
                var idx = _ReactionsKV.TryGet(a._Event, out var reactions);
                if (idx >= 0)
                {
                    reactions.Add(a);
                }
                else
                {
                    _ReactionsKV.DirectInsert(a._Event, new() { a });
                }
            }
        }
    }

    public override bool _IsConflictToEnterRegion(
        State substateToPend,
        SortedSet<State> enterRegionUnextended)
    {
        // Conflicts if any substate is already exist in region
        return enterRegionUnextended.Any<State>(
            state => _IsAncestorStateOf(state));
    }

    public override void _ExtendEnterRegion(
        SortedSet<State> enterRegion,
        SortedSet<State> enterRegionEdge,
        SortedSet<State> extraEnterRegion,
        bool needCheckContain)
    {
        /*
        if need check (state is in region, checked by parent):
            if any substate in region:
                extend this substate, still need check
            else (no substate in region)
                extend initial state, need no check
        else (need no check)
            add state to extra-region
            extend initial state, need no check
        */

        // Need no check, add to extra
        if (!needCheckContain)
        {
            extraEnterRegion.Add(this);
        }

        // Need check
        if (needCheckContain)
        {
            for (int i = 0; i < _Substates.Count; ++i)
            {
                var substate = _Substates[i];
                if (enterRegion.Contains(substate))
                {
                    substate._ExtendEnterRegion(
                        enterRegion, enterRegionEdge, extraEnterRegion, true);
                    return;
                }
            }
        }

        // Need no check, or need check but no substate in region
        if (_InitialState != null)
        {
            _InitialState._ExtendEnterRegion(
                enterRegion, enterRegionEdge, extraEnterRegion, false);
        }
    }

    public override void _SubmitActiveState(SortedSet<State> activeStates)
    {
        activeStates.Add(this);
        _CurrentState?._SubmitActiveState(activeStates);
    }

    public override int _SelectTransitions(
        SortedSet<Transition> enabledTransitions, TEvent @event, TDuct duct)
    {
        int handleInfo = -1;
        // if (HostState._CurrentState != null)
        if (_CurrentState != null)
        {
            handleInfo = _CurrentState._SelectTransitions(enabledTransitions, @event, duct);
        }

        /*
        Check source's transitions:
            - < 0, check any
            - == 0, only check targetless
            - > 0, do nothing
        */
        if (handleInfo > 0)
        {
            return handleInfo;
        }

        var idx = _TransitionsKV.TryGet(@event, out var transitions);
        if (idx >= 0)
        {
            for (int i = 0; i < transitions.Count; ++i)
            {
                // If == 0, only check targetless
                var t = transitions[i];
                if (handleInfo == 0)
                {
                    if (!t._IsTargetless)
                    {
                        continue;
                    }
                }
                if (t._Check(duct))
                {
                    enabledTransitions.Add(t);
                    handleInfo = 1;
                    break;
                }
            }
        }

        return handleInfo;
    }

    public override int _SelectAutoTransitions(
        SortedSet<Transition> enabledTransitions, TDuct duct)
    {
        int handleInfo = -1;
        // if (HostState._CurrentState != null)
        if (_CurrentState != null)
        {
            handleInfo = _CurrentState._SelectAutoTransitions(enabledTransitions, duct);
        }

        /*
        Check source's transitions:
            - < 0, check any
            - == 0, only check targetless
            - > 0, do nothing
        */
        if (handleInfo > 0)
        {
            return handleInfo;
        }

        for (int i = 0; i < _AutoTransitions.Count; ++i)
        {
            // If == 0, only check targetless
            var t = _AutoTransitions[i];
            if (handleInfo == 0)
            {
                if (!t._IsTargetless)
                {
                    continue;
                }
            }

            if (t._Check(duct))
            {
                enabledTransitions.Add(t);
                handleInfo = 1;
                break;
            }
        }

        return handleInfo;
    }

    public override void _DeduceDescendantsRecur(
        SortedSet<State> deducedSet, DeduceDescendantsModeEnum deduceMode)
    {
        State substateToAdd;
        DeduceDescendantsModeEnum substateDeduceMode;

        switch (deduceMode)
        {
            case DeduceDescendantsModeEnum.Initial:
                substateToAdd = _InitialState;
                substateDeduceMode = DeduceDescendantsModeEnum.Initial;
                break;
            case DeduceDescendantsModeEnum.History:
                substateToAdd = _CurrentState;
                substateDeduceMode = DeduceDescendantsModeEnum.Initial;
                break;
            case DeduceDescendantsModeEnum.DeepHistory:
                substateToAdd = _CurrentState;
                substateDeduceMode = DeduceDescendantsModeEnum.DeepHistory;
                break;
            default:
                substateToAdd = _InitialState;
                substateDeduceMode = DeduceDescendantsModeEnum.Initial;
                break;
        }

        if (substateToAdd == null)
        {
            return;
        }
        _ = deducedSet.Add(substateToAdd);
        substateToAdd._DeduceDescendantsRecur(deducedSet, substateDeduceMode);
    }

    public override void _SaveAllStateConfig(List<int> snapshot)
    {
        if (_CurrentState is null)
        {
            return;
        }
        snapshot.Add(_CurrentState._SubstateIdx);
        for (int i = 0; i < _Substates.Count; ++i)
        {
            _Substates[i]._SaveAllStateConfig(snapshot);
        }
    }

    public override void _SaveActiveStateConfig(List<int> snapshot)
    {
        if (_CurrentState is null)
        {
            return;
        }
        snapshot.Add(_CurrentState._SubstateIdx);
        _CurrentState._SaveActiveStateConfig(snapshot);
    }

    public override int _LoadAllStateConfig(int[] config, int configIdx)
    {
        if (_Substates.Count == 0)
        {
            return configIdx;
        }

        if (configIdx >= config.Length)
        {
            return -1;
        }

        _CurrentState = _Substates[config[configIdx]];
        ++configIdx;
        for (int i = 0; i < _Substates.Count; ++i)
        {
            configIdx = _Substates[i]._LoadAllStateConfig(config, configIdx);
            if (configIdx == -1)
            {
                return -1;
            }
        }

        return configIdx;
    }

    public override int _LoadActiveStateConfig(int[] config, int configIdx)
    {
        if (_Substates.Count == 0)
        {
            return configIdx;
        }

        if (configIdx >= config.Length)
        {
            return -1;
        }

        _CurrentState = _Substates[config[configIdx]];
        ++configIdx;

        return _CurrentState._LoadActiveStateConfig(config, configIdx);
    }

    public override Composition Duplicate()
    {
        return new Compound(_HostStatechart, _Enters, _Exits, _InitialState);
    }

    /// <summary>
    /// Use it before statechart is ready.
    /// </summary>
    /// <param name="initialState"></param>
    /// <returns></returns>
    public Compound SetInitialState(State initialState)
    {
        if (_HostStatechart.IsReady)
        {
            _InitialState = initialState;
        }
        return this;
    }
}

}
