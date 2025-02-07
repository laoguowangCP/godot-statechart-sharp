using System;
using System.Collections.Generic;

namespace LGWCP.Godot.StatechartSharp.Nodeless.Internal;

public partial class StatechartInt<TDuct, TEvent>
    where TDuct : StatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{

public class HistoryInt : StateInt
{
    public HistoryInt(StatechartBuilder<TDuct, TEvent>.History state)
    {
        state._CompInt = this;
    }

    public override bool IsValidState()
    {
        return false;
    }

    /*
    public override void Setup(
        StatechartInt<TDuct, TEvent> hostStatechart,
        StatechartBuilder<TDuct, TEvent>.BuildComposition buildComp,
        ref int orderId,
        int substateIdx)
    {
        base.Setup(hostStatechart, buildComp, ref orderId, substateIdx);
    }*/

    public override void ExtendEnterRegion(
        SortedSet<StateInt> enterRegion,
        SortedSet<StateInt> enterRegionEdge,
        SortedSet<StateInt> extraEnterRegion,
        bool needCheckContain)
    {
        enterRegion.Remove(this);
        enterRegionEdge.Add(this);
    }

    public override void DeduceDescendants(Func<StateInt, bool> submitDeducedSet)
    {
        /*
        History state(s) in region edge start the deduction:
            1. Parent can be compound or parallel
            2. Let parent handles sibling(s) of this history state
            3. Should not be called recursively by other states
        */
        ParentState.DeduceDescendantsRecur(submitDeducedSet, DeduceDescendantsModeEnum.History);
    }

    /*
    public override void DeduceDescendantsRecur(
        Func<StateInt, bool> submitDeducedSet,
        DeduceDescendantsModeEnum deduceMode)
    {
        return;
    }*/
}

}
