using Godot;
using System.Collections.Generic;


namespace LGWCP.Godot.StatechartSharp;

[Tool]
[GlobalClass]
[Icon("res://addons/statechart_sharp/icon/Reaction.svg")]
public partial class Reaction : StatechartComposition
{
    #region signals

    [Signal] public delegate void InvokeEventHandler(StatechartDuct duct);
    
    #endregion


    #region properties

    [Export]
    public ReactionEventNameEnum ReactionEvent
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
    [Export]
    public StringName CustomEventName
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


    #region methods

    public override void _Ready()
    {
        #if TOOLS
        if (Engine.IsEditorHint())
        {
            UpdateConfigurationWarnings();
        }
        #endif
    }

    public override void Setup(Statechart hostStatechart, ref int parentOrderId)
    {
        base.Setup(hostStatechart, ref parentOrderId);

        #if DEBUG
        if (ReactionEvent == ReactionEventNameEnum.Custom && CustomEventName == null)
        {
            GD.PushError(GetPath(), ": no event name for custom-event.");
        }
        #endif

        EventName = StatechartEventName.GetReactionEventName(ReactionEvent, CustomEventName);
    }

    public bool Check(StringName eventName)
    {
        return EventName == eventName;
    }

    public void ReactionInvoke()
    {
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
        Node parent = GetParentOrNull<Node>();

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

    #endregion
}
