using System;


namespace LGWCP.Godot.StatechartSharp.Nodeless.Internal;

public class DeepHistoryInt<TDuct, TEvent> : StateInt<TDuct, TEvent>
    where TDuct : IStatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{
    public DeepHistoryInt() {}
}
