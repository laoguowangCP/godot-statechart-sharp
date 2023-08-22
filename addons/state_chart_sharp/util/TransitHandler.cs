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
        }
    }
}