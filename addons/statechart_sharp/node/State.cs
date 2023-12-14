using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace LGWCP.GodotPlugin.StatechartSharp
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

        [Signal] public delegate void EnterEventHandler();
        [Signal] public delegate void ExitEventHandler();
        
        #endregion

        [Export] public StateModeEnum StateMode { get; protected set; } = StateModeEnum.Compond;
        [Export] public State InitialState { get; protected set; }
        [Export] public bool IsDeepHistory { get; protected set; }
        
        public Statechart HostStatechart { get; protected set; }
        public State ParentState { get; protected set; }
        public State CurrentState { get; set; }
        public int StateId { get; protected set; }
        public List<State> Substates { get; protected set; }
        public List<Transition> Transitions { get; protected set; }
        public bool IsActive { get; set; }
        // public int DescendantCount { get; protected set; }
        protected StateComponent StateComponent { get; set; }

        public override void _Ready()
        {
            Substates = new List<State>();
            Transitions = new List<Transition>();
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
                    StateComponent = new ParallelComponent(this);
                    break;
            }
            
            IsActive = false;
            Node parent = GetParent<Node>();
            if (parent != null && parent is State)
            {
                ParentState = parent as State;
            }
            else
            {
                ParentState = null;
            }

            // Get Substates & Transitions
            var children = GetChildren();
            foreach(Node child in children)
            {
                if (child is State s)
                {
                    Substates.Add(s);
                }
                else if (child is Transition t)
                {
                    // Root state should not have transition
                    if (ParentState != null)
                    {
                        Transitions.Add(t);
                    }
                }
            }

            // Initial state
            if (StateMode == StateModeEnum.Compond && Substates.Count > 0)
            {
                if (InitialState != null && InitialState.ParentState == this)
                {
                    CurrentState = InitialState;
                }
                else
                {
                    CurrentState = Substates[0];
                }
            }
        }

        public void Init(Statechart stateChart, int stateId)
        {
            #if DEBUG
            GD.Print("State Init: ", Name);
            #endif

            // stateComponent.Init(stateChart, parentState);
            HostStatechart = stateChart;
            StateId = stateId;
        }

        public bool SelectTransitions(StringName eventName)
        {
            return StateComponent.SelectTransitions(eventName);
        }

        public void DeduceDescendants(SortedSet<State> deducedSet, bool isHistory = false)
        {
            StateComponent.DeduceDescendants(deducedSet, isHistory);
        }

        public void RefineEnterRegion(SortedSet<State> enterRegion, SortedSet<State> enterRegionEdge, SortedSet<State> extraEnterRegion = null)
        {
            StateComponent.RefineEnterRegion(enterRegion, enterRegionEdge, extraEnterRegion);
        }
    }
}