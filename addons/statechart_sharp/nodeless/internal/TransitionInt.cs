using System;
using System.Collections.Generic;
using System.Linq;

namespace LGWCP.Godot.StatechartSharp.Nodeless.Internal;

public partial class StatechartInt<TDuct, TEvent>
    where TDuct : StatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{

public class TransitionInt : Composition
{
    protected Action<TDuct>[] Guards;
    protected Action<TDuct>[] Invokes;
    public TEvent Event;
    protected StateInt[] TargetStates;
    public StateInt SourceState;
    public StateInt LcaState;
    public SortedSet<StateInt> EnterRegion;
    protected SortedSet<StateInt> EnterRegionEdge;
    protected SortedSet<StateInt> DeducedEnterStates;
    public bool IsTargetless;
    public bool IsAuto;
    protected bool IsValid;

    public TransitionInt(StatechartBuilder<TDuct, TEvent>.Transition transition)
    {
        transition._CompInt = this;
        Event = transition.Event;
        // TODO: TargetStates = transition.Targets;
        IsAuto = transition.IsAuto;
        Guards = transition.Guards;
        Invokes = transition.Invokes;

        EnterRegion = new SortedSet<StateInt>(new StatechartComparer<StateInt>());
        EnterRegionEdge = new SortedSet<StateInt>(new StatechartComparer<StateInt>());
        DeducedEnterStates = new SortedSet<StateInt>(new StatechartComparer<StateInt>());

        IsValid = true;
    }

    
    public override void Setup(
        StatechartInt<TDuct, TEvent> hostStatechart,
        StatechartBuilder<TDuct, TEvent>.BuildComposition buildComp,
        ref int orderId)
    {
        base.Setup(hostStatechart, buildComp, ref orderId);

        // Get source state
        var pComp = buildComp._PComp;
        if (pComp is not null)
        {
            if (pComp._CompInt is StateInt parentState)
            {
                SourceState = parentState;
            }
        }
    }

    public override void SetupPost(StatechartBuilder<TDuct, TEvent>.BuildComposition buildComp)
    {
        /*
        Post setup:
        1. Find LCA(Least Common Ancestor) of source and targets
        2. Record path from LCA to targets
        3. Update enter region
        */

        // Targetless transition's LCA is source's parent state as defined
        if (buildComp is StatechartBuilder<TDuct, TEvent>.Transition tComp)
        {
            TargetStates = tComp.Targets.Select(state => (StateInt)state._CompInt).ToArray();

            // Targetless + Auto -> Invalid
            IsTargetless = tComp.Targets.Length == 0;
            if (IsTargetless && IsAuto)
            {
                IsValid = false;
                return;
            }

            if (IsTargetless)
            {
                LcaState = SourceState.ParentState;
                return;
            }

            // Record source-to-root
            StateInt iterState = SourceState;
            List<StateInt> srcToRoot = new();
            while (iterState != null)
            {
                srcToRoot.Add(iterState);
                iterState = iterState.ParentState;
            }

            // Init LCA as source state, the first state in srcToRoot
            int reversedLcaIdx = srcToRoot.Count;
            List<StateInt> tgtToRoot = new();
            for (int i = 0; i < TargetStates.Length; i++)
            {
                var target = TargetStates[i];
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
    #if DEBUG
                    GD.PushWarning(
                        GetPath(), ": target ", target.GetPath(), " conflicts with previous target(s).");
    #endif
                    continue;
                }

                // Local LCA between src and 1 tgt
                int maxReversedIdx = 1; // Reversed
                int maxCountToRoot = srcToRoot.Count < tgtToRoot.Count
                    ? srcToRoot.Count : tgtToRoot.Count;
                for (int i = 1; i < maxCountToRoot; ++i)
                {
                    if (srcToRoot[^i] == tgtToRoot[^i])
                    {
                        maxReversedIdx = i;
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
                for (int i = 1; i <= tgtToRoot.Count; ++i)
                {
                    _EnterRegion.Add(tgtToRoot[^i]);
                }
            }

            // LCA
            _LcaState = srcToRoot[^reversedLcaIdx];

            // Extend enter region under LCA
            SortedSet<State> extraEnterRegion = new(new StatechartComparer<State>());
            _LcaState._ExtendEnterRegion(_EnterRegion, EnterRegionEdge, extraEnterRegion, true);
            _EnterRegion.UnionWith(extraEnterRegion);

            // Remove states from root to LCA (include LCA)
            for (int i = 1; i <= reversedLcaIdx; ++i)
            {
                _EnterRegion.Remove(srcToRoot[^i]);
            }

            // Check auto-transition loop case
            if (_IsAuto && _EnterRegion.Contains(_SourceState))
            {
                IsValid = false;
    #if DEBUG
                GD.PushWarning(
                    GetPath(),
                    ": auto transition's enter region contains source state ",
                    "causes transition invalid, this may cause loop transition.");
    #endif
            }
        }
        else
        {
            IsValid = false;
        }
    }

    public void TransitionInvoke(TDuct duct)
    {
        for (int i = 0; i < Invokes.Length; ++i)
        {
            Invokes[i](duct);
        }
    }

    public SortedSet<StateInt> GetDeducedEnterStates()
    {
        DeducedEnterStates.Clear();
        foreach (var edgeState in EnterRegionEdge)
        {
            edgeState.DeduceDescendants(DeducedEnterStates.Add);
        }
        return DeducedEnterStates;
    }

    public bool Check(TDuct duct)
    {
        if (!IsValid)
        {
            return false;
        }

        duct.IsTransitionEnabled = true;
        for (int i = 0; i < Guards.Length; ++i)
        {
            Guards[i](duct);
        }
        return duct.IsTransitionEnabled;
    }

    protected virtual void CustomTransitionInvoke(TDuct duct)
    {
        for (int i = 0; i < Invokes.Length; ++i)
        {
            Invokes[i](duct);
        }
    }
}

}
