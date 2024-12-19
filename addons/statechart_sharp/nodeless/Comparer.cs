using System;
using System.Collections.Generic;

namespace LGWCP.Godot.StatechartSharp.Nodeless;

public partial class Statechart<TDuct, TEvent>
    where TDuct : IStatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{
    protected class StatechartComparer<TComposition> : IComparer<TComposition>
        where TComposition : Composition<TComposition>
    {
        public int Compare(TComposition x, TComposition y)
        {
            return x.OrderId - y.OrderId;
        }
    }

    protected class StatechartReversedComparer<TComposition> : IComparer<TComposition>
        where TComposition : Composition<TComposition>
    {
        public int Compare(TComposition x, TComposition y)
        {
            return y.OrderId - x.OrderId;
        }
    }
}
