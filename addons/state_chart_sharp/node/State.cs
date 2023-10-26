using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

namespace LGWCP.GodotPlugin.StateChartSharp
{
    
    public enum StateModeEnum : int
    {
        Atomic, // TODO: maybe not necessary?
        Compond,
        Parallel
    }

    [GlobalClass]
    // [GlobalClass, Icon("res://Stats/StatsIcon.svg")]
    public partial class State : Node
    {
        #region define signals

        [Signal] public delegate void EnterEventHandler();
        [Signal] public delegate void ExitEventHandler();
        [Signal] public delegate void ProcessEventHandler(double delta);
        [Signal] public delegate void PhysicsProcessEventHandler(double delta);
        [Signal] public delegate void InputEventHandler(InputEvent @event);
        [Signal] public delegate void UnhandledInputEventHandler(InputEvent @event);
        
        #endregion

        [Export] protected StateModeEnum stateMode = StateModeEnum.Compond;
        [Export] protected bool isInstant = false;
        protected StateComponent stateComponent;

        public StateChart StateChart { get; protected set; }
        public int StateLevel { get; protected set; }
        public State ParentState { get; protected set; }

        public override void _Ready()
        {
            // Initialize state component
            switch (stateMode)
            {
                case StateModeEnum.Atomic:
                    stateComponent = new AtomicStateComponent(this);
                    break;
                case StateModeEnum.Compond:
                    stateComponent = new CompondStateComponent(this);
                    break;
                case StateModeEnum.Parallel:
                    stateComponent = new ParallelStateComponent(this);
                    break;
                default:
                    break;
            }
        }

        public void Init(StateChart stateChart, State parentState = null)
        {
            // stateComponent.Init(stateChart, parentState);
            StateChart = stateChart;
            ParentState = parentState;

            // Set state level, depth in state chart
            if (parentState is null)
            {
                StateLevel = 0;
            }
            else
            {
                StateLevel = parentState.StateLevel + 1;
            }

            stateComponent.Init(stateChart, parentState);
        }

        public List<Transition> GetTransitions(TransitionModeEnum mode)
        {
            return stateComponent.GetTransitions(mode);
        }

        public StateModeEnum GetStateMode()
        {
            return stateMode;
        }

        public void SubstateTransit(TransitionModeEnum mode, State fromState = null, State toState = null, bool recursive = true)
        {
            stateComponent.SubstateTransit(mode, fromState, toState, recursive);
        }
        
        /*
        public void InstantTransit(TransitionModeEnum mode)
        {
            stateComponent.InstantTransit(mode);
        }*/

        public bool IsInstant() { return isInstant; }

        public void StateEnter()
        {
            stateComponent.StateEnter();
        }

        public void StateEnter(TransitionModeEnum mode)
        {
            stateComponent.StateEnter(mode);
        }

        public void StateExit()
        {
            stateComponent.StateExit();
        }

        public void StateInput(InputEvent @event)
        {
            stateComponent.StateInput(@event);
        }

        public void StateUnhandledInput(InputEvent @event)
        {
            stateComponent.StateUnhandledInput(@event);
        }

        public void StateProcess(double delta)
        {
            stateComponent.StateProcess(delta);
        }

        public void StatePhysicsProcess(double delta)
        {
            stateComponent.StatePhysicsProcess(delta);
        }
    }
}