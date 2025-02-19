using System;
using System.Collections.Generic;


namespace LGWCP.Godot.StatechartSharp.Nodeless;


public partial class Statechart<TDuct, TEvent>
    where TDuct : StatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{

public abstract class State : Composition
{
    public State _ParentState;
    public State _LowerState;
    public State _UpperState;
    public int _SubstateIdx;
    public Action<TDuct>[] _Enters;
    public Action<TDuct>[] _Exits;
    public List<State> _Substates = new();
    public List<Transition> _AutoTransitions = new();
    public Dictionary<TEvent, List<Transition>> _TransitionsKV = new();
    public Dictionary<TEvent, List<Reaction>> _ReactionsKV = new();

    protected State(Statechart<TDuct, TEvent> statechart) : base(statechart) {}

    public override void _Setup(ref int orderId)
    {
        _Setup(ref orderId, 0);
    }

    public virtual void _Setup(ref int orderId, int substateIdx)
    {
        base._Setup(ref orderId);
        _Enters ??= Array.Empty<Action<StatechartDuct>>();
        _Exits ??= Array.Empty<Action<StatechartDuct>>();
        // Get parent state when adding comps
        _SubstateIdx = substateIdx;
    }

    public override void _BeAppended(Composition pComp)
    {
        if (pComp is State s && s._IsValidState())
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

    public bool _IsAncestorStateOf(State state)
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

    public virtual void _SubmitActiveState(SortedSet<State> activeStates) {}

    public virtual bool _IsConflictToEnterRegion(State substateToPend, SortedSet<State> enterRegionUnextended)
    {
        return false;
    }

    public virtual int _SelectTransitions(SortedSet<Transition> enabledTransitions, TEvent @event, TDuct duct)
    {
        return 1;
    }

    public virtual int _SelectAutoTransitions(SortedSet<Transition> enabledTransitions, TDuct duct)
    {
        return 1;
    }

    public virtual void _ExtendEnterRegion(SortedSet<State> enterRegion, SortedSet<State> enterRegionEdge, SortedSet<State> extraEnterRegion, bool needCheckContain) {}

    public virtual void _DeduceDescendants(SortedSet<State> deducedSet) {}

    public virtual void _DeduceDescendantsRecur(SortedSet<State> deducedSet, DeduceDescendantsModeEnum deduceMode) {}

    public virtual void _HandleSubstateEnter(State substate) {}

    public virtual void _SelectReactions(SortedSet<Reaction> enabledReactions, TEvent @event)
    {
        if (_ReactionsKV.TryGetValue(@event, out var reactions))
        {
            for (int i = 0; i < reactions.Count; ++i)
            {
                enabledReactions.Add(reactions[i]);
            }
        }
    }

    public virtual void _SaveAllStateConfig(List<int> snapshot) {}

    public virtual void _SaveActiveStateConfig(List<int> snapshot) {}

    public virtual int _LoadAllStateConfig(int[] config, int configIdx) { return configIdx; }

    public virtual int _LoadActiveStateConfig(int[] config, int configIdx) { return configIdx; }

    public abstract bool _IsValidState();
}

}
