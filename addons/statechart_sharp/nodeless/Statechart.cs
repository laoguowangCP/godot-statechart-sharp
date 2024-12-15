

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
    protected State RootState;
    protected SortedSet<State> ActiveStates;
    protected Queue<string> QueuedEvents;
    protected SortedSet<Transition> EnabledTransitions;
    protected SortedSet<Transition> EnabledFilteredTransitions;
    protected SortedSet<State> ExitSet;
    protected SortedSet<State> EnterSet;
    protected SortedSet<Reaction> EnabledReactions;
    protected List<int> SnapshotConfig;
    protected string LastStepEventName = "_";
    protected int StateLength = 0;
    public TDuct Duct { get; protected set; }

#endregion

    public Statechart()
    {
        Duct = new TDuct();
    }

#region method
#endregion

#region state

    public class State : StatechartComposition<State>
    {
        #region delegate
        public delegate void Enter(TDuct duct);
        #endregion

        protected TDuct Duct;
        public void Foo(BaseStatechartDuct duct)
        {
            duct.SetTransitionEnabled();
        }
    }

#endregion

#region transition

    public class Transition : StatechartComposition<Transition>
    {
        
    }

#endregion

#region reaction
    public class Reaction : StatechartComposition<Reaction>
    {
        
    }
#endregion

    public class StatechartBuilder
    {
        
    }
}

