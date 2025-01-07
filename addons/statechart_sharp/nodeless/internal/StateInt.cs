using System;
using System.Collections.Generic;

namespace LGWCP.Godot.StatechartSharp.Nodeless.Internal;

public enum DeduceDescendantsModeEnum : int
{
    Initial,
    History,
    DeepHistory
}

public abstract class StateInt<TDuct, TEvent> : Composition<TDuct, TEvent>
    where TDuct : IStatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{
    protected delegate void EnterEvent(TDuct duct);
    protected event EnterEvent Enter;
    protected delegate void ExitEvent(TDuct duct);
    protected event ExitEvent Exit;

    protected StateInt<TDuct, TEvent> InitialState;
    public StateInt<TDuct, TEvent> ParentState;
    public List<StateInt<TDuct, TEvent>> Substates;
    protected Dictionary<TEvent, List<TransitionInt<TDuct, TEvent>>> Transitions = new();
    protected List<TransitionInt<TDuct, TEvent>> AutoTransitions = new();
    protected Dictionary<TEvent, List<ReactionInt<TDuct, TEvent>>> Reactions = new();
    public StateInt<TDuct, TEvent> LowerState;
    public StateInt<TDuct, TEvent> UpperState;
    public int SubstateIdx; // The index of this state enlisted in parent state.

    public override void Setup() {}

    public virtual void Setup(StatechartInt<TDuct, TEvent> hostStatechart, ref int orderId, int substateIdx)
    {

    }

    public void StateEnter(TDuct duct)
    {
        ParentState?.HandleSubstateEnter(this);
        Enter.Invoke(duct);
    }

    public void StateExit(TDuct duct)
    {
        Exit.Invoke(duct);
    }

    public bool IsAncestorStateOf(StateInt<TDuct, TEvent> state)
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

    public virtual void SubmitActiveState(SortedSet<StateInt<TDuct, TEvent>> activeStates) {}

    public virtual bool IsConflictToEnterRegion(StateInt<TDuct, TEvent> substateToPend, SortedSet<StateInt<TDuct, TEvent>> enterRegionUnextended)
    {
        return false;
    }

    public virtual int SelectTransitions(SortedSet<TransitionInt<TDuct, TEvent>> enabledTransitions, TEvent @event)
    {
        return 1;
    }

    public virtual void ExtendEnterRegion(SortedSet<StateInt<TDuct, TEvent>> enterRegion, SortedSet<StateInt<TDuct, TEvent>> enterRegionEdge, SortedSet<StateInt<TDuct, TEvent>> extraEnterRegion, bool needCheckContain) {}

    public virtual void DeduceDescendants(SortedSet<StateInt<TDuct, TEvent>> deducedSet, bool isHistory, bool isEdgeState) {}

    public virtual void HandleSubstateEnter(StateInt<TDuct, TEvent> substate) {}

    public virtual void SelectReactions(SortedSet<ReactionInt<TDuct, TEvent>> enabledReactions, TEvent @event)
    {
        if (Reactions.TryGetValue(@event, out var eventReactions))
        {
            foreach (var a in eventReactions)
            {
                enabledReactions.Add(a);
            }
        }
    }

    public virtual void SaveAllStateConfig(List<int> snapshot) {}

    public virtual void SaveActiveStateConfig(List<int> snapshot) {}

    public virtual int LoadAllStateConfig(int[] config, int configIdx) { return configIdx; }

    public virtual int LoadActiveStateConfig(int[] config, int configIdx) { return configIdx; }
}
