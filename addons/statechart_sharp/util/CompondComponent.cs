using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Godot;

namespace LGWCP.GodotPlugin.StatechartSharp
{
    public class CompondComponent : StateComponent
    {
        public CompondComponent(State state) : base(state) {}

        public override bool RegisterTransition(StringName eventName)
        {
            return base.RegisterTransition(eventName);
        }
    }
}