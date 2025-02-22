using System;
using System.Collections.Generic;


namespace LGWCP.Godot.StatechartSharp.Nodeless;

public partial class Statechart<TDuct, TEvent>
    where TDuct : StatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{

public abstract class Composition
{
    public int _OrderId;
    public Statechart<TDuct, TEvent> _HostStatechart;
    public List<Composition> _Comps = new();

    public Composition(Statechart<TDuct, TEvent> statechart)
    {
        _HostStatechart = statechart;
    }

    public virtual void _Setup() {}

    public virtual void _Setup(ref int orderId)
    {
        // HostStatechart = hostStatechart;
        // Duct = hostStatechart.Duct;
        _OrderId = orderId;
        ++orderId;
    }

    public virtual void _SetupPost() {}

    public Composition Append(Composition comp)
    {
        _Comps.Add(comp);
        comp._BeAppended(this);
        return this;
    }

    public virtual void _BeAppended(Composition pComp) {}

    public static bool _IsCommonHost(Composition x, Composition y)
    {
        if (x == null || y == null)
        {
            return false;
        }
        return x._HostStatechart == y._HostStatechart;
    }

    public abstract Composition Duplicate(bool isDeepDuplicate);
}

}
