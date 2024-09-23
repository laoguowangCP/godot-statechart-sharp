using System.Collections.Generic;


namespace LGWCP.Godot.StatechartSharp;

class StatechartComparer<T> : IComparer<T>
    where T : StatechartComposition
{
    public int Compare(T x, T y)
    {
        return x.OrderId - y.OrderId;
    }
}

class StatechartReversedComparer<T> : IComparer<T>
    where T : StatechartComposition
{
    public int Compare(T x, T y)
    {
        return y.OrderId - x.OrderId;
    }
}
