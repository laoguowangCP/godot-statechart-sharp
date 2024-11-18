using Godot;


namespace LGWCP.Godot.StatechartSharp;


[Tool]
public partial class StatechartComposition : Node
{
    public int OrderId;
    public Statechart HostStatechart;

    public virtual void _Setup() {}
    public virtual void Setup(Statechart hostStatechart, ref int orderId)
    {
        HostStatechart = hostStatechart;
        OrderId = orderId;
        ++orderId;
        ProcessMode = ProcessModeEnum.Disabled;
    }
    public virtual void _SetupPost() {}
    
    public static bool IsCommonHost(StatechartComposition x, StatechartComposition y)
    {
        if (x == null || y == null)
        {
            return false;
        }
        return x.HostStatechart == y.HostStatechart;
    }

    public StatechartComposition Append(StatechartComposition compositionNode)
    {
        AddChild(compositionNode);
        return this;
    }
}
