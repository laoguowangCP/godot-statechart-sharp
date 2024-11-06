using Godot;


namespace LGWCP.Godot.StatechartSharp;

[Tool]
[GlobalClass]
public partial class StatechartDuct : RefCounted
{
    public StatechartDuct(Statechart statechart)
    {
        HostStatechart = statechart;
    }
    protected Statechart HostStatechart;
    public double Delta = 0.0;
    public double PhysicsDelta = 0.0;
    public InputEvent Input;
    public InputEvent ShortcutInput;
    public InputEvent UnhandledKeyInput;
    public InputEvent UnhandledInput;
    public bool IsTransitionEnabled = false;
    public Node CompositionNode;
}
