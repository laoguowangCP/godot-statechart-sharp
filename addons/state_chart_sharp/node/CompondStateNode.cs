using Godot;
using Godot.Collections;

namespace LGWCP.GodotPlugin.StateChartSharp
{
    public partial class CompondStateNode : StateNode
    {
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
                else if (child is TransitionNode t)
                {
                    transitions.Add(t);
                }
                else
                {
                    GD.PushError("LGWCP.GodotPlugin.CompoundStateNode: Child must be StateNode or Transition.");
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

        public override void SubstateTransit(TransitionNode.TransitionModeEnum mode)
        {
            foreach(TransitionNode t in currentSubstate.transitions)
            {
                if (t.transitionMode == mode && t.CheckWithTransit(this))
                    break;
            }
            currentSubstate.SubstateTransit(mode);
        }
    }
}