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
                bool isConflict = EnterRegion.Contains(target);

                iterState = target;
                bool isReachEnterRegion = false;
                while (!isConflict && iterState != null)
                {
                    // Check target conflicts
                    var iterParent = iterState.ParentState;

                    if (!isReachEnterRegion && iterParent != null)
                    {
                        isReachEnterRegion = EnterRegion.Contains(iterParent);
                        if (isReachEnterRegion)
                        {
                            isConflict = iterParent.IsConflictToEnterRegion(
                                iterState, EnterRegion);
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
                    EnterRegion.Add(tgtToRoot[^j]);
                }
            }

            // LCA
            LcaState = srcToRoot[^reversedLcaIdx];

            // Extend enter region under LCA
            var extraEnterRegion = new SortedSet<StateInt>(new StatechartComparer<StateInt>());
            LcaState.ExtendEnterRegion(EnterRegion, EnterRegionEdge, extraEnterRegion, true);
            EnterRegion.UnionWith(extraEnterRegion);

            // Remove states from root to LCA (include LCA)
            for (int i = 1; i <= reversedLcaIdx; ++i)
            {
                EnterRegion.Remove(srcToRoot[^i]);
            }

            // Check auto-transition loop case
            if (IsAuto && EnterRegion.Contains(SourceState))
            {
                IsValid = false;
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
            edgeState.DeduceDescendants(DeducedEnterStates);
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
