using System;


namespace LGWCP.Godot.StatechartSharp.Nodeless.Internal;

public class ParallelInt<TDuct, TEvent> : StateInt<TDuct, TEvent>
    where TDuct : IStatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{
    public ParallelInt() {}
}
