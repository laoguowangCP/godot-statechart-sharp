using Godot;
using Godot.Collections;

namespace LGWCP.GodotPlugin.StateChartSharp
{
    [GlobalClass]
    public partial class CompondState : State
    {
        /// <summary>
        /// If state is instant, transitions will be checked instantly
        /// when entered.
        /// </summary>
        [Export] protected bool isInstant = false;
        [Export] protected State defaultSubState;

        public override void Init()
        {
            base.Init();
            bool isFirstSubstate = true;
            
            // Load child nodes
            foreach (Node child in GetChildren())
            {
                if (child is State s)
                {
                    s.Init();
                    substates.Add(s.Name, s);
                    if (isFirstSubstate && defaultSubState is null)
                    {
                        defaultSubState = s;
                    }
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

        public override bool IsInstant() { return isInstant; }

        public override void SubstateTransit(Transition.TransitionModeEnum mode)
        {
            do
            {
                var transitions = currentSubstate.GetTransitions(mode);
                if (transitions is null)
                {
                    GD.PushError("LGWCP.GodotPlugin.StateChartSharp: Invalid transition mode.");
                }
                
                foreach(Transition t in transitions)
                {
                    if (t.CheckWithTransit(this))
                    {
                        break;
                    }
                }
            } while (currentSubstate.IsInstant());
            
            currentSubstate.SubstateTransit(mode);
        }

        public override void StateEnter()
        {
            if (substates.Count > 0)
            {
                // First child state is default substate
                currentSubstate = defaultSubState;
                currentSubstate.StateEnter();
            }
            EmitSignal(SignalName.Enter);
        }

        public override void StateEnter(string statePath)
        {
            
        }

        public override void StateExit()
        {
            if (substates.Count > 0)
            {
                currentSubstate.StateExit();
            }
            EmitSignal(SignalName.Exit);
        }

        public override void StateInput(InputEvent @event)
        {
            EmitSignal(SignalName.Input, @event);
            currentSubstate.StateInput(@event);
        }

        public override void StateUnhandledInput(InputEvent @event)
        {
            EmitSignal(SignalName.UnhandledInput, @event);
            currentSubstate.StateUnhandledInput(@event);
        }

        public override void StateProcess(double delta)
        {
            EmitSignal(SignalName.Process, delta);
            currentSubstate.StateProcess(delta);
        }

        public override void StatePhysicsProcess(double delta)
        {
            EmitSignal(SignalName.PhysicsProcess, delta);
            currentSubstate.StatePhysicsProcess(delta);
        }
    }
}