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

        // If transitEvent is null, transition is auto (checked on state enter)
        [Export] public StringName TransitionEvent { get; protected set; }
        [Export] protected Array<Transition> TargetStatesArray;
        public List<Transition> TargetStates { get; protected set; }
        public State SourceState { get; protected set; }
        public StateChart StateChart { get; protected set; }
        public bool IsChecked { get; set; }
        public double Delta
        {
            get { return StateChart.Delta; }
        }
        public InputEvent GameInput
        {
            get { return StateChart.GameInput; }
        }

        public void Init(State sourceState)
        {
            SourceState = sourceState;

            if (SourceState.ParentState is null)
            {
                GD.PushWarning(Name, ": root state need no Transition.");
                return;
            }

            // TODO: check is this available
            TargetStates = new List<Transition>(TargetStatesArray);
        }

        public bool Check()
        {

            if (SourceState.ParentState is null || TargetState is null)
            {
                return false;
            }
            
            if (SourceState.StateChart != TargetState.StateChart)
            {
                GD.PushWarning(Name, ": target state must be in same statechart");
                return false;
            }

            // Transition condition is not met in default.
            IsChecked = false;
            EmitSignal(SignalName.Guard, this);
            return IsChecked;
        }

        public State GetToState()
        {
            return TargetState;
        }

        public StringName GetTransitionEvent()
        {
            return TransitionEvent;
        }
    }
}