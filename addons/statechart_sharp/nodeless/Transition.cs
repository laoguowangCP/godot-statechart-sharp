using System;
using System.Collections.Generic;


namespace LGWCP.Godot.StatechartSharp.Nodeless;

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

    // Not auto ctor
    public Transition(
        Statechart<TDuct, TEvent> hostStatechart,
        TEvent @event,
        Action<TDuct>[] guards,
        Action<TDuct>[] invokes,
        State[] targetStates) : base(hostStatechart)
    {
        _Event = @event;
        _Guards = guards;
        _Invokes = invokes;
        _TargetStates = targetStates;
        _IsAuto = false;

        _EnterRegion = new SortedSet<State>(new StatechartComparer<State>());
        _EnterRegionEdge = new SortedSet<State>(new StatechartComparer<State>());
        _DeducedEnterStates = new SortedSet<State>(new StatechartComparer<State>());
    }

    // Auto transition ctor
    public Transition(
        Statechart<TDuct, TEvent> hostStatechart,
        Action<TDuct>[] guards,
        Action<TDuct>[] invokes,
        State[] targetStates) : base(hostStatechart)
    {
        _Guards = guards;
        _Invokes = invokes;
        _TargetStates = targetStates;
        _IsAuto = true;

        _EnterRegion = new SortedSet<State>(new StatechartComparer<State>());
        _EnterRegionEdge = new SortedSet<State>(new StatechartComparer<State>());
        _DeducedEnterStates = new SortedSet<State>(new StatechartComparer<State>());
    }

    public override void _BeAppended(Composition pComp)
    {
        if (pComp is State s && s._IsValidState())
        {
            _SourceState = s;
        }
    }

    public override void _Setup(ref int parentOrderId)
    {
        base._Setup(ref parentOrderId);
        _Guards ??= Array.Empty<Action<StatechartDuct>>();
        _Invokes ??= Array.Empty<Action<StatechartDuct>>();
        _TargetStates ??= Array.Empty<State>();

        // Set targetless
        _IsTargetless = _TargetStates.Length == 0;

        if (_IsTargetless && _IsAuto)
        {
            _IsValid = false;
        }
    }

    public override void _SetupPost()
    {
        /*
        Post setup:
        1. Find LCA(Least Common Ancestor) of source and targets
        2. Record path from LCA to targets
        3. Update enter region
        */

        // Targetless transition's LCA is source's parent state as defined
        if (_IsTargetless)
        {
            _LcaState = _SourceState._ParentState;
            return;
        }

        // Record source-to-root
        State iterState = _SourceState;
        List<State> srcToRoot = new();
        while (iterState != null)
        {
            srcToRoot.Add(iterState);
            iterState = iterState._ParentState;
        }

        // Init LCA as source state, the first state in srcToRoot
        int reversedLcaIdx = srcToRoot.Count;
        List<State> tgtToRoot = new();
        for (int i = 0; i < _TargetStates.Length; ++i)
        {
            var target = _TargetStates[i];

            // Check under same statechart
            if (!_IsCommonHost(target, _SourceState))
            {
                continue;
            }

            /*
            Record target-to-root with conflict check:
            1. Do conflict check once when iterated state's parent first reach enter region
            2. If target is already in enter region, conflicts
            */
            tgtToRoot.Clear();
            bool isConflict = _EnterRegion.Contains(target);

            iterState = target;
            bool isReachEnterRegion = false;
            while (!isConflict && iterState != null)
            {
                // Check target conflicts
                State iterParent = iterState._ParentState;

                if (!isReachEnterRegion && iterParent != null)
                {
                    isReachEnterRegion = _EnterRegion.Contains(iterParent);
                    if (isReachEnterRegion)
                    {
                        isConflict = iterParent._IsConflictToEnterRegion(
                            iterState, _EnterRegion);
                    }
                }

                tgtToRoot.Add(iterState);
                iterState = iterParent;
            }

            // Target conflicts, disposed from enter region.
            if (isConflict)
            {
                continue;
            }

            // Local LCA between src and 1 tgt
            int maxReversedIdx = 1; // Reversed
            int maxCountToRoot = srcToRoot.Count < tgtToRoot.Count
                ? srcToRoot.Count : tgtToRoot.Count;
            for (int j = 1; j < maxCountToRoot; ++j)
            {
                if (srcToRoot[^j] == tgtToRoot[^j])
                {
                    maxReversedIdx = j;
                }
                else
                {
                    // Last state is LCA
                    break;
                }
            }

            // If local-LCA is ancestor of current LCA, update LCA
            if (maxReversedIdx < reversedLcaIdx)
            {
                reversedLcaIdx = maxReversedIdx;
            }

            /*
            Add tgtToRoot to enter region, because:
            1. Later target(s) need check conflict in local-LCA's ancestor
            2. When extending, states need check if their substate in enter region
            */
            for (int j = 1; j <= tgtToRoot.Count; ++j)
            {
                _EnterRegion.Add(tgtToRoot[^j]);
            }
        }

        // LCA
        _LcaState = srcToRoot[^reversedLcaIdx];

        // Extend enter region under LCA
        SortedSet<State> extraEnterRegion = new(new StatechartComparer<State>());
        _LcaState._ExtendEnterRegion(_EnterRegion, _EnterRegionEdge, extraEnterRegion, true);
        _EnterRegion.UnionWith(extraEnterRegion);

        // Remove states from root to LCA (include LCA)
        for (int i = 1; i <= reversedLcaIdx; ++i)
        {
            _EnterRegion.Remove(srcToRoot[^i]);
        }

        // Check auto-transition loop case
        if (_IsAuto && _EnterRegion.Contains(_SourceState))
        {
            _IsValid = false;
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

    public void _TransitionInvoke(TDuct duct)
    {
        for (int i = 0; i < _Invokes.Length; ++i)
        {
            _Invokes[i](duct);
        }
    }

    public SortedSet<State> _GetDeducedEnterStates()
    {
        _DeducedEnterStates.Clear();
        foreach (State edgeState in _EnterRegionEdge)
        {
            edgeState._DeduceDescendants(_DeducedEnterStates);
        }
        return _DeducedEnterStates;
    }

    /// <summary>
    /// Target states won't duplicate.
    /// </summary>
    /// <returns></returns>
    public override Composition Duplicate(bool isDeepDuplicate)
    {
        // target states won't duplicate
        if (_IsAuto)
        {
            return new Transition(_HostStatechart, _Guards, _Invokes, null);
        }
        else
        {
            return new Transition(_HostStatechart, _Event, _Guards, _Invokes, null);
        }
    }

    /// <summary>
    /// Use it before statechart is ready.
    /// </summary>
    /// <param name="targetStates"></param>
    /// <returns></returns>
    public Transition SetTargetState(State[] targetStates)
    {
        if (!_HostStatechart.IsReady)
        {
            _TargetStates = targetStates;
        }
        return this;
    }
}

}
