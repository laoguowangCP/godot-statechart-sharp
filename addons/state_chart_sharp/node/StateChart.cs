using Godot;
using Godot.Collections;

namespace LGWCP.GodotPlugin.StateChartSharp
{
    [GlobalClass, Icon("res://addons/state_chart_sharp/icon/StateChart.svg")]
    public partial class StateChart : Node
    {
        #region Preset EventName

        readonly StringName process = "process";
        readonly StringName beforeProcess = "before_process";
        readonly StringName physicsProcess = "physics_process";
        readonly StringName beforePhysicsProcess = "before_physics_process";
        readonly StringName input = "input";
        readonly StringName beforeInput = "before_input";
        readonly StringName unhandledInput = "unhandled_input";
        readonly StringName beforeUnhandledInput = "before_unhandled_input";


        #endregion

        protected State _rootState = null;
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
            if (child is not State)
            {
                GD.PushError("LGWCP.GodotPlugin.StateChartSharp: Root state must be a State.");
                return;
            }

            _rootState = child as State;
            // TODO: initiate statechart

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

            _rootState.SubstateTransit(beforeProcess);
            _rootState.SubstateTransit(process);
            
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

            _rootState.SubstateTransit(beforePhysicsProcess);
            _rootState.SubstateTransit(physicsProcess);

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

            _rootState.SubstateTransit(beforeInput);
            _rootState.SubstateTransit(input);

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

            _rootState.SubstateTransit(beforeUnhandledInput);
            _rootState.SubstateTransit(unhandledInput);

            isRunning = false;
        }

        public void Step(StringName eventName = null)
        {
            if (isRunning)
            {
                GD.PushWarning("State chart triggered on running.");
                return;
            }
            
            isRunning = true;

            _rootState.SubstateTransit(eventName);

            isRunning = false;
        }
    }
}