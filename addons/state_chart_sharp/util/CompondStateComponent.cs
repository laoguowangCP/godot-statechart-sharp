using Godot;
using Godot.Collections;

namespace LGWCP.GodotPlugin.StateChartSharp
{
    public class CompondStateComponent : StateComponent
    {
        protected State currentSubstate = null;
        protected State defaultSubstate = null;

        public CompondStateComponent(State state) : base(state)
        {}

        public override void Init(StateChart stateChart,  State parentState = null)
        {
            // base.Init(stateChart, parentState);
            
            // Load child nodes
            foreach (Node child in state.GetChildren())
            {
                if (child is State substate)
                {
                    substate.Init(stateChart, parentState);
                    substates.Add(substate);
                }
                else if (child is Transition t)
                {
                    t.Init();
                    GetTransitions(t.transitionMode).Add(t);
                }
                else
				{
					GD.PushWarning("LGWCP.GodotPlugin.StateChartSharp: Child should be State or Transition.");
				}
            }

            if (substates.Count > 0)
            {
                // First as default substate
                defaultSubstate = substates[0];
            }
        }

        public override void SubstateTransit(TransitionModeEnum mode)
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
                    if (t.CheckWithTransit(state))
                    {
                        break;
                    }
                }
            } while (currentSubstate.IsInstant());
            
            currentSubstate.SubstateTransit(mode);
        }
    }
}