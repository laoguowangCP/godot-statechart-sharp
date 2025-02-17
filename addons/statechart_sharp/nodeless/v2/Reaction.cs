using System;


namespace LGWCP.Godot.StatechartSharp.NodelessV2;

public partial class Statechart<TDuct, TEvent>
    where TDuct : StatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{

public class Reaction : Composition
{
    public Action<TDuct>[] _Invokes;
    public TEvent _Event;

}

}
