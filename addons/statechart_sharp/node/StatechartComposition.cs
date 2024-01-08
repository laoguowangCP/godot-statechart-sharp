using Godot;

namespace LGWCP.StatechartSharp
{

public partial class StatechartComposition : Node
{
    internal int OrderId;
    public Statechart HostStatechart { get; protected set; }

    
    public double Delta { get => HostStatechart.Delta; }
    public double PhysicsDelta { get=>HostStatechart.PhysicsDelta; }
    public InputEvent Input { get => HostStatechart.Input; }
    public InputEvent UnhandledInput { get => HostStatechart.UnhandledInput; }

    internal virtual void Init() {}
    internal virtual void Init(Statechart hostStatechart, ref int ancestorId)
    {
        HostStatechart = hostStatechart;
        ++ancestorId;
        OrderId = ancestorId;
        
        if (HostStatechart != this)
        {
            ProcessMode = ProcessModeEnum.Disabled;
        }
    }
    internal virtual void PostInit() {}
    internal static bool IsCommonHost(StatechartComposition x, StatechartComposition y)
    {
        if (x == null || y == null)
        {
            return false;
        }
        return x.HostStatechart == y.HostStatechart;
    } 
}

} // end of namespace