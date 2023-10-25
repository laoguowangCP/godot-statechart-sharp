using Godot;
using System.Collections.Generic;

namespace LGWCP.GodotPlugin.StateChartSharp
{
    public class ParallelStateComponent : StateComponent
    {
        protected List<State> substates;
        public ParallelStateComponent(State state) : base(state)
		{
            // Initialize substate list
            substates = new List<State>();
		}

        public override void Init(StateChart stateChart, State parentState = null)
        {
            // base.Init(stateChart, parentState);

            foreach (Node child in state.GetChildren())
			{
				if (child is State substate)
				{
					substate.Init(stateChart, parentState);
					substates.Add(substate);
				}
				else if (child is Transition t)
				{
					t.Init(state);
					GetTransitions(t.GetTransitionMode()).Add(t);
				}
				else
				{
					GD.PushWarning("LGWCP.GodotPlugin.StateChartSharp: Child should be State or Transition.");
				}
			}
        }

        public override void SubstateTransit(TransitionModeEnum mode, bool recursive = true)
		{
			foreach(State substate in substates)
			{
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
		public override void StateEnter(TransitionModeEnum mode)
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