using Godot;
using System.Collections.Generic;


namespace LGWCP.Godot.StatechartSharp;

public class DeepHistoryImpl : StateImpl
{
    public DeepHistoryImpl(State state) : base(state) {}

    public override bool _IsValidState()
    {
        return false;
    }

    public override void _Setup(Statechart hostStateChart, ref int parentOrderId, int substateIdx)
    {
        base._Setup(hostStateChart, ref parentOrderId, substateIdx);
    }

    public override bool _GetPromoteStates(List<State> states)
    {
        // History do not promote
        return false;
    }

    public override void _ExtendEnterRegion(
        SortedSet<State> enterRegion,
        SortedSet<State> enterRegionEdge,
        SortedSet<State> extraEnterRegion,
        bool needCheckContain)
    {
        enterRegion.Remove(HostState);
        enterRegionEdge.Add(HostState);
    }

    public override void _DeduceDescendants(
        SortedSet<State> deducedSet)
    {
        /*
        History state(s) in region edge start the deduction:
            1. Parent can be compound or parallel
            2. Let parent handles sibling(s) of this history state
            3. Should not be called recursively by other states
            4. Parse IsDeepHistory in IsHistory arg
        */
        ParentState._DeduceDescendantsRecurr(deducedSet, DeduceDescendantsModeEnum.DeepHistory);
    }

    public override void _DeduceDescendantsRecurr(
        SortedSet<State> deducedSet, DeduceDescendantsModeEnum deduceMode)
    {
        return;
    }

#if TOOLS
    public override void _GetConfigurationWarnings(List<string> warnings)
    {
        // Check parent
        bool isParentWarning = true;
        Node parent = HostState.GetParentOrNull<Node>();
        if (parent != null)
        {
            if (parent is State state)
            {
                isParentWarning = !state._IsValidState();
            }
        }

        if (isParentWarning)
        {
            warnings.Add("History state should be child to a non-history state.");
        }

        // Check children
        if (HostState.GetChildren().Count > 0)
        {
            warnings.Add("History state should not have child.");
        }
    }

#endif
}
