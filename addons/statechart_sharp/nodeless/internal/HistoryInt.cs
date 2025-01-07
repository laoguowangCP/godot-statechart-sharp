using System;


namespace LGWCP.Godot.StatechartSharp.Nodeless.Internal;

public class HistoryInt<TDuct, TEvent> : StateInt<TDuct, TEvent>
    where TDuct : IStatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{
    public HistoryInt() {}

    public HistoryInt(State state) {}
}
