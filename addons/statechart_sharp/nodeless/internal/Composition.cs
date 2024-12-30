using System;

namespace LGWCP.Godot.StatechartSharp.Nodeless;

public partial class Statechart<TDuct, TEvent>
    where TDuct : IStatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{
    protected interface IComposition {}

    protected abstract class Composition<T> : IComposition
        where T : Composition<T>
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

        public static bool IsCommonHost(T x, T y)
        {
            if (x == null || y == null)
            {
                return false;
            }
            return x.HostStatechart == y.HostStatechart;
        }
    }
}
