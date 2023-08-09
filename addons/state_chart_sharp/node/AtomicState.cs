using Godot;
using Godot.Collections;

namespace LGWCP.GodotPlugin.StateChartSharp
{
    [GlobalClass]
    public partial class AtomicState : State
    {
        /// <summary>
        /// If state is instant, transitions will be checked instantly
        /// when entered.
        /// </summary>
        [Export] protected bool isInstant = false;
        public override void Init()
        {
            base.Init();
            
            foreach (Node child in GetChildren())
            {
                if (child is Transition t)
                {
                    t.Init();
                    GetTransitions(t.transitionMode).Add(t);
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