using System;

namespace LGWCP.Godot.StatechartSharp.Nodeless;

public partial class Statechart<TDuct, TEvent>
    where TDuct : IStatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{
    protected interface IStatechartComposition {}

    protected abstract class StatechartComposition<T> : IStatechartComposition
        where T : StatechartComposition<T>
    {
        // protected abstract void Setup();
        public int OrderId;
        public Statechart<TDuct, TEvent> HostStatechart;

        public virtual void SetupPre() {}

        public virtual void Setup()
        {
            OrderId = HostStatechart.GetOrderId();
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
