using System.Collections.Generic;
using System.Linq;
using Godot;


namespace LGWCP.Godot.StatechartSharp;

public class CompoundImpl : StateImpl
{
	private State CurrentState;
	private State InitialState;

	public CompoundImpl(State state) : base(state) {}

	public override void _Setup(Statechart hostStateChart, ref int parentOrderId, int substateIdx)
	{
		base._Setup(hostStateChart, ref parentOrderId, substateIdx);

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
				s._Setup(hostStateChart, ref parentOrderId, Substates.Count);
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
				t._Setup(hostStateChart, ref parentOrderId);
			}
			else if (child is Reaction a)
			{
				a._Setup(hostStateChart, ref parentOrderId);
			}
		}

		if (lastSubstate != null)
		{
			if (lastSubstate._UpperState != null)
			{
				// Last substate's upper is upper-state
				UpperState = lastSubstate._UpperState;
			}
			else
			{
				// Last substate is upper-state
				UpperState = lastSubstate;
			}
		}
		// Else state is atomic, lower and upper are both null.

		// Set initial state
		InitialState = HostState.InitialState;
		if (InitialState != null)
		{
			// Check selected initial-state is non-history substate
			if (InitialState._ParentState != HostState || !InitialState._IsValidState())
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
				if (substate._IsValidState())
				{
					InitialState = substate;
					break;
				}
			}
		}

		// Set current state
		CurrentState = InitialState;
	}

	public override void _SetupPost()
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
				if (t._IsAuto)
				{
					AutoTransitions.Add(t);
					continue;
				}

				// Add transition
				if (Transitions.TryGetValue(t._EventName, out var eventTransitions))
				{
					eventTransitions.Add(t);
				}
				else
				{
					Transitions[t._EventName] = new() { t };
				}
			}
			else if (child is Reaction a)
			{
				a._SetupPost();

				// Add reaction
				if (Reactions.TryGetValue(a._EventName, out var eventReactions))
				{
					eventReactions.Add(a);
				}
				else
				{
					Reactions[a._EventName] = new() { a };
				}
			}
		}

		base._SetupPost();
	}

	public override bool _IsConflictToEnterRegion(
		State substateToPend,
		SortedSet<State> enterRegionUnextended)
	{
		// Conflicts if any substate is already exist in region
		return enterRegionUnextended.Any<State>(
			state => HostState._IsAncestorStateOf(state));
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
			extraEnterRegion.Add(HostState);
		}

		// Need check
		if (needCheckContain)
		{
			foreach (State substate in Substates)
			{
				if (enterRegion.Contains(substate))
				{
					substate._ExtendEnterRegion(
						enterRegion, enterRegionEdge, extraEnterRegion, true);
					return;
				}
			}
		}

		// Need no check, or need check but no substate in region
		if (InitialState != null)
		{
			InitialState._ExtendEnterRegion(
				enterRegion, enterRegionEdge, extraEnterRegion, false);
		}
	}

	public override bool _GetPromoteStates(List<State> states)
	{
		bool isPromote = true;
		foreach (Node child in HostState.GetChildren())
		{
			if (child is State state)
			{
				bool isChildPromoted = state._GetPromoteStates(states);
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

	public override void _SubmitActiveState(SortedSet<State> activeStates)
	{
		activeStates.Add(HostState);
		CurrentState?._SubmitActiveState(activeStates);
	}

	public override int _SelectTransitions(
		SortedSet<Transition> enabledTransitions, string eventName)
	{
		int handleInfo = -1;
		// if (HostState._CurrentState != null)
		if (CurrentState != null)
		{
			handleInfo = CurrentState._SelectTransitions(enabledTransitions, eventName);
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

		
		// TODO: revert to state-wise TA list
		/*
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
			matched = CurrentTAMap[_StateId].Transitions;
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
				if (!t._IsTargetless)
				{
					continue;
				}
			}

			bool isEnabled = t._Check();
			if (isEnabled)
			{
				enabledTransitions.Add(t);
				handleInfo = 1;
				break;
			}
		}
		*/

		if (eventName is null)
		{
			foreach (Transition t in AutoTransitions)
			{
				// If == 0, only check targetless
				if (handleInfo == 0)
				{
					if (!t._IsTargetless)
					{
						continue;
					}
				}

				bool isEnabled = t._Check();
				if (isEnabled)
				{
					enabledTransitions.Add(t);
					handleInfo = 1;
					break;
				}
			}
		}
		else
		{
			if (Transitions.TryGetValue(eventName, out var eventTransitions))
			{
				foreach (Transition t in eventTransitions)
				{
					// If == 0, only check targetless
					if (handleInfo == 0)
					{
						if (!t._IsTargetless)
						{
							continue;
						}
					}

					bool isEnabled = t._Check();
					if (isEnabled)
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

	public override void _DeduceDescendantsRecurr(
		SortedSet<State> deducedSet, DeduceDescendantsModeEnum deduceMode)
	{
		/*
		If is edge state:
			1. It is called from history substate
			2. IsHistory arg represents IsDeepHistory

		if (isEdgeState)
		{
			if (CurrentState != null)
			{
				bool isDeepHistory = isHistory;
				CurrentState._DeduceDescendants(deducedSet, isDeepHistory, false);
			}
			return;
		}

		// Not edge state
		deducedSet.Add(HostState);
		State deducedSubstate = isHistory ? CurrentState : InitialState;

		if (deducedSubstate != null)
		{
			deducedSubstate._DeduceDescendants(deducedSet, isHistory, false);
		}
		*/
		State substateToAdd;
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
		substateToAdd._DeduceDescendantsRecurr(deducedSet, substateDeduceMode);
	}

	public override void _HandleSubstateEnter(State substate)
	{
		CurrentState = substate;
	}

	public override void _SaveAllStateConfig(List<int> snapshot)
	{
		if (CurrentState is null)
		{
			return;
		}
		snapshot.Add(CurrentState._SubstateIdx);
		foreach (State substate in Substates)
		{
			substate._SaveAllStateConfig(snapshot);
		}
	}

	public override void _SaveActiveStateConfig(List<int> snapshot)
	{
		if (CurrentState is null)
		{
			return;
		}
		snapshot.Add(CurrentState._SubstateIdx);
		CurrentState._SaveActiveStateConfig(snapshot);
	}

	public override int _LoadAllStateConfig(int[] config, int configIdx)
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
			configIdx = substate._LoadAllStateConfig(config, configIdx);
			if (configIdx == -1)
			{
				return -1;
			}
		}

		return configIdx;
	}

	public override int _LoadActiveStateConfig(int[] config, int configIdx)
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

		return CurrentState._LoadActiveStateConfig(config, configIdx);
	}

#if TOOLS
	public override void _GetConfigurationWarnings(List<string> warnings)
	{
		// Check parent
		base._GetConfigurationWarnings(warnings);

		// Check child
		if (InitialState != null)
		{
			var initialStateParent = InitialState.GetParentOrNull<Node>();
			if (initialStateParent is null
				|| initialStateParent != HostState
				|| !InitialState._IsValidState())
			{
				warnings.Add("Initial state should be a non-history substate.");
			}
		}
	}
#endif
}
