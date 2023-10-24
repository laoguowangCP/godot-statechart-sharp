using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

namespace LGWCP.GodotPlugin.StateChartSharp
{
    
    public enum StateModeEnum : int
    {
        Compond,
        Parallel,
        Atomic // TODO: maybe not necessary?
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

        [Export] StateModeEnum stateMode = StateModeEnum.Compond;
        [Export] protected bool isInstant = false;
        protected StateComponent stateComponent;

        public StateChart stateChart { get; protected set; }
        public int stateLevel { get; protected set; }
        public State parentState { get; protected set; }
        public State currentSubstate;

        // TODO: transfer lists to StateComponent
        protected List<State> substates;
        public List<Transition> processTrans;
        public List<Transition> physicsProcessTrans;
        public List<Transition> inputTrans;
        public List<Transition> unhandledInputTrans;

        public override void _Ready()
        {
            // Initialize substate list
            substates = new List<State>();

            // Initialize transition lists
            processTrans = new List<Transition>();
            physicsProcessTrans = new List<Transition>();
            inputTrans = new List<Transition>();
            unhandledInputTrans = new List<Transition>();
            
            // Initialize state component
            switch (stateMode)
            {
                case StateModeEnum.Compond:
                    stateComponent = new CompondStateComponent(this);
                    break;
                case StateModeEnum.Atomic:
                    stateComponent = new AtomicStateComponent();
                    break;
                case StateModeEnum.Parallel:
                    stateComponent = new ParallelStateComponent(this);
                    break;
                default:
                    break;
            }
        }

        public virtual void Init(StateChart stateChart, State parentState = null)
        {
            // stateComponent.Init(stateChart, parentState);
            this.stateChart = stateChart;
            this.parentState = parentState;

            // Set state level, depth in state chart
            if (parentState is null)
            {
                stateLevel = 0;
            }
            else
            {
                stateLevel = parentState.stateLevel + 1;
            }

            stateComponent.Init(stateChart, parentState);
        }

        public List<Transition> GetTransitions(TransitionModeEnum mode) => mode switch
        {
            TransitionModeEnum.Process
                => processTrans,
            TransitionModeEnum.PhysicsProcess
                => physicsProcessTrans,
            TransitionModeEnum.Input
                => inputTrans,
            TransitionModeEnum.UnhandledInput
                => unhandledInputTrans,
            _ => null
        };

        public virtual void SubstateTransit(TransitionModeEnum mode)
        {
            stateComponent.SubstateTransit(mode);
        }

        public bool IsInstant() { return isInstant; }

        public virtual void StateEnter()
        {
            // stateComponent.StateEnter();
            EmitSignal(SignalName.Enter);
        }

        public virtual void StateExit()
        {
            // stateComponent.StateExit();
            EmitSignal(SignalName.Exit);
        }

        public virtual void StateInput(InputEvent @event)
        {
            // stateComponent.StateInput(@event);
            EmitSignal(SignalName.Input, @event);
        }

        public virtual void StateUnhandledInput(InputEvent @event)
        {
            // stateComponent.StateUnhandledInput(@event);
            EmitSignal(SignalName.UnhandledInput, @event);
        }

        public virtual void StateProcess(double delta)
        {
            // stateComponent.StateProcess(delta);
            EmitSignal(SignalName.Process, delta);
        }

        public virtual void StatePhysicsProcess(double delta)
        {
            // stateComponent.StatePhysicsProcess(delta);
            EmitSignal(SignalName.PhysicsProcess, delta);
        }
    }
}