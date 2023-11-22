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

        [Export] protected StateModeEnum stateMode = StateModeEnum.Compond;
        protected StateComponent stateComponent;

        public StateChart StateChart { get; protected set; }
        public State ParentState { get; protected set; }
        public List<State> Substates { get; protected set; }
        public bool isActive;

        public override void _Ready()
        {
            // Initialize state component
            switch (stateMode)
            {
                case StateModeEnum.Compond:
                    stateComponent = new CompondComponent(this);
                    break;
                case StateModeEnum.Parallel:
                    stateComponent = new ParallelComponent(this);
                    break;
            }

            ProcessMode = ProcessModeEnum.Disabled;
        }

        public void Init(StateChart stateChart, int stateId)
        {
            // stateComponent.Init(stateChart, parentState);
            StateChart = stateChart;
            Node parent = GetParent<Node>();
            if (parent is State)
            {
                ParentState = parent as State;
            }
            else
            {
                ParentState = null;
            }

            isActive = false;
        }

        public List<Transition> GetTransitions(TransitionModeEnum mode)
        {
            return stateComponent.GetTransitions(mode);
        }

        public StateModeEnum GetStateMode()
        {
            return stateMode;
        }

        public void SubstateTransit(StringName eventName, double delta = 0.0, InputEvent @event = null)
        {
            
        }
    }
}