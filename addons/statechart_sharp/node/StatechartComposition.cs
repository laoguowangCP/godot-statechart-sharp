using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LGWCP.GodotPlugin.StatechartSharp
{
    public partial class StatechartComposition : Node
    {
        public int OrderId;
        public Statechart HostStatechart { get; protected set; }

        public virtual void Init() {}
        public virtual void Init(Statechart hostStatechart, ref int ancestorId)
        {
            HostStatechart = hostStatechart;
            ++ancestorId;
            OrderId = ancestorId;
        }
        public virtual void PostInit() {}
        public static bool IsCommonHost(StatechartComposition x, StatechartComposition y)
        {
            return x.HostStatechart == y.HostStatechart;
        } 
    }
}