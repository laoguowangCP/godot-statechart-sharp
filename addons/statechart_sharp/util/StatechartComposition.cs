using Godot;

namespace LGWCP.StatechartSharp;

[Tool]
public partial class StatechartComposition<T> : Node
    where T : StatechartComposition<T>
{
    internal int OrderId;
    internal Statechart HostStatechart { get; set; }

    internal virtual void Setup() {}
    internal virtual void Setup(Statechart hostStatechart, ref int parentOrderId)
    {
        HostStatechart = hostStatechart;
        OrderId = parentOrderId;
        ++parentOrderId;
        ProcessMode = ProcessModeEnum.Disabled;
    }
    internal virtual void PostSetup() {}
    internal static bool IsCommonHost<U, V>(U x, V y)
        where U : StatechartComposition<U>
        where V : StatechartComposition<V>
    {
        if (x == null || y == null)
        {
            return false;
        }
        return x.HostStatechart == y.HostStatechart;
    }

    public T Append<U>(U CompositionNode)
        where U : StatechartComposition<U>
    {
        AddChild(CompositionNode);
        return (T)this;
    }
}
