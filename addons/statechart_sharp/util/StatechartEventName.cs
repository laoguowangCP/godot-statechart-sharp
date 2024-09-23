using Godot;
using System;


namespace LGWCP.Godot.StatechartSharp;

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

    // Extend transition event here:
    /*
        MyEvent,
    */

    Custom,
    Auto
}

public enum ReactionEventNameEnum : int
{
    Process,
    PhysicsProcess,
    Input,
    ShortcutInput,
    UnhandledKeyInput,
    UnhandledInput,

    // Extend action event here:
    /*
        MyEvent,
    */

    Custom
}


public partial class StatechartEventName
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
        protected static readonly StringName MY_EVENT = "_my_event";  
    */

    #endregion
    
    public static StringName GetTransitionEventName(TransitionEventNameEnum transitionEvent, StringName customEventName) => transitionEvent switch
    {
        TransitionEventNameEnum.Process => EVENT_PROCESS,
        TransitionEventNameEnum.PhysicsProcess => EVENT_PHYSICS_PROCESS,
        TransitionEventNameEnum.Input => EVENT_INPUT,
        TransitionEventNameEnum.ShortcutInput => EVENT_SHORTCUT_INPUT,
        TransitionEventNameEnum.UnhandledKeyInput => EVENT_UNHANDLED_KEY_INPUT,
        TransitionEventNameEnum.UnhandledInput => EVENT_UNHANDLED_INPUT,

        // Extend transition event here:
        /*
            TransitionEventNameEnum.MyEvent => MY_EVENT,
        */

        TransitionEventNameEnum.Custom => customEventName,
        _ => null
    };

    public static StringName GetReactionEventName(ReactionEventNameEnum transitionEvent, StringName customEventName) => transitionEvent switch
    {
        ReactionEventNameEnum.Process => EVENT_PROCESS,
        ReactionEventNameEnum.PhysicsProcess => EVENT_PHYSICS_PROCESS,
        ReactionEventNameEnum.Input => EVENT_INPUT,
        ReactionEventNameEnum.ShortcutInput => EVENT_SHORTCUT_INPUT,
        ReactionEventNameEnum.UnhandledKeyInput => EVENT_UNHANDLED_KEY_INPUT,
        ReactionEventNameEnum.UnhandledInput => EVENT_UNHANDLED_INPUT,

        // Extend reaction event here:
        /*
            ReactionEventNameEnum.MyEvent => MY_EVENT,
        */
        
        ReactionEventNameEnum.Custom => customEventName,
        _ => null
    };
}
