using System;
using System.Collections.Generic;
using LGWCP.Godot.StatechartSharp.Nodeless.Internal;

namespace LGWCP.Godot.StatechartSharp.Nodeless;

public enum StateModeEnum : int
{
    Compound,
    Parallel,
    History,
    DeepHistory
}

public partial class StatechartBuilder<TDuct, TEvent>
    where TDuct : StatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{
    public abstract class State : BuildComposition
    {
        public Action<TDuct>[] Enters;
        public Action<TDuct>[] Exits;
    }
}
