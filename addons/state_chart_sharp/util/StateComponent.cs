using Godot;
using System;
using System.Collections.Generic;

namespace LGWCP.GodotPlugin.StateChartSharp
{
    public class StateComponent
    {
        protected State state;
        
        public List<Transition> processTrans;
        public List<Transition> physicsProcessTrans;
        public List<Transition> inputTrans;
        public List<Transition> unhandledInputTrans;
        public List<Transition> stepTrans;

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

        public virtual void Init(StateChart stateChart, State parentState) {}
    }
}