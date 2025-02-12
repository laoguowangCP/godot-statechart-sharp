using System;
using System.Collections.Generic;
using System.Linq;
using LGWCP.Util;

namespace LGWCP.Godot.StatechartSharp.Nodeless.Internal;

public partial class StatechartInt<TDuct, TEvent>
    where TDuct : StatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{

public class CompoundInt : StateInt
{
    public StateInt InitialState;
    public StateInt CurrentState;

    public CompoundInt(StatechartBuilder<TDuct, TEvent>.Compound state)
    {
        state._CompInt = this;
        Enters = state.Enters;
        Exits = state.Exits;
    }

    public override void Setup(
        StatechartInt<TDuct, TEvent> hostStatechart,
        StatechartBuilder<TDuct, TEvent>.BuildComposition buildComp,
        ref int orderId,
        int substateIdx)
    {
        base.Setup(hostStatechart, buildComp, ref orderId, substateIdx);

        // Init & collect states, transitions, actions
        // Get lower state and upper state
        StateInt lastSubstate = null;
        List<StateInt> substates = new(); // temp substates
        foreach (var childComp in buildComp._Comps)
        {
            if (childComp is StatechartBuilder<TDuct, TEvent>.State sComp)
            {
                var s = (StateInt)sComp._GetInternalComposition();
                s.Setup(hostStatechart, childComp, ref orderId, substates.Count);
                substates.Add(s);

                if (LowerState is null)
                {
                    LowerState = s;
                }
                lastSubstate = s;
            }
            else if (childComp is StatechartBuilder<TDuct, TEvent>.Transition tComp)
            {
                // Root state should not have transition
                if (ParentState is null)
                {
                    continue;
                }
                var t = (TransitionInt)tComp._GetInternalComposition();
                t.Setup(hostStatechart, childComp, ref orderId);
            }
            else if (childComp is StatechartBuilder<TDuct, TEvent>.Reaction aComp)
            {
                var a = (ReactionInt)aComp._GetInternalComposition();
                a.Setup(hostStatechart, childComp, ref orderId);
            }
        }

        if (lastSubstate is not null)
        {
            if (lastSubstate.UpperState is not null)
            {
                UpperState = lastSubstate.UpperState;
            }
            else
            {
                UpperState = lastSubstate;
            }
        }
        // Else state is atomic, lower and upper are both null.

        // Set initial state
        if (buildComp is StatechartBuilder<TDuct, TEvent>.Compound compound)
        {
            InitialState = (StateInt)compound.Initial._CompInt;
        }

        if (InitialState is null)
        {
            foreach (var substate in substates)
            {
                if (substate.IsValidState())
                {
                    InitialState = substate;
                    break;
                }
            }
        }

        CurrentState = InitialState;

        // Convert to array
        Substates = substates.ToArray();
    }

    public override void SetupPost(StatechartBuilder<TDuct, TEvent>.BuildComposition buildComp)
    {
        List<TransitionInt> autoTransitions = new();
        List<TEvent> kTransitions = new();
        List<List<TransitionInt>> vTransitions = new();
        List<TEvent> kReactions = new();
        List<List<ReactionInt>> vReactions = new();

        foreach (var childComp in buildComp._Comps)
        {
            if (childComp is StatechartBuilder<TDuct, TEvent>.State sComp)
            {
                sComp._CompInt.SetupPost(childComp);
            }
            else if (childComp is StatechartBuilder<TDuct, TEvent>.Transition tComp)
            {
                if (ParentState is null)
                {
                    continue;
                }

                var t = (TransitionInt)tComp._CompInt;
                t.SetupPost(childComp);

                // Add transition
                if (t.IsAuto)
                {
                    autoTransitions.Add(t);
                    continue;
                }
                ArrayHelper.KVListInsert(t.Event, t, kTransitions, vTransitions);
            }
            else if (childComp is StatechartBuilder<TDuct, TEvent>.Reaction aComp)
            {
                var a = (ReactionInt)aComp._CompInt;
                a.SetupPost(childComp);

                // Add reaction
                ArrayHelper.KVListInsert(a.Event, a, kReactions, vReactions);
            }
        }

        // Convert to array
        AutoTransitions = autoTransitions.ToArray();
        ArrayHelper.KVListToArray(kTransitions, vTransitions, out KTransitions, out VTransitions);
        ArrayHelper.KVListToArray(kReactions, vReactions, out KReactions, out VReactions);
    }

    public override void SubmitActiveState(SortedSet<StateInt> activeStates)
    {
        activeStates.Add(this);
        CurrentState?.SubmitActiveState(activeStates);
    }

    public override bool IsConflictToEnterRegion(
        StateInt substateToPend,
        SortedSet<StateInt> enterRegionUnextended)
    {
        // Conflicts if any substate is already exist in region
        return enterRegionUnextended.Any<StateInt>(
            state => IsAncestorStateOf(state));
    }


    public override void ExtendEnterRegion(
        SortedSet<StateInt> enterRegion,
        SortedSet<StateInt> enterRegionEdge,
        SortedSet<StateInt> extraEnterRegion,
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
            for (int i = 0; i < Substates.Length; ++i)
            {
                var substate = Substates[i];
                if (enterRegion.Contains(substate))
                {
                    substate.ExtendEnterRegion(
                        enterRegion, enterRegionEdge, extraEnterRegion, true);
                    return;
                }
            }
        }

        // Need no check, or need check but no substate in region
        if (InitialState != null)
        {
            InitialState.ExtendEnterRegion(
                enterRegion, enterRegionEdge, extraEnterRegion, false);
        }
    }

    public override int SelectTransitions(
        SortedSet<TransitionInt> enabledTransition, TEvent @event, TDuct duct)
    {
        int handleInfo = -1;
        if (CurrentState != null)
        {
            handleInfo = CurrentState.SelectTransitions(enabledTransition, @event, duct);
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

        if (@event is null)
        {
            for (int i = 0; i < AutoTransitions.Length; ++i)
            {
                var t = AutoTransitions[i];
                // If == 0, only check targetless
                if (handleInfo == 0)
                {
                    if (!t.IsTargetless)
                    {
                        continue;
                    }
                }

                if (t.Check(duct))
                {
                    enabledTransition.Add(t);
                    handleInfo = 1;
                    break;
                }
            }
        }
        else
        {
            TransitionInt[] eventTransitions = null;
            if (ArrayHelper.ArrayDictTryGet(
                @event, KTransitions, VTransitions, ref eventTransitions) >= 0)
            {
                for (int i = 0; i < eventTransitions.Length; ++i)
                {
                    var t = eventTransitions[i];
                    // If == 0, only check targetless
                    if (handleInfo == 0)
                    {
                        if (!t.IsTargetless)
                        {
                            continue;
                        }
                    }

                    if (t.Check(duct))
                    {
                        enabledTransition.Add(t);
                        handleInfo = 1;
                        break;
                    }
                }
            }
        }

        return handleInfo;
    }


    public override void DeduceDescendantsRecur(
        SortedSet<StateInt> deducedSet, DeduceDescendantsModeEnum deduceMode)
    {
        StateInt substateToAdd;
        DeduceDescendantsModeEnum substateDeduceMode;

        switch (deduceMode)
        {
            case DeduceDescendantsModeEnum.Initial:
                substateToAdd = InitialState;
                substateDeduceMode = DeduceDescendantsModeEnum.Initial;
                break;
            case DeduceDescendantsModeEnum.History:
                substateToAdd = CurrentState;
                substateDeduceMode = DeduceDescendantsModeEnum.Initial;
                break;
            case DeduceDescendantsModeEnum.DeepHistory:
                substateToAdd = CurrentState;
                substateDeduceMode = DeduceDescendantsModeEnum.DeepHistory;
                break;
            default:
                substateToAdd = InitialState;
                substateDeduceMode = DeduceDescendantsModeEnum.Initial;
                break;
        }

        if (substateToAdd is null)
        {
            return;
        }
        deducedSet.Add(substateToAdd);
        substateToAdd.DeduceDescendantsRecur(deducedSet, substateDeduceMode);
    }

    public override void HandleSubstateEnter(StateInt substate)
    {
        CurrentState = substate;
    }

    public override void SaveAllStateConfig(List<int> snapshotConfig)
    {
        if (CurrentState is null)
        {
            return;
        }
        snapshotConfig.Add(CurrentState.SubstateIdx);

        for (int i = 0; i < Substates.Length; ++i)
        {
            Substates[i].SaveAllStateConfig(snapshotConfig);
        }
    }

    public override void SaveActiveStateConfig(List<int> snapshotConfig)
    {
        if (CurrentState is null)
        {
            return;
        }
        snapshotConfig.Add(CurrentState.SubstateIdx);
        CurrentState.SaveActiveStateConfig(snapshotConfig);
    }

    public override int LoadAllStateConfig(int[] config, int configIdx)
    {
        if (Substates.Length == 0)
        {
            return configIdx;
        }

        if (configIdx >= config.Length)
        {
            return -1;
        }

        CurrentState = Substates[config[configIdx]];
        ++configIdx;

        for (int i = 0; i < Substates.Length; ++i)
        {
            configIdx = Substates[i].LoadAllStateConfig(config, configIdx);
            if (configIdx == -1)
            {
                return -1;
            }
        }

        return configIdx;
    }

    public override int LoadActiveStateConfig(int[] config, int configIdx)
    {
        if (Substates.Length == 0)
        {
            return configIdx;
        }

        if (configIdx >= config.Length)
        {
            return -1;
        }

        CurrentState = Substates[config[configIdx]];
        ++configIdx;

        return CurrentState.LoadActiveStateConfig(config, configIdx);
    }

    public override bool IsValidState()
    {
        return true;
    }
}

}
