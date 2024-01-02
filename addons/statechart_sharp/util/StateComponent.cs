using Godot;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;

namespace LGWCP.StatechartSharp
{
    public class StateComponent
    {
        protected State HostState;
        protected Statechart HostStatechart { get => HostState.HostStatechart; }
        protected State ParentState
        {
            get => HostState.ParentState;
            set { HostState.ParentState = value; }
        }
        protected List<State> Substates { get => HostState.Substates; }
        protected State LowerState
        {
            get => HostState.LowerState;
            set { HostState.LowerState = value; }
        }
        protected State UpperState
        {
            get => HostState.UpperState;
            set { HostState.UpperState = value; }
        }
        protected List<Transition> Transitions { get => HostState.Transitions; }
        protected List<Reaction> Actions { get => HostState.Actions; }

        public StateComponent(State state)
        {
            HostState = state;
        }

        internal virtual void Init(Statechart hostStateChart, ref int ancestorId)
        {
            // Get parent state
            Node parent = HostState.GetParent<Node>();
            if (parent != null && parent is State)
            {
                ParentState = parent as State;
            }

            #if DEBUG
            if (ParentState == null && HostState != HostStatechart.RootState)
            {
                GD.PushWarning(
                    HostState.GetPath(),
                    ": non-root state should have parent-state.");
            }
            #endif

            // Register in host-statechart
            HostStatechart.RegisterState(HostState);
        }

        internal virtual void PostInit() {}

        internal virtual void RegisterActiveState(SortedSet<State> activeStates) {}

        internal virtual bool IsConflictToEnterRegion(State substate, SortedSet<State> enterRegion)
        {
            return false;
        }

        internal virtual bool SelectTransitions(List<Transition> enabledTransitions, StringName eventName)
        {
            foreach (Transition t in Transitions)
            {
                // t.EventName == null && eventName == null
                bool isEnabled = t.Check(eventName);
                if (isEnabled)
                {
                    enabledTransitions.Add(t);
                    return true;
                }
            }
            return false;
        }
        
        internal virtual void ExtendEnterRegion(SortedSet<State> enterRegion, SortedSet<State> enterRegionEdge, SortedSet<State> extraEnterRegion, bool needCheckContain) {}

        internal virtual void DeduceDescendants(SortedSet<State> deducedSet, bool isHistory, bool isEdgeState) {}

        internal virtual void HandleSubstateEnter(State substate) {}
    }
}