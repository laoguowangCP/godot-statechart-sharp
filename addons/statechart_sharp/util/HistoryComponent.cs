using Godot;
using System.Collections.Generic;

namespace LGWCP.StatechartSharp;

public class HistoryComponent : StateComponent
{
    private bool IsDeepHistory { get => HostState.IsDeepHistory; }

    public HistoryComponent(State state) : base(state) {}

    internal override void Setup(Statechart hostStateChart, ref int ancestorId)
    {
        base.Setup(hostStateChart, ref ancestorId);
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
    }

    #if TOOLS
    internal override void GetConfigurationWarnings(List<string> warnings)
    {
        // Check parent
        bool isParentWarning = true;
        bool isParentParallel = false;
        Node parent = HostState.GetParent<Node>();
        if (parent != null)
        {
            if (parent is State state)
            {
                isParentWarning = state.IsHistory;
                isParentParallel = state.StateMode == StateModeEnum.Parallel;
            }
        }

        if (isParentWarning)
        {
            warnings.Add("History state should be child to a non-history state.");
        }

        if (isParentParallel && !IsDeepHistory)
        {
            warnings.Add("Parallel's shallow history is not recommended.");
        }

        // Check children
        if (HostState.GetChildren().Count > 0)
        {
            warnings.Add("History state should not have child.");
        }
    }
    #endif
}
