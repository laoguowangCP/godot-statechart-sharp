using Godot;

namespace LGWCP.GodotPlugin.StateChartSharp
{
    [GlobalClass]
    public partial class StateChartHandler : IStateChartComponent
    {
        private StateNode _rootState = null;
        public override void _Ready()
        {
            if (GetChildCount() != 1)
            {
                GD.PushError("LGWCP.GodotPlugin.StateChartSharp: Expecting exactly 1 child as root state.");
                return;
            }

            var child = GetChild<Node>(0);
            if (!(child is StateNode))
            {
                GD.PushError("LGWCP.GodotPlugin.StateChartSharp: Root state must be a StateNode.");
                return;
            }

            _rootState = child as StateNode;
            _rootState.Init();
            _rootState.StateEnter();
        }

        public override void _Process(double delta)
        {
            _rootState.SubstateTransit(Transition.TransitionModeEnum.Process);
            _rootState.StateProcess(delta);
        }

        public override void _PhysicsProcess(double delta)
        {
            _rootState.SubstateTransit(Transition.TransitionModeEnum.PhysicsProcess);
            _rootState.StatePhysicsProcess(delta);
        }

        public override void _Input(InputEvent @event)
        {
            _rootState.SubstateTransit(Transition.TransitionModeEnum.Input);
            _rootState.StateInput(@event);
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            _rootState.SubstateTransit(Transition.TransitionModeEnum.UnhandledInput);
            _rootState.StateUnhandledInput(@event);
        }
    }
}