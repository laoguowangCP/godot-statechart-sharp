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

public partial class Statechart<TDuct, TEvent>
    where TDuct : IStatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{
    public class State : BuildComposition<State>
    {
        private StateInt _s;
        public State(StateModeEnum mode, Action<TDuct>[] enters, Action<TDuct>[] exits)
        {
            // TODO: move deep history to a state mode
            _s = GetStateInt(mode);
        }

        public void SetAsRootState(Statechart<TDuct, TEvent> statechart)
        {
            statechart.RootState = _s;
        }

        private StateInt GetStateInt(StateModeEnum mode) => mode switch
        {
            StateModeEnum.Compound => new CompoundInt(),
            // StateModeEnum.Parallel => new ParallelInt(),
            // StateModeEnum.History => new HistoryInt(),
            _ => new CompoundInt()
        };
    }

    protected abstract class StateInt : Composition<StateInt>
    {
        protected delegate void EnterEvent(TDuct duct);
        protected event EnterEvent Enter;
        protected delegate void ExitEvent(TDuct duct);
        protected event ExitEvent Exit;

        protected StateInt InitialState;
        public StateInt ParentState;
        public List<StateInt> Substates;
        public List<TransitionInt> AutoTransitions;
        public StateInt LowerState;
        public StateInt UpperState;
        public int SubstateIdx; // The index of this state enlisted in parent state.
        public int StateId; // The ID of this state in statechart.
        protected (List<TransitionInt> Transitions, List<ReactionInt> Reactions)[] CurrentTA
        {
            get => HostStatechart.CurrentTA;
        }

        public override void Setup()
        {
            OrderId = HostStatechart.GetOrderId();
            StateId = HostStatechart.GetStateId();
        }

        public virtual void Setup(int substateIdx)
        {
            SubstateIdx = substateIdx;
            OrderId = HostStatechart.GetOrderId();
            StateId = HostStatechart.GetStateId();
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

        public virtual int SelectTransitions(SortedSet<TransitionInt> enabledTransitions, bool isAuto)
        {
            return 1;
        }

        public virtual void ExtendEnterRegion(SortedSet<StateInt> enterRegion, SortedSet<StateInt> enterRegionEdge, SortedSet<StateInt> extraEnterRegion, bool needCheckContain) {}

        public virtual void DeduceDescendants(SortedSet<StateInt> deducedSet, bool isHistory, bool isEdgeState) {}

        public virtual void HandleSubstateEnter(StateInt substate) {}

        public virtual void SelectReactions(SortedSet<ReactionInt> enabledReactions)
        {
            if (CurrentTA is null)
            {
                return;
            }

            var matched = CurrentTA[StateId].Reactions;
            if (matched is null)
            {
                return;
            }

            foreach (var a in matched)
            {
                enabledReactions.Add(a);
            }
        }

        public virtual void SaveAllStateConfig(List<int> snapshot) {}

        public virtual void SaveActiveStateConfig(List<int> snapshot) {}

        public virtual int LoadAllStateConfig(int[] config, int configIdx) { return configIdx; }

        public virtual int LoadActiveStateConfig(int[] config, int configIdx) { return configIdx; }
    }
}
