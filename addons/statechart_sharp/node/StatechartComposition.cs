using Godot;

namespace LGWCP.StatechartSharp
{
    public partial class StatechartComposition : Node
    {
        internal int OrderId;
        public Statechart HostStatechart { get; protected set; }

        internal virtual void Init() {}
        internal virtual void Init(Statechart hostStatechart, ref int ancestorId)
        {
            HostStatechart = hostStatechart;
            ++ancestorId;
            OrderId = ancestorId;
        }
        internal virtual void PostInit() {}
        public static bool IsCommonHost(StatechartComposition x, StatechartComposition y)
        {
            return x.HostStatechart == y.HostStatechart;
        } 
    }
}