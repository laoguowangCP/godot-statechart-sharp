using System;
using System.Collections.Generic;

namespace LGWCP.Godot.StatechartSharp.Nodeless;

public partial class Statechart<TDuct, TEvent>
    where TDuct : StatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{

public class History : State
{
    public History(Statechart<TDuct, TEvent> statechart) : base(statechart) {}

    public override bool _IsValidState() => false;

    public override void _Setup(ref int parentOrderId, int substateIdx)
    {
        base._Setup(ref parentOrderId, substateIdx);
    }

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
        */
        _ParentState._DeduceDescendantsRecur(deducedSet, DeduceDescendantsModeEnum.History);
    }

    public override void _DeduceDescendantsRecur(
        SortedSet<State> deducedSet, DeduceDescendantsModeEnum deduceMode)
    {
        return;
    }

    public override Composition Duplicate()
    {
        return new History(_HostStatechart);
    }
}

}
