using Godot;

namespace LGWCP.StatechartSharp
{

[GlobalClass, Icon("res://addons/statechart_sharp/icon/Reaction.svg")]
public partial class Reaction : StatechartComposition
{
    #region define signals

    [Signal] public delegate void InvokeEventHandler(Reaction reaction);
    
    #endregion

    #region define properties

    [Export] protected ReactionEventNameEnum ReactionEvent { get; set; } = ReactionEventNameEnum.PROCESS;
    [Export] protected StringName CustomEventName { get; set; }
    
    public StringName EventName { get; protected set; }

    #endregion

    internal override void Init(Statechart hostStatechart, ref int ancestorId)
    {
        base.Init(hostStatechart, ref ancestorId);
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

}

} // Namespace