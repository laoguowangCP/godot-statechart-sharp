using Godot;


namespace LGWCP.GodotPlugin.StateChartSharp
{
    public partial class TransitionNode : Node
    {
        [Export] protected StateNode toState;
        /// <summary>
        /// Time on which the transition is checked.
        /// </summary>
        [Export] public TransitionModeEnum transitionMode = TransitionModeEnum.Process;
        // protected StateNode fromState;
        [Signal] public delegate void TransitionCheckEventHandler(TransitionNode transition);
        private bool _isChecked;

        public enum TransitionModeEnum : int
        {
            Process,
            PhysicsProcess,
            Input,
            UnhandledInput
        }
        
        /// <summary>
        /// Check whether the condition is met.
        /// </summary>
        public void SetChecked(bool isChecked)
        {
            _isChecked = isChecked;
        }

        /// <summary>
        /// Call delegated check handler, 
        /// return weather transition condition is met.
        /// </summary>
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
        public bool CheckWithTransit(StateNode parentState)
        {
            // Transition condition is not met in default.
            _isChecked = false;
            EmitSignal(SignalName.TransitionCheck, this);
            
            // Commit the transition.
            if (_isChecked)
            {
                parentState.currentSubstate.StateExit();
                parentState.currentSubstate = toState;
                parentState.currentSubstate.StateEnter();
            }

            return _isChecked;
        }
    }
}