using Godot;
using System.Collections.Generic;


namespace LGWCP.Godot.StatechartSharp;


public class HistoryImpl : StateImpl
{
    public HistoryImpl(State state) : base(state) {}


    public override bool _IsValidState()
    {
        return false;
    }

    public override void _Setup(Statechart hostStatechart, ref int parentOrderId, int substateIdx)
    {
        base._Setup(hostStatechart, ref parentOrderId, substateIdx);
    }

    public override bool _SubmitPromoteStates(List<State> states)
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
        */
        ParentState._DeduceDescendantsRecur(deducedSet, DeduceDescendantsModeEnum.History);
    }

    public override void _DeduceDescendantsRecur(
        SortedSet<State> deducedSet, DeduceDescendantsModeEnum deduceMode)
    {
        return;
    }

#if TOOLS
    public override void _GetConfigurationWarnings(List<string> warnings)
    {
        // Check parent
        bool isParentWarning = true;
        bool isParentParallel = false;
        Node parent = HostState.GetParentOrNull<Node>();
        if (parent != null)
        {
            if (parent is State state)
            {
                isParentWarning = !state._IsValidState();
                isParentParallel = state.StateMode == StateModeEnum.Parallel;
            }
        }

        if (isParentWarning)
        {
            warnings.Add("History state should be child to a non-history state.");
        }

        if (isParentParallel)
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
