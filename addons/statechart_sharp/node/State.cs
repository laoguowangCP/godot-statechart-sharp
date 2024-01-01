using Godot;
using System.Collections.Generic;

namespace LGWCP.StatechartSharp
{
    public enum StateModeEnum : int
    {
        Compond,
        Parallel,
        History
    }

    [GlobalClass, Icon("res://addons/statechart_sharp/icon/State.svg")]
    public partial class State : StatechartComposition
    {
        #region define signals

        [Signal] public delegate void EnterEventHandler(State state);
        [Signal] public delegate void ExitEventHandler(State state);
        
        #endregion

        [Export] public StateModeEnum StateMode { get; protected set; } = StateModeEnum.Compond;
        [Export] public bool IsDeepHistory { get; protected set; }
        [Export] public State InitialState { get; set; }
        
        // public Statechart HostStatechart { get; protected set; }
        public State ParentState { get; set; }
        public State CurrentState { get; set; }
        public List<State> Substates { get; set; }
        public List<Transition> Transitions { get; set; }
        public List<Reaction> Actions { get; set; }
        protected StateComponent StateComponent { get; set; }
        public State LowerState { get; set; }
        public State UpperState { get; set; }
        public bool IsHistory { get => StateMode == StateModeEnum.History; }

        public override void _Ready()
        {
            Substates = new List<State>();
            Transitions = new List<Transition>();
            Actions = new List<Reaction>();
            switch (StateMode)
            {
                case StateModeEnum.Compond:
                    StateComponent = new CompondComponent(this);
                    break;
                case StateModeEnum.Parallel:
                    StateComponent = new ParallelComponent(this);
                    break;
                case StateModeEnum.History:
                    StateComponent = new HistoryComponent(this);
                    break;
            }
        }

        internal override void Init(Statechart hostStateChart, ref int ancestorId)
        {
            base.Init(hostStateChart, ref ancestorId);
            StateComponent.Init(hostStateChart, ref ancestorId);
        }

        internal override void PostInit()
        {
            StateComponent.PostInit();
        }

        internal void StateEnter()
        {
            if (ParentState != null)
            {
                ParentState.HandleSubstateEnter(this);
            }
            
            EmitSignal(SignalName.Enter, this);
        }

        internal void StateExit()
        {
            EmitSignal(SignalName.Exit, this);
        }

        internal void StateInvoke(StringName eventName)
        {
            foreach (Reaction a in Actions)
            {
                a.ReactionInvoke(eventName);
            }
        }

        internal void RegisterActiveState(SortedSet<State> activeStates)
        {
            StateComponent.RegisterActiveState(activeStates);
        }

        internal bool IsConflictToEnterRegion(State substate, SortedSet<State> enterRegion)
        {
            return StateComponent.IsConflictToEnterRegion(substate, enterRegion);
        }

        internal void ExtendEnterRegion(SortedSet<State> enterRegion, SortedSet<State> enterRegionEdge, SortedSet<State> extraEnterRegion, bool needCheckContain = true)
        {
            StateComponent.ExtendEnterRegion(enterRegion, enterRegionEdge, extraEnterRegion, needCheckContain);
        }

        internal bool SelectTransitions(List<Transition> enabledTransitions, StringName eventName = null)
        {
            return StateComponent.SelectTransitions(enabledTransitions, eventName);
        }

        internal void DeduceDescendants(SortedSet<State> deducedSet, bool isHistory = false, bool isEdgeState = false)
        {
            StateComponent.DeduceDescendants(deducedSet, isHistory, isEdgeState);
        }

        internal void HandleSubstateEnter(State substate)
        {
            StateComponent.HandleSubstateEnter(substate);
        }
    }
}