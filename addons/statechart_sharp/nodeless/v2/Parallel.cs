using System;
using System.Collections.Generic;
using System.Linq;

namespace LGWCP.Godot.StatechartSharp.NodelessV2;

public partial class Statechart<TDuct, TEvent>
    where TDuct : StatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{

public class Parallel : State
{
    protected int ValidSubstateCnt = 0;

    public Parallel(
        Action<TDuct>[] enters,
        Action<TDuct>[] exits)
    {
        _Enters = enters;
        _Exits = exits;
    }

    public override bool _IsValidState() => true;

    public override void _Setup(ref int parentOrderId, int substateIdx)
    {
        base._Setup(ref parentOrderId, substateIdx);

        // Init & collect states, transitions, actions
        // Get lower-state and upper-state
        State lastSubstate = null;
        for (int i = 0; i < _Comps.Count; ++i)
        {
            var comp = _Comps[i];
            if (comp is State s)
            {
                s._Setup(ref parentOrderId, _Substates.Count);
                _Substates.Add(s);

                // First substate is lower-state
                if (_LowerState == null)
                {
                    _LowerState = s;
                }
                lastSubstate = s;

                if (s._IsValidState())
                {
                    ++ValidSubstateCnt;
                }
            }
            else if (comp is Transition t)
            {
                // Root state should not have transition
                if (_ParentState == null)
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

        if (lastSubstate != null)
        {
            if (lastSubstate._UpperState != null)
            {
                // Last substate's upper is upper-state
                _UpperState = lastSubstate._UpperState;
            }
            else
            {
                // Last substate is upper-state
                _UpperState = lastSubstate;
            }
        }
        // Else state is atomic, lower and upper are null
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
                if (_ParentState == null)
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
        /*
        Deals history cases:
            1. Pending substate is history, conflicts if any descendant in region.
            2. Any history substate in enter region, conflicts.
        */
        // At this point history is not excluded from enter region

        // Pending substate is history
        if (!substateToPend._IsValidState())
        {
            return enterRegionUnextended.Any<State>(
                state => _IsAncestorStateOf(state));
        }

        // Any history substate in region, conflicts.
        for (int i = 0; i < _Substates.Count; ++i)
        {
            var substate = _Substates[i];
            if (!substate._IsValidState()
                && enterRegionUnextended.Contains(substate))
            {
                return true;
            }
        }

        // No conflicts.
        return false;
    }

    public override void _ExtendEnterRegion(
        SortedSet<State> enterRegion,
        SortedSet<State> enterRegionEdge,
        SortedSet<State> extraEnterRegion,
        bool needCheckContain)
    {
        /*
        if need check (state in region, checked by parent):
            if any history substate in region:
                extend this history
                return
            else (no history substate in region)
                foreach substate:
                    if substate in region:
                        extend, still need check
                    else (substate not in region)
                        extend, need no check
        else (state not in region)
            add state to extra-region
            foreach substate:
                extend, need no check
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
                if (!substate._IsValidState()
                    && enterRegion.Contains(substate))
                {
                    substate._ExtendEnterRegion(
                        enterRegion, enterRegionEdge, extraEnterRegion, true);
                    return;
                }
            }
        }

        // No history substate in region
        for (int i = 0; i < _Substates.Count; ++i)
        {
            var substate = _Substates[i];
            if (!substate._IsValidState())
            {
                continue;
            }

            // Need check && substate in region => still need check
            bool stillNeedCheck =
                needCheckContain && enterRegion.Contains(substate);
            substate._ExtendEnterRegion(
                enterRegion, enterRegionEdge, extraEnterRegion, stillNeedCheck);
        }
    }

    public override void _SubmitActiveState(SortedSet<State> activeStates)
    {
        activeStates.Add(this);
        for (int i = 0; i < _Substates.Count; ++i)
        {
            _Substates[i]._SubmitActiveState(activeStates);
        }
    }

    public override int _SelectTransitions(
        SortedSet<Transition> enabledTransitions, TEvent @event, TDuct duct)
    {
        int handleInfo = -1;
        if (ValidSubstateCnt > 0)
        {
            int negCnt = 0;
            int posCnt = 0;
            for (int i = 0; i < _Substates.Count; ++i)
            {
                var substate = _Substates[i];
                if (!substate._IsValidState())
                {
                    continue;
                }

                int substateHandleInfo = substate._SelectTransitions(enabledTransitions, @event, duct);
                if (substateHandleInfo < 0)
                {
                    negCnt += 1;
                }
                else if (substateHandleInfo > 0)
                {
                    posCnt += 1;
                }
            }

            if (negCnt == ValidSubstateCnt) // No selected
            {
                handleInfo = -1;
            }
            else if (posCnt == ValidSubstateCnt) // All done
            {
                handleInfo = 1;
            }
            else // Selected but not all done
            {
                handleInfo = 0;
            }
        }

        /*
        Check source's transitions:
            a) < 0, check any
            b) == 0, check targetless
            c) > 0, check none
        */
        if (handleInfo > 0)
        {
            return handleInfo;
        }

        if (@event is null)
        {
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
        }
        else
        {
            if (_TransitionsKV.TryGet(@event, out var transitions) >= 0)
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
        }

        return handleInfo;
    }

    public override void _DeduceDescendantsRecur(
        SortedSet<State> deducedSet, DeduceDescendantsModeEnum deduceMode)
    {
        DeduceDescendantsModeEnum substateDeduceMode;

        switch (deduceMode)
        {
            case DeduceDescendantsModeEnum.Initial:
                substateDeduceMode = DeduceDescendantsModeEnum.Initial;
                break;
            case DeduceDescendantsModeEnum.History:
                substateDeduceMode = DeduceDescendantsModeEnum.Initial;
                break;
            case DeduceDescendantsModeEnum.DeepHistory:
                substateDeduceMode = DeduceDescendantsModeEnum.DeepHistory;
                break;
            default:
                substateDeduceMode = DeduceDescendantsModeEnum.Initial;
                break;
        }

        for (int i = 0; i < _Substates.Count; ++i)
        {
            // Ignore history states
            var substate = _Substates[i];
            if (!substate._IsValidState())
            {
                continue;
            }
            deducedSet.Add(substate);
            substate._DeduceDescendantsRecur(deducedSet, substateDeduceMode);
        }
    }

    public override void _SaveAllStateConfig(List<int> snapshot)
    {
        for (int i = 0; i < _Substates.Count; ++i)
        {
            _Substates[i]._SaveAllStateConfig(snapshot);
        }
    }

    public override void _SaveActiveStateConfig(List<int> snapshot)
    {
        for (int i = 0; i < _Substates.Count; ++i)
        {
            _Substates[i]._SaveActiveStateConfig(snapshot);
        }
    }

    public override int _LoadAllStateConfig(int[] config, int configIdx)
    {
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
}

}
