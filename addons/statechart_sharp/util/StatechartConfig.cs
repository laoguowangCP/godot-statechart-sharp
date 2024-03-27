using System;
using Godot;


namespace LGWCP.StatechartSharp;

[Flags]
public enum EventFlagEnum
{
    Process = 1,
    PhysicsProcess = 2,
    Input = 4,
    ShortcutInput = 8,
    UnhandledKeyInput = 16,
    UnhandledInput = 32,
}

public enum TransitionEventNameEnum : int
{
    Process,
    PhysicsProcess,
    Input,
    ShortcutInput,
    UnhandledKeyInput,
    UnhandledInput,
    Custom,
    Auto

    // Extend transition event here:
    /*
        MY_EVENT,
    */
}

public enum ReactionEventNameEnum : int
{
    Process,
    PhysicsProcess,
    Input,
    ShortcutInput,
    UnhandledKeyInput,
    UnhandledInput,
    Custom

    // Extend action event here:
    /*
        MY_EVENT,
    */

}

public partial class StatechartConfig : Node
{
    
    #region Preset EventName

    public static readonly StringName EVENT_PROCESS = "_process";
    public static readonly StringName EVENT_PHYSICS_PROCESS = "_physics_process";
    public static readonly StringName EVENT_INPUT = "_input";
    public static readonly StringName EVENT_SHORTCUT_INPUT = "_shortcut_input";
    public static readonly StringName EVENT_UNHANDLED_KEY_INPUT = "_unhandled_key_input";
    public static readonly StringName EVENT_UNHANDLED_INPUT = "_unhandled_input";
    
    // Extend event here:
    /*
        protected static readonly StringName MY_EVENT = "my_event";  
    */

    #endregion
    
    public static StringName GetTransitionEventName(TransitionEventNameEnum transitionEvent, StringName customEventName) => transitionEvent switch
    {
        TransitionEventNameEnum.Process => "_process",
        TransitionEventNameEnum.PhysicsProcess => "_physics_process",
        TransitionEventNameEnum.Input => "_input",
        TransitionEventNameEnum.ShortcutInput => "_shortcut_input",
        TransitionEventNameEnum.UnhandledKeyInput => "_unhandled_key_input",
        TransitionEventNameEnum.UnhandledInput => "_unhandled_input",
        TransitionEventNameEnum.Custom => customEventName,

        // Extend transition event here:
        /*
            EventNameEnum.MY_EVENT => "my_event",
        */
        
        _ => null
    };

    public static StringName GetReactionEventName(ReactionEventNameEnum transitionEvent, StringName customEventName) => transitionEvent switch
    {
        ReactionEventNameEnum.Process => "_process",
        ReactionEventNameEnum.PhysicsProcess => "_physics_process",
        ReactionEventNameEnum.Input => "_input",
        ReactionEventNameEnum.ShortcutInput => "_shortcut_input",
        ReactionEventNameEnum.UnhandledKeyInput => "_unhandled_key_input",
        ReactionEventNameEnum.UnhandledInput => "_unhandled_input",
        ReactionEventNameEnum.Custom => customEventName,

        // Extend reaction event here:
        /*
            EventNameEnum.MY_EVENT => "my_event",
        */
        
        _ => null
    };
}
