using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;


namespace LGWCP.StatechartSharp
{
    public enum EventNameEnum : int
    {
        PROCESS,
        PHYSICS_PROCESS,
        INPUT,
        UNHANDLED_INPUT,

        // Extend eventset here:
        /*
            MY_EVENT,
        */

        CUSTOM,
        AUTO
    }

    public partial class StatechartConfig : Node
    {
        
        #region Preset EventName

        public static readonly StringName EVENT_PROCESS = "_process";
        public static readonly StringName EVENT_PHYSICS_PROCESS = "_physics_process";
        public static readonly StringName EVENT_INPUT = "_input";
        public static readonly StringName EVENT_UNHANDLED_INPUT = "_unhandled_input";
        
        // Extend event set here:
        /*
            protected static readonly StringName MY_EVENT = "my_event";  
        */

        #endregion
        
        public static StringName GetEventName(EventNameEnum transitionEvent, StringName customEventName) => transitionEvent switch
        {
            EventNameEnum.PROCESS => "_process",
            EventNameEnum.PHYSICS_PROCESS => "_physics_process",
            EventNameEnum.INPUT => "_input",
            EventNameEnum.UNHANDLED_INPUT => "_unhandled_input",

            // Extend event set here:
            /*
                EventNameEnum.MY_EVENT => "my_event",
            */
            
            EventNameEnum.CUSTOM => customEventName,
            _ => null
        };
    }
}