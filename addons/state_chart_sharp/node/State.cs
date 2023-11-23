using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

namespace LGWCP.GodotPlugin.StateChartSharp
{
    
    public enum StateModeEnum : int
    {
        Compond,
        Parallel
    }

    [GlobalClass, Icon("res://addons/state_chart_sharp/icon/State.svg")]
    public partial class State : Node
    {
        #region define signals

        [Signal] public delegate void EnterEventHandler();
        [Signal] public delegate void ExitEventHandler();
        
        #endregion

        [Export] public StateModeEnum StateMode { get; protected set; } = StateModeEnum.Compond;
        
        public StateChart StateChart { get; protected set; }
        public State ParentState { get; protected set; }
        public int StateId { get; protected set; }
        public List<State> Substates { get; protected set; }
        public List<Transition> Transitions { get; protected set; }
        public bool IsActive { get; set; }

        public override void _Ready()
        {
            // Initialize state component
            Substates = new List<State>();
            ProcessMode = ProcessModeEnum.Disabled;
        }

        public void Init(StateChart stateChart, int stateId)
        {
            // stateComponent.Init(stateChart, parentState);
            StateChart = stateChart;
            Node parent = GetParent<Node>();
            if (parent is null)
            {
                ParentState = null;
            }
            else if (parent is State)
            {
                ParentState = parent as State;
            }

            // TODO: get Substates
            // TODO: get Transitions

            StateId = stateId;
            IsActive = false;
        }
    }
}