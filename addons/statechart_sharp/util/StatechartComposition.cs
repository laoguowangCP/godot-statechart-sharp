using Godot;


namespace LGWCP.Godot.StatechartSharp;


[Tool]
public partial class StatechartComposition : Node
{
    public int _OrderId;
    public Statechart _HostStatechart;

    public virtual void _Setup() {}

    public virtual void _Setup(Statechart hostStatechart, ref int orderId)
    {
        _HostStatechart = hostStatechart;
        _OrderId = orderId;
        ++orderId;
        ProcessMode = ProcessModeEnum.Disabled;
    }

    public virtual void _SetupPost() {}

    public static bool _IsCommonHost(StatechartComposition x, StatechartComposition y)
    {
        if (x == null || y == null)
        {
            return false;
        }
        return x._HostStatechart == y._HostStatechart;
    }

    public StatechartComposition Append(StatechartComposition compositionNode)
    {
        AddChild(compositionNode);
        return this;
    }
}
