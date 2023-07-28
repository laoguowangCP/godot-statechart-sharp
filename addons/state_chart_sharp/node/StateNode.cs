using Godot;
using Godot.Collections;

namespace LGWCP.GodotPlugin.StateChartSharp
{
    public partial class StateNode : Node
    {
        [Signal] public delegate void EnterEventHandler();
        [Signal] public delegate void ExitEventHandler();
        [Signal] public delegate void ProcessEventHandler(double delta);
        [Signal] public delegate void PhysicsProcessEventHandler(double delta);
        [Signal] public delegate void InputEventHandler(InputEvent @event);
        [Signal] public delegate void UnhandledInputEventHandler(InputEvent @event);
        // private StateNode _parentState;
        public StateNode currentSubstate;
        protected Array<StateNode> substates;
        public Array<TransitionNode> transitions;

        public override void _Ready()
        {
            substates = new Array<StateNode>();
            transitions = new Array<TransitionNode>();
        }

        public virtual void Init() {}

        public virtual void StateEnter()
        {
            EmitSignal(SignalName.Enter);
        }
        public virtual void StateExit()
        {
            EmitSignal(SignalName.Exit);
        }

        public virtual void SubstateTransit(TransitionNode.TransitionModeEnum mode) {}

        public virtual void StateInput(InputEvent @event)
        {
            EmitSignal(SignalName.Input, @event);
        }

        public virtual void StateUnhandledInput(InputEvent @event)
        {
            EmitSignal(SignalName.UnhandledInput, @event);
        }

        public virtual void StateProcess(double delta)
        {
            EmitSignal(SignalName.Process, delta);
        }

        public virtual void StatePhysicsProcess(double delta)
        {
            EmitSignal(SignalName.PhysicsProcess, delta);
        }
    }
}