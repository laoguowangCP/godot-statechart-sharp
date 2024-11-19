using System.Collections.Generic;


namespace LGWCP.Godot.StatechartSharp;

class StatechartComparer<T> : IComparer<T>
    where T : StatechartComposition
{
    public int Compare(T x, T y)
    {
        return x._OrderId - y._OrderId;
    }
}

class StatechartReversedComparer<T> : IComparer<T>
    where T : StatechartComposition
{
    public int Compare(T x, T y)
    {
        return y._OrderId - x._OrderId;
    }
}
