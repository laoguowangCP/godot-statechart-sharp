using Godot;


namespace LGWCP.GodotPlugin.StateChartSharp
{
    [GlobalClass]
    public partial class Transition : IStateChartComponent
    {
        protected StateNode fromState;
        [Export] protected StateNode toState;
        /// <summary>
        /// Time on which the transition is checked.
        /// </summary>
        [Export] public TransitionModeEnum transitionMode = TransitionModeEnum.Process;
        // protected StateNode fromState;
        [Signal] public delegate void TransitionCheckEventHandler(Transition transition);
        private bool _isChecked;

        public enum TransitionModeEnum : int
        {
            Process,
            PhysicsProcess,
            Input,
            UnhandledInput
        }

        public override void Init()
        {
            var parent = GetParent<Node>();
            if (!(parent is StateNode))
            {
                GD.PushError("LGWCP.GodotPlugin.StateChartSharp: Transition needs StateNode as parent.");
                return;
            }

            fromState = parent as StateNode;

            if (toState is null)
            {
                GD.PushError("LGWCP.GodotPlugin.StateChartSharp: Transition needs an assigned 'toState'.");
                return;
            }
            if (toState.GetParent<Node>() != fromState.GetParent<Node>())
            {
                GD.PushError("LGWCP.GodotPlugin.StateChartSharp: 'toState' must be sibling of attached state.");
                return;
            }
            if (toState.IsInstant() && fromState.IsInstant())
            {
                GD.PushWarning("LGWCP.GodotPlugin.StateChartSharp: Both attached state and 'toState' are instant, loop may happen in instant transition.");
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
        public bool CheckWithTransit(StateNode parentState)
        {
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
    }
}