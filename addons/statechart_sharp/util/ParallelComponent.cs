using System.Collections.Generic;
using Godot;

namespace LGWCP.StatechartSharp;

public class ParallelComponent : StateComponent
{
    protected int NonHistorySubCnt = 0;
    public ParallelComponent(State state) : base(state) {}

    internal override void Setup(Statechart hostStateChart, ref int ancestorId)
    {
        base.Setup(hostStateChart, ref ancestorId);

        // Init & collect states, transitions, actions
        // Get lower-state and upper-state
        State lastSubstate = null;
        foreach (Node child in HostState.GetChildren())
        {
            if (child is State s)
            {
                s.Setup(hostStateChart, ref ancestorId);
                Substates.Add(s);

                // First substate is lower-state
                if (LowerState == null)
                {
                    LowerState = s;
                }
                lastSubstate = s;

                if (!s.IsHistory)
                {
                    NonHistorySubCnt += 1;
                }
            }
            else if (child is Transition t)
            {
                // Root state should not have transition
                if (ParentState != null)
                {
                    t.Setup(hostStateChart, ref ancestorId);
                    Transitions.Add(t);
                }
            }
            else if (child is Reaction a)
            {
                a.Setup(hostStateChart, ref ancestorId);
                Reactions.Add(a);
            }
        }

        if (lastSubstate != null)
        {
            if (lastSubstate.UpperState != null)
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
        // Else state is atomic, lower and upper are null
    }

    internal override void ExtendEnterRegion(
        SortedSet<State> enterRegion,
        SortedSet<State> enterRegionEdge,
        SortedSet<State> extraEnterRegion,
        bool needCheckContain)
    {
        /*
        if need check (state in region, checked by parent):
            if any history substate in region:
                extend this very history
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
            extraEnterRegion.Add(HostState);
        }

        // Need check
        if (needCheckContain)
        {
            foreach (State substate in Substates)
            {
                if (substate.IsHistory && enterRegion.Contains(substate))
                {
                    substate.ExtendEnterRegion(
                        enterRegion, enterRegionEdge, extraEnterRegion);
                    return;
                }
            }
        }

        // No history substate in region
        foreach (State substate in Substates)
        {
            if (substate.IsHistory)
            {
                continue;
            }
            // Need check && substate in region  => still need check
            bool stillNeedCheck =
                needCheckContain && enterRegion.Contains(substate);
            substate.ExtendEnterRegion(
                enterRegion, enterRegionEdge, extraEnterRegion, stillNeedCheck);
        }

    }

    internal override void PostSetup()
    {
        foreach (State state in Substates)
        {
            state.PostSetup();
        }

        foreach (Transition trans in Transitions)
        {
            trans.PostSetup();
        }

        foreach (Reaction react in Reactions)
        {
            react.PostSetup();
        }
    }

    internal override bool IsConflictToEnterRegion(
        State tgtSubstate, SortedSet<State> enterRegion)
    {
        /*
        Deals history cases:
            1. Target substate is history, conflicts if any descendant in region.
            2. Already a history substate in region, always conflicts.
        */
        foreach (State substate in Substates)
        {
            // A history substate in region, conflicts.
            if (substate.IsHistory && enterRegion.Contains(substate))
            {
                return true;
            }
        }

        // Target substate is history
        if (tgtSubstate.IsHistory)
        {
            SortedSet<State> descInRegion = enterRegion.GetViewBetween(
                LowerState, UpperState);
            return descInRegion.Count > 0;
        }

        // If nothing involves history, no conflicts.
        return false;
    }

    internal override void RegisterActiveState(SortedSet<State> activeStates)
    {
        activeStates.Add(HostState);
        foreach (State substate in HostState.Substates)
        {
            substate.RegisterActiveState(activeStates);
        }
    }

    internal override int SelectTransitions(SortedSet<Transition> enabledTransitions, StringName eventName)
    {
        int handleInfo = -1;
        if (NonHistorySubCnt > 0)
        {
            int negCnt = 0;
            int posCnt = 0;
            foreach (State substate in Substates)
            {
                if (substate.IsHistory)
                {
                    continue;
                }

                int substateHandleInfo = substate.SelectTransitions(
                    enabledTransitions, eventName);
                if (substateHandleInfo < 0)
                {
                    negCnt += 1;
                }
                else if (substateHandleInfo > 0)
                {
                    posCnt += 1;
                }
            }

            if (negCnt == NonHistorySubCnt) // No selected
            {
                handleInfo = -1;
            }
            else if (posCnt == NonHistorySubCnt) // All done
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
            - < 0, check any
            - == 0, only check targetless
            - > 0, do nothing
        */
        if (handleInfo > 0)
        {
            return handleInfo;
        }

        foreach (Transition t in Transitions)
        {
            // If == 0, only check targetless
            if (handleInfo == 0)
            {
                if (!t.IsTargetless)
                {
                    continue;
                }
            }

            bool isEnabled = t.Check(eventName);
            if (isEnabled)
            {
                enabledTransitions.Add(t);
                handleInfo = 1;
                break;
            }
        }

        return handleInfo;
    }

    internal override void DeduceDescendants(
        SortedSet<State> deducedSet, bool isHistory, bool isEdgeState)
    {
        /* 
        If is edge-state:
            - It is called from history substate.
            - "IsHistory" argument represents "IsDeepHistory"
        */
        if (!isEdgeState)
        {
            deducedSet.Add(HostState);
        }

        foreach (State substate in Substates)
        {
            // Pass history-states
            if (substate.IsHistory)
            {
                continue;
            }
            substate.DeduceDescendants(deducedSet, isHistory);
        }
    }
}
