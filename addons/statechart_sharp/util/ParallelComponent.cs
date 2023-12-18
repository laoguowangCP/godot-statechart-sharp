using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Godot;

namespace LGWCP.GodotPlugin.StatechartSharp
{
    public class ParallelComponent : StateComponent
    {
        public ParallelComponent(State state) : base(state) {}

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
                    if (s.StateMode == StateModeEnum.History)
                    {
                        GD.PushWarning(HostState.GetPath(), ": parallel-state should not have history as substate.");
                        continue;
                    }
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
        }

        public override bool SelectTransitions(StringName eventName)
        {
            bool isHandled = false;
            if (HostState.Substates.Count > 0)
            {
                isHandled = true;
                foreach (State s in HostState.Substates)
                {
                    isHandled = isHandled && s.SelectTransitions(eventName);
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
                return base.SelectTransitions(eventName);
            }
        }

        public override void DeduceDescendants(SortedSet<State> deducedSet, bool isDeepHistory)
        {
            foreach (State s in HostState.Substates)
            {
                deducedSet.Add(s);
                s.DeduceDescendants(deducedSet, isDeepHistory);
            }
        }
    }
}