using Godot;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;

namespace LGWCP.GodotPlugin.StatechartSharp
{
    public class HistoryComponent : StateComponent
    {
        public HistoryComponent(State state) : base(state) {}

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