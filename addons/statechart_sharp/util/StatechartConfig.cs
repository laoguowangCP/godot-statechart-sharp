using System;
using Godot;


namespace LGWCP.StatechartSharp
{

[Flags]
public enum EventMaskEnum
{
    Process = 1,
    Physics_Process = 2,
    Input = 4,
    Unhandled_Input = 8,
}

public enum TransitionEventNameEnum : int
{
    Process,
    Physics_Process,
    Input,
    Unhandled_Input,

    // Extend transition event-set here:
    /*
        MY_EVENT,
    */

    Custom,
    Auto
}

public enum ReactionEventNameEnum : int
{
    Process,
    Physics_Process,
    Input,
    Unhandled_Input,

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
        TransitionEventNameEnum.Process => "_process",
        TransitionEventNameEnum.Physics_Process => "_physics_process",
        TransitionEventNameEnum.Input => "_input",
        TransitionEventNameEnum.Unhandled_Input => "_unhandled_input",

        // Extend transition event-set here:
        /*
            EventNameEnum.MY_EVENT => "my_event",
        */
        
        TransitionEventNameEnum.Custom => customEventName,
        _ => null
    };

    public static StringName GetReactionEventName(ReactionEventNameEnum transitionEvent, StringName customEventName) => transitionEvent switch
    {
        ReactionEventNameEnum.Process => "_process",
        ReactionEventNameEnum.Physics_Process => "_physics_process",
        ReactionEventNameEnum.Input => "_input",
        ReactionEventNameEnum.Unhandled_Input => "_unhandled_input",

        // Extend action event-set here:
        /*
            EventNameEnum.MY_EVENT => "my_event",
        */
        
        ReactionEventNameEnum.CUSTOM => customEventName,
        _ => null
    };
}

} // end of namespace