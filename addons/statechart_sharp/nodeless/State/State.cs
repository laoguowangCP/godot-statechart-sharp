using System;
using System.Collections.Generic;

namespace LGWCP.Godot.StatechartSharp.Nodeless;

public enum StateModeEnum : int
{
    Compound,
    Parallel,
    History,
    DeepHistory
}

public enum DeduceDescendantsModeEnum : int
{
	Initial,
	History,
	DeepHistory
}

public partial class Statechart<TDuct, TEvent>
    where TDuct : IStatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{
    public class State : BuildComposition<State>
    {
        public State() {}
        public StateModeEnum Mode;

        public State(StateModeEnum mode, Action<TDuct>[] enters, Action<TDuct>[] exits)
        {
            // TODO: store info
        }

        public override object Clone()
        {
            // TODO: impl clone
            return new State();
        }
    }

    protected static StateInt GetStateInt(State state) => state.Mode switch
    {
        StateModeEnum.Compound => new CompoundInt(),
        // StateModeEnum.Parallel => new ParallelInt(),
        // StateModeEnum.History => new HistoryInt(),
        _ => null
    };

    protected abstract class StateInt : Composition<StateInt>
    {
        protected delegate void EnterEvent(TDuct duct);
        protected event EnterEvent Enter;
        protected delegate void ExitEvent(TDuct duct);
        protected event ExitEvent Exit;

        protected StateInt InitialState;
        public StateInt ParentState;
        public List<StateInt> Substates;
        protected Dictionary<TEvent, List<TransitionInt>> Transitions = new();
        protected List<TransitionInt> AutoTransitions = new();
        protected Dictionary<TEvent, List<ReactionInt>> Reactions = new();
        public StateInt LowerState;
        public StateInt UpperState;
        public int SubstateIdx; // The index of this state enlisted in parent state.

        public override void Setup() {}

        public virtual void Setup(Statechart<TDuct, TEvent> hostStatechart, ref int orderId, int substateIdx)
        {

        }

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

        public virtual bool GetPromoteStates(List<StateInt> states) { return false; }

        public virtual void SubmitActiveState(SortedSet<StateInt> activeStates) {}

        public virtual bool IsConflictToEnterRegion(StateInt substateToPend, SortedSet<StateInt> enterRegionUnextended)
        {
            return false;
        }

        public virtual int SelectTransitions(SortedSet<TransitionInt> enabledTransitions, TEvent @event)
        {
            return 1;
        }

        public virtual void ExtendEnterRegion(SortedSet<StateInt> enterRegion, SortedSet<StateInt> enterRegionEdge, SortedSet<StateInt> extraEnterRegion, bool needCheckContain) {}

        public virtual void DeduceDescendants(SortedSet<StateInt> deducedSet, bool isHistory, bool isEdgeState) {}

        public virtual void HandleSubstateEnter(StateInt substate) {}

        public virtual void SelectReactions(SortedSet<ReactionInt> enabledReactions, TEvent @event)
        {
            if (Reactions.TryGetValue(@event, out var eventReactions))
            {
                foreach (ReactionInt a in eventReactions)
                {
                    enabledReactions.Add(a);
                }
            }
        }

        public virtual void SaveAllStateConfig(List<int> snapshot) {}

        public virtual void SaveActiveStateConfig(List<int> snapshot) {}

        public virtual int LoadAllStateConfig(int[] config, int configIdx) { return configIdx; }

        public virtual int LoadActiveStateConfig(int[] config, int configIdx) { return configIdx; }
    }
}
