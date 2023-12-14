using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Godot;

namespace LGWCP.GodotPlugin.StatechartSharp
{
    public class CompondComponent : StateComponent
    {
        public CompondComponent(State state) : base(state) {}

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
            SortedSet<State> descendantInRegion = enterRegion.GetViewBetween(HostState, );
        }
    }
}