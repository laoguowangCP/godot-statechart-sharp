using Godot;
using Godot.Collections;

namespace LGWCP.GodotPlugin.StateChartSharp
{
    public partial class AtomicState : StateNode
    {
        public override void Init()
        {
            substates.Clear();
            transitions.Clear();
            
            foreach (Node child in GetChildren())
            {
                if (child is Transition t)
                {
                    transitions.Add(t);
                }
                else
                {
                    GD.PushError("LGWCP.GodotPlugin.AtomicState: Child node must be Transition.");
                }
            }
        }
    }
}