using System.Collections.Generic;

namespace LGWCP.StatechartSharp
{
    class CompositionComparer : IComparer<StatechartComposition>
    {
        public int Compare(StatechartComposition x, StatechartComposition y)
        {
            return x.OrderId - y.OrderId;
        }
    }

    class ReversedCompositionComparer : IComparer<StatechartComposition>
    {
        public int Compare(StatechartComposition x, StatechartComposition y)
        {
            return y.OrderId - x.OrderId;
        }
    }

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

    class ReversedTransitionComparer : IComparer<Transition>
    {
        public int Compare(Transition x, Transition y)
        {
            return y.OrderId - x.OrderId;
        }
    }

}