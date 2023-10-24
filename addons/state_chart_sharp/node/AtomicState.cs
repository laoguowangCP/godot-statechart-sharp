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
        public override void Init(StateChart stateChart, State parentState = null)
        {
            base.Init(stateChart, parentState);
            
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
    }
}