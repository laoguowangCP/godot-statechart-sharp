using System;
using System.Collections.Generic;

namespace LGWCP.Godot.StatechartSharp.Nodeless.Internal;

public enum DeduceDescendantsModeEnum : int
{
    Initial,
    History,
    DeepHistory
}

public partial class StatechartInt<TDuct, TEvent>
    where TDuct : StatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{

public abstract class StateInt : Composition
{
    public Action<TDuct>[] Enters;
    public Action<TDuct>[] Exits;
    public StateInt ParentState;
    public StateInt[] Substates;
    protected TEvent[] KTransitions;
    protected TransitionInt[][] VTransitions;
    protected TransitionInt[] AutoTransitions;
    protected TEvent[] KReactions;
    protected ReactionInt[][] VReactions;
    public StateInt LowerState;
    public StateInt UpperState;
    public int SubstateIdx; // The index of this state enlisted in parent state.

    public override void Setup(
        StatechartInt<TDuct, TEvent> hostStatechart,
        StatechartBuilder<TDuct, TEvent>.BuildComposition buildComp,
        ref int orderId)
    {
        Setup(hostStatechart, buildComp, ref orderId, 0);
    }

    public virtual void Setup(
        StatechartInt<TDuct, TEvent> hostStatechart,
        StatechartBuilder<TDuct, TEvent>.BuildComposition buildComp,
        ref int orderId,
        int substateIdx)
    {
        base.Setup(hostStatechart, buildComp, ref orderId);

        // Get parent state
        var pComp = buildComp._PComp;
        if (pComp is not null)
        {
            if (pComp._CompInt is StateInt parentState)
            {
                ParentState = parentState;
            }
        }

        SubstateIdx = substateIdx;
    }

    public void StateEnter(TDuct duct)
    {
        ParentState?.HandleSubstateEnter(this);
        foreach (var action in Enters)
        {
            action(duct);
        }
    }

    public void StateExit(TDuct duct)
    {
        foreach (var action in Exits)
        {
            action(duct);
        }
    }

    public bool IsAncestorStateOf(StateInt state)
    {
        int id = state.OrderId;

        // Leaf state
        if (LowerState is null || UpperState is null)
        {
            return false;
        }

        return id >= LowerState.OrderId
            && id <= UpperState.OrderId;
    }

    public virtual void SubmitActiveState(Func<StateInt, bool> submit) {}

    public virtual bool IsConflictToEnterRegion(StateInt substateToPend, SortedSet<StateInt> enterRegionUnextended)
    {
        return false;
    }

    public virtual int SelectTransitions(SortedSet<TransitionInt> enabledTransitions, TEvent @event)
    {
        return 1;
    }

    public virtual void ExtendEnterRegion(SortedSet<StateInt> enterRegion, SortedSet<StateInt> enterRegionEdge, SortedSet<StateInt> extraEnterRegion, bool needCheckContain) {}

    public virtual void DeduceDescendants(SortedSet<StateInt> deducedSet, bool isHistory, bool isEdgeState) {}

    public virtual void HandleSubstateEnter(StateInt substate) {}

    public virtual void SelectReactions(SortedSet<ReactionInt> enabledReactions, TEvent @event)
    {

        for (int i = 0; i < KReactions.Length; ++i)
        {
            if (KReactions[i].Equals(@event))
            {
                foreach (var a in VReactions[i])
                {
                    enabledReactions.Add(a);
                }
                break;
            }
        }
    }

    public virtual void SaveAllStateConfig(List<int> snapshot) {}

    public virtual void SaveActiveStateConfig(List<int> snapshot) {}

    public virtual int LoadAllStateConfig(int[] config, int configIdx) { return configIdx; }

    public virtual int LoadActiveStateConfig(int[] config, int configIdx) { return configIdx; }

    public abstract bool IsValidState();
}

}
