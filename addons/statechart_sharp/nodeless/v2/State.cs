using System;
using System.Collections.Generic;
using LGWCP.Util;

namespace LGWCP.Godot.StatechartSharp.NodelessV2;

public abstract class State<TDuct, TEvent> : Composition<TDuct, TEvent>
    where TDuct : StatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{
    public State<TDuct, TEvent> _ParentState;
    public State<TDuct, TEvent> _LowerState;
    public State<TDuct, TEvent> _UpperState;
    public int _SubstateIdx;
    public Action<TDuct>[] _Enters;
    public Action<TDuct>[] _Exits;
    public Composition<TDuct, TEvent>[] _Comps;
    public State<TDuct, TEvent>[] _Substates;
    protected Transition<TDuct, TEvent>[] AutoTransitions;
    protected TEvent[] KTransitions;
    protected Transition<TDuct, TEvent>[][] VTransitions;
    protected TEvent[] KReactions;
    protected Reaction<TDuct, TEvent>[][] VReactions;

    public void StateEnter(TDuct duct)
    {
        _ParentState?.HandleSubstateEnter(this);
        foreach (var action in _Enters)
        {
            action(duct);
        }
    }

    public void StateExit(TDuct duct)
    {
        foreach (var action in _Exits)
        {
            action(duct);
        }
    }

    public bool IsAncestorStateOf(State<TDuct, TEvent> state)
    {
        int id = state._OrderId;

        // Leaf state
        if (_LowerState is null || _UpperState is null)
        {
            return false;
        }

        return id >= _LowerState._OrderId
            && id <= _UpperState._OrderId;
    }

    public virtual void SubmitActiveState(SortedSet<State<TDuct, TEvent>> activeStates) {}

    public virtual bool IsConflictToEnterRegion(State<TDuct, TEvent> substateToPend, SortedSet<State<TDuct, TEvent>> enterRegionUnextended)
    {
        return false;
    }

    public virtual int SelectTransitions(SortedSet<Transition<TDuct, TEvent>> enabledTransitions, TEvent @event, TDuct duct)
    {
        return 1;
    }

    public virtual void ExtendEnterRegion(SortedSet<State<TDuct, TEvent>> enterRegion, SortedSet<State<TDuct, TEvent>> enterRegionEdge, SortedSet<State<TDuct, TEvent>> extraEnterRegion, bool needCheckContain) {}

    public virtual void DeduceDescendants(SortedSet<State<TDuct, TEvent>> deducedSet) {}

    public virtual void DeduceDescendantsRecur(SortedSet<State<TDuct, TEvent>> deducedSet, DeduceDescendantsModeEnum deduceMode) {}

    public virtual void HandleSubstateEnter(State<TDuct, TEvent> substate) {}

    public virtual void SelectReactions(SortedSet<Reaction<TDuct, TEvent>> enabledReactions, TEvent @event)
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

    public virtual void SaveAllStateConfig(int[] snapshot) {}

    public virtual void SaveActiveStateConfig(int[] snapshot) {}

    public virtual int LoadAllStateConfig(int[] config, int configIdx) { return configIdx; }

    public virtual int LoadActiveStateConfig(int[] config, int configIdx) { return configIdx; }

    public abstract bool IsValidState();
}
