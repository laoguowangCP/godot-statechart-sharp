using Godot;
using Godot.Collections;

namespace LGWCP.GodotPlugin.StateChartSharp
{
    [GlobalClass]
    public partial class ParallelState : StateNode
    {
        public override void Init()
        {
            substates.Clear();
            transitions.Clear();
            
            foreach (Node child in GetChildren())
            {
                if (child is StateNode s)
                {
                    s.Init();
                    substates.Add(s);
                }
                else if (child is Transition t)
                {
                    transitions.Add(t);
                }
                else
                {
                    GD.PushError("LGWCP.GodotPlugin.StateChartSharp: Child must be StateNode or Transition.");
                }
            }
        }

        public override void StateEnter()
        {
            foreach(StateNode s in substates)
            {
                s.StateEnter();
            }
            EmitSignal(SignalName.Enter);
        }

        public override void StateExit()
        {
            foreach(StateNode s in substates)
            {
                s.StateExit();
            }
            EmitSignal(SignalName.Exit);
        }

        public override void SubstateTransit(Transition.TransitionModeEnum mode)
        {
            foreach(StateNode s in substates)
            {
                s.SubstateTransit(mode);
            }
        }

    }
}