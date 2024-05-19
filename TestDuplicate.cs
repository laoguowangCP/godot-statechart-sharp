using Godot;
using LGWCP.StatechartSharp;
using System;

public partial class TestDuplicate : Node
{
    public override void _Ready()
    {
        var statechart = GetNodeOrNull<Statechart>("Statechart");
        statechart.Duplicate();
    }
}
