using Godot;
using System.Collections.Generic;

namespace LGWCP.StatechartSharp
{
    public class HistoryComponent : StateComponent
    {
        public HistoryComponent(State state) : base(state) {}

        public override void Init(Statechart hostStateChart, ref int ancestorId)
        {
            base.Init(hostStateChart, ref ancestorId);
            
            // No substates, transitions, or actions
            #if DEBUG
            foreach (Node child in HostState.GetChildren())
            {
                GD.PushWarning(HostState.GetPath(), ": History state should not have child.");
                break;
            }
            #endif
        }

        public override bool SelectTransitions(StringName eventName)
        {
            #if DEBUG
            GD.PushError("Should not select transition in history state.");
            #endif
            return false;
        }

        public virtual void DeduceDescendants(SortedSet<State> deducedSet)
        {
            
        }
    }
}