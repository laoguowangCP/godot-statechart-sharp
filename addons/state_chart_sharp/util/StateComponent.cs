using Godot;
using System.Collections.Generic;

namespace LGWCP.GodotPlugin.StateChartSharp
{
    public class StateComponent
    {
        // TODO: Move pstate, cstate functions here
        // Use components instead of derivatives
        // Q: How nodes icon differs in Godot editor?
        // A: Use "C_", "P_" prefix to name state node.

        protected State state;
        
        public List<Transition> processTrans;
        public List<Transition> physicsProcessTrans;
        public List<Transition> inputTrans;
        public List<Transition> unhandledInputTrans;
        public List<Transition> stepTrans;

        protected List<Transition> GetTransitions(TransitionModeEnum mode) => mode switch
        {
            TransitionModeEnum.Process
                => processTrans,
            TransitionModeEnum.PhysicsProcess
                => physicsProcessTrans,
            TransitionModeEnum.Input
                => inputTrans,
            TransitionModeEnum.UnhandledInput
                => unhandledInputTrans,
            TransitionModeEnum.Step
                => stepTrans,
            _ => null
        };

        public StateComponent(State state)
        {
            this.state = state;

            // Initialize transition lists
            processTrans = new List<Transition>();
            physicsProcessTrans = new List<Transition>();
            inputTrans = new List<Transition>();
            unhandledInputTrans = new List<Transition>();
            stepTrans = new List<Transition>();
        }

        public virtual void Init(StateChart stateChart, State parentState = null) {}

        public virtual void SubstateTransit(TransitionModeEnum mode) {}

        public virtual void StateEnter() {}
        
        public virtual void StateExit() {}

        public virtual void StateInput(InputEvent @event) {}

        public virtual void StateUnhandledInput(InputEvent @event) {}

        public virtual void StateProcess(double delta) {}

        public virtual void StatePhysicsProcess(double delta) {}
    }
}