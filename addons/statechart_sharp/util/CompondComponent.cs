using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Godot;

namespace LGWCP.GodotPlugin.StatechartSharp
{
    public class CompondComponent : StateComponent
    {
        public CompondComponent(State state) : base(state) {}

        public override bool SelectTransitions(StringName eventName)
        {
            bool isHandled = false;
            if (HostState.CurrentState != null)
            {
                isHandled = HostState.CurrentState.SelectTransitions(eventName);
            }

            if (isHandled)
            {
                // Descendants have registered an enabled transition
                return true;
            }
            else
            {
                // No transition enabled in descendants, or atomic state
                return base.SelectTransitions(eventName);
            }
        }
    }
}