using System.Collections.Generic;
using Godot;
using Godot.Collections;


namespace LGWCP.GodotPlugin.StateChartSharp
{
    public enum TransitionModeEnum : int
    {
        Process,
        PhysicsProcess,
        Input,
        UnhandledInput,
        Step
    }

    [GlobalClass, Icon("res://addons/state_chart_sharp/icon/Transition.svg")]
    public partial class Transition : Node
    {
        /// <summary>
        /// Time on which the transition is checked.
        /// </summary>

        #region define signals

        [Signal] public delegate void GuardEventHandler(Transition t);
        
        #endregion

        #region define properties

        // If transitEvent is null, transition is auto (checked on state enter)
        [Export] public StringName TransitionEvent { get; protected set; }
        [Export] protected Array<State> TargetStatesArray;
        protected List<State> TargetStates { get; set; }
        public State SourceState { get; protected set; }
        public State _lccaState;
        public State LccaState
        {
            get
            {
                if (_lccaState is null)
                {
                    ComputeTransition();
                }
                return _lccaState;
            }
            protected set { _lccaState = value; }
        }
        public List<State> _enterStates;
        public List<State> EnterStates
        {
            get
            {
                if (_enterStates.Count == 0)
                {
                    ComputeTransition();
                }
                return _enterStates;
            }
            protected set { _enterStates = value; }
        }
        public StateChart HostStateChart { get; protected set; }
        public bool IsChecked { get; set; }
        public double Delta
        {
            get { return HostStateChart.Delta; }
        }
        public InputEvent GameInput
        {
            get { return HostStateChart.GameInput; }
        }

        #endregion

        public void Init(State sourceState)
        {
            SourceState = sourceState;
            HostStateChart = SourceState.HostStateChart;

            if (SourceState.ParentState is null)
            {
                GD.PushWarning(Name, ": root state need no Transition.");
                return;
            }

            // Convert GD collection to CS collection
            TargetStates = new List<State>(TargetStatesArray);
            EnterStates = new List<State>();
        }

        public bool Check()
        {
            // Transition condition is not met in default.
            IsChecked = false;
            EmitSignal(SignalName.Guard, this);
            return IsChecked;
        }

        protected void ComputeTransition()
        {
            /*
                TODO:
                    1. Find LCCA(Least Common Compound Ancestor) of source and targets.
                    2. Record path from LCCA to targets in EnterStates, order: parent first, then reversed children
            */
        }
    }
}