using System.Collections.Generic;
using Godot;

namespace LGWCP.Godot.StatechartSharp;

public partial class StatechartMonitor
{
    protected Statechart Statechart;
    public StatechartMonitor(Statechart statechart)
    {
        Statechart = statechart;
    }

    public StringName EventName;

}


#region Asserter

public partial class Statechart
{

public abstract class StatechartAsserter
{
    protected Statechart Statechart;
    public StatechartAsserter(Statechart statechart)
    {
        Statechart = statechart;
    }
    public virtual bool Assert()
    {
        return false;
    }
}

// Holds a snapshot, check if consistent with not-running statechart
public class SnapshotAsserter : StatechartAsserter
{
    protected StatechartSnapshot Snapshot;

    public SnapshotAsserter(Statechart statechart, StatechartSnapshot snapshot) : base(statechart)
    {
        Snapshot = snapshot;
    }

    public override bool Assert()
    {
        var currSnapshot = Statechart.Save(Snapshot.IsAllStateConfig);
        return currSnapshot.Equals(Snapshot);
    }
}

public class HasStateAsserter : StatechartAsserter
{
    protected State State;

    public HasStateAsserter(Statechart statechart, State state) : base(statechart)
    {
        State = state;
    }

    public override bool Assert()
    {
        return Statechart.IsRunning && Statechart.ActiveStates.Contains(State);
    }
}

}

#endregion
