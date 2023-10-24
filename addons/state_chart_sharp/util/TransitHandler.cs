using Godot;
using Godot.Collections;

namespace LGWCP.GodotPlugin.StateChartSharp
{
    public class TransitHandler
    {
        void Transit(State from, State to)
        {
            if (from.stateChart != to.stateChart)
            {
                GD.PushError("Target state must be in same statechart");
            }

            /*
                TODO: handle all state transit, to support "cross-level" transitions:
                1. need to know the "depth" of from-state and to-state
                2. go upwards recursively to find sibling state
                    2.1 dont do exit or enter when recursing
                    2.2 for to-state, recognize path while recursing
                    2.3 all states passed by should be compond-state
            */
        }
    }
}