using System.Collections.Generic;

namespace LGWCP.StatechartSharp;

class StateComparer : IComparer<State>
{
    public int Compare(State x, State y)
    {
        return x.OrderId - y.OrderId;
    }
}

class ReversedStateComparer : IComparer<State>
{
    public int Compare(State x, State y)
    {
        return y.OrderId - x.OrderId;
    }
}

class TransitionComparer : IComparer<Transition>
{
    public int Compare(Transition x, Transition y)
    {
        return x.OrderId - y.OrderId;
    }
}

class ReactionComparer : IComparer<Reaction>
{
    public int Compare(Reaction x, Reaction y)
    {
        return x.OrderId - y.OrderId;
    }
}
