using Godot;
using Godot.Collections;

namespace LGWCP.GodotPlugin.StateChartSharp
{
    [GlobalClass]
    public partial class StateChart : IStateChartComponent
    {
        protected State _rootState = null;
        protected TransitHandler transitHandler;
        // State chart should not be triggered while running
        private bool isRunning;

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

            transitHandler = new TransitHandler();

            isRunning = false;
        }

        public override void _Process(double delta)
        {
            if (isRunning)
            {
                GD.PushWarning("State chart triggered on running.");
                return;
            }
            
            isRunning = true;

            _rootState.SubstateTransit(Transition.TransitionModeEnum.Process);
            _rootState.StateProcess(delta);
            
            isRunning = false;
        }

        public override void _PhysicsProcess(double delta)
        {
            if (isRunning)
            {
                GD.PushWarning("State chart triggered on running.");
                return;
            }
            
            isRunning = true;

            _rootState.SubstateTransit(Transition.TransitionModeEnum.PhysicsProcess);
            _rootState.StatePhysicsProcess(delta);

            isRunning = false;
        }

        public override void _Input(InputEvent @event)
        {
            if (isRunning)
            {
                GD.PushWarning("State chart triggered on running.");
                return;
            }
            
            isRunning = true;

            _rootState.SubstateTransit(Transition.TransitionModeEnum.Input);
            _rootState.StateInput(@event);

            isRunning = false;
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (isRunning)
            {
                GD.PushWarning("State chart triggered on running.");
                return;
            }
            
            isRunning = true;

            _rootState.SubstateTransit(Transition.TransitionModeEnum.UnhandledInput);
            _rootState.StateUnhandledInput(@event);

            isRunning = false;
        }
    }
}