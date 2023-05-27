using Godot;

namespace LGWCP.GodotPlugin.StateChartSharp
{
    public partial class StateChartHandler : Node
    {
        private StateNode _rootState = null;
        public override void _Ready()
        {
            // State chart should have 1 child as root state.
            base._Ready();
            if (GetChildCount() != 1)
            {
                GD.PushError("LGWCP.GodotPlugin.StateChartSharp: Expecting exactly 1 child as root state.");
                return;
            }

            // Root state should be derived from class State.
            var child = GetChild<Node>(0);
            if (!(child is StateNode))
            {
                GD.PushError("LGWCP.GodotPlugin.StateChartSharp: Root state must be a StateNode.");
                return;
            }

            _rootState = child as StateNode;
            _rootState.Init();
            _rootState.Enter();
        }
    }
}