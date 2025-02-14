using System;
using System.Collections.Generic;

namespace LGWCP.Godot.StatechartSharp.Nodeless.Internal;

public partial class StatechartInt<TDuct, TEvent>
    where TDuct : StatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{
    protected class StatechartComparer<TComposition> : IComparer<TComposition>
        where TComposition : Composition
    {
        public int Compare(TComposition x, TComposition y)
        {
            return x._OrderId - y._OrderId;
        }
    }

    protected class StatechartReversedComparer<TComposition> : IComparer<TComposition>
        where TComposition : Composition
    {
        public int Compare(TComposition x, TComposition y)
        {
            return y._OrderId - x._OrderId;
        }
    }
}
