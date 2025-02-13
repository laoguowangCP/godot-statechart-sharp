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

    public override bool IsValidState()
    {
        return true;
    }
}