using Godot;


namespace LGWCP.Godot.StatechartSharp;

[Tool]
[GlobalClass]
public partial class StatechartDuct : RefCounted
{
    public Statechart HostStatechart { private get; set; } = null;
    public double Delta { get; set; } = 0.0;
    public double PhysicsDelta { get; set; } = 0.0;
    public InputEvent Input { get; set; }
    public InputEvent ShortcutInput { get; set; }
    public InputEvent UnhandledKeyInput { get; set; }
    public InputEvent UnhandledInput { get; set; }
    public bool IsTransitionEnabled { get; set; } = false;
    public Node CompositionNode { get; set; }
    public bool IsRunning { get => HostStatechart is not null && HostStatechart.IsRunning; }
}
