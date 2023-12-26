using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Godot;

namespace LGWCP.StatechartSharp
{
    public class CompondComponent : StateComponent
    {
        protected State CurrentState { get => HostState.CurrentState; }
        protected State InitialState { get => HostState.InitialState; set { HostState.InitialState = value; } }
        
        public CompondComponent(State state) : base(state) {}

        internal override void Init(Statechart hostStateChart, ref int ancestorId)
        {
            base.Init(hostStateChart, ref ancestorId);

            // Init & collect states, transitions, actions
            // Get lower-state and upper-state
            State lastSubstate = null;
            foreach (Node child in HostState.GetChildren())
            {
                if (child is State s)
                {
                    s.Init(hostStateChart, ref ancestorId);
                    Substates.Add(s);

                    // First substate is lower-state
                    if (LowerState == null)
                    {
                        LowerState = s;
                    }
                    lastSubstate = s;
                }
                else if (child is Transition t)
                {
                    // Root state should not have transition
                    if (ParentState == null)
                    {
                        continue;
                    }
                    t.Init(hostStateChart, ref ancestorId);
                    Transitions.Add(t);
                }
                else if (child is Action a)
                {
                    a.Init(hostStateChart, ref ancestorId);
                    Actions.Add(a);
                }
            }

            if (lastSubstate != null)
            {
                if (lastSubstate.UpperState != null)
                {
                    // Last substate's upper is upper-state
                    UpperState = lastSubstate.UpperState;
                }
                else
                {
                    // Last substate is upper-state
                    UpperState = lastSubstate;
                }
            }
            // Else state is atomic, lower and upper are null

            // Set initial-state
            if (InitialState != null)
            {
                // Check selected initial-state is substate
                if (InitialState.ParentState != HostState)
                {
                    #if DEBUG
                    GD.PushWarning(HostState.GetPath(), ": initial-state should be a substate.");
                    #endif
                    HostState.InitialState = null;
                }

                // Check selected initial-state is not history
                if (InitialState.StateMode == StateModeEnum.History)
                {
                    #if DEBUG
                    GD.PushWarning(HostState.GetPath(), @": initial-state should not be history.");
                    #endif
                    HostState.InitialState = null;
                }
            }

            // No assigned initial-state, use first non-history substate
            if (HostState.InitialState == null)
            {
                foreach (State s in HostState.Substates)
                {
                    if (s.StateMode != StateModeEnum.History)
                    {
                        HostState.InitialState = s;
                        break;
                    }
                }
            }

            // Set current-state
            HostState.CurrentState = HostState.InitialState;
        }

        internal override void PostInit()
        {
            foreach (State s in HostState.Substates)
            {
                s.PostInit();
            }

            foreach (Transition t in HostState.Transitions)
            {
                t.PostInit();
            }

            foreach (Action a in HostState.Actions)
            {
                a.PostInit();
            }
        }

        public override bool IsConflictToEnterRegion(State newSubstate, SortedSet<State> enterRegion)
        {
            bool isConflict = false;
            /*
            Conflicts if:
                (0. It is a compond state)
                1. State exists in enter-region.
                2. Another substate is already exist in enter-region.
            */
            if (enterRegion.Contains(HostState))
            {
                foreach (State s in HostState.Substates)
                {
                    if (s == newSubstate)
                    {
                        continue;
                    }
                    
                    if (enterRegion.Contains(s))
                    {
                        isConflict = true;
                        break;
                    }
                }
            }
            return isConflict;
        }

        public override void ExtendEnterRegion(
            SortedSet<State> enterRegion,
            SortedSet<State> enterRegionEdge,
            SortedSet<State> extraEnterRegion,
            bool needCheckContain)
        {
            if (HostState.Substates.Count == 0)
            {
                return;
            }

            /*
            Check if any substate in enter-region (if need check contain):
                1. If so, extend this substate (still need check contain)
                2. Else, add and extend initial-state (descendants need no check)
            */
            if (needCheckContain)
            {
                foreach (State s in HostState.Substates)
                {
                    if (enterRegion.Contains(s))
                    {
                        s.ExtendEnterRegion(enterRegion, enterRegionEdge, extraEnterRegion, true);
                        return;
                    }
                }
            }

            extraEnterRegion.Add(HostState.InitialState);
            HostState.InitialState.ExtendEnterRegion(enterRegion, enterRegionEdge, extraEnterRegion, false);
        }
        
        public override void RegisterActiveState(SortedSet<State> activeStates)
        {
            activeStates.Add(HostState);
            if (CurrentState != null)
            {
                CurrentState.RegisterActiveState(activeStates);
            }
        }

        public override bool SelectTransitions(List<Transition> enabledTransitions, StringName eventName)
        {
            bool isHandled = false;
            if (HostState.CurrentState != null)
            {
                isHandled = HostState.CurrentState.SelectTransitions(enabledTransitions, eventName);
            }

            if (isHandled)
            {
                // Descendants have registered an enabled transition
                return true;
            }
            else
            {
                // No transition enabled in descendants, or atomic state
                return base.SelectTransitions(enabledTransitions, eventName);
            }
        }

        public override void DeduceDescendants(SortedSet<State> deducedSet, bool isHistory)
        {
            State deducedSubstate;
            if (isHistory)
            {
                deducedSubstate = HostState.CurrentState;
            }
            else
            {
                deducedSubstate = HostState.InitialState;
            }

            if (deducedSubstate != null)
            {
                deducedSet.Add(deducedSubstate);
                deducedSubstate.DeduceDescendants(deducedSet, isHistory);
            }
        }

        public override void HandleSubstateEnter(State substate)
        {
            HostState.CurrentState = substate;
        }
    }
}