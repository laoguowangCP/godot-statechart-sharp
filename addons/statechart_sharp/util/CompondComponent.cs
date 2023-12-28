using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Godot;

namespace LGWCP.StatechartSharp
{
    public class CompondComponent : StateComponent
    {
        private State CurrentState
        {
            get => HostState.CurrentState;
            set { HostState.CurrentState = value; }
        }
        private State InitialState
        {
            get => HostState.InitialState;
            set { HostState.InitialState = value; }
        }
        
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
                    GD.PushWarning(
                        HostState.GetPath(),
                        ": initial-state should be a substate.");
                    #endif
                    InitialState = null;
                }

                // Check selected initial-state is not history
                if (InitialState.StateMode == StateModeEnum.History)
                {
                    #if DEBUG
                    GD.PushWarning(
                        HostState.GetPath(),
                        @": initial-state should not be history.");
                    #endif
                    InitialState = null;
                }
            }

            // No assigned initial-state, use first non-history substate
            if (InitialState == null)
            {
                foreach (State substate in Substates)
                {
                    if (substate.StateMode != StateModeEnum.History)
                    {
                        InitialState = substate;
                        break;
                    }
                }
            }

            // Set current-state
            CurrentState = InitialState;
        }

        internal override void PostInit()
        {
            foreach (State s in Substates)
            {
                s.PostInit();
            }

            foreach (Transition t in Transitions)
            {
                t.PostInit();
            }

            foreach (Action a in Actions)
            {
                a.PostInit();
            }
        }

        public override bool IsConflictToEnterRegion(
            State newSubstate, SortedSet<State> enterRegion)
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

        internal override void ExtendEnterRegion(
            SortedSet<State> enterRegion,
            SortedSet<State> enterRegionEdge,
            SortedSet<State> extraEnterRegion,
            bool needCheckContain)
        {
            /*
            if need check (state is in region, checked by parent):
                if any substate in region:
                    extend this very substate, still need check
                else (no substate in region)
                    extend initial state, need no check
            else (need no check)
                add state to extra-region
                extend initial state, need no check
            */

            // Need no check, add to extra
            if (!needCheckContain)
            {
                extraEnterRegion.Add(HostState);
            }

            // Need check
            if (needCheckContain)
            {
                foreach (State substate in Substates)
                {
                    if (enterRegion.Contains(substate))
                    {
                        substate.ExtendEnterRegion(
                            enterRegion, enterRegionEdge, extraEnterRegion);
                        return;
                    }
                }
            }

            // Need no check, or need check but no substate in region
            if (InitialState != null)
            {
                InitialState.ExtendEnterRegion(
                    enterRegion, enterRegionEdge, extraEnterRegion, false);
            }
        }
        
        public override void RegisterActiveState(SortedSet<State> activeStates)
        {
            activeStates.Add(HostState);
            if (CurrentState != null)
            {
                CurrentState.RegisterActiveState(activeStates);
            }
        }

        public override bool SelectTransitions(
            List<Transition> enabledTransitions, StringName eventName)
        {
            bool isHandled = false;
            if (HostState.CurrentState != null)
            {
                isHandled = CurrentState.SelectTransitions(
                    enabledTransitions, eventName);
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

        internal override void DeduceDescendants(
            SortedSet<State> deducedSet, bool isHistory, bool isEdgeState)
        {
            /* 
            If is edge-state:
                - It is called from history substate.
                - "IsHistory" argument represents "IsDeepHistory"
            */
            if (isEdgeState)
            {
                if (CurrentState != null)
                {
                    bool isDeepHistory = isHistory;
                    CurrentState.DeduceDescendants(deducedSet, isDeepHistory);
                }
                return;
            }

            // Not edge-state
            deducedSet.Add(HostState);
            State deducedSubstate = isHistory ? CurrentState : InitialState;

            if (deducedSubstate != null)
            {
                deducedSubstate.DeduceDescendants(deducedSet, isHistory);
            }
        }

        internal override void HandleSubstateEnter(State substate)
        {
            HostState.CurrentState = substate;
        }
    }
}