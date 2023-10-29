using Godot;
using System.Collections.Generic;

namespace LGWCP.GodotPlugin.StateChartSharp
{
    public class CompondStateComponent : StateComponent
    {
        protected List<State> substates;
        protected State currentSubstate = null;
        protected State defaultSubstate = null;

        public CompondStateComponent(State state) : base(state)
        {
            // Initialize substate list
            substates = new List<State>();
        }

        public override void Init(StateChart stateChart,  State parentState)
        {
            // base.Init(stateChart, parentState);
            
            // Load child nodes
            foreach (Node child in state.GetChildren())
            {
                if (child is State substate)
                {
                    substate.Init(stateChart, state);
                    substates.Add(substate);
                }
                else if (child is Transition t)
                {
                    t.Init(state);
                    GetTransitions(t.GetTransitionMode()).Add(t);
                }
                else
				{
					GD.PushWarning("LGWCP.GodotPlugin.StateChartSharp: Child should be State or Transition.");
				}
            }

            if (substates.Count > 0)
            {
                // First as default substate
                defaultSubstate = substates[0];
            }
        }

        public override void SubstateTransit(
            TransitionModeEnum mode,
            State fromState,
            State toState,
            bool recursive)
        {
            bool doTransit = false;

            if (fromState is not null && toState is not null && fromState != toState)
            {
                // handle remote transition
                fromState.StateExit();
                currentSubstate = toState;
                currentSubstate.StateEnter(mode);
                return;
            }
            
            // fromState is null || toState is null
            // normal transition
            if (currentSubstate is null)
            {
                return;
            }

            fromState = currentSubstate;
            toState = null;
            
            var transitions = currentSubstate.GetTransitions(mode);
            foreach(Transition t in transitions)
            {
                if (t.Check())
                {
                    toState = t.GetToState();
                    doTransit = true;
                    break;
                }
            }

            if (doTransit)
            {
                // cross level transit
                while (fromState.ParentState != toState.ParentState)
                {
                    if (fromState.StateLevel >= toState.StateLevel)
                    {
                        fromState = fromState.ParentState;
                    }
                    else // from.StateLevel < to.StateLevel
                    {
                        toState = toState.ParentState;
                    }
                }

                // from.ParentState == to.ParentState
                if (fromState.ParentState == state)
                {
                    // Local transit
                    if (fromState != toState)
                    {
                        fromState.StateExit();
                        currentSubstate = toState;
                        currentSubstate.StateEnter(mode);
                    }
                }
                else if (fromState.ParentState.GetStateMode() == StateModeEnum.Compond)
                {
                    // remote transit
                    fromState.ParentState.SubstateTransit(mode, fromState, toState);
                }
                else
                {
                    // invalid transit
                    doTransit = false;
                }
            }
            
            // if no transit, do recurse 
            if (!doTransit)
            {
                if (recursive)
                {
                    currentSubstate.SubstateTransit(mode);
                }
                else
                {
                    currentSubstate.StateEnter(mode, checkInstant: false);
                }
                return;
            }
        }

        public override void StateEnter()
        {
            state.EmitSignal(State.SignalName.Enter);
            if (defaultSubstate is not null)
            {
                // First child state is default substate
                currentSubstate = defaultSubstate;
                currentSubstate.StateEnter();
            }
        }

        public override void StateEnter(TransitionModeEnum mode, bool checkInstant = true)
        {
            state.EmitSignal(State.SignalName.Enter);
            // handle instant state transition
            // no instant transition on init()
            if (checkInstant && state.IsInstant())
            {
                state.ParentState.SubstateTransit(mode, recursive: false);
                return;
            }

            if (defaultSubstate is not null)
            {
                // First child state is default substate
                currentSubstate = defaultSubstate;
                currentSubstate.StateEnter(mode);
            }
        }

        public override void StateExit()
        {
            if (currentSubstate is not null)
            {
                currentSubstate.StateExit();
                currentSubstate = null;
            }
            state.EmitSignal(State.SignalName.Exit);
        }

        public override void StateInput(InputEvent @event)
        {
            state.EmitSignal(State.SignalName.Input, @event);
            currentSubstate?.StateInput(@event);
        }

        public override void StateUnhandledInput(InputEvent @event)
        {
            state.EmitSignal(State.SignalName.UnhandledInput, @event);
            currentSubstate?.StateUnhandledInput(@event);
        }

        public override void StateProcess(double delta)
        {
            state.EmitSignal(State.SignalName.Process, delta);
            currentSubstate?.StateProcess(delta);
        }

        public override void StatePhysicsProcess(double delta)
        {
            state.EmitSignal(State.SignalName.PhysicsProcess, delta);
            currentSubstate?.StatePhysicsProcess(delta);
        }
    }
}