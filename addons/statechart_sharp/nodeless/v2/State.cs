using System;
using System.Collections.Generic;
using System.Linq;
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

    public virtual void _Setup(Statechart<TDuct, TEvent> hostStatechart, ref int orderId, int substateIdx)
    {
        base._Setup(hostStatechart, ref orderId);
        // Get parent state when adding comps
        _SubstateIdx = substateIdx;
    }

    public override void _BeAdded(Composition<TDuct, TEvent> pComp)
    {
        if (pComp is State<TDuct, TEvent> s && s._IsValidState())
        {
            _ParentState = s;
        }
    }

    public void _StateEnter(TDuct duct)
    {
        _ParentState?._HandleSubstateEnter(this);
        for (int i = 0; i < _Enters.Length; ++i)
        {
            _Enters[i](duct);
        }
    }

    public void _StateExit(TDuct duct)
    {
        for (int i = 0; i < _Exits.Length; i++)
        {
            _Exits[i](duct);
        }
    }

    public bool _IsAncestorStateOf(State<TDuct, TEvent> state)
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

    public virtual void _SubmitActiveState(SortedSet<State<TDuct, TEvent>> activeStates) {}

    public virtual bool _IsConflictToEnterRegion(State<TDuct, TEvent> substateToPend, SortedSet<State<TDuct, TEvent>> enterRegionUnextended)
    {
        return false;
    }

    public virtual int _SelectTransitions(SortedSet<Transition<TDuct, TEvent>> enabledTransitions, TEvent @event, TDuct duct)
    {
        return 1;
    }

    public virtual void _ExtendEnterRegion(SortedSet<State<TDuct, TEvent>> enterRegion, SortedSet<State<TDuct, TEvent>> enterRegionEdge, SortedSet<State<TDuct, TEvent>> extraEnterRegion, bool needCheckContain) {}

    public virtual void _DeduceDescendants(SortedSet<State<TDuct, TEvent>> deducedSet) {}

    public virtual void _DeduceDescendantsRecur(SortedSet<State<TDuct, TEvent>> deducedSet, DeduceDescendantsModeEnum deduceMode) {}

    public virtual void _HandleSubstateEnter(State<TDuct, TEvent> substate) {}

    public virtual void _SelectReactions(SortedSet<Reaction<TDuct, TEvent>> enabledReactions, TEvent @event)
    {
        if (_ReactionsKV.TryGet(@event, out var reactions) >= 0)
        {
            for (int i = 0; i < reactions.Count; ++i)
            {
                enabledReactions.Add(reactions[i]);
            }
        }
    }

    public virtual void _SaveAllStateConfig(int[] snapshot) {}

    public virtual void _SaveActiveStateConfig(int[] snapshot) {}

    public virtual int _LoadAllStateConfig(int[] config, int configIdx) { return configIdx; }

    public virtual int _LoadActiveStateConfig(int[] config, int configIdx) { return configIdx; }

    public abstract bool _IsValidState();
}
