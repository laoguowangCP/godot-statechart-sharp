using Godot;
using Godot.Collections;

namespace LGWCP.GodotPlugin.StateChartSharp
{
    public partial class ParallelStateNode : StateNode
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
                else if (child is TransitionNode t)
                {
                    transitions.Add(t);
                }
                else
                {
                    GD.PushError("LGWCP.GodotPlugin.ParallelStateNode: Child must be StateNode or Transition.");
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

        public override void SubstateTransit(TransitionNode.TransitionModeEnum mode)
        {
            foreach(StateNode s in substates)
            {
                s.SubstateTransit(mode);
            }
        }

    }
}