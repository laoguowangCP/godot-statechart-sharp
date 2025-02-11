using System;


namespace LGWCP.Godot.StatechartSharp.NodelessV2;

public abstract class Composition<TDuct, TEvent>
    where TDuct : StatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{
    public int _OrderId;

    public virtual void _Setup(Statechart<TDuct, TEvent> hostStatechart) {}

    public virtual void _Setup(Statechart<TDuct, TEvent> hostStatechart, ref int orderId)
    {
        // HostStatechart = hostStatechart;
        // Duct = hostStatechart.Duct;
        _OrderId = orderId;
        ++orderId;
    }

    public virtual void _SetupPost() {}
}
