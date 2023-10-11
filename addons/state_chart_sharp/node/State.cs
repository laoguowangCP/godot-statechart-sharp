using Godot;
using Godot.Collections;
using System;

namespace LGWCP.GodotPlugin.StateChartSharp
{
    [GlobalClass]
    public partial class State : Node
    {
        [Signal] public delegate void EnterEventHandler();
        [Signal] public delegate void ExitEventHandler();
        [Signal] public delegate void ProcessEventHandler(double delta);
        [Signal] public delegate void PhysicsProcessEventHandler(double delta);
        [Signal] public delegate void InputEventHandler(InputEvent @event);
        [Signal] public delegate void UnhandledInputEventHandler(InputEvent @event);
        
        [Export] protected string stateName;
        public StateChart stateChart { get; protected set; }
        public int stateLevel { get; protected set; }
        public State parentState { get; protected set; }
        public State currentSubstate;
        protected Dictionary<StringName, State> substates;
        public Array<Transition> processTrans;
        public Array<Transition> physicsProcessTrans;
        public Array<Transition> inputTrans;
        public Array<Transition> unhandledInputTrans;

        public override void _Ready()
        {
            substates = new Dictionary<StringName, State>();
            processTrans = new Array<Transition>();
            physicsProcessTrans = new Array<Transition>();
            inputTrans = new Array<Transition>();
            unhandledInputTrans = new Array<Transition>();
        }

        public virtual void Init(StateChart stateChart, State parentState = null)
        {
            this.stateChart = stateChart;
            this.parentState = parentState;

            substates.Clear();
            processTrans.Clear();
            physicsProcessTrans.Clear();
            inputTrans.Clear();
            unhandledInputTrans.Clear();
        }

        public Array<Transition> GetTransitions(Transition.TransitionModeEnum mode) => mode switch
        {
            Transition.TransitionModeEnum.Process => processTrans,
            Transition.TransitionModeEnum.PhysicsProcess => physicsProcessTrans,
            Transition.TransitionModeEnum.Input => inputTrans,
            Transition.TransitionModeEnum.UnhandledInput => unhandledInputTrans,
            _ => null
        };

        public virtual void SubstateTransit(Transition.TransitionModeEnum mode) {}

        public virtual bool IsInstant() { return false; }

        public virtual void StateEnter()
        {
            EmitSignal(SignalName.Enter);
        }
        public virtual void StateEnter(string statePath)
        {
            EmitSignal(SignalName.Enter);
        }
        public virtual void StateExit()
        {
            EmitSignal(SignalName.Exit);
        }

        public virtual void StateInput(InputEvent @event)
        {
            EmitSignal(SignalName.Input, @event);
        }

        public virtual void StateUnhandledInput(InputEvent @event)
        {
            EmitSignal(SignalName.UnhandledInput, @event);
        }

        public virtual void StateProcess(double delta)
        {
            EmitSignal(SignalName.Process, delta);
        }

        public virtual void StatePhysicsProcess(double delta)
        {
            EmitSignal(SignalName.PhysicsProcess, delta);
        }
    }
}