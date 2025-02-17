using System;
using System.Collections.Generic;


namespace LGWCP.Godot.StatechartSharp.NodelessV2;

public partial class Statechart<TDuct, TEvent>
    where TDuct : StatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{

public abstract class Composition
{
    public int _OrderId;
    public List<Composition> _Comps = new();

    public virtual void _Setup() {}

    public virtual void _Setup(ref int orderId)
    {
        // HostStatechart = hostStatechart;
        // Duct = hostStatechart.Duct;
        _OrderId = orderId;
        ++orderId;
    }

    public virtual void _SetupPost() {}

    public Composition Add(Composition comp)
    {
        _Comps.Add(comp);
        comp._BeAdded(this);
        return this;
    }

    public virtual void _BeAdded(Composition pComp) {}
}

}
