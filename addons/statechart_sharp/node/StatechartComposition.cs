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
        public StatechartComposition Host { get; protected set; }
        public static bool HaveCommonHost(StatechartComposition x, StatechartComposition y)
        {
            return x.Host == y.Host;
        } 
    }
}