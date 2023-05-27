using Godot;
using Godot.Collections;

namespace LGWCP.GodotPlugin.StateChartSharp
{
    public partial class ParallelStateNode : StateNode
    {
        public override void Enter()
        {
            base.Enter();
            // Handle sub states
            foreach(StateNode s in subStates)
            {
                s.Enter();
            }
        }

        public override void Exit()
        {
            // Handle sub states
            foreach(StateNode s in subStates)
            {
                s.Exit();
            }
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

        // TODO: Delete "check transition procedure" in parallel state.
        /*
        public override void _Input(InputEvent @event)
        {
            base._Input(@event);

            // For each substates, check their transitions.
            foreach(StateNode s in subStates)
            {
                s.CheckTransitions(TransitionNode.TransitionModeEnum.Input);
            }
            // Substates's _Input() will be called in BFS order.
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            base._UnhandledInput(@event);

            // For each substates, check their transitions.
            foreach(StateNode s in subStates)
            {
                s.CheckTransitions(TransitionNode.TransitionModeEnum.UnhandledInput);
            }
            // Substates's _UnhandledInput() will be called in BFS order.
        }

        public override void _Process(double delta)
        {
            base._Process(delta);

            // For each substates, check their transitions.
            foreach(StateNode s in subStates)
            {
                s.CheckTransitions(TransitionNode.TransitionModeEnum.Process);
            }
            // Substates's _Process() will be called in BFS order.
        }

        public override void _PhysicsProcess(double delta)
        {
            base._PhysicsProcess(delta);

            // For each substates, check their transitions.
            foreach(StateNode s in subStates)
            {
                s.CheckTransitions(TransitionNode.TransitionModeEnum.PhysicsProcess);
            }
            // Substates's _PhysicsProcess() will be called in BFS order.
        }
        */
    }
}