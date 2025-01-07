using System;


namespace LGWCP.Godot.StatechartSharp.Nodeless.Internal;

public class CompoundInt<TDuct, TEvent> : StateInt<TDuct, TEvent>
    where TDuct : IStatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{
    public CompoundInt() {}

    public CompoundInt(State state) {}
}
