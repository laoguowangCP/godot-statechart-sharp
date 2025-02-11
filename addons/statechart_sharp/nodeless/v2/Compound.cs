using System;
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
        Action<TDuct>[] exits,
        Composition<TDuct, TEvent>[] comps,
        int substatesCnt,
        int autoTransitionCnt,
        Span<int> transitionCntPerEvent,
        Span<int> reactionCntPerEvent)
    {
        _Enters = enters;
        _Exits = exits;
        _Comps = comps;

        _Substates = new State<TDuct, TEvent>[substatesCnt];
        AutoTransitions = new Transition<TDuct, TEvent>[autoTransitionCnt];
        
        ArrayHelper.KVArrayDictAlloc(transitionCntPerEvent, out KTransitions, out VTransitions);
        ArrayHelper.KVArrayDictAlloc(reactionCntPerEvent, out KReactions, out VReactions);
    }

    public override bool IsValidState()
    {
        return true;
    }
}