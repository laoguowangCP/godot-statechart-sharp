using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Godot;

namespace LGWCP.StatechartSharp
{
    public class CompondComponent : StateComponent
    {
        public CompondComponent(State state) : base(state) {}

        public override void Init(Statechart hostStateChart, ref int ancestorId)
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
                    HostState.Substates.Add(s);

                    // First substate is lower-state
                    if (HostState.LowerState == null)
                    {
                        HostState.LowerState = s;
                    }
                    lastSubstate = s;
                }
                else if (child is Transition t)
                {
                    // Root state should not have transition
                    if (HostState.ParentState == null)
                    {
                        continue;
                    }
                    t.Init(hostStateChart, ref ancestorId);
                    HostState.Transitions.Add(t);
                }
                else if (child is Action a)
                {
                    a.Init(hostStateChart, ref ancestorId);
                    HostState.Actions.Add(a);
                }
            }

            if (lastSubstate != null)
            {
                State upper = lastSubstate.UpperState;
                if (upper != null)
                {
                    // Last substate's upper is upper-state
                    HostState.UpperState = upper;
                }
                else
                {
                    // Last substate is upper-state
                    HostState.UpperState = lastSubstate;
                }
            }
            // Else state is atomic, lower and upper are null

            // Set initial-state
            if (HostState.InitialState != null)
            {
                // Check selected initial-state is substate
                if (HostState.InitialState.ParentState != HostState)
                {
                    GD.PushWarning(HostState.GetPath(), ": initial-state should be a substate.");
                    HostState.InitialState = null;
                }

                // Check selected initial-state is not history
                if (HostState.InitialState.StateMode == StateModeEnum.History)
                {
                    GD.PushWarning(HostState.GetPath(), ": initial-state should not be history.");
                    HostState.InitialState = null;
                }
            }

            // No selected initial-state, use first non-history substate
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

        public override bool SelectTransitions(StringName eventName)
        {
            bool isHandled = false;
            if (HostState.CurrentState != null)
            {
                isHandled = HostState.CurrentState.SelectTransitions(eventName);
            }

            if (isHandled)
            {
                // Descendants have registered an enabled transition
                return true;
            }
            else
            {
                // No transition enabled in descendants, or atomic state
                return base.SelectTransitions(eventName);
            }
        }

        public override void DeduceDescendants(SortedSet<State> deducedSet, bool isDeepHistory)
        {
            State initialState;
            if (isDeepHistory)
            {
                initialState = HostState.CurrentState;
            }
            else
            {
                initialState = HostState.InitialState;
            }

            if (initialState != null)
            {
                deducedSet.Add(initialState);
                initialState.DeduceDescendants(deducedSet, isDeepHistory);
            }
        }

        public override void ExtendEnterRegion(SortedSet<State> enterRegion, SortedSet<State> enterRegionEdge, SortedSet<State> extraEnterRegion)
        {
            if (HostState.Substates.Count == 0)
            {
                return;
            }

            // TODO: get lower descendant and upper descendants
            // SortedSet<State> descendantInRegion = enterRegion.GetViewBetween(HostState, );
        }
    }
}