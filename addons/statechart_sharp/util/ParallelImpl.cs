using Godot;
using System.Collections.Generic;
using System.Linq;


namespace LGWCP.Godot.StatechartSharp;


public class ParallelImpl : StateImpl
{
    protected int NonHistorySubstateCnt = 0;
    public ParallelImpl(State state) : base(state) {}

    public override bool IsAvailableRootState()
    {
        return true;
    }

    public override void Setup(Statechart hostStateChart, ref int parentOrderId, int substateIdx)
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
				if (ParentState is null)
				{
					continue;
				}

				t.Setup(hostStateChart, ref parentOrderId);

                if (t.IsAuto)
				{
					AutoTransitions.Add(t);
					continue;
				}

				bool hasEvent = Transitions.TryGetValue(t.EventName, out var matched);
				if (hasEvent)
				{
					matched.Add(t);
				}
				else
				{
					List<Transition> transitions = new() { t };
					Transitions.Add(t.EventName, transitions);
				}
			}
			else if (child is Reaction a)
			{
				a.Setup(hostStateChart, ref parentOrderId);
				bool hasEvent = Reactions.TryGetValue(a.EventName, out var matched);
				if (hasEvent)
				{
					matched.Add(a);
				}
				else
				{
					List<Reaction> reactions = new() { a };
					Reactions.Add(a.EventName, reactions);
				}
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

    public override void PostSetup()
    {
        foreach (State state in Substates)
        {
            state.PostSetup();
        }

        // Post process is order unconcerned
		foreach (var pair in Transitions)
		{
			foreach(var t in pair.Value)
			{
				t.PostSetup();
			}
		}
        
		foreach (var t in AutoTransitions)
		{
			t.PostSetup();
		}

		foreach (var pair in Reactions)
		{
			foreach(var a in pair.Value)
			{
				a.PostSetup();
			}
		}
    }

    public override bool GetPromoteStates(List<State> states)
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

    public override bool IsConflictToEnterRegion(
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
    
    public override void ExtendEnterRegion(
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

    public override void RegisterActiveState(SortedSet<State> activeStates)
    {
        activeStates.Add(HostState);
        foreach (State substate in HostState.Substates)
        {
            substate.RegisterActiveState(activeStates);
        }
    }

    public override int SelectTransitions(SortedSet<Transition> enabledTransitions, StringName eventName)
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

        List<Transition> matched;
		if (eventName is null)
		{
			matched = AutoTransitions;
		}
		else
		{
			bool hasEvent = Transitions.TryGetValue(eventName, out matched);
			if (!hasEvent)
			{
				return handleInfo;
			}
		}

        foreach (Transition t in matched)
        {
            if (handleInfo == 0)
            {
                if (!t.IsTargetless)
                {
                    continue;
                }
            }

            bool isEnabled = t.Check();
            if (isEnabled)
            {
                enabledTransitions.Add(t);
                handleInfo = 1;
                break;
            }
        }

        return handleInfo;
    }

    public override void DeduceDescendants(
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

    public override void SaveAllStateConfig(ref List<int> snapshot)
    {
        foreach (State substate in Substates)
        {
            substate.SaveActiveStateConfig(ref snapshot);
        }
    }

    public override void SaveActiveStateConfig(ref List<int> snapshot)
    {
        foreach (State substate in Substates)
        {
            substate.SaveActiveStateConfig(ref snapshot);
        }
    }

    public override bool LoadAllStateConfig(ref int[] config, ref int configIdx)
    {
        bool isLoadSuccess;
        foreach (State substate in Substates)
        {
            isLoadSuccess = substate.LoadAllStateConfig(ref config, ref configIdx);
            if (!isLoadSuccess)
            {
                return false;
            }
        }

        return true;
    }

    public override bool LoadActiveStateConfig(ref int[] config, ref int configIdx)
    {
        bool isLoadSuccess;
        foreach (State substate in Substates)
        {
            isLoadSuccess = substate.LoadAllStateConfig(ref config, ref configIdx);
            if (!isLoadSuccess)
            {
                return false;
            }
        }

        return true;
    }
}
