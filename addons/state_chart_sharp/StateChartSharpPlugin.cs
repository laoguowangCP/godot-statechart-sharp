#if TOOLS
using Godot;
using System;

[Tool]
public partial class StateChartSharpPlugin : EditorPlugin
{
	public override void _EnterTree()
	{
		// Initialization of the plugin goes here.
		AddCustomType("StateChartHandler", "Node",
			GD.Load<Script>("res://addons/state_chart_sharp/node/StateChartHandler.cs"),
			null);
		AddCustomType("CompondStateNode", "Node",
			GD.Load<Script>("res://addons/state_chart_sharp/node/CompondStateNode.cs"),
			null);
		/*
		AddCustomType("StateNode", "Node",
			GD.Load<Script>("res://addons/state_chart_sharp/node/StateNode.cs"),
			null);
		AddCustomType("ParallelStateNode", "StateNode",
			GD.Load<Script>("res://addons/state_chart_sharp/node/ParallelStateNode.cs"),
			null);
		*/
		
	}

	public override void _ExitTree()
	{
		// Clean-up of the plugin goes here.
		RemoveCustomType("StateChartHandler");
		RemoveCustomType("CompondStateNode");
		// RemoveCustomType("StateNode");
		// RemoveCustomType("ParallelStateNode");
	}
}
#endif
