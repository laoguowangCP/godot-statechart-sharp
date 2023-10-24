using Godot;
using Godot.Collections;

namespace LGWCP.GodotPlugin.StateChartSharp
{
    public class ParallelStateComponent : StateComponent
    {
        public ParallelStateComponent(State state) : base(state) {}

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
					t.Init();
					GetTransitions(t.transitionMode).Add(t);
				}
				else
				{
					GD.PushWarning("LGWCP.GodotPlugin.StateChartSharp: Child should be State or Transition.");
				}
			}
        }

        public override void SubstateTransit(TransitionModeEnum mode)
		{
			foreach(State substate in substates)
			{
				substate.SubstateTransit(mode);
			}
		}

        public override void StateEnter()
		{
			foreach(State substate in substates)
			{
				substate.StateEnter();
			}
			state.EmitSignal(State.SignalName.Enter);
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