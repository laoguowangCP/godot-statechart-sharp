using Godot;
using Godot.Collections;

namespace LGWCP.GodotPlugin.StateChartSharp
{
    public class AtomicStateComponent : StateComponent
    {
        public AtomicStateComponent(State state) : base(state) {}

        public override void Init(StateChart stateChart, State parentState = null)
        {
            base.Init(stateChart, parentState);
            
            foreach (Node child in state.GetChildren())
            {
                if (child is Transition t)
                {
                    t.Init(state);
                    GetTransitions(t.GetTransitionMode()).Add(t);
                }
                else
                {
                    GD.PushWarning("LGWCP.GodotPlugin.StateChartSharp: Child node must be Transition.");
                }
            }
        }
        
        public override void StateEnter()
        {
            state.EmitSignal(State.SignalName.Enter);
        }

        public override void StateEnter(TransitionModeEnum mode)
        {
            state.EmitSignal(State.SignalName.Enter);
        }
        
        public override void StateExit()
        {
            state.EmitSignal(State.SignalName.Exit);
        }

        public override void StateInput(InputEvent @event)
        {
            state.EmitSignal(State.SignalName.Input, @event);
        }

        public override void StateUnhandledInput(InputEvent @event)
        {
            state.EmitSignal(State.SignalName.UnhandledInput, @event);
        }

        public override void StateProcess(double delta)
        {
            state.EmitSignal(State.SignalName.Process, delta);
        }

        public override void StatePhysicsProcess(double delta)
        {
            state.EmitSignal(State.SignalName.PhysicsProcess, delta);
        }
    }
}