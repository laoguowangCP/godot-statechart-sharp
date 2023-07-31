#if TOOLS
using Godot;
using System;

[Tool]
public partial class StateChartSharpPlugin : EditorPlugin
{
	public override void _EnterTree()
	{
		AddCustomType("StateChartHandler", "Node",
			GD.Load<Script>("res://addons/state_chart_sharp/node/StateChartHandler.cs"),
			null);
		AddCustomType("CompondStateNode", "Node",
			GD.Load<Script>("res://addons/state_chart_sharp/node/CompondStateNode.cs"),
			null);
		AddCustomType("ParallelStateNode", "Node",
			GD.Load<Script>("res://addons/state_chart_sharp/node/ParallelStateNode.cs"),
			null);
		AddCustomType("AtomicStateNode", "Node",
			GD.Load<Script>("res://addons/state_chart_sharp/node/AtomicStateNode.cs"),
			null);
		
	}

	public override void _ExitTree()
	{
		RemoveCustomType("StateChartHandler");
		RemoveCustomType("CompondStateNode");
		RemoveCustomType("ParallelStateNode");
		RemoveCustomType("AtomicStateNode");
	}
}
#endif
