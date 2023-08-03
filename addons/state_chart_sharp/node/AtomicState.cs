using Godot;
using Godot.Collections;

namespace LGWCP.GodotPlugin.StateChartSharp
{
    [GlobalClass]
    public partial class AtomicState : StateNode
    {
        /// <summary>
        /// If state is instant, transitions will be checked instantly
        /// when entered.
        /// </summary>
        [Export] protected bool isInstant = false;
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
                    GD.PushError("LGWCP.GodotPlugin.StateChartSharp: Child node must be Transition.");
                }
            }
        }

        public override bool IsInstant() { return isInstant; }
    }
}