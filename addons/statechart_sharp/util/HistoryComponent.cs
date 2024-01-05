using Godot;
using System.Collections.Generic;

namespace LGWCP.StatechartSharp
{
    public class HistoryComponent : StateComponent
    {
        private bool IsDeepHistory { get => HostState.IsDeepHistory; }

        public HistoryComponent(State state) : base(state) {}

        internal override void Init(Statechart hostStateChart, ref int ancestorId)
        {
            base.Init(hostStateChart, ref ancestorId);
            
            // No substates, transitions, or actions
            #if DEBUG
            foreach (Node child in HostState.GetChildren())
            {
                GD.PushWarning(
                    HostState.GetPath(),
                    ": History state should not have child.");
                break;
            }
            #endif

            #if DEBUG
            if (ParentState.StateMode == StateModeEnum.Parallel && !IsDeepHistory)
            {
                GD.PushWarning(
                    HostState.GetPath(),
                    @": shallow history state under parallel has no function
                        , you may remove history or switch to deep history.");
            }
            #endif
        }

        internal override void ExtendEnterRegion(
            SortedSet<State> enterRegion,
            SortedSet<State> enterRegionEdge,
            SortedSet<State> extraEnterRegion,
            bool needCheckContain)
        {
            enterRegion.Remove(HostState);
            enterRegionEdge.Add(HostState);
        }
        
        internal override bool SelectTransitions(List<Transition> enabledTransitions, StringName eventName)
        {
            // Do nothing
            return false;
        }

        internal override void DeduceDescendants(
            SortedSet<State> deducedSet, bool isHistory, bool isEdgeState)
        {
            /*
            History-state start the deduction:
                1. Parent can be compound or parallel.
                2. Handle the sibling.
                3. Should not be called recursively by other states.
            */
            if (isEdgeState)
            {
                ParentState.DeduceDescendants(deducedSet, IsDeepHistory, isEdgeState: true);
            }
            
            // ParentState.DeduceDescendantsFromHistory(deducedSet, IsDeepHistory);

            #if DEBUG
            if (!isEdgeState)
            {
                GD.PushError(
                    HostState.GetPath(),
                    @"history's DeduceDescendants should only be called
                        from transition when deducing enter-region-edge."
                );
            }
            #endif
        }
    }
}