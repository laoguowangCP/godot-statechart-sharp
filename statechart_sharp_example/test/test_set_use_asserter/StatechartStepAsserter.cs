using System.Collections.Generic;
using Godot;
using Godot.Collections;
using LGWCP.Godot.StatechartSharp;


// Assert snapshot before each step
public partial class StatechartStepAsserter : Node
{
    [Export]
    protected StatechartProxy Statechart;
    [Export]
    protected Array<StatechartSnapshot> SnapshotBeforeSteps;
    protected List<StatechartSnapshot> Snapshots;

    public override void _Ready()
    {
        Snapshots = new List<StatechartSnapshot>(SnapshotBeforeSteps);
    }
}

