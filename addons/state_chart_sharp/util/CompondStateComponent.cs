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

        public override void SubstateTransit(TransitionModeEnum mode, bool recursive = true)
        {
            bool isTransited = false;
            
            var transitions = currentSubstate.GetTransitions(mode);
            foreach(Transition t in transitions)
            {
                if (t.Check())
                {
                    // Transit to new state.
                    // If new state is instant, keep check.
                    currentSubstate.StateExit();
                    currentSubstate = t.GetToState();
                    currentSubstate.StateEnter(mode);
                    isTransited = true;
                    break;
                }
            }

            if (recursive && !isTransited)
            {
                currentSubstate.SubstateTransit(mode);
            }
        }

        public override void InstantTransit(TransitionModeEnum mode)
        {
            SubstateTransit(mode, false);
        }

        public override void StateEnter()
        {
            state.EmitSignal(State.SignalName.Enter);
            if (defaultSubstate is not null)
            {
                // First child state is default substate
                currentSubstate = defaultSubstate;
                currentSubstate.StateEnter();
            }
        }

        public override void StateEnter(TransitionModeEnum mode)
        {
            state.EmitSignal(State.SignalName.Enter);
            // TODO: handle instant state transition here?
            // Forget about instant trans on init()
            // Just fix instant trans on transition
            if (state.IsInstant())
            {
                state.ParentState.InstantTransit(mode);
            }

            if (defaultSubstate is not null)
            {
                // First child state is default substate
                currentSubstate = defaultSubstate;
                currentSubstate.StateEnter(mode);
            }
        }

        public override void StateExit()
        {
            if (currentSubstate is not null)
            {
                currentSubstate.StateExit();
                currentSubstate = null;
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