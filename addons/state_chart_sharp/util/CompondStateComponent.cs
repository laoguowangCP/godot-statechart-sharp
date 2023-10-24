using Godot;
using System.Collections.Generic;

namespace LGWCP.GodotPlugin.StateChartSharp
{
    public class CompondStateComponent : StateComponent
    {
        protected List<State> substates;
        protected State currentSubstate = null;
        protected State defaultSubstate = null;

        public CompondStateComponent(State state) : base(state)
        {
            // Initialize substate list
            substates = new List<State>();
        }

        public override void Init(StateChart stateChart,  State parentState = null)
        {
            // base.Init(stateChart, parentState);
            
            // Load child nodes
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

            if (substates.Count > 0)
            {
                // First as default substate
                defaultSubstate = substates[0];
            }
        }

        public override void SubstateTransit(TransitionModeEnum mode)
        {
            bool isKeepCheck = true;
            bool isTransited = false;
            while (isKeepCheck)
            {
                isKeepCheck = false;
                var transitions = currentSubstate.GetTransitions(mode);
                if (transitions is null)
                {
                    break;
                }
                foreach(Transition t in transitions)
                {
                    if (t.CheckWithTransit(state))
                    {
                        // Transit to new state.
                        // If new state is instant, keep check.
                        isTransited = true;
                        isKeepCheck = currentSubstate.IsInstant();
                        break;
                    }
                }
            }

            if (!isTransited)
            {
                currentSubstate.SubstateTransit(mode);
            }
        }

        public override void StateEnter()
        {
            state.EmitSignal(State.SignalName.Enter);
            // TODO: handle instant state transition here?
            if (defaultSubstate is not null)
            {
                // First child state is default substate
                currentSubstate = defaultSubstate;
                currentSubstate.StateEnter();
            }
        }

        public override void StateExit()
        {
            if (currentSubstate is not null)
            {
                currentSubstate.StateExit();
            }
            state.EmitSignal(State.SignalName.Exit);
        }

        public override void StateInput(InputEvent @event)
        {
            state.EmitSignal(State.SignalName.Input, @event);
            currentSubstate.StateInput(@event);
        }

        public override void StateUnhandledInput(InputEvent @event)
        {
            state.EmitSignal(State.SignalName.UnhandledInput, @event);
            currentSubstate.StateUnhandledInput(@event);
        }

        public override void StateProcess(double delta)
        {
            state.EmitSignal(State.SignalName.Process, delta);
            currentSubstate.StateProcess(delta);
        }

        public override void StatePhysicsProcess(double delta)
        {
            state.EmitSignal(State.SignalName.PhysicsProcess, delta);
            currentSubstate.StatePhysicsProcess(delta);
        }
    }
}