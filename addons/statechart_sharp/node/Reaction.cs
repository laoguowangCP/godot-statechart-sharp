using System.Collections.Generic;
using Godot;


namespace LGWCP.StatechartSharp
{

[Tool]
[GlobalClass, Icon("res://addons/statechart_sharp/icon/Reaction.svg")]
public partial class Reaction : StatechartComposition
{
    #region define signals

    [Signal] public delegate void InvokeEventHandler(Reaction reaction);
    
    #endregion

    #region define properties

    [Export] protected ReactionEventNameEnum ReactionEvent
    {
        get => _reactionEvent;
        set
        {
            _reactionEvent = value;
            #if TOOLS
            UpdateConfigurationWarnings();
            #endif
        }
    }
    private ReactionEventNameEnum _reactionEvent = ReactionEventNameEnum.Process;

    [Export] protected StringName CustomEventName
    {
        get => _customEventName;
        set
        {
            _customEventName = value;
            #if TOOLS
            UpdateConfigurationWarnings();
            #endif
        }
    }
    private StringName _customEventName;
    public StringName EventName { get; protected set; }

    #endregion

    internal override void Setup(Statechart hostStatechart, ref int ancestorId)
    {
        base.Setup(hostStatechart, ref ancestorId);
        #if DEBUG
        if (ReactionEvent == ReactionEventNameEnum.CUSTOM && CustomEventName == null)
        {
            GD.PushError(Name, ": no event name for custom-event.");
        }
        #endif
        EventName = StatechartConfig.GetReactionEventName(ReactionEvent, CustomEventName);
    }

    internal void ReactionInvoke(StringName eventName)
    {
        if (EventName == eventName)
        {
            EmitSignal(SignalName.Invoke, this);
        }
    }

    #if TOOLS
    public override string[] _GetConfigurationWarnings()
    {
        var warnings = new List<string>();

        if (ReactionEvent == ReactionEventNameEnum.CUSTOM
            && (CustomEventName == null || CustomEventName == ""))
        {
            warnings.Add("No event name for custom event.");
        }

        return warnings.ToArray();
    }
    #endif
}

} // end of namespace