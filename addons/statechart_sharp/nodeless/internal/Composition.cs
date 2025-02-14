using System;

namespace LGWCP.Godot.StatechartSharp.Nodeless.Internal;


public interface IComposition {}

public partial class StatechartInt<TDuct, TEvent>
    where TDuct : StatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{

public abstract class Composition : IComposition
{
    // protected abstract void Setup();
    public int _OrderId;
    // public StatechartInt<TDuct, TEvent> HostStatechart;
    // public TDuct Duct;

    public virtual void Setup(
        StatechartInt<TDuct, TEvent> hostStatechart,
        StatechartBuilder<TDuct, TEvent>.BuildComposition buildComp)
    {}

    public virtual void Setup(
        StatechartInt<TDuct, TEvent> hostStatechart,
        StatechartBuilder<TDuct, TEvent>.BuildComposition buildComp,
        ref int orderId)
    {
        // HostStatechart = hostStatechart;
        // Duct = hostStatechart.Duct;
        _OrderId = orderId;
        ++orderId;
    }

    public virtual void SetupPost(StatechartBuilder<TDuct, TEvent>.BuildComposition buildComp) {}

    /*
    public static bool IsCommonHost(Composition x, Composition y)
    {
        if (x == null || y == null)
        {
            return false;
        }
        return x.HostStatechart == y.HostStatechart;
    }*/
}

}
