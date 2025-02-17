using System;
using System.Collections.Generic;


namespace LGWCP.Godot.StatechartSharp.NodelessV2;

public partial class Statechart<TDuct, TEvent>
    where TDuct : StatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{

public class Transition : Composition
{
    public Action<TDuct>[] _Guards;
    public Action<TDuct>[] _Invokes;
    public TEvent _Event;
    public State[] _TargetStates;
    public State _SourceState;
    public State _LcaState;
    public SortedSet<State> _EnterRegion;
    public SortedSet<State> _EnterRegionEdge;
    public SortedSet<State> _DeducedEnterStates;
    public bool _IsTargetless;
    public bool _IsAuto;
    public bool _IsValid = true;

    public Transition(
        Action<TDuct>[] guards,
        Action<TDuct>[] invokes,
        State[] targetStates
    )
    {
        _Guards = guards;
        _Invokes = invokes;

        _EnterRegion = new SortedSet<State>(new StatechartComparer<State>());
        _EnterRegionEdge = new SortedSet<State>(new StatechartComparer<State>());
        _DeducedEnterStates = new SortedSet<State>(new StatechartComparer<State>());

        _IsValid = true;
    }

    public override void _BeAdded(Composition pComp)
    {
        if (pComp is State s && s._IsValidState())
        {
            _SourceState = s;
        }
    }

    public bool _Check(TDuct duct)
    {
        if (!_IsValid)
        {
            return false;
        }

        duct.IsTransitionEnabled = true;
        for (int i = 0; i < _Guards.Length; ++i)
        {
            _Guards[i](duct);
        }
        return duct.IsTransitionEnabled;
    }
}

}
