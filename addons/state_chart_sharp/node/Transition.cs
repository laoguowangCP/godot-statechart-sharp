using System.Collections.Generic;
using Godot;


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
        protected State fromState;
        [Export] protected State toState;
        /// <summary>
        /// Time on which the transition is checked.
        /// </summary>
        [Export] protected StringName transitEvent;
        [Export] protected bool isInstant = false;
        [Signal] public delegate void TransitionCheckEventHandler(Transition transition);
        private bool _isChecked;
        private InputEvent _inputEvent;
        private double _deltaTime;
        // private State _siblingFromState = null;

        public void Init(State parent)
        {
            fromState = parent;

            if (fromState.ParentState is null)
            {
                GD.PushWarning(Name, ": root state need no Transition.");
                return;
            }

            if (toState is null)
            {
                GD.PushWarning(Name, ": transition needs an assigned 'toState'.");
                return;
            }
        }
        
        /// <summary>
        /// Check whether the condition is met.
        /// </summary>
        public void SetChecked(bool isChecked)
        {
            _isChecked = isChecked;
        }

        public double GetDeltaTime()
        {
            return _deltaTime;
        }

        public InputEvent GetInputEvent()
        {
            return _inputEvent;
        }

        public bool IsInstant()
        {
            return isInstant;
        }

        public bool Check(double deltaTime = 0.0, InputEvent inputEvent = null)
        {
            _deltaTime = deltaTime;
            _inputEvent = inputEvent;

            if (fromState.ParentState is null || toState is null)
            {
                return false;
            }
            
            if (fromState.StateChart != toState.StateChart)
            {
                GD.PushWarning(Name, ": target state must be in same statechart");
                return false;
            }

            // Transition condition is not met in default.
            _isChecked = false;
            EmitSignal(SignalName.TransitionCheck, this);
            return _isChecked;
        }

        public State GetToState()
        {
            return toState;
        }

        public StringName GetTransitionEvent()
        {
            return transitEvent;
        }
    }
}