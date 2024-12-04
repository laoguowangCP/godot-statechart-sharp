using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using Godot;


namespace LGWCP.Godot.StatechartSharp;


[GlobalClass]
[Icon("res://addons/statechart_sharp/icon/StatechartProxy.svg")]
public partial class StatechartProxy : Node
{

#region signal

    [Signal]
    public delegate void BeforeStepProcessEventHandler();
    [Signal]
    public delegate void AfterStepProcessEventHandler();
    [Signal]
    public delegate void BeforeStepPhysicsProcessEventHandler();
    [Signal]
    public delegate void AfterStepPhysicsProcessEventHandler();
    [Signal]
    public delegate void BeforeStepInputEventHandler();
    [Signal]
    public delegate void AfterStepInputEventHandler();
    [Signal]
    public delegate void BeforeStepShortcutInputEventHandler();
    [Signal]
    public delegate void AfterStepShortcutInputEventHandler();
    [Signal]
    public delegate void BeforeStepUnhandledKeyInputEventHandler();
    [Signal]
    public delegate void AfterStepUnhandledKeyInputEventHandler();
    [Signal]
    public delegate void BeforeStepUnhandledInputEventHandler();
    [Signal]
    public delegate void AfterStepUnhandledInputEventHandler();

#endregion


#region property

    [Export]
    public Statechart Statechart;
    public StatechartDuct Duct;

#endregion


#region method

    public override void _Ready()
    {
        // Disable statechart, proxy node loop events
        if (Statechart is null)
        {
            GD.PushWarning("No proxied statechart.");
            ProcessMode = ProcessModeEnum.Disabled;
            return;
        }

        if (Statechart.IsNodeReady())
        {
            GD.PushWarning("Proxied statechart is not ready, proxy wont work properly.");
            ProcessMode = ProcessModeEnum.Disabled;
            return;
        }

        Statechart.ProcessMode = ProcessModeEnum.Disabled;
        Duct = Statechart._Duct;

        var eventFlag = Statechart.EventFlag;
        SetProcess(eventFlag.HasFlag(EventFlagEnum.Process));
        SetPhysicsProcess(eventFlag.HasFlag(EventFlagEnum.PhysicsProcess));
        SetProcessInput(eventFlag.HasFlag(EventFlagEnum.Input));
        SetProcessShortcutInput(eventFlag.HasFlag(EventFlagEnum.ShortcutInput));
        SetProcessUnhandledKeyInput(eventFlag.HasFlag(EventFlagEnum.UnhandledKeyInput));
        SetProcessUnhandledInput(eventFlag.HasFlag(EventFlagEnum.UnhandledInput));
    }

    public virtual void Step(StringName eventName)
    {
        Statechart.Step(eventName);
    }

	public override void _Process(double delta)
	{
        EmitSignal(SignalName.BeforeStepProcess);
		Duct.Delta = delta;
		Step(StatechartEventName.EVENT_PROCESS);
        EmitSignal(SignalName.AfterStepProcess);
	}

	public override void _PhysicsProcess(double delta)
	{
        EmitSignal(SignalName.BeforeStepPhysicsProcess);
		Duct.PhysicsDelta = delta;
		Step(StatechartEventName.EVENT_PHYSICS_PROCESS);
        EmitSignal(SignalName.AfterStepPhysicsProcess);
	}

	public override void _Input(InputEvent @event)
	{
		using(@event)
		{
            EmitSignal(SignalName.BeforeStepInput);
			Duct.Input = @event;
			Step(StatechartEventName.EVENT_INPUT);
            EmitSignal(SignalName.AfterStepInput);
		}
	}

	public override void _ShortcutInput(InputEvent @event)
	{
		using(@event)
		{
            EmitSignal(SignalName.BeforeStepShortcutInput);
			Duct.ShortcutInput = @event;
			Step(StatechartEventName.EVENT_SHORTCUT_INPUT);
            EmitSignal(SignalName.AfterStepShortcutInput);
		}
	}

	public override void _UnhandledKeyInput(InputEvent @event)
	{
		using(@event)
		{
            EmitSignal(SignalName.BeforeStepUnhandledKeyInput);
			Duct.UnhandledKeyInput = @event;
			Statechart.Step(StatechartEventName.EVENT_UNHANDLED_KEY_INPUT);
            EmitSignal(SignalName.AfterStepUnhandledKeyInput);
		}
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		using(@event)
		{
            EmitSignal(SignalName.BeforeStepUnhandledInput);
			Duct.UnhandledInput = @event;
			Statechart.Step(StatechartEventName.EVENT_UNHANDLED_INPUT);
            EmitSignal(SignalName.AfterStepUnhandledInput);
		}
	}

#endregion

}