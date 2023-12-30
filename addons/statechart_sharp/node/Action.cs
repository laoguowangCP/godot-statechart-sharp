using Godot;

namespace LGWCP.StatechartSharp
{

[GlobalClass, Icon("res://addons/statechart_sharp/icon/Action.svg")]
public partial class Action : StatechartComposition
{
    #region define signals

    [Signal] public delegate void InvokeEventHandler(Action action);
    
    #endregion

    #region define properties

    [Export] protected TransitionEventNameEnum ActionEvent { get; set; } = TransitionEventNameEnum.PROCESS;
    [Export] protected StringName CustomEventName { get; set; }
    
    public StringName EventName { get; protected set; }

    #endregion

    internal override void Init(Statechart hostStatechart, ref int ancestorId)
    {
        base.Init(hostStatechart, ref ancestorId);
        #if DEBUG
        if (ActionEvent == TransitionEventNameEnum.CUSTOM && CustomEventName == null)
        {
            GD.PushError(Name, ": no event name for custom-event.");
        }
        #endif
        EventName = StatechartConfig.GetTransitionEventName(ActionEvent, CustomEventName);
    }

    internal void ActionInvoke(StringName eventName)
    {
        if (EventName == eventName)
        {
            EmitSignal(SignalName.Invoke, this);
        }
    }

}

} // Namespace