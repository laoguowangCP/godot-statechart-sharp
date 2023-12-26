using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace LGWCP.StatechartSharp
{
    public enum StateModeEnum : int
    {
        Compond,
        Parallel,
        History
    }

    [GlobalClass, Icon("res://addons/statechart_sharp/icon/State.svg"), Tool]
    public partial class State : StatechartComposition
    {
        #region define signals

        [Signal] public delegate void EnterEventHandler();
        [Signal] public delegate void ExitEventHandler();
        
        #endregion

        [Export] public StateModeEnum StateMode { get; protected set; } = StateModeEnum.Compond;
        [Export] public bool IsDeepHistory { get; protected set; }
        [Export] public State InitialState { get; set; }
        
        // public Statechart HostStatechart { get; protected set; }
        public State ParentState { get; set; }
        public State CurrentState { get; set; }
        public List<State> Substates { get; set; }
        public List<Transition> Transitions { get; set; }
        public List<Action> Actions { get; set; }
        protected StateComponent StateComponent { get; set; }
        public State LowerState { get; set; }
        public State UpperState { get; set; }

        public override void _Ready()
        {
            Substates = new List<State>();
            Transitions = new List<Transition>();
            Actions = new List<Action>();
            ProcessMode = ProcessModeEnum.Disabled;
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

        public void StateEnter()
        {
            ParentState.HandleSubstateEnter(this);
            EmitSignal(SignalName.Enter);
        }

        public void StateExit()
        {
            EmitSignal(SignalName.Exit);
        }

        public void StateInvoke(StringName eventName)
        {
            foreach (Action a in Actions)
            {
                a.ActionInvoke(eventName);
            }
        }

        public void RegisterActiveState(SortedSet<State> activeStates)
        {
            StateComponent.RegisterActiveState(activeStates);
        }

        public bool IsConflictToEnterRegion(State substate, SortedSet<State> enterRegion)
        {
            return StateComponent.IsConflictToEnterRegion(substate, enterRegion);
        }

        public void ExtendEnterRegion(SortedSet<State> enterRegion, SortedSet<State> enterRegionEdge, SortedSet<State> extraEnterRegion, bool needCheckContain = true)
        {
            StateComponent.ExtendEnterRegion(enterRegion, enterRegionEdge, extraEnterRegion, needCheckContain);
        }

        public bool SelectTransitions(List<Transition> enabledTransitions, StringName eventName = null)
        {
            return StateComponent.SelectTransitions(enabledTransitions, eventName);
        }

        public void DeduceDescendants(SortedSet<State> deducedSet, bool isHistory = false)
        {
            StateComponent.DeduceDescendants(deducedSet, isHistory);
        }
        public void DeduceDescendantsFromHistory(SortedSet<State> deducedSet, bool isDeepHistory = false)
        {
            StateComponent.DeduceDescendantsFromHistory(deducedSet, isDeepHistory);
        }

        public void HandleSubstateEnter(State substate)
        {
            StateComponent.HandleSubstateEnter(substate);
        }
    }
}