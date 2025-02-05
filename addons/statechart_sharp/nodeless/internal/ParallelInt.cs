using System;
using System.Collections.Generic;
using LGWCP.Util;

namespace LGWCP.Godot.StatechartSharp.Nodeless.Internal;

public partial class StatechartInt<TDuct, TEvent>
    where TDuct : StatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{

public class ParallelInt : StateInt
{
    protected int ValidSubstateCnt = 0;

    public ParallelInt(StatechartBuilder<TDuct, TEvent>.Parallel state)
    {
        state._CompInt = this;
        Enters = state.Enters;
        Exits = state.Exits;
    }

    public override void Setup(
        StatechartInt<TDuct, TEvent> hostStatechart,
        StatechartBuilder<TDuct, TEvent>.BuildComposition buildComp,
        ref int orderId,
        int substateIdx)
    {
        base.Setup(hostStatechart, buildComp, ref orderId, substateIdx);

        // Init & collect states, transitions, actions
        // Get lower-state and upper-state
        StateInt lastSubstate = null;
        List<StateInt> substates = new(); // temp substates
        foreach (var childComp in buildComp._Comps)
        {
            if (childComp is StatechartBuilder<TDuct, TEvent>.State sComp)
            {
                var s = (StateInt)sComp._GetInternalComposition();
                s.Setup(hostStatechart, sComp, ref orderId, substates.Count);
                substates.Add(s);

                // First substate is lower-state
                if (LowerState == null)
                {
                    LowerState = s;
                }
                lastSubstate = s;

                if (s.IsValidState())
                {
                    ++ValidSubstateCnt;
                }
            }
            else if (childComp is StatechartBuilder<TDuct, TEvent>.Transition tComp)
            {
                // Root state should not have transition
                if (ParentState is null)
                {
                    continue;
                }
                var t = (TransitionInt)tComp._GetInternalComposition();
                t.Setup(hostStatechart, tComp, ref orderId);
            }
            else if (childComp is StatechartBuilder<TDuct, TEvent>.Reaction aComp)
            {
                var a = (ReactionInt)aComp._GetInternalComposition();
                a.Setup(hostStatechart, aComp, ref orderId);
            }
        }

        if (lastSubstate is not null)
        {
            if (lastSubstate.UpperState is not null)
            {
                // Last substate's upper is upper-state
                UpperState = lastSubstate.UpperState;
            }
            else
            {
                // Last substate is upper-state
                UpperState = lastSubstate;
            }
        }
    }

    public override void SetupPost(StatechartBuilder<TDuct, TEvent>.BuildComposition buildComp)
    {
        List<TransitionInt> autoTransitions = new();
        List<TEvent> kTransitions = new();
        List<List<TransitionInt>> vTransitions = new();
        List<TEvent> kReactions = new();
        List<List<ReactionInt>> vReactions = new();
        
        foreach (var childComp in buildComp._Comps)
        {
            if (childComp is StatechartBuilder<TDuct, TEvent>.State sComp)
            {
                sComp._CompInt.SetupPost(childComp);
            }
            else if (childComp is StatechartBuilder<TDuct, TEvent>.Transition tComp)
            {
                // Root state should not have transition
                if (ParentState is null)
                {
                    continue;
                }

                var t = (TransitionInt)tComp._CompInt;
                t.SetupPost(childComp);

                // Add transition
                if (t.IsAuto)
                {
                    autoTransitions.Add(t);
                    continue;
                }
                ArrayHelper.KVListInsert(t.Event, t, kTransitions, vTransitions);
            }
            else if (childComp is StatechartBuilder<TDuct, TEvent>.Reaction aComp)
            {
                var a = (ReactionInt)aComp._CompInt;
                a.SetupPost(childComp);

                // Add reaction
                ArrayHelper.KVListInsert(a.Event, a, kReactions, vReactions);
            }
        }
        
        // Convert to array
        AutoTransitions = autoTransitions.ToArray();
        ArrayHelper.KVListToArray(kTransitions, vTransitions, out KTransitions, out VTransitions);
        ArrayHelper.KVListToArray(kReactions, vReactions, out KReactions, out VReactions);
    }

    public override bool IsValidState()
    {
        return true;
    }
}

}
