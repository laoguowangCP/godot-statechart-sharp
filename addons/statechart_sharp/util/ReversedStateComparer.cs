using System.Collections.Generic;

namespace LGWCP.GodotPlugin.StatechartSharp
{
    class ReversedStateComparer : IComparer<State>
    {
        public int Compare(State x, State y)
        {
            return y.StateId - x.StateId;
        }
    }
}