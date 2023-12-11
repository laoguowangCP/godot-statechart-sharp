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
                // All substates handled transition
                return true;
            }
            else
            {
                // No transition enabled in at least 1 substates, or atomic state
                return base.SelectTransitions(eventName);
            }
        }
    }
}