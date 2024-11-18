using System.Collections.Generic;
using System.Linq;
using Godot;


namespace LGWCP.Godot.StatechartSharp;


public class CompoundImpl : StateImpl
{
	private State CurrentState
	{
		get => HostState.CurrentState;
		set { HostState.CurrentState = value; }
	}
	private State InitialState
	{
		get => HostState.InitialState;
		set { HostState.InitialState = value; }
	}
	
	public CompoundImpl(State state) : base(state) {}

	public override bool IsAvailableRootState()
	{
		return true;
	}

	public override void Setup(Statechart hostStateChart, ref int parentOrderId, int substateIdx)
	{
		base.Setup(hostStateChart, ref parentOrderId, substateIdx);

		// Init & collect states, transitions, actions
		// Get lower state and upper state
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
				if (LowerState is null)
				{
					LowerState = s;
				}
				lastSubstate = s;
			}
			else if (child is Transition t)
			{
				// Root state should not have transition
				if (ParentState is null)
				{
					continue;
				}
				t.Setup(hostStateChart, ref parentOrderId);
			}
			else if (child is Reaction a)
			{
				a.Setup(hostStateChart, ref parentOrderId);
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
		// Else state is atomic, lower and upper are both null.

		// Set initial state
		if (InitialState != null)
		{
			// Check selected initial-state is non-history substate
			if (InitialState.ParentState != HostState || InitialState.IsHistory)
			{
#if DEBUG
				GD.PushWarning(
					HostState.GetPath(),
					": initial-state should be a non-history substate.");
#endif
				InitialState = null;
			}
		}

		// No assigned initial state, use first non-history substate
		if (InitialState == null)
		{
			foreach (State substate in Substates)
			{
				if (substate.StateMode != StateModeEnum.History)
				{
					InitialState = substate;
					break;
				}
			}
		}

		// Set current state
		CurrentState = InitialState;
	}

	public override void SetupPost()
	{
		foreach (Node child in HostState.GetChildren())
		{
			if (child.IsQueuedForDeletion())
			{
				continue;
			}
			
			if (child is State s)
			{
				s._SetupPost();
			}
			else if (child is Transition t)
			{
				// Root state should not have transition
				if (ParentState is null)
				{
					continue;
				}

				t._SetupPost();
				if (t.IsAuto)
				{
					AutoTransitions.Add(t);
					continue;
				}
				HostStatechart.RegistGlobalTransition(StateId, t.EventName, t);
			}
			else if (child is Reaction a)
			{
				a._SetupPost();
				HostStatechart.RegistGlobalReaction(StateId, a.EventName, a);
			}
		}

		base.SetupPost();
	}

	public override bool IsConflictToEnterRegion(
		State substateToPend,
		SortedSet<State> enterRegionUnextended)
	{
		// Conflicts if any substate is already exist in region
		return enterRegionUnextended.Any<State>(
			state => HostState.IsAncestorStateOf(state));
	}

	public override void ExtendEnterRegion(
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
			extraEnterRegion.Add(HostState);
		}

		// Need check
		if (needCheckContain)
		{
			foreach (State substate in Substates)
			{
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
	
	public override void SubmitActiveState(SortedSet<State> activeStates)
	{
		activeStates.Add(HostState);
		CurrentState?.SubmitActiveState(activeStates);
	}

	public override int SelectTransitions(
		SortedSet<Transition> enabledTransitions, bool isAuto)
	{
		int handleInfo = -1;
		if (HostState.CurrentState != null)
		{
			handleInfo = CurrentState.SelectTransitions(enabledTransitions, isAuto);
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

		List<Transition> matched;
		if (isAuto)
		{
			matched = AutoTransitions;
		}
		else
		{
			if (CurrentTAMap is null)
			{
				return handleInfo;
			}
			matched = CurrentTAMap[StateId].Transitions;
			if (matched is null)
			{
				return handleInfo;
			}
		}

		foreach (Transition t in matched)
		{
			// If == 0, only check targetless
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
		If is edge state:
			1. It is called from history substate
			2. IsHistory arg represents IsDeepHistory
		*/
		if (isEdgeState)
		{
			if (CurrentState != null)
			{
				bool isDeepHistory = isHistory;
				CurrentState.DeduceDescendants(deducedSet, isDeepHistory, false);
			}
			return;
		}

		// Not edge state
		deducedSet.Add(HostState);
		State deducedSubstate = isHistory ? CurrentState : InitialState;

		if (deducedSubstate != null)
		{
			deducedSubstate.DeduceDescendants(deducedSet, isHistory, false);
		}
	}

	public override void HandleSubstateEnter(State substate)
	{
		HostState.CurrentState = substate;
	}

	public override void SaveAllStateConfig(List<int> snapshot)
	{
		// Breadth first for better load order
		if (CurrentState is null)
		{
			return;
		}
		snapshot.Add(CurrentState.SubstateIdx);
		foreach (State substate in Substates)
		{
			substate.SaveActiveStateConfig(snapshot);
		}
	}

	public override void SaveActiveStateConfig(List<int> snapshot)
	{
		// Breadth first for better load order
		if (CurrentState is null)
		{
			return;
		}
		snapshot.Add(CurrentState.SubstateIdx);
		CurrentState.SaveActiveStateConfig(snapshot);
	}

	public override int LoadAllStateConfig(int[] config, int configIdx)
	{
		if (Substates.Count == 0)
		{
			return configIdx;
		}

		if (configIdx >= config.Length)
		{
			return -1;
		}

		CurrentState = Substates[config[configIdx]];
		++configIdx;
		foreach (State substate in Substates)
		{
			configIdx = substate.LoadAllStateConfig(config, configIdx);
			if (configIdx == -1)
			{
				return -1;
			}
		}

		return configIdx;
	}

	public override int LoadActiveStateConfig(int[] config, int configIdx)
	{
		if (Substates.Count == 0)
		{
			return configIdx;
		}

		if (configIdx > config.Length)
		{
			return -1;
		}

		CurrentState = Substates[config[configIdx]];
		++configIdx;

		return CurrentState.LoadActiveStateConfig(config, configIdx);
	}

#if TOOLS
	public override void GetConfigurationWarnings(List<string> warnings)
	{
		// Check parent
		base.GetConfigurationWarnings(warnings);

		// Check child
		if (InitialState != null)
		{
			var initialStateParent = InitialState.GetParentOrNull<Node>();
			if (initialStateParent is null
				|| initialStateParent != HostState
				|| InitialState.IsHistory)
			{
				warnings.Add("Initial state should be a non-history substate.");
			}
		}
	}
#endif
}
