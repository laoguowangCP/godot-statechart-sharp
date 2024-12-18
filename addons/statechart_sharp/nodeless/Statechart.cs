using System;
using System.Collections.Generic;
using Godot;


namespace LGWCP.Godot.StatechartSharp.Nodeless;

public partial class Statechart<TDuct, TEvent>
    where TDuct : IStatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{

#region composition

    protected interface IStatechartComposition {}

    protected abstract class StatechartComposition<T> : IStatechartComposition
        where T : StatechartComposition<T>
    {
        // protected abstract void Setup();
        public int OrderId;
        protected Statechart<TDuct, TEvent> HostStatechart;

        public virtual void SetupPre() {}

        public virtual void Setup() {}

        public virtual void Setup(Statechart<TDuct, TEvent> hostStatechart, ref int orderId)
        {
            HostStatechart = hostStatechart;
            OrderId = orderId;
            ++orderId;
        }

        public virtual void SetupPost() {}

        public static bool IsCommonHost(T x, T y)
        {
            if (x == null || y == null)
            {
                return false;
            }
            return x.HostStatechart == y.HostStatechart;
        }
    }

#endregion


#region comparer

    protected class StatechartComparer<TComposition> : IComparer<TComposition>
        where TComposition : StatechartComposition<TComposition>
    {
        public int Compare(TComposition x, TComposition y)
        {
            return x.OrderId - y.OrderId;
        }
    }

    protected class StatechartReversedComparer<TComposition> : IComparer<TComposition>
        where TComposition : StatechartComposition<TComposition>
    {
        public int Compare(TComposition x, TComposition y)
        {
            return y.OrderId - x.OrderId;
        }
    }

#endregion


#region statechart

    protected int MaxInternalEventCount = 8;
    protected int MaxAutoTransitionRound = 8;
    protected bool IsRunning;
    protected int EventCount;
    protected StateInt RootState;
    protected SortedSet<StateInt> ActiveStates;
    protected Queue<TEvent> QueuedEvents;
    protected SortedSet<TransitionInt> EnabledTransitions;
    protected SortedSet<TransitionInt> EnabledFilteredTransitions;
    protected SortedSet<StateInt> ExitSet;
    protected SortedSet<StateInt> EnterSet;
    protected SortedSet<ReactionInt> EnabledReactions;
    protected List<int> SnapshotConfig;
    protected string LastStepEventName = "_";
    protected int StateLength = 0;
    protected TDuct Duct;
    protected Dictionary<TEvent, (List<TransitionInt>, List<ReactionInt>)[]> GlobalEventTAMap;
    protected (List<TransitionInt>, List<ReactionInt>)[] CurrentTAMap;

    public Statechart()
    {
        Duct = new TDuct();
        IsRunning = false;
        EventCount = 0;

        ActiveStates = new SortedSet<StateInt>(new StatechartComparer<StateInt>());
        QueuedEvents = new Queue<TEvent>();
        EnabledTransitions = new SortedSet<TransitionInt>(new StatechartComparer<TransitionInt>());
        EnabledFilteredTransitions = new SortedSet<TransitionInt>(new StatechartComparer<TransitionInt>());
        ExitSet = new SortedSet<StateInt>(new StatechartReversedComparer<StateInt>());
        EnterSet = new SortedSet<StateInt>(new StatechartComparer<StateInt>());
        EnabledReactions = new SortedSet<ReactionInt>(new StatechartComparer<ReactionInt>());
        GlobalEventTAMap = new Dictionary<TEvent, (List<TransitionInt>, List<ReactionInt>)[]>();
        SnapshotConfig = new List<int>();
    }

    public void Init()
    {
        if (RootState is null)
        {
            return;
        }

        RootState.SetupPre();

        int orderId = -1;
        RootState.Setup(this, ref orderId);

        RootState.SubmitActiveState(ActiveStates);
        RootState.SetupPost();
        foreach (var state in ActiveStates)
		{
			state.StateEnter();
		}
    }

#endregion


#region state

    protected class StateInt : StatechartComposition<StateInt>
    {
        public delegate void EnterEvent(TDuct duct);
        protected event EnterEvent Enter;
        public Action<SortedSet<StateInt>> SubmitActiveState;
        public Action<StateInt> HandleSubstateEnter;
        protected TDuct Duct;
        public StateInt ParentState;

        public StateInt() {}

        public void StateEnter()
        {
            ParentState?.HandleSubstateEnter(this);
            CustomStateEnter(Duct);
        }

        protected virtual void CustomStateEnter(TDuct duct)
        {
            // Use signal by default
            Enter.Invoke(duct);
        }

        protected void Foo(TransitionInt t)
        {
            var bar = t.Bar;
        }
        protected void Foo(Statechart<TDuct, TEvent> sc)
        {
            var globalMap = sc.CurrentTAMap;
        }
    }

    public class State
    {
        private StateInt _state;
        public State(StateModeEnum mode, bool isDeepHistory, Action<TDuct>[] enters, Action<TDuct>[] exits)
        {
            _state = new();
        }
    }

#endregion


#region transition

    protected class TransitionInt : StatechartComposition<TransitionInt>
    {
        public int Bar;
    }

    public class Transition
    {
        public Transition(TEvent @event, State[] targets=null, bool isAuto=false, Action<TDuct>[] guards=null, Action<TDuct>[] invokes = null)
        {

        }
    }

#endregion


#region reaction

    protected class ReactionInt : StatechartComposition<ReactionInt>
    {

    }

#endregion

    public class StatechartBuilder
    {

    }
}


public class TestStatechartBuild
{
    public void Build()
    {
        var sc = new Statechart<BaseStatechartDuct, string>();
    }
}

