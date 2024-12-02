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
    public List<Asserter> BeforeStepAsserters;
    public List<Asserter> AfterStepAsserters;

    public abstract class Asserter
    {
        public virtual bool Assert()
        {
            return false;
        }
    }

    // Holds a snapshot, check if consistent with not-running statechart
    public class SnapshotAsserter : Asserter
    {
        protected StatechartSnapshot Snapshot;

        public SnapshotAsserter(StatechartSnapshot snapshot)
        {
            Snapshot = snapshot;
        }

        public override bool Assert()
        {
            return false;
        }
    }
}
