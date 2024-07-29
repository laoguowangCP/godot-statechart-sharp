using Godot;
using LGWCP.StatechartSharp;

public partial class StatechartBuildTest : Node
{
    public override void _Ready()
    {
        Statechart statechart = new();
        State root = new() { Name = "Root" };
        Transition t1 = new() { TargetStatesArray = { root } };
        statechart.Append(root.Append(t1));
    }
}