using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Godot;

namespace LGWCP.GodotPlugin.StatechartSharp
{
    public class ParallelComponent : StateComponent
    {
        public ParallelComponent(State state) : base(state) {}

        public override bool SelectTransitions(StringName eventName)
        {
            bool isHandled = false;
            if (HostState.Substates.Count > 0)
            {
                isHandled = true;
                foreach (State s in HostState.Substates)
                {
                    isHandled = isHandled && s.SelectTransitions(eventName);
                }
            }

            if (isHandled)
            {
                // Any descendant handled transition
                return true;
            }
            else
            {
                // Atomic state, or no transition enabled in descendants
                return base.SelectTransitions(eventName);
            }
        }

        public override void DeduceDescendants(SortedSet<State> deducedSet, bool isDeepHistory)
        {
            foreach (State s in HostState.Substates)
            {
                deducedSet.Add(s);
                s.DeduceDescendants(deducedSet, isDeepHistory);
            }
        }
    }
}