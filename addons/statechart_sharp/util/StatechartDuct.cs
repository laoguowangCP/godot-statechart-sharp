using Godot;


namespace LGWCP.StatechartSharp;

[Tool]
public partial class StatechartDuct : GodotObject
{
    internal Statechart HostStatechart { private get; set; } = null;
    public double Delta { get; internal set; } = 0.0;
    public double PhysicsDelta { get; internal set; } = 0.0;
    public InputEvent Input { get; internal set; }
    public InputEvent ShortcutInput { get; internal set; }
    public InputEvent UnhandledKeyInput { get; internal set; }
    public InputEvent UnhandledInput { get; internal set; }
    public bool IsTransitionEnabled { internal get; set; } = false;
    public StatechartComposition CompositionNode { get; internal set; }
    public bool IsRunning { get => HostStatechart is not null && HostStatechart.IsRunning; }
}
