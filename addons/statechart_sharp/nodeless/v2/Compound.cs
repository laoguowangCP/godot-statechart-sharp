using System;
using System.Collections.Generic;
using LGWCP.Util;

namespace LGWCP.Godot.StatechartSharp.NodelessV2;

public class Compound<TDuct, TEvent> : State<TDuct, TEvent>
    where TDuct : StatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{
    public State<TDuct, TEvent> _InitialState;
    public State<TDuct, TEvent> _CurrentState;

    public Compound (
        Action<TDuct>[] enters,
        Action<TDuct>[] exits)
    {
        _Enters = enters;
        _Exits = exits;
    }

    public override bool _IsValidState()
    {
        return true;
    }

    public override void _Setup(Statechart<TDuct, TEvent> hostStatechart, ref int orderId)
    {
        base._Setup(hostStatechart, ref orderId);

        // Init & collect states, transitions, actions
        // Get lower state and upper state
        State<TDuct, TEvent> lastSubstate = null;
        for (int i = 0; i < _Comps.Count; ++i)
        {
            var comp = _Comps[i];
            if (comp is State<TDuct, TEvent> s)
            {
                s._Setup(hostStatechart, ref orderId, _Substates.Count);
                substates.Add(s);

                if (LowerState is null)
                {
                    LowerState = s;
                }
                lastSubstate = s;
            }
            else if (childComp is StatechartBuilder<TDuct, TEvent>.Transition tComp)
            {
                // Root state should not have transition
                if (ParentState is null)
                {
                    continue;
                }
                var t = (TransitionInt)tComp._GetInternalComposition();
                t.Setup(hostStatechart, childComp, ref orderId);
            }
            else if (childComp is StatechartBuilder<TDuct, TEvent>.Reaction aComp)
            {
                var a = (ReactionInt)aComp._GetInternalComposition();
                a.Setup(hostStatechart, childComp, ref orderId);
            }
        }

        if (lastSubstate is not null)
        {
            if (lastSubstate.UpperState is not null)
            {
                UpperState = lastSubstate.UpperState;
            }
            else
            {
                UpperState = lastSubstate;
            }
        }
        // Else state is atomic, lower and upper are both null.

        // Set initial state
        if (buildComp is StatechartBuilder<TDuct, TEvent>.Compound compound)
        {
            InitialState = (StateInt)compound.Initial._CompInt;
        }

        if (InitialState is null)
        {
            foreach (var substate in substates)
            {
                if (substate.IsValidState())
                {
                    InitialState = substate;
                    break;
                }
            }
        }

        CurrentState = InitialState;

        // Convert to array
        Substates = substates.ToArray();
    }
}
