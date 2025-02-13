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
    public List<State<TDuct, TEvent>> _Substates;
    public List<Transition<TDuct, TEvent>> _AutoTransitions;
    public SmallListDictionary<TEvent, List<Transition<TDuct, TEvent>>> _TransitionsKV;
    public SmallListDictionary<TEvent, List<Reaction<TDuct, TEvent>>> _ReactionsKV;

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
        for (int i = 0; i < _Exits.Length; i++)
        {
            _Exits[i](duct);
        }
    }

    public bool IsAncestorStateOf(State<TDuct, TEvent> state)
    {
        int id = state._OrderId;

        // Leaf state
        if (_LowerState == null || _UpperState == null)
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
        if (_ReactionsKV.TryGet(@event, out var reactions) >= 0)
        {
            for (int i = 0; i < reactions.Count; ++i)
            {
                enabledReactions.Add(reactions[j]);
            }
        }
    }

    public virtual void SaveAllStateConfig(int[] snapshot) {}

    public virtual void SaveActiveStateConfig(int[] snapshot) {}

    public virtual int LoadAllStateConfig(int[] config, int configIdx) { return configIdx; }

    public virtual int LoadActiveStateConfig(int[] config, int configIdx) { return configIdx; }

    public abstract bool IsValidState();
}
