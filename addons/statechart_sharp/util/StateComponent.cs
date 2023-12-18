using Godot;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;

namespace LGWCP.GodotPlugin.StatechartSharp
{
    public class StateComponent
    {
        protected State HostState;
        protected Statechart HostStatechart
        {
            get => HostState.HostStatechart;
        }
        public StateComponent(State state)
        {
            HostState = state;
        }

        public virtual void Init(Statechart hostStateChart, ref int ancestorId)
        {
            // Get parent state
            Node parent = HostState.GetParent<Node>();
            if (parent != null && parent is State)
            {
                HostState.ParentState = parent as State;
            }

            // Register in host-statechart
            HostStatechart.States.Add(HostState);
        }

        public virtual bool SelectTransitions(StringName eventName)
        {
            foreach (Transition t in HostState.Transitions)
            {
                if (t.EventName == eventName)
                {
                    t.Check();
                    if (t.IsEnabled)
                    {
                        HostStatechart.RegisterTransition(t);
                        return true;
                    }
                }
            }
            return false;
        }

        public virtual void DeduceDescendants(SortedSet<State> deducedSet, bool isHistory) {}
        public virtual void RefineEnterRegion(SortedSet<State> enterRegion, SortedSet<State> enterRegionEdge, SortedSet<State> extraEnterRegion) {}
    }
}