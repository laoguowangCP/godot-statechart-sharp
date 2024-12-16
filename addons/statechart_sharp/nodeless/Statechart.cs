

using System;
using System.Collections.Generic;

namespace LGWCP.Godot.StatechartSharp.Nodeless;


public interface IStatechartComposition {}

public abstract class StatechartComposition<T> : IStatechartComposition
    where T : StatechartComposition<T>
{
    // protected abstract void Setup();
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
    protected StateInternal RootState;
    protected SortedSet<StateInternal> ActiveStates;
    protected Queue<string> QueuedEvents;
    protected SortedSet<TransitionInternal> EnabledTransitions;
    protected SortedSet<TransitionInternal> EnabledFilteredTransitions;
    protected SortedSet<StateInternal> ExitSet;
    protected SortedSet<StateInternal> EnterSet;
    protected SortedSet<ReactionInternal> EnabledReactions;
    protected List<int> SnapshotConfig;
    protected string LastStepEventName = "_";
    protected int StateLength = 0;
    protected TDuct Duct { get; set; }

#endregion

    public Statechart()
    {
        Duct = new TDuct();
    }

#region method
#endregion

#region state

    protected class StateInternal : StatechartComposition<StateInternal>
    {
        #region delegate
        public delegate void Enter(TDuct duct);
        #endregion

        #region property
        #endregion


        protected TDuct Duct;
        protected void Foo(TransitionInternal t)
        {
            var bar = t.Bar;
        }
    }

    public class State
    {
        private StateInternal _state;
        public State(StateModeEnum mode, bool isDeepHistory)
        {
            _state = new();
        }
    }

#endregion

#region transition

    protected class TransitionInternal : StatechartComposition<TransitionInternal>
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
    protected class ReactionInternal : StatechartComposition<ReactionInternal>
    {

    }
#endregion

    public class StatechartBuilder
    {

    }
}
