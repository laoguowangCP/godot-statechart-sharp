using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Godot;

namespace LGWCP.GodotPlugin.StatechartSharp
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
                // Check selected initial-state is available
                if (HostState.InitialState.ParentState != HostState)
                {
                    GD.PushWarning(HostState.GetPath(), ": initial-state should be a substate.");
                }
            }
            else
            {
                // No selected initial-state, use first non-history substate
                foreach (State s in HostState.Substates)
                {
                    if (s.StateMode != StateModeEnum.History)
                    {
                        HostState.InitialState = s;
                        break;
                    }
                }
            }
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

        public override void RefineEnterRegion(SortedSet<State> enterRegion, SortedSet<State> enterRegionEdge, SortedSet<State> extraEnterRegion)
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