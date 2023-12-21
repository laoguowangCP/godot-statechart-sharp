using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;


namespace LGWCP.StatechartSharp
{
    public enum TransitionEventNameEnum : int
    {
        PROCESS,
        PHYSICS_PROCESS,
        INPUT,
        UNHANDLED_INPUT,

        // Extend transition event-set here:
        /*
            MY_EVENT,
        */

        CUSTOM,
        AUTO
    }

    public enum ActionEventNameEnum : int
    {
        PROCESS,
        PHYSICS_PROCESS,
        INPUT,
        UNHANDLED_INPUT,

        // Extend action event-set here:
        /*
            MY_EVENT,
        */

        CUSTOM
    }

    public partial class StatechartConfig : Node
    {
        
        #region Preset EventName

        public static readonly StringName EVENT_PROCESS = "_process";
        public static readonly StringName EVENT_PHYSICS_PROCESS = "_physics_process";
        public static readonly StringName EVENT_INPUT = "_input";
        public static readonly StringName EVENT_UNHANDLED_INPUT = "_unhandled_input";
        
        // Extend event-set here:
        /*
            protected static readonly StringName MY_EVENT = "my_event";  
        */

        #endregion
        
        public static StringName GetTransitionEventName(TransitionEventNameEnum transitionEvent, StringName customEventName) => transitionEvent switch
        {
            TransitionEventNameEnum.PROCESS => "_process",
            TransitionEventNameEnum.PHYSICS_PROCESS => "_physics_process",
            TransitionEventNameEnum.INPUT => "_input",
            TransitionEventNameEnum.UNHANDLED_INPUT => "_unhandled_input",

            // Extend transition event-set here:
            /*
                EventNameEnum.MY_EVENT => "my_event",
            */
            
            TransitionEventNameEnum.CUSTOM => customEventName,
            _ => null
        };

        public static StringName GetActionEventName(ActionEventNameEnum transitionEvent, StringName customEventName) => transitionEvent switch
        {
            ActionEventNameEnum.PROCESS => "_process",
            ActionEventNameEnum.PHYSICS_PROCESS => "_physics_process",
            ActionEventNameEnum.INPUT => "_input",
            ActionEventNameEnum.UNHANDLED_INPUT => "_unhandled_input",

            // Extend action event-set here:
            /*
                EventNameEnum.MY_EVENT => "my_event",
            */
            
            ActionEventNameEnum.CUSTOM => customEventName,
            _ => null
        };
    }
}