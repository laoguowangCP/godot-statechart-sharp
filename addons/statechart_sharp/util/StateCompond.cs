using Godot;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;

namespace LGWCP.GodotPlugin.StatechartSharp
{
    public class StateComponent
    {
        protected State State;
        public StateComponent(State state)
        {
            State = state;
        }
        public virtual bool RegisterTransition(StringName eventName) { return false; }
    }
}