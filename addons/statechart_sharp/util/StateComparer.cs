using System.Collections.Generic;

namespace LGWCP.GodotPlugin.StatechartSharp
{
    class StateComparer : IComparer<State>
    {
        public int Compare(State x, State y)
        {
            return x.StateId - y.StateId;
        }
    }
}