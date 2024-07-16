using Godot;
using System.Collections.Generic;
using System.Linq;


namespace LGWCP.StatechartSharp;

public class ParallelComponent : StateComponent
{
    protected int NonHistorySubstateCnt = 0;
    public ParallelComponent(State state) : base(state) {}

    internal override bool IsAvailableRootState()
    {
        return true;
    }

    internal override void Setup(Statechart hostStateChart, ref int parentOrderId, int substateIdx)
    {
        base.Setup(hostStateChart, ref parentOrderId, substateIdx);

        // Init & collect states, transitions, actions
        // Get lower-state and upper-state
        State lastSubstate = null;
        foreach (Node child in HostState.GetChildren())
        {
            if (child.IsQueuedForDeletion())
            {
                continue;
            }
            
            if (child is State s)
            {
                s.Setup(hostStateChart, ref parentOrderId, Substates.Count);
                Substates.Add(s);

                // First substate is lower-state
                if (LowerState == null)
                {
                    LowerState = s;
                }
                lastSubstate = s;

                if (!s.IsHistory)
                {
                    NonHistorySubstateCnt += 1;
                }
            }
            else if (child is Transition t)
            {
                // Root state should not have transition
                if (ParentState != null)
                {
                    t.Setup(hostStateChart, ref parentOrderId);
                    Transitions.Add(t);
                }
            }
            else if (child is Reaction a)
            {
                a.Setup(hostStateChart, ref parentOrderId);
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

    internal override void PostSetup()
    {
        foreach (State state in Substates)
        {
            state.PostSetup();
        }

        foreach (Transition transistion in Transitions)
        {
            transistion.PostSetup();
        }

        foreach (Reaction reaction in Reactions)
        {
            reaction.PostSetup();
        }
    }

    internal override bool GetPromoteStates(List<State> states)
    {
        bool isPromote = true;
        foreach (Node child in HostState.GetChildren())
        {
            if (child is State state)
            {
                bool isChildPromoted = state.GetPromoteStates(states);
                if (isChildPromoted)
                {
                    isPromote = false;
                    break;
                }
            }
        }

        if (isPromote)
        {
            states.Add(HostState);
        }

        // Make sure promoted
        return true;
    }

    internal override bool IsConflictToEnterRegion(
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
        if (substateToPend.IsHistory)
        {
            return enterRegionUnextended.Any<State>(
                state => HostState.IsAncestorStateOf(state));
        }

        // Any history substate in region, conflicts.
        foreach (State substate in Substates)
        {
            if (substate.IsHistory
                && enterRegionUnextended.Contains(substate))
            {
                return true;
            }
        }

        // No conflicts.
        return false;
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

            // Need check && substate in region => still need check
            bool stillNeedCheck =
                needCheckContain && enterRegion.Contains(substate);
            substate.ExtendEnterRegion(
                enterRegion, enterRegionEdge, extraEnterRegion, stillNeedCheck);
        }
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
        if (NonHistorySubstateCnt > 0)
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

            if (negCnt == NonHistorySubstateCnt) // No selected
            {
                handleInfo = -1;
            }
            else if (posCnt == NonHistorySubstateCnt) // All done
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

        foreach (Transition transition in Transitions)
        {
            if (handleInfo == 0)
            {
                if (!transition.IsTargetless)
                {
                    continue;
                }
            }

            bool isEnabled = transition.Check(eventName);
            if (isEnabled)
            {
                enabledTransitions.Add(transition);
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
            1. Called from history substate.
            2. IsHistory arg represents IsDeepHistory
        */
        if (!isEdgeState)
        {
            deducedSet.Add(HostState);
        }

        foreach (State substate in Substates)
        {
            // Ignore history states
            if (substate.IsHistory)
            {
                continue;
            }
            substate.DeduceDescendants(deducedSet, isHistory);
        }
    }

    internal override void SaveAllStateConfig(ref List<int> snapshot)
    {
        foreach (State substate in Substates)
        {
            substate.SaveActiveStateConfig(ref snapshot);
        }
    }

    internal override void SaveActiveStateConfig(ref List<int> snapshot)
    {
        foreach (State substate in Substates)
        {
            substate.SaveActiveStateConfig(ref snapshot);
        }
    }
}
