using Godot;
using System.Collections.Generic;


namespace LGWCP.StatechartSharp;

public class HistoryComponent : StateComponent
{
    private bool IsDeepHistory { get => HostState.IsDeepHistory; }

    public HistoryComponent(State state) : base(state) {}

    internal override void Setup(Statechart hostStateChart, ref int parentOrderId)
    {
        base.Setup(hostStateChart, ref parentOrderId);
    }

    internal override bool GetPromoteStates(List<State> states)
    {
        // History do not promote
        return false;
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
        History state(s) in region nedge start the deduction:
            1. Parent can be compound or parallel
            2. Let parent handles sibling(s) of this history state
            3. Should not be called recursively by other states
            4. Parse IsDeepHistory in IsHistory arg
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
        Node parent = HostState.GetParentOrNull<Node>();
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
            warnings.Add("Parallel's shallow history is of no avail. You may want parallel's deep history.");
        }

        // Check children
        if (HostState.GetChildren().Count > 0)
        {
            warnings.Add("History state should not have child.");
        }
    }
    #endif
}
