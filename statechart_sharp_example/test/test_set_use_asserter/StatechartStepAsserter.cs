using Godot;
using Godot.Collections;
using LGWCP.Godot.StatechartSharp;


// Assert snapshot before each step
public partial class StatechartStepAsserter : Node
{
    [Export]
    protected StatechartProxy Statechart;
    [Export]
    protected Array<StatechartSnapshot> Snapshots;
    [Export]
    protected Array<State> Step00Snapshot;
    [Export]
    protected Array<State> Step01Snapshot;
    [Export]
    protected Array<State> Step02Snapshot;
    [Export]
    protected Array<State> Step03Snapshot;
    [Export]
    protected Array<State> Step04Snapshot;
    [Export]
    protected Array<State> Step05Snapshot;
    [Export]
    protected Array<State> Step06Snapshot;
    [Export]
    protected Array<State> Step07Snapshot;
    [Export]
    protected Array<State> Step08Snapshot;
}

