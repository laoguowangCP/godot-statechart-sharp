using Godot;


namespace LGWCP.StatechartSharp
{

public partial class StatechartDuct : GodotObject
{
    public double Delta { get; internal set; }
    public double PhysicsDelta { get; internal set; }
    public InputEvent Input { get; internal set; }
    public InputEvent ShortcutInput { get; internal set; }
    public InputEvent UnhandledKeyInput { get; internal set; }
    public InputEvent UnhandledInput { get; internal set; }
    public bool IsEnabled { internal get; set; }
    public StatechartComposition CompositionNode { get; internal set; }
}

} // end of namespace