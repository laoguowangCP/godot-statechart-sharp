using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LGWCP.GodotPlugin.StatechartSharp
{
    
    public enum StateModeEnum : int
    {
        Compond,
        Parallel
    }

    [GlobalClass, Icon("res://addons/statechart_sharp/icon/State.svg")]
    public partial class State : Node
    {
        #region define signals

        [Signal] public delegate void EnterEventHandler();
        [Signal] public delegate void ExitEventHandler();
        
        #endregion

        [Export] public StateModeEnum StateMode { get; protected set; } = StateModeEnum.Compond;
        
        public Statechart HostStatechart { get; protected set; }
        public State ParentState { get; protected set; }
        public State CurrentState { get; set; }
        public int StateId { get; protected set; }
        public List<State> Substates { get; protected set; }
        public List<Transition> Transitions { get; protected set; }
        public bool IsActive { get; set; }

        public override void _Ready()
        {
            Substates = new List<State>();
            Transitions = new List<Transition>();
            ProcessMode = ProcessModeEnum.Disabled;
        }

        public void Init(Statechart stateChart, int stateId)
        {
            #if DEBUG
            GD.Print("State Init: ", Name);
            #endif

            // stateComponent.Init(stateChart, parentState);
            HostStatechart = stateChart;
            StateId = stateId;
            IsActive = false;
            Node parent = GetParent<Node>();
            if (parent is null)
            {
                ParentState = null;
            }
            else if (parent is State)
            {
                ParentState = parent as State;
            }

            // TODO: get Substates & Transitions
            foreach(Node child in GetChildren())
            {
                if (child is State s)
                {
                    Substates.Add(s);
                }
                else if (child is Transition t)
                {
                    // Init transition later
                    Transitions.Add(t);
                }
            }

            if (Substates.Count > 0)
            {
                CurrentState = Substates[0];
            }
        }
    }
}