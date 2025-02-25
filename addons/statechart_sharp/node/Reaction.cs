using Godot;
using System.Collections.Generic;


namespace LGWCP.Godot.StatechartSharp;

[Tool]
[GlobalClass]
[Icon("res://addons/statechart_sharp/icon/Reaction.svg")]
public partial class Reaction : StatechartComposition
{
#region signal

    [Signal] public delegate void InvokeEventHandler(StatechartDuct duct);

#endregion


#region property

    [Export]
    public ReactionEventNameEnum ReactionEvent
#if DEBUG
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
    private ReactionEventNameEnum _reactionEvent
#endif
        = ReactionEventNameEnum.Process;

    [Export]
    public StringName CustomEventName
#if DEBUG
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
    private StringName _customEventName
#endif
        ;

    public StringName _EventName;
    protected StatechartDuct Duct;

#endregion


#region method

    public override void _Ready()
    {
#if TOOLS
        if (Engine.IsEditorHint())
        {
            UpdateConfigurationWarnings();
        }
#endif
    }

    public override void _Setup(Statechart hostStatechart, ref int parentOrderId)
    {
        base._Setup(hostStatechart, ref parentOrderId);
        Duct = _HostStatechart._Duct;

#if DEBUG
        if (ReactionEvent == ReactionEventNameEnum.Custom && CustomEventName == null)
        {
            GD.PushError(GetPath(), ": no event name for custom-event.");
        }
#endif
        _EventName = StatechartEventName.GetReactionEventName(ReactionEvent, CustomEventName);
    }

    public void _ReactionInvoke()
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
            isParentWarning = !state._IsValidState();
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
