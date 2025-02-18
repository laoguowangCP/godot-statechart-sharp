using System;
using System.Collections.Generic;


namespace LGWCP.Godot.StatechartSharp.Nodeless;

public partial class Statechart<TDuct, TEvent>
    where TDuct : StatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{

public class StatechartComparer<T> : IComparer<T>
    where T : Composition
{
    public int Compare(T x, T y)
    {
        return x._OrderId - y._OrderId;
    }
}

public class StatechartReversedComparer<T> : IComparer<T>
    where T : Composition
{
    public int Compare(T x, T y)
    {
        return y._OrderId - x._OrderId;
    }
}

}
