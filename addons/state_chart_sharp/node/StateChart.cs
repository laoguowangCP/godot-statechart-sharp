using Godot;

namespace LGWCP.GodotPlugin.StateChartSharp
{
    [GlobalClass]
    public partial class StateChart : IStateChartComponent
    {
        private State _rootState = null;
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
                GD.PushError("LGWCP.GodotPlugin.StateChartSharp: Root state must be a State.");
                return;
            }

            _rootState = child as State;
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