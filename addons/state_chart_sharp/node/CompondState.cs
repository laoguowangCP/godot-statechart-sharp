using Godot;
using Godot.Collections;

namespace LGWCP.GodotPlugin.StateChartSharp
{
    [GlobalClass]
    public partial class CompondState : StateNode
    {
        /// <summary>
        /// If state is instant, transitions will be checked instantly
        /// when entered.
        /// </summary>
        [Export] protected bool isInstant = false;
        [Export] protected StateNode defaultSubState;

        public override void Init()
        {
            substates.Clear();
            transitions.Clear();
            
            // Load child nodes
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
            if (substates.Count > 0)
            {
                // First child state is default substate
                currentSubstate = substates[0];
                currentSubstate.StateEnter();
            }
            EmitSignal(SignalName.Enter);
        }

        public override void StateExit()
        {
            if (substates.Count > 0)
            {
                currentSubstate.StateExit();
            }
            EmitSignal(SignalName.Exit);
        }

        public override bool IsInstant() { return isInstant; }

        public override void SubstateTransit(Transition.TransitionModeEnum mode)
        {
            do
            {
                foreach(Transition t in currentSubstate.transitions)
                {
                    if (t.transitionMode == mode && t.CheckWithTransit(this))
                        break;
                }
            } while (currentSubstate.IsInstant());
            
            currentSubstate.SubstateTransit(mode);
        }
    }
}