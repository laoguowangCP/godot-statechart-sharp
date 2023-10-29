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

            if (fromState.StateLevel < toState.StateLevel)
            {
                GD.PushWarning(Name, ": fromState should be deeper than toState.");
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

        public bool Check()
        {
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

        /*
        public bool CheckWithTransit(State parentState)
        {
            if (fromState.ParentState is null || toState is null)
            {
                return false;
            }

            // Transition condition is not met in default.
            _isChecked = false;
            EmitSignal(SignalName.TransitionCheck, this);

            currentSubstate.StateExit();
            currentSubstate = t.GetToState();
            currentSubstate.StateEnter(mode);

            return _isChecked;
        }

        protected bool Transit(State from, State to)
        {
            if (from.StateChart != to.StateChart)
            {
                GD.PushWarning("Target state must be in same statechart");
                return false;
            }

            while (from.ParentState != to.ParentState)
            {
                if (from.StateLevel > to.StateLevel)
                {
                    from = from.ParentState;
                }
                else // from.StateLevel <= to.StateLevel
                {
                    return false;
                }
            }

            // from.ParentState == to.ParentState
            if (from.ParentState is CompondState)
            {
                var innnerMostParent = from.ParentState as CompondState;
            }
            else
            {
                GD.PushWarning("Transition should happens under Compond State");
                return false;
            }

            return true;
        }*/

        public TransitionModeEnum GetTransitionMode()
        {
            return transitionMode;
        }
    }
}