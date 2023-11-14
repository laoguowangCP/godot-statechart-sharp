using Godot;
using System.Collections.Generic;

namespace LGWCP.GodotPlugin.StateChartSharp
{
    public class ParallelComponent : StateComponent
    {
        protected List<State> substates;
        public ParallelComponent(State state) : base(state)
		{
            // Initialize substate list
            substates = new List<State>();
		}

        public override void Init(StateChart stateChart, State parentState)
        {
            // base.Init(stateChart, parentState);

            foreach (Node child in state.GetChildren())
			{
				if (child is State substate)
				{
					substate.Init(stateChart, state);
					substates.Add(substate);
				}
				else if (child is Transition t)
				{
					t.Init(state);
					// GetTransitions(t.GetTransitionMode()).Add(t);
				}
				else
				{
					GD.PushWarning("LGWCP.GodotPlugin.StateChartSharp: Child should be State or Transition.");
				}
			}
        }

        public override void SubstateTransit(
			TransitionModeEnum mode,
			State fromState,
			State toState,
			bool recursive)
		{
			if (fromState is not null || toState is not null)
			{
				return;
			}

			// fromState is not null && toState is not null
			// normal transition
			foreach(State substate in substates)
			{
				bool doTransit = false;

				fromState = substate;
            	toState = null;

				var transitions = substate.GetTransitions(mode);
                foreach(Transition t in transitions)
                {

                    if (t.Check())
                    {
                        toState = t.GetToState();
                        doTransit = true;
                        break;
                    }
                }

				if (doTransit)
				{
					// cross level transit
					while (fromState.ParentState != toState.ParentState)
					{
						if (fromState.StateLevel >= toState.StateLevel)
						{
							fromState = fromState.ParentState;
						}
						else // from.StateLevel < to.StateLevel
						{
							toState = toState.ParentState;
						}
					}

					if (fromState.ParentState.GetStateMode() == StateModeEnum.Compond)
					{
						// remote transit
						fromState.ParentState.SubstateTransit(mode, fromState, toState);
					}
					else
					{
						// invalid transit or local transit
						doTransit = false;
					}
				}

				// if do transit, must be remote transit, stop check
				if (doTransit)
				{
					break;
				}

				substate.SubstateTransit(mode);
			}
		}

        public override void StateEnter()
		{
			state.EmitSignal(State.SignalName.Enter);
			foreach(State substate in substates)
			{
				substate.StateEnter();
			}
		}
		public override void StateEnter(TransitionModeEnum mode, bool checkInstant)
		{
			state.EmitSignal(State.SignalName.Enter);
			foreach(State substate in substates)
			{
				substate.StateEnter(mode);
			}
		}

        public override void StateExit()
		{
			foreach(State substate in substates)
			{
				substate.StateExit();
			}
			state.EmitSignal(State.SignalName.Exit);
		}

        public override void StateInput(InputEvent @event)
		{
			state.EmitSignal(State.SignalName.Input, @event);
			foreach(State substate in substates)
			{
				substate.StateInput(@event);
			}
		}

		public override void StateUnhandledInput(InputEvent @event)
		{
			state.EmitSignal(State.SignalName.UnhandledInput, @event);
			foreach(State substate in substates)
			{
				substate.StateUnhandledInput(@event);
			}
		}

		public override void StateProcess(double delta)
		{
			state.EmitSignal(State.SignalName.Process, delta);
			foreach(State substate in substates)
			{
				substate.StateProcess(delta);
			}
		}

		public override void StatePhysicsProcess(double delta)
		{
			state.EmitSignal(State.SignalName.PhysicsProcess, delta);
			foreach(State substate in substates)
			{
				substate.StatePhysicsProcess(delta);
			}
		}
    }
}