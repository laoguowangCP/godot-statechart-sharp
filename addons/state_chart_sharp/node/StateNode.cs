using Godot;
using Godot.Collections;

namespace LGWCP.GodotPlugin.StateChartSharp
{
    public partial class StateNode : Node
    {
        [Signal] public delegate void StateEnteredEventHandler();
        [Signal] public delegate void StateExitedEventHandler();
        [Signal] public delegate void StateProcessEventHandler(double delta);
        [Signal] public delegate void StatePhysicsProcessEventHandler(double delta);
        [Signal] public delegate void StateInputEventHandler(InputEvent @event);
        [Signal] public delegate void StateUnhandledInputEventHandler(InputEvent @event);
        // private StateNode _parentState;
        public StateNode currentSubState;
        protected Array<StateNode> subStates;
        protected Array<TransitionNode> transitions; // Transition nodes of this state.

        public override void _Ready()
        {
            subStates = new Array<StateNode>();
            transitions = new Array<TransitionNode>();
        }

        /// <summary>
        /// Initiate state recursively, compose direct child states and transitions.
        /// </summary>
        public virtual void Init() {}

        public virtual void Enter()
        {
            // Enable current state
            ProcessMode = Node.ProcessModeEnum.Inherit;
            // Emit signal: state entered
            EmitSignal(SignalName.StateEntered);
        }

        public virtual void Exit()
        {
            // Disable current state
            ProcessMode = Node.ProcessModeEnum.Disabled;
            // Emit signal: state exited
            EmitSignal(SignalName.StateExited);
        }

        public void CheckTransitions(TransitionNode.TransitionModeEnum transitOn)
        {
            foreach(TransitionNode t in transitions)
            {
                // Check transitions,
                // If timing and condition are both met, stop iteration.
                if (t.transitOn == transitOn)
                {
                    if (t.CheckWithTransit(this))
                    {
                        break;
                    }
                }

            }
        }

        public override void _Input(InputEvent @event)
        {
            EmitSignal(SignalName.StateInput, @event);
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            EmitSignal(SignalName.StateUnhandledInput, @event);
        }

        public override void _Process(double delta)
        {
            EmitSignal(SignalName.StateProcess, delta);
        }

        public override void _PhysicsProcess(double delta)
        {
            EmitSignal(SignalName.StatePhysicsProcess, delta);
        }
    }
}