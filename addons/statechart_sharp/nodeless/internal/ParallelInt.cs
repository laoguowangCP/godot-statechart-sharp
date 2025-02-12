using System;
using System.Collections.Generic;
using System.Linq;
using LGWCP.Util;

namespace LGWCP.Godot.StatechartSharp.Nodeless.Internal;

public partial class StatechartInt<TDuct, TEvent>
    where TDuct : StatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{

public class ParallelInt : StateInt
{
    protected int ValidSubstateCnt = 0;

    public ParallelInt(StatechartBuilder<TDuct, TEvent>.Parallel state)
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
        // Get lower-state and upper-state
        StateInt lastSubstate = null;
        List<StateInt> substates = new(); // temp substates
        foreach (var childComp in buildComp._Comps)
        {
            if (childComp is StatechartBuilder<TDuct, TEvent>.State sComp)
            {
                var s = (StateInt)sComp._GetInternalComposition();
                s.Setup(hostStatechart, sComp, ref orderId, substates.Count);
                substates.Add(s);

                // First substate is lower-state
                if (LowerState == null)
                {
                    LowerState = s;
                }
                lastSubstate = s;

                if (s.IsValidState())
                {
                    ++ValidSubstateCnt;
                }
            }
            else if (childComp is StatechartBuilder<TDuct, TEvent>.Transition tComp)
            {
                // Root state should not have transition
                if (ParentState is null)
                {
                    continue;
                }
                var t = (TransitionInt)tComp._GetInternalComposition();
                t.Setup(hostStatechart, tComp, ref orderId);
            }
            else if (childComp is StatechartBuilder<TDuct, TEvent>.Reaction aComp)
            {
                var a = (ReactionInt)aComp._GetInternalComposition();
                a.Setup(hostStatechart, aComp, ref orderId);
            }
        }

        if (lastSubstate is not null)
        {
            if (lastSubstate.UpperState is not null)
            {
                // Last substate's upper is upper-state
                UpperState = lastSubstate.UpperState;
            }
            else
            {
                // Last substate is upper-state
                UpperState = lastSubstate;
            }
        }
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
                // Root state should not have transition
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
        for (int i = 0; i < Substates.Length; ++i)
        {
            Substates[i].SubmitActiveState(activeStates);
        }
    }

    public override bool IsConflictToEnterRegion(
        StateInt substateToPend,
        SortedSet<StateInt> enterRegionUnextended)
    {
        /*
        Deals history cases:
            1. Pending substate is history, conflicts if any descendant in region.
            2. Any history substate in enter region, conflicts.
        */
        // At this point history is not excluded from enter region

        // Pending substate is history
        if (!substateToPend.IsValidState())
        {
            return enterRegionUnextended.Any<StateInt>(
                state => IsAncestorStateOf(state));
        }

        // Any history substate in region, conflicts.
        for (int i = 0; i < Substates.Length; ++i)
        {
            var substate = Substates[i];
            if (!substate.IsValidState()
                && enterRegionUnextended.Contains(substate))
            {
                return true;
            }
        }

        // No conflicts.
        return false;
    }

    public override void ExtendEnterRegion(
        SortedSet<StateInt> enterRegion,
        SortedSet<StateInt> enterRegionEdge,
        SortedSet<StateInt> extraEnterRegion,
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
            for (int i = 0; i < Substates.Length; ++i)
            {
                var substate = Substates[i];
                if (!substate.IsValidState()
                    && enterRegion.Contains(substate))
                {
                    substate.ExtendEnterRegion(
                        enterRegion, enterRegionEdge, extraEnterRegion, true);
                    return;
                }
            }
        }

        // No history substate in region
        for (int i = 0; i < Substates.Length; ++i)
        {
            var substate = Substates[i];
            if (!substate.IsValidState())
            {
                continue;
            }

            // Need check && substate in region => still need check
            bool stillNeedCheck =
                needCheckContain && enterRegion.Contains(substate);
            substate.ExtendEnterRegion(
                enterRegion, enterRegionEdge, extraEnterRegion, stillNeedCheck);
        }
    }

    public override int SelectTransitions(
        SortedSet<TransitionInt> enabledTransition, TEvent @event, TDuct duct)
    {
        int handleInfo = -1;
        if (ValidSubstateCnt > 0)
        {
            int negCnt = 0;
            int posCnt = 0;
            for (int i = 0; i < Substates.Length; ++i)
            {
                var substate = Substates[i];
                if (!substate.IsValidState())
                {
                    continue;
                }

                int substateHandleInfo = substate.SelectTransitions(enabledTransition, @event, duct);
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


        for (int i = 0; i < Substates.Length; ++i)
        {
            var substate = Substates[i];
            // Ignore history states
            if (!substate.IsValidState())
            {
                continue;
            }
            deducedSet.Add(substate);
            substate.DeduceDescendantsRecur(deducedSet, substateDeduceMode);
        }
    }

    public override void SaveAllStateConfig(List<int> snapshotConfig)
    {
        for (int i = 0; i < Substates.Length; ++i)
        {
            Substates[i].SaveAllStateConfig(snapshotConfig);
        }
    }

    public override void SaveActiveStateConfig(List<int> snapshotConfig)
    {
        for (int i = 0; i < Substates.Length; ++i)
        {
            Substates[i].SaveActiveStateConfig(snapshotConfig);
        }
    }

    public override int LoadAllStateConfig(int[] config, int configIdx)
    {
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

    public override bool IsValidState()
    {
        return true;
    }
}

}
