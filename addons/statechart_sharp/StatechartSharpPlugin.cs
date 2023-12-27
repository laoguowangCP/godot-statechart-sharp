#if TOOLS
using Godot;
using LGWCP.StatechartSharp;

[Tool]
public partial class StatechartSharpPlugin : EditorPlugin
{
	private StatechartInspectorPlugin StateInspectorPlugin;
	
	public override void _EnterTree()
	{
		// StateInspectorPlugin = new StatechartInspectorPlugin();
		// AddInspectorPlugin(StateInspectorPlugin);
	}

	public override void _ExitTree()
	{
		// RemoveInspectorPlugin(StateInspectorPlugin);
	}
}
#endif
