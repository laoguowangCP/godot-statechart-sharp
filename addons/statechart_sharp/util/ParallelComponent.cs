using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Godot;

namespace LGWCP.StatechartSharp
{
    public class ParallelComponent : StateComponent
    {
        public ParallelComponent(State state) : base(state) {}

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
        }

        internal override void ExtendEnterRegion(
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
            Check if substates in enter-region (if need check contain):
                1. If so, extend this substate (still need check)
                2. Else, add and extend this substate (no need for check)
            */
            
            bool stillNeedCheck = false;
            foreach (State substate in Substates)
            {
                if (needCheckContain)
                {
                    stillNeedCheck = enterRegion.Contains(substate);
                }
                if (!stillNeedCheck)
                {
                    extraEnterRegion.Add(substate);
                }
                substate.ExtendEnterRegion(enterRegion, enterRegionEdge, extraEnterRegion, stillNeedCheck);
            }
        }

        internal override void PostInit()
        {
            foreach (State s in Substates)
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

        public override void RegisterActiveState(SortedSet<State> activeStates)
        {
            activeStates.Add(HostState);
            foreach (State substate in HostState.Substates)
            {
                substate.RegisterActiveState(activeStates);
            }
        }

        public override bool SelectTransitions(List<Transition> enabledTransitions, StringName eventName)
        {
            bool isHandled = false;
            if (Substates.Count > 0)
            {
                isHandled = true;
                foreach (State s in Substates)
                {
                    isHandled = isHandled && s.SelectTransitions(enabledTransitions, eventName);
                }
            }

            if (isHandled)
            {
                // Any descendant handled transition
                return true;
            }
            else
            {
                // Atomic state, or no transition enabled in descendants
                return base.SelectTransitions(enabledTransitions, eventName);
            }
        }

        internal override void DeduceDescendants(SortedSet<State> deducedSet, bool isHistory)
        {
            deducedSet.Add(HostState);
            foreach (State s in Substates)
            {
                // Pass history-states
                if (s.StateMode == StateModeEnum.History)
                {
                    continue;
                }
                s.DeduceDescendants(deducedSet, isHistory);
            }
        }

        internal override void DeduceDescendantsFromHistory(SortedSet<State> deducedSet, bool isDeepHistory)
        {
            foreach (State substate in Substates)
            {
                // Pass history-states
                if (substate.StateMode == StateModeEnum.History)
                {
                    continue;
                }
                substate.DeduceDescendants(deducedSet, HostState.IsDeepHistory);
            }
            
        }
    }
}