

using System;
using System.Collections.Generic;

namespace LGWCP.Godot.StatechartSharp.Nodeless;


public interface IStatechartComposition {}

public abstract class StatechartComposition<T> : IStatechartComposition
    where T : StatechartComposition<T>
{
    // protected abstract void Setup();
    protected int OrderId;
    protected Statechart HostStatechart;

    protected virtual void SetupPre() {} // Handle stuffs in ready

    protected virtual void Setup() {}
    
    protected virtual void Setup(Statechart hostStatechart, ref int orderId)
    {
        HostStatechart = hostStatechart;
        OrderId = orderId;
        ++orderId;
    }

    protected virtual void SetupPost() {}

    protected static bool IsCommonHost(T x, T y)
    {
        if (x == null || y == null)
        {
            return false;
        }
        return x.HostStatechart == y.HostStatechart;
    }

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
}

public partial class Statechart<TDuct, TEvent> : StatechartComposition<Statechart<TDuct, TEvent>>
    where TDuct : IStatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{
#region property

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

#endregion

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

    public void Ready()
    {
        Setup();
		SetupPost();
    }

#region method
#endregion

#region state

    protected class StateInt : StatechartComposition<StateInt>
    {
        #region delegate
        public delegate void Enter(TDuct duct);
        #endregion

        #region property
        #endregion


        protected TDuct Duct;

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



