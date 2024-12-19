using System;
using System.Collections.Generic;
using System.Linq;


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
        public Statechart<TDuct, TEvent> HostStatechart;

        public virtual void SetupPre() {}

        public virtual void Setup() {}

        public virtual void Setup(Statechart<TDuct, TEvent> statechart, ref int orderId)
        {
            HostStatechart = statechart;
            OrderId = HostStatechart.GetOrderId();
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
    protected TEvent LastStepEvent = default;
    protected int StatechartLength = 0;
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
            state.StateEnter(Duct);
        }
    }

    protected int GetOrderId()
    {
        int id = StatechartLength;
        ++StateLength;
        return id;
    }

    protected int GetStateId()
    {
        int id = StateLength;
        ++StateLength;
        return id;
    }

    protected void RegistGlobalTransition(int stateId, TEvent @event, TransitionInt t)
    {
        (List<TransitionInt> Transitions, List<ReactionInt> _)[] globalTA;
        bool isEventInTable = GlobalEventTAMap.TryGetValue(@event, out globalTA);
        if (isEventInTable)
        {
            if (globalTA[stateId].Transitions is null)
            {
                globalTA[stateId].Transitions = new() { t };
            }
            else
            {
                globalTA[stateId].Transitions.Add(t);
            }
        }
        else
        {
            globalTA = new (List<TransitionInt>, List<ReactionInt>)[StateLength];
            globalTA[stateId].Transitions = new() { t };
            GlobalEventTAMap.Add(@event, globalTA);
        }
    }

    protected void RegistGlobalReaction(int stateId, TEvent @event, ReactionInt a)
    {
        (List<TransitionInt> _, List<ReactionInt> Reactions)[] globalTA;
        bool isEventInTable = GlobalEventTAMap.TryGetValue(@event, out globalTA);
        if (isEventInTable)
        {
            if (globalTA[stateId].Reactions is null)
            {
                globalTA[stateId].Reactions = new() { a };
            }
            else
            {
                globalTA[stateId].Reactions.Add(a);
            }
        }
        else
        {
            globalTA = new (List<TransitionInt>, List<ReactionInt>)[StateLength];
            globalTA[stateId].Reactions = new() { a };
            GlobalEventTAMap.Add(@event, globalTA);
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

        // Set global transitions/reactions, null if no matched event
        if (LastStepEvent.Equals(@event))
        {
            GlobalEventTAMap.TryGetValue(@event, out CurrentTAMap);
            LastStepEvent = @event;
        }
        // Use last query in GlobalEventTAMap if eventname not changed
        // GlobalEventTAMap.TryGetValue(eventName, out CurrentTAMap);

        // 1. Select transitions
        RootState.SelectTransitions(EnabledTransitions, false);

        // 2. Do transitions
        DoTransitions();

        // 3. Select and do automatic transitions
        for (int i = 1; i <= MaxAutoTransitionRound; ++i)
        {
            RootState.SelectTransitions(EnabledTransitions, true);

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
            state.SelectReactions(EnabledReactions);
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
                || ExitSet.Any<StateInt>(
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

#endregion


#region state

    protected class StateInt : StatechartComposition<StateInt>
    {
        protected delegate void EnterEvent(TDuct duct);
        protected event EnterEvent Enter;
        protected delegate void ExitEvent(TDuct duct);
        protected event ExitEvent Exit;

        public Action<SortedSet<StateInt>> SubmitActiveState;
        public Action<SortedSet<StateInt>, SortedSet<StateInt>, SortedSet<StateInt>, bool> ExtendEnterRegion;
        public Func<SortedSet<TransitionInt>, bool, int> SelectTransitions;
        public Action<SortedSet<StateInt>, bool, bool> DeduceDescendants;
        public Action<StateInt> HandleSubstateEnter;
        public Action<SortedSet<ReactionInt>> SelectReactions;
        public Action<List<int>> SaveAllStateConfig;
        public Action<List<int>> SaveActiveStateConfig;
        public Func<int[], int, int> LoadAllStateConfig;
        public Func<int[], int, int> LoadActiveStateConfig;


        public StateModeEnum StateMode;
        protected StateInt InitialState;
        public StateInt ParentState;
        public List<StateInt> Substates;
        public List<TransitionInt> AutoTransitions;
        protected StateImpl StateImpl;
        public StateInt LowerState;
        public StateInt UpperState;
        public int SubstateIdx; // The index of this state enlisted in parent state.
        public int StateId; // The ID of this state in statechart.
        public bool _IsHistory;

        public StateInt(StateModeEnum stateMode)
        {
            Substates = new();
            AutoTransitions = new();

            StateMode = stateMode;
            StateImpl = GetStateImpl(stateMode);
            IsHistory = StateMode == StateModeEnum.History;

            // Cache methods
            if (StateImpl is not null)
            {
                _GetPromoteStates = StateImpl._GetPromoteStates;
                _IsAvailableRootState = StateImpl._IsAvailableRootState;
                _SubmitActiveState = StateImpl._SubmitActiveState;
                _IsConflictToEnterRegion = StateImpl._IsConflictToEnterRegion;
                _ExtendEnterRegion = StateImpl._ExtendEnterRegion;
                _SelectTransitions = StateImpl._SelectTransitions;
                _DeduceDescendants = StateImpl._DeduceDescendants;
                _HandleSubstateEnter = StateImpl._HandleSubstateEnter;
                _SelectReactions = StateImpl._SelectReactions;

                _SaveAllStateConfig = StateImpl._SaveAllStateConfig;
                _SaveActiveStateConfig = StateImpl._SaveActiveStateConfig;
                _LoadAllStateConfig = StateImpl._LoadAllStateConfig;
                _LoadActiveStateConfig = StateImpl._LoadActiveStateConfig;
            }
        }

        protected StateImpl GetStateImpl(StateModeEnum mode) => mode switch
        {
            StateModeEnum.Compound => new CompoundImpl(this),
            StateModeEnum.Parallel => new ParallelImpl(this),
            StateModeEnum.History => new HistoryImpl(this),
            _ => new CompoundImpl(this)
        };

        public void StateEnter(TDuct duct)
        {
            ParentState?.HandleSubstateEnter(this);
            Enter.Invoke(duct);
        }

        public void StateExit(TDuct duct)
        {
            Exit.Invoke(duct);
        }

        public bool IsAncestorStateOf(StateInt state)
        {
            int id = state.OrderId;

            // Leaf state
            if (LowerState is null || UpperState is null)
            {
                return false;
            }

            return id >= LowerState.OrderId
                && id <= UpperState.OrderId;
        }
    }

    public class State
    {
        private StateInt _s;
        public State(StateModeEnum mode, bool isDeepHistory, Action<TDuct>[] enters, Action<TDuct>[] exits)
        {
            // TODO: move deep history to a state mode
            _s = new(mode);
        }
    }

#endregion


#region transition

    protected class TransitionInt : StatechartComposition<TransitionInt>
    {
        protected delegate void GuardEvent(TDuct duct);
        protected event GuardEvent Guard;
        protected delegate void InvokeEvent(TDuct duct);
        protected event InvokeEvent Invoke;

        public TEvent Event;
        protected List<StateInt> TargetStates;
        public StateInt SourceState;
        public StateInt LcaState;
        public SortedSet<StateInt> EnterRegion;
        protected SortedSet<StateInt> EnterRegionEdge;
        protected SortedSet<StateInt> DeducedEnterStates;
        public bool IsTargetless;

        public void TransitionInvoke(TDuct duct)
        {
            Invoke.Invoke(duct);
        }

        public SortedSet<StateInt> GetDeducedEnterStates()
        {
            DeducedEnterStates.Clear();
            foreach (var edgeState in EnterRegionEdge)
            {
                edgeState.DeduceDescendants(DeducedEnterStates, false, true);
            }
            return DeducedEnterStates;
        }
    }

    public class Transition
    {
        private TransitionInt _t;

        public Transition(TEvent @event, State[] targets=null, bool isAuto=false, Action<TDuct>[] guards=null, Action<TDuct>[] invokes = null)
        {
        }
    }

#endregion


#region reaction

    protected class ReactionInt : StatechartComposition<ReactionInt>
    {
        protected delegate void InvokeEvent(TDuct duct);
        protected event InvokeEvent Invoke;

        public void ReactionInvoke(TDuct duct)
        {
            Invoke.Invoke(duct);
        }
    }

    public class Reaction
    {
        private ReactionInt _a;
        public Reaction()
        {

        }
    }

#endregion


#region state impl

    protected abstract class StateImpl
    {
        public virtual bool IsAvailableRootState()
        {
            return false;
        }

        public virtual void Setup(StateInt hostState, ref int parentOrderId, int substateIdx)
        {
            hostState.SubstateIdx = substateIdx;
            hostState.StateId = hostState.HostStatechart.GetStateId();
        }

        public virtual void _SetupPost() {}

        public virtual bool _GetPromoteStates(List<State> states) { return false; }

        public virtual void _SubmitActiveState(SortedSet<State> activeStates) {}

        public virtual bool _IsConflictToEnterRegion(State substateToPend, SortedSet<State> enterRegionUnextended)
        {
            return false;
        }

        public virtual int _SelectTransitions(SortedSet<Transition> enabledTransitions, bool isAuto)
        {
            return 1;
        }

        public virtual void _ExtendEnterRegion(SortedSet<State> enterRegion, SortedSet<State> enterRegionEdge, SortedSet<State> extraEnterRegion, bool needCheckContain) {}

        public virtual void _DeduceDescendants(SortedSet<State> deducedSet, bool isHistory, bool isEdgeState) {}

        public virtual void _HandleSubstateEnter(State substate) {}

        public virtual void _SelectReactions(SortedSet<Reaction> enabledReactions)
        {
            if (CurrentTAMap is null)
            {
                return;
            }

            var matched = CurrentTAMap[_StateId].Reactions;
            if (matched is null)
            {
                return;
            }

            foreach (Reaction reaction in matched)
            {
                enabledReactions.Add(reaction);
            }
        }

        public virtual void _SaveAllStateConfig(List<int> snapshot) {}

        public virtual void _SaveActiveStateConfig(List<int> snapshot) {}

        public virtual int _LoadAllStateConfig(int[] config, int configIdx) { return configIdx; }

        public virtual int _LoadActiveStateConfig(int[] config, int configIdx) { return configIdx; }

    }

#endregion

}

public enum StateModeEnum : int
{
    Compound,
    Parallel,
    History,
    DeepHistory
}


public class TestStatechartBuild
{
    public void Build()
    {
        var sc = new Statechart<BaseStatechartDuct, string>();
    }
}

