using System;


namespace LGWCP.Godot.StatechartSharp.NodelessV2;

public partial class State<TDuct, TEvent>
    where TDuct : StatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{
    public State(
        Span<Composition> comps,
        int substatesCnt,
        int autoTransitionCnt,
        Span<int> transitionCnt,
        Span<int> reactionCnt)
    {

    }
}
