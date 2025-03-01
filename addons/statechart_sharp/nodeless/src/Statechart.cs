using System;
using System.Collections.Generic;


namespace LGWCP.Godot.StatechartSharp.Nodeless;

public partial class Statechart<TDuct, TEvent>
    where TDuct : StatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{
    protected int MaxInternalEventCount;
    protected int MaxAutoTransitionRound;
    protected bool IsRunning;
    protected int EventCount;
    protected State RootState;
    protected SortedSet<State> ActiveStates;
    protected Queue<TEvent> QueuedEvents = new();
    protected SortedSet<Transition> EnabledTransitions;
    protected SortedSet<Transition> EnabledFilteredTransitions;
    protected SortedSet<State> ExitSet;
    protected SortedSet<State> EnterSet;
    protected SortedSet<Reaction> EnabledReactions;
    protected TDuct Duct;
    protected bool IsReady = false;

    public Statechart(
        TDuct duct = null,
        int maxInternalEventCount = 8,
        int maxAutoTransitionRound = 8)
    {
        Duct = duct ?? new();
        MaxInternalEventCount = maxInternalEventCount;
        MaxAutoTransitionRound = maxAutoTransitionRound;

        IsRunning = false;
        EventCount = 0;

        ActiveStates = new (new StatechartComparer<State>());
        EnabledTransitions = new (new StatechartComparer<Transition>());
        EnabledFilteredTransitions = new (new StatechartComparer<Transition>());
        ExitSet = new (new StatechartReversedComparer<State>());
        EnterSet = new (new StatechartComparer<State>());
        EnabledReactions = new (new StatechartComparer<Reaction>());
    }

    public void Ready(State rootState, bool isInitInvokeEnter = true)
    {
        // Setup
        RootState = rootState;

        int orderId = 0;
        RootState._Setup(ref orderId);
        RootState._SubmitActiveState(ActiveStates);
        RootState._SetupPost();

        if (isInitInvokeEnter)
        {
            foreach (State state in ActiveStates)
            {
                state._StateEnter(Duct);
            }
        }
        else
        {
            foreach (State state in ActiveStates)
            {
                state._ParentState?._HandleSubstateEnter(state);
            }
        }

        IsReady = true;
    }

    public void Step(TEvent eventName)
    {
        if (IsRunning)
        {
            if (EventCount <= MaxInternalEventCount)
            {
                ++EventCount;
                QueuedEvents.Enqueue(eventName);
            }
            return;
        }

        // Else is not running
        ++EventCount;
        QueuedEvents.Enqueue(eventName);

        IsRunning = true;

        while (QueuedEvents.Count > 0)
        {
            var nextEvent = QueuedEvents.Dequeue();
            HandleEvent(nextEvent);
        }

        IsRunning = false;
        EventCount = 0;
    }


    protected void HandleEvent(TEvent @event)
    {
        if (RootState == null)
        {
            return;
        }
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
        RootState._SelectTransitions(EnabledTransitions, @event, Duct);

        // 2. Do transitions
        DoTransitions();

        // 3. Select and do automatic transitions
        for (int i = 0; i < MaxAutoTransitionRound; ++i)
        {
            RootState._SelectAutoTransitions(EnabledTransitions, Duct);

            // Stop if active states are stable
            if (EnabledTransitions.Count == 0)
            {
                break;
            }
            DoTransitions();
        }

        // 4. Select and do reactions
        foreach (State state in ActiveStates)
        {
            state._SelectReactions(EnabledReactions, @event);
        }

        foreach (Reaction reaction in EnabledReactions)
        {
            reaction._ReactionInvoke(Duct);
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
        foreach (Transition transition in EnabledTransitions)
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
            if (transition._IsTargetless)
            {
                EnabledFilteredTransitions.Add(transition);
                continue;
            }

            bool hasConfliction = ExitSet.Contains(transition._SourceState)
                || transition._LcaState._IsAncestorStateOfAnyReversed(ExitSet);


            if (hasConfliction)
            {
                continue;
            }

            EnabledFilteredTransitions.Add(transition);

            var exitStates = ActiveStates.GetViewBetween(
                transition._LcaState._LowerState, transition._LcaState._UpperState);
            ExitSet.UnionWith(exitStates);
        }

        ActiveStates.ExceptWith(ExitSet);
        foreach (State state in ExitSet)
        {
            state._StateExit(Duct);
        }

        // 2. Invoke transitions
        // 3. Deduce and merge enter set
        foreach (Transition transition in EnabledFilteredTransitions)
        {
            transition._TransitionInvoke(Duct);

            // If transition is targetless, enter-region is null.
            if (transition._IsTargetless)
            {
                continue;
            }

            SortedSet<State> enterRegion = transition._EnterRegion;
            SortedSet<State> deducedEnterStates = transition._GetDeducedEnterStates();
            EnterSet.UnionWith(enterRegion);
            EnterSet.UnionWith(deducedEnterStates);
        }

        ActiveStates.UnionWith(EnterSet);
        foreach (State state in EnterSet)
        {
            state._StateEnter(Duct);
        }

        // 4. Clear
        EnabledTransitions.Clear();
        EnabledFilteredTransitions.Clear();
        ExitSet.Clear();
        EnterSet.Clear();
    }

    public StatechartSnapshot Save(bool isAllStateConfig = false)
    {
        StatechartSnapshot snapshot = new()
        {
            IsAllStateConfig = isAllStateConfig
        };

        return Save(snapshot) ? snapshot : null;
    }

    public bool Save(StatechartSnapshot snapshot)
    {
        if (IsRunning)
        {
            return false;
        }

        var config = snapshot.Config;
        config.Clear();
        if (snapshot.IsAllStateConfig)
        {
            RootState._SaveAllStateConfig(config);
        }
        else
        {
            RootState._SaveActiveStateConfig(config);
        }
        return true;
    }

    public bool Load(StatechartSnapshot snapshot, bool isExitOnLoad = false, bool isEnterOnLoad = false)
    {
        if (IsRunning)
        {
            return false;
        }

        if (snapshot is null)
        {
            return false;
        }

        /*
        1. Iterate state configuration:
            - Set current idx
            - Add to statesToLoad
            - If config not aligned, abort
        2. Deal enter/exit on load:
            - Get differed states
            - Exit old states
            - Enter new states
        3. Update active states
        */
        var config = snapshot.Config;
        if (config.Count == 0)
        {
            return false;
        }

        int loadIdxResult;
        if (snapshot.IsAllStateConfig)
        {
            loadIdxResult = RootState._LoadAllStateConfig(config, 0);
        }
        else
        {
            loadIdxResult = RootState._LoadActiveStateConfig(config, 0);
        }

        if (loadIdxResult == -1)
        {
            return false;
        }

        // Exit on load
        if (isExitOnLoad)
        {
            foreach (State state in ActiveStates.Reverse())
            {
                state._StateExit(Duct);
            }
        }

        ActiveStates.Clear();
        RootState._SubmitActiveState(ActiveStates);

        // Enter on load
        if (isEnterOnLoad)
        {
            foreach (State state in ActiveStates)
            {
                state._StateEnter(Duct);
            }
        }

        return true;
    }

    public Compound GetCompound(
        Action<TDuct>[] enters = null,
        Action<TDuct>[] exits = null,
        State initialState = null)
    {
        return new Compound(this, enters, exits, initialState);
    }

    public Parallel GetParallel(
        Action<TDuct>[] enters = null,
        Action<TDuct>[] exits = null)
    {
        return new Parallel(this, enters, exits);
    }

    public History GetHistory()
    {
        return new History(this);
    }

    public DeepHistory GetDeepHistory()
    {
        return new DeepHistory(this);
    }

    public Transition GetTransition(
        TEvent @event,
        Action<TDuct>[] guards = null,
        Action<TDuct>[] invokes = null,
        State[] targetStates = null)
    {
        return new Transition(this, @event, guards, invokes, targetStates);
    }

    public Transition GetAutoTransition(
        Action<TDuct>[] guards = null,
        Action<TDuct>[] invokes = null,
        State[] targetStates = null)
    {
        return new Transition(this, guards, invokes, targetStates);
    }

    public Reaction GetReaction(
        TEvent @event,
        Action<TDuct>[] invokes = null)
    {
        return new Reaction(this, @event, invokes);
    }
}
