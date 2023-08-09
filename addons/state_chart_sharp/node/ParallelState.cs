using Godot;
using Godot.Collections;

namespace LGWCP.GodotPlugin.StateChartSharp
{
    [GlobalClass]
    public partial class ParallelState : State
    {
        public override void Init()
        {
            base.Init();
            
            foreach (Node child in GetChildren())
            {
                if (child is State s)
                {
                    s.Init();
                    substates.Add(s);
                }
                else if (child is Transition t)
                {
                    t.Init();
                    GetTransitions(t.transitionMode).Add(t);
                }
                else
                {
                    GD.PushError("LGWCP.GodotPlugin.StateChartSharp: Child must be StateNode or Transition.");
                }
            }
        }
        
        public override void SubstateTransit(Transition.TransitionModeEnum mode)
        {
            foreach(State s in substates)
            {
                s.SubstateTransit(mode);
            }
        }

        public override void StateEnter()
        {
            foreach(State s in substates)
            {
                s.StateEnter();
            }
            EmitSignal(SignalName.Enter);
        }

        public override void StateExit()
        {
            foreach(State s in substates)
            {
                s.StateExit();
            }
            EmitSignal(SignalName.Exit);
        }
        
        public override void StateInput(InputEvent @event)
        {
            EmitSignal(SignalName.Input, @event);
            foreach(State s in substates)
            {
                s.StateInput(@event);
            }
        }

        public override void StateUnhandledInput(InputEvent @event)
        {
            EmitSignal(SignalName.UnhandledInput, @event);
            foreach(State s in substates)
            {
                s.StateUnhandledInput(@event);
            }
        }

        public override void StateProcess(double delta)
        {
            EmitSignal(SignalName.Process, delta);
            foreach(State s in substates)
            {
                s.StateProcess(delta);
            }
        }

        public override void StatePhysicsProcess(double delta)
        {
            EmitSignal(SignalName.PhysicsProcess, delta);
            foreach(State s in substates)
            {
                s.StatePhysicsProcess(delta);
            }
        }
    }
}