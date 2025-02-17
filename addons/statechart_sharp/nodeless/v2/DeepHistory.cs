using System;
using System.Collections.Generic;

namespace LGWCP.Godot.StatechartSharp.NodelessV2;

public partial class Statechart<TDuct, TEvent>
    where TDuct : StatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{

public class DeepHistory : State
{
    public DeepHistory() {}

    public override bool _IsValidState() => false;

    public override void _ExtendEnterRegion(
        SortedSet<State> enterRegion,
        SortedSet<State> enterRegionEdge,
        SortedSet<State> extraEnterRegion,
        bool needCheckContain)
    {
        enterRegion.Remove(this);
        enterRegionEdge.Add(this);
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
        _ParentState._DeduceDescendantsRecur(deducedSet, DeduceDescendantsModeEnum.DeepHistory);
    }

    public override void _DeduceDescendantsRecur(
        SortedSet<State> deducedSet, DeduceDescendantsModeEnum deduceMode)
    {
        return;
    }
}

}
