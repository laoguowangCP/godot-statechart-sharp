using Godot;
using Godot.Collections;

namespace LGWCP.GodotPlugin.StateChartSharp
{
    public partial class AtomicStateNode : StateNode
    {
        public override void Init()
        {
            substates.Clear();
            transitions.Clear();
            
            foreach (Node child in GetChildren())
            {
                if (child is TransitionNode t)
                {
                    transitions.Add(t);
                }
                else
                {
                    GD.PushError("LGWCP.GodotPlugin.AtomicStateNode: Child node must be Transition.");
                }
            }
        }
    }
}