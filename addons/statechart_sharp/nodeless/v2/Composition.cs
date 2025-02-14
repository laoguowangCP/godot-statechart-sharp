using System;
using System.Collections.Generic;


namespace LGWCP.Godot.StatechartSharp.NodelessV2;

public abstract class Composition<TDuct, TEvent>
    where TDuct : StatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{
    public int _OrderId;
    public List<Composition<TDuct, TEvent>> _Comps = new();

    public virtual void _Setup(Statechart<TDuct, TEvent> hostStatechart) {}

    public virtual void _Setup(Statechart<TDuct, TEvent> hostStatechart, ref int orderId)
    {
        // HostStatechart = hostStatechart;
        // Duct = hostStatechart.Duct;
        _OrderId = orderId;
        ++orderId;
    }

    public virtual void _SetupPost() {}

    public Composition<TDuct, TEvent> Add(Composition<TDuct, TEvent> comp)
    {
        _Comps.Add(comp);
        comp._BeAdded(this);
        return this;
    }

    public virtual void _BeAdded(Composition<TDuct, TEvent> pComp) {}
}
