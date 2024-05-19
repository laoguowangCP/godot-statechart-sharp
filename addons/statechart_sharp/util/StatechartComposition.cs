using Godot;

namespace LGWCP.StatechartSharp;

[Tool]
public partial class StatechartComposition : Node
{
    internal int OrderId;
    internal Statechart HostStatechart { get; set; }

    internal virtual void Setup() {}
    internal virtual void Setup(Statechart hostStatechart, ref int ancestorId)
    {
        HostStatechart = hostStatechart;
        ++ancestorId;
        OrderId = ancestorId;
        
        if (HostStatechart != this)
        {
            ProcessMode = ProcessModeEnum.Disabled;
        }
    }
    internal virtual void PostSetup() {}
    internal static bool IsCommonHost(StatechartComposition x, StatechartComposition y)
    {
        if (x == null || y == null)
        {
            return false;
        }
        return x.HostStatechart == y.HostStatechart;
    } 
}
