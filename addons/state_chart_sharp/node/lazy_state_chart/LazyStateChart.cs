using Godot;
using Godot.Collections;

namespace LGWCP.GodotPlugin.StateChartSharp
{
    [GlobalClass]
    public partial class LazyStateChart : IStateChartComponent
    {
        private LazyState _rootState = null;
        public override void _Ready()
        {
            if (GetChildCount() != 1)
            {
                GD.PushError("LGWCP.GodotPlugin.StateChartSharp: Expecting 1 child root state.");
                return;
            }

            var child = GetChild<Node>(0);
            if (!(child is State))
            {
                GD.PushError("LGWCP.GodotPlugin.StateChartSharp: Root state must be a StateLazy.");
                return;
            }

            _rootState = child as LazyState;
            _rootState.Init();
            _rootState.StateEnter();
        }

        public void Transit()
        {
            _rootState.SubstateTransit();
        }
    }
}