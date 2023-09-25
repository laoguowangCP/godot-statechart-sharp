#if TOOLS
using Godot;
using System;

[Tool]
public partial class StateChartSharpPlugin : EditorPlugin
{
	
	public override void _EnterTree()
	{
		/*
		AddCustomType("StateChartHandler", "Node",
			GD.Load<Script>("res://addons/state_chart_sharp/node/StateChartHandler.cs"),
			null);
		AddCustomType("CompondState", "Node",
			GD.Load<Script>("res://addons/state_chart_sharp/node/CompondState.cs"),
			null);
		AddCustomType("ParallelState", "Node",
			GD.Load<Script>("res://addons/state_chart_sharp/node/ParallelState.cs"),
			null);
		AddCustomType("AtomicState", "Node",
			GD.Load<Script>("res://addons/state_chart_sharp/node/AtomicState.cs"),
			null);
		AddCustomType("Transition", "Node",
			GD.Load<Script>("res://addons/state_chart_sharp/node/Transition.cs"),
			null);
		*/
	}

	public override void _ExitTree()
	{
		/*
		RemoveCustomType("StateChartHandler");
		RemoveCustomType("CompondState");
		RemoveCustomType("ParallelState");
		RemoveCustomType("AtomicState");
		RemoveCustomType("Transition");
		*/
	}
}
#endif
