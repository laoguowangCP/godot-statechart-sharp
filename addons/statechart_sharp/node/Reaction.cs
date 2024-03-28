using Godot;
using System.Collections.Generic;


namespace LGWCP.StatechartSharp;

[Tool]
[GlobalClass, Icon("res://addons/statechart_sharp/icon/Reaction.svg")]
public partial class Reaction : StatechartComposition
{
    #region define signals

    [Signal] public delegate void InvokeEventHandler(StatechartDuct duct);
    
    #endregion

    #region define properties

    [Export] protected ReactionEventNameEnum ReactionEvent
    {
        get => _reactionEvent;
        set
        {
            _reactionEvent = value;
            #if TOOLS
            if (Engine.IsEditorHint())
            {
                UpdateConfigurationWarnings();
            }
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
            if (Engine.IsEditorHint())
            {
                UpdateConfigurationWarnings();
            }
            #endif
        }
    }
    private StringName _customEventName;
    public StringName EventName { get; protected set; }
    protected StatechartDuct Duct { get => HostStatechart.Duct; }

    #endregion

    public override void _Ready()
    {
        #if TOOLS
        if (Engine.IsEditorHint())
        {
            UpdateConfigurationWarnings();
        }
        #endif
    }

    internal override void Setup(Statechart hostStatechart, ref int ancestorId)
    {
        base.Setup(hostStatechart, ref ancestorId);
        #if DEBUG
        if (ReactionEvent == ReactionEventNameEnum.Custom && CustomEventName == null)
        {
            GD.PushError(Name, ": no event name for custom-event.");
        }
        #endif
        EventName = StatechartConfig.GetReactionEventName(ReactionEvent, CustomEventName);
    }

    internal bool Check(StringName eventName)
    {
        return EventName == eventName;
    }

    internal void ReactionInvoke()
    {
        // Duct.CompositionNode = this;
        // EmitSignal(SignalName.Invoke, Duct);
        CustomReactionInvoke(Duct);
    }

    protected virtual void CustomReactionInvoke(StatechartDuct duct)
    {
        // Use signal by default
        duct.CompositionNode = this;
        EmitSignal(SignalName.Invoke, duct);
    }

    #if TOOLS
    public override string[] _GetConfigurationWarnings()
    {
        var warnings = new List<string>();

        // Check parent
        bool isParentWarning = true;
        Node parent = GetParent<Node>();

        if (parent != null && parent is State state)
        {
            isParentWarning = state.IsHistory;
        }

        if (isParentWarning)
        {
            warnings.Add("Reaction should be child to non-history state.");
        }

        // Check children
        if (GetChildren().Count > 0)
        {
            warnings.Add("Reaction should not have child.");
        }

        // Check event
        if (ReactionEvent == ReactionEventNameEnum.Custom
            && (CustomEventName == null || CustomEventName == ""))
        {
            warnings.Add("No event name for custom event.");
        }

        return warnings.ToArray();
    }
    #endif
}
