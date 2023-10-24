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

    [GlobalClass]
    public partial class Transition : Node
    {
        protected State fromState;
        [Export] protected State toState;
        /// <summary>
        /// Time on which the transition is checked.
        /// </summary>
        [Export] protected TransitionModeEnum transitionMode = TransitionModeEnum.Process;
        // protected StateNode fromState;
        [Signal] public delegate void TransitionCheckEventHandler(Transition transition);
        private bool _isChecked;

        public void Init(State parent)
        {
            fromState = parent;

            if (toState is null)
            {
                GD.PushWarning("LGWCP.GodotPlugin.StateChartSharp: Transition needs an assigned 'toState'.");
                return;
            }
            if (toState.IsInstant() && fromState.IsInstant())
            {
                GD.PushWarning("LGWCP.GodotPlugin.StateChartSharp: Both 'fromState' and 'toState' are instant, loop may happen in instant transition.");
            }
        }
        
        /// <summary>
        /// Check whether the condition is met.
        /// </summary>
        public void SetChecked(bool isChecked)
        {
            _isChecked = isChecked;
        }

        public bool Check()
        {
            // Transition condition is not met in default.
            _isChecked = false;

            EmitSignal(SignalName.TransitionCheck, this);
            return _isChecked;
        }

        /// <summary>
        /// Call delegated check handler, 
        /// return weather transition condition is met.</br>
        /// If condition is met, commit the transition for parent state.
        /// </summary>
        public bool CheckWithTransit(State parentState)
        {
            if (toState is null)
            {
                return false;
            }
            
            // Transition condition is not met in default.
            _isChecked = false;
            EmitSignal(SignalName.TransitionCheck, this);
            
            // Commit the transition.
            if (_isChecked)
            {
                fromState.StateExit();
                toState.StateEnter();
                parentState.currentSubstate = toState;
            }

            return _isChecked;
        }

        public TransitionModeEnum GetTransitionMode()
        {
            return transitionMode;
        }
    }
}