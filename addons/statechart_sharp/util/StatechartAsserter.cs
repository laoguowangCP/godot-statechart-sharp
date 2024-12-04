using System.Collections.Generic;

namespace LGWCP.Godot.StatechartSharp;


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

    public class ActiveStatesAsserter : StatechartAsserter
    {
        protected List<State> States;

        public ActiveStatesAsserter(Statechart statechart, State[] states) : base(statechart)
        {
            States = new List<State>(states);
        }

        public override bool Assert()
        {
            if (Statechart.IsRunning)
            {
                return false;
            }

            foreach (var state in States)
            {
                if (!Statechart.ActiveStates.Contains(state))
                {
                    return false;
                }
            }

            return true;
        }
    }

    public class AllAsserter : StatechartAsserter
    {
        protected List<StatechartAsserter> Asserters;

        public AllAsserter(Statechart statechart) : base(statechart)
        {
            Asserters = new List<StatechartAsserter>();
        }

        public AllAsserter(Statechart statechart, StatechartAsserter[] asserters) : base(statechart)
        {
            Asserters = new List<StatechartAsserter>(asserters);
        }

        public void Add(StatechartAsserter asserter)
        {
            Asserters.Add(asserter);
        }

        public void Clear()
        {
            Asserters.Clear();
        }

        public override bool Assert()
        {
            // Make sure all asserters checked
            bool isSuccess = true;
            foreach (var asserter in Asserters)
            {
                if (!asserter.Assert())
                {
                    isSuccess = false;
                }
            }

            return isSuccess;
        }
    }

    public class AnyAsserter : StatechartAsserter
    {
        protected List<StatechartAsserter> Asserters;

        public AnyAsserter(Statechart statechart) : base(statechart)
        {
            Asserters = new List<StatechartAsserter>();
        }

        public AnyAsserter(Statechart statechart, StatechartAsserter[] asserters) : base(statechart)
        {
            Asserters = new List<StatechartAsserter>(asserters);
        }

        public void Add(StatechartAsserter asserter)
        {
            Asserters.Add(asserter);
        }

        public void Clear()
        {
            Asserters.Clear();
        }

        public override bool Assert()
        {
            // Make sure all asserters checked
            bool isSuccess = false;
            foreach (var asserter in Asserters)
            {
                if (asserter.Assert())
                {
                    isSuccess = true;
                }
            }

            return isSuccess;
        }
    }
}

#endregion
