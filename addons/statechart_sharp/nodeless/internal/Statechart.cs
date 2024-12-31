using System;
using System.Collections.Generic;
using System.Linq;


namespace LGWCP.Godot.StatechartSharp.Nodeless.Internal;

public partial class Statechart<TDuct, TEvent>
    where TDuct : IStatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{
    protected int MaxInternalEventCount = 8;
    protected int MaxAutoTransitionRound = 8;
    protected bool IsRunning;
    protected int EventCount;
    public StateInt RootState;
    protected SortedSet<StateInt> ActiveStates;
    protected Queue<TEvent> QueuedEvents;
    protected SortedSet<TransitionInt> EnabledTransitions;
    protected SortedSet<TransitionInt> EnabledFilteredTransitions;
    protected SortedSet<StateInt> ExitSet;
    protected SortedSet<StateInt> EnterSet;
    protected SortedSet<ReactionInt> EnabledReactions;
    protected List<int> SnapshotConfig;
    protected TDuct Duct;

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
        SnapshotConfig = new List<int>();
    }

    public void Init()
    {
        if (RootState is null)
        {
            return;
        }

        RootState.Setup();
        RootState.SubmitActiveState(ActiveStates);
        RootState.SetupPost();
        foreach (var state in ActiveStates)
        {
            state.StateEnter(Duct);
        }
    }

    public void Step(TEvent @event)
    {
        if (@event == null) // Empty StringName is not constructed
        {
            return;
        }

        if (IsRunning)
        {
            if (EventCount <= MaxInternalEventCount)
            {
                ++EventCount;
                QueuedEvents.Enqueue(@event);
            }
            return;
        }

        // Else is not running
        ++EventCount;
        QueuedEvents.Enqueue(@event);

        IsRunning = true;

        while (QueuedEvents.Count > 0)
        {
            TEvent nextEvent = QueuedEvents.Dequeue();
            HandleEvent(nextEvent);
        }

        IsRunning = false;
        EventCount = 0;
    }

    protected void HandleEvent(TEvent @event)
    {
        /*
        Handle event:
        1. Select transitions
        2. Do transitions
        3. While iter < MAX_AUTO_ROUND:
            - Select auto-transition
            - Do auto-transition
            - Break if no queued auto-transition
        4. Do reactions
        */

        // 1. Select transitions
        RootState.SelectTransitions(EnabledTransitions, @event);

        // 2. Do transitions
        DoTransitions();

        // 3. Select and do automatic transitions
        for (int i = 1; i <= MaxAutoTransitionRound; ++i)
        {
            RootState.SelectTransitions(EnabledTransitions, @event);

            // Stop if active states are stable
            if (EnabledTransitions.Count == 0)
            {
                break;
            }
            DoTransitions();
        }

        // 4. Select and do reactions
        foreach (var state in ActiveStates)
        {
            state.SelectReactions(EnabledReactions, @event);
        }

        foreach (var react in EnabledReactions)
        {
            react.ReactionInvoke(Duct);
        }

        EnabledReactions.Clear();
    }

    protected void DoTransitions()
    {
        /*
        Execute transitions:
            1. Process exit set (with filter)
            2. Invoke transitions
            3. Process enter set
        */

        // 1. Deduce and merge exit set
        foreach (var t in EnabledTransitions)
        {
            /*
            Check confliction
            1. If this source is descendant to other LCA.
                <=> source is in exit set
            2. Else, if other source descendant to this LCA
                <=> Any other source's most anscetor state in set is also descendant to this LCA (or it will be case 1)
                <=> Any state in exit set is descendant to this LCA
            */

            // Targetless has no confliction
            if (t.IsTargetless)
            {
                EnabledFilteredTransitions.Add(t);
                continue;
            }

            bool hasConfliction = ExitSet.Contains(t.SourceState)
                || ExitSet.Any(
                    state => t.LcaState.IsAncestorStateOf(state));

            if (hasConfliction)
            {
                continue;
            }

            EnabledFilteredTransitions.Add(t);

            var exitStates = ActiveStates.GetViewBetween(
                t.LcaState.LowerState, t.LcaState.UpperState);
            ExitSet.UnionWith(exitStates);
        }

        ActiveStates.ExceptWith(ExitSet);
        foreach (var s in ExitSet)
        {
            s.StateExit(Duct);
        }

        // 2. Invoke transitions
        // 3. Deduce and merge enter set
        foreach (var t in EnabledFilteredTransitions)
        {
            t.TransitionInvoke(Duct);

            // If transition is targetless, enter-region is null.
            if (t.IsTargetless)
            {
                continue;
            }

            SortedSet<StateInt> enterRegion = t.EnterRegion;
            SortedSet<StateInt> deducedEnterStates = t.GetDeducedEnterStates();
            EnterSet.UnionWith(enterRegion);
            EnterSet.UnionWith(deducedEnterStates);
        }

        ActiveStates.UnionWith(EnterSet);
        foreach (var s in EnterSet)
        {
            s.StateEnter(Duct);
        }

        // 4. Clear
        EnabledTransitions.Clear();
        EnabledFilteredTransitions.Clear();
        ExitSet.Clear();
        EnterSet.Clear();
    }

    public static StateInt GetStateInt(StatechartBuilder<TDuct, TEvent>.State state) => state.Mode switch
    {
        StateModeEnum.Compound => new CompoundInt(),
        // StateModeEnum.Parallel => new ParallelInt(),
        // StateModeEnum.History => new HistoryInt(),
        _ => null
    };
}
