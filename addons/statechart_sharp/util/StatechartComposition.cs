using Godot;


namespace LGWCP.Godot.StatechartSharp;


[Tool]
public partial class StatechartComposition : Node
{
    public int OrderId;
    public Statechart HostStatechart { get; set; }

    public virtual void Setup() {}
    public virtual void Setup(Statechart hostStatechart, ref int parentOrderId)
    {
        HostStatechart = hostStatechart;
        OrderId = parentOrderId;
        ++parentOrderId;
        ProcessMode = ProcessModeEnum.Disabled;
    }
    public virtual void PostSetup() {}
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
