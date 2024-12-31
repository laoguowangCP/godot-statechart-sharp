using System;

namespace LGWCP.Godot.StatechartSharp.Nodeless.Internal;

public partial class Statechart<TDuct, TEvent>
    where TDuct : IStatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{
    protected interface IComposition {}

    public abstract class Composition : IComposition
    {
        // protected abstract void Setup();
        public int OrderId;
        public Statechart<TDuct, TEvent> HostStatechart;

        public virtual void Setup() {}

        public virtual void Setup(Statechart<TDuct, TEvent> hostStatechart, ref int orderId)
        {
            HostStatechart = hostStatechart;
            OrderId = orderId;
            ++orderId;
        }

        public virtual void SetupPost() {}

        public static bool IsCommonHost(Composition x, Composition y)
        {
            if (x == null || y == null)
            {
                return false;
            }
            return x.HostStatechart == y.HostStatechart;
        }
    }
}
