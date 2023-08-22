using Godot;

namespace LGWCP.GodotPlugin.StateChartSharp
{
    [GlobalClass]
    public partial class IStateChartComponent : Node
    {
        public virtual void Init() {}
        public virtual void Init(StateChart stateChart) {}
        public virtual void Init(StateChart stateChart, State superState) {}
    }
}