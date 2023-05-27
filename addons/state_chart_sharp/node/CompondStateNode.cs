using Godot;
using Godot.Collections;

namespace LGWCP.GodotPlugin.StateChartSharp
{
    public partial class CompondStateNode : StateNode
    {
        [Export] protected StateNode defaultSubState;


        public override void Enter()
        {
            base.Enter();
            currentSubState.Enter();
        }

        public override void Exit()
        {
            currentSubState.Exit();
            base.Exit();
        }

        public override void Init()
        {
            subStates.Clear();
            transitions.Clear();
            
            // Load child nodes
            foreach (Node child in GetChildren())
            {
                if (child is StateNode s)
                {
                    // Initiate and compose all child states
                    s.Init();
                    subStates.Add(s);
                }
                else if (child is TransitionNode t)
                {
                    // Compose all child transitions
                    transitions.Add(t);
                }
                else
                {
                    // Child node is not State or Transition.
                    GD.PushError("LGWCP.GodotPlugin.State: Child node must be a State or a Transition.");
                }
            }
        }

        public override void _Input(InputEvent @event)
        {
            base._Input(@event);

            // Check current substate transitions.
            currentSubState.CheckTransitions(TransitionNode.TransitionModeEnum.Input);
            
            // Substates's _Input() will be called in BFS order.
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            base._UnhandledInput(@event);

            // Check current substate transitions.
            currentSubState.CheckTransitions(TransitionNode.TransitionModeEnum.UnhandledInput);
            
            // Substates's _UnhandledInput() will be called in BFS order.
        }

        public override void _Process(double delta)
        {
            base._Process(delta);

            // Check current substate transitions.
            currentSubState.CheckTransitions(TransitionNode.TransitionModeEnum.Process);
            
            // Substates's _Process() will be called in BFS order.
        }

        public override void _PhysicsProcess(double delta)
        {
            base._PhysicsProcess(delta);

            // Check current substate transitions.
            currentSubState.CheckTransitions(TransitionNode.TransitionModeEnum.PhysicsProcess);
            
            // Substates's _PhysicsProcess() will be called in BFS order.
        }
    }
}