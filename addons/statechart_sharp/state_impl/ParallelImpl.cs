using Godot;
using System.Collections.Generic;
using System.Linq;


namespace LGWCP.Godot.StatechartSharp;

public class ParallelImpl : StateImpl
{
	protected int ValidSubstateCnt = 0; // Compound or parallel, not history

	public ParallelImpl(State state) : base(state) {}

	public override void _Setup(Statechart hostStateChart, ref int parentOrderId, int substateIdx)
	{
		base._Setup(hostStateChart, ref parentOrderId, substateIdx);

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
				s._Setup(hostStateChart, ref parentOrderId, Substates.Count);
				Substates.Add(s);

				// First substate is lower-state
				if (LowerState == null)
				{
					LowerState = s;
				}
				lastSubstate = s;

				if (s._IsValidState())
				{
					++ValidSubstateCnt;
				}
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

		if (lastSubstate is not null)
		{
			if (lastSubstate._UpperState is not null)
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
		// Else state is atomic, lower and upper are null
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
	}

	public override bool _SubmitPromoteStates(List<State> states)
	{
		bool isPromote = true;
		foreach (Node child in HostState.GetChildren())
		{
			if (child is State state
                && state._SubmitPromoteStates(states))
            {
                isPromote = false;
                break;
            }
		}

		if (isPromote)
		{
			states.Add(HostState);
		}

		// Make sure promoted
		return true;
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
				state => HostState._IsAncestorStateOf(state));
		}

		// Any history substate in region, conflicts.
		foreach (State substate in Substates)
		{
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
			extraEnterRegion.Add(HostState);
		}

		// Need check
		if (needCheckContain)
		{
			foreach (State substate in Substates)
			{
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
		foreach (State substate in Substates)
		{
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
		activeStates.Add(HostState);
		foreach (State substate in HostState._Substates)
		{
			substate._SubmitActiveState(activeStates);
		}
	}

	public override int _SelectTransitions(
		SortedSet<Transition> enabledTransitions, string eventName)
	{
		int handleInfo = -1;
		if (ValidSubstateCnt > 0)
		{
			int negCnt = 0;
			int posCnt = 0;
			foreach (State substate in Substates)
			{
				if (!substate._IsValidState())
				{
					continue;
				}

				int substateHandleInfo = substate._SelectTransitions(enabledTransitions, eventName);
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

				if (t._Check())
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

					if (t._Check())
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
		/*
		If is edge-state:
			1. Called from history substate.
			2. IsHistory arg represents IsDeepHistory

		if (!isEdgeState)
		{
			deducedSet.Add(HostState);
		}

		foreach (State substate in Substates)
		{
			// Ignore history states
			if (!substate._IsValidState())
			{
				continue;
			}
			substate._DeduceDescendants(deducedSet, isHistory, false);
		}
		*/

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

		foreach (State substate in Substates)
		{
			// Ignore history states
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
		foreach (State substate in Substates)
		{
			substate._SaveAllStateConfig(snapshot);
		}
	}

	public override void _SaveActiveStateConfig(List<int> snapshot)
	{
		foreach (State substate in Substates)
		{
			substate._SaveActiveStateConfig(snapshot);
		}
	}

	public override int _LoadAllStateConfig(int[] config, int configIdx)
	{
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
}
