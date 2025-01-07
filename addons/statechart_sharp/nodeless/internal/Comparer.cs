using System;
using System.Collections.Generic;

namespace LGWCP.Godot.StatechartSharp.Nodeless.Internal;

public partial class StatechartInt<TDuct, TEvent>
    where TDuct : IStatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{
    protected class StatechartComparer<TComposition> : IComparer<TComposition>
        where TComposition : Composition<TDuct, TEvent>
    {
        public int Compare(TComposition x, TComposition y)
        {
            return x.OrderId - y.OrderId;
        }
    }

    protected class StatechartReversedComparer<TComposition> : IComparer<TComposition>
        where TComposition : Composition<TDuct, TEvent>
    {
        public int Compare(TComposition x, TComposition y)
        {
            return y.OrderId - x.OrderId;
        }
    }
}
