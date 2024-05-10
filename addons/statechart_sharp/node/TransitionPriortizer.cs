using Godot;
using Godot.Collections;
using System.Collections.Generic;


namespace LGWCP.StatechartSharp;

[Tool]
[GlobalClass, Icon("res://addons/statechart_sharp/icon/TransitionPriortizer.svg")]
public partial class TransitionPriortizer : StatechartComposition
{
    public override void _Ready()
    {
        #if TOOLS
        if (Engine.IsEditorHint())
        {
            UpdateConfigurationWarnings();
            return;
        }
        #endif

        Transition transition = GetParentOrNull<Transition>();
        if (transition is null)
        {
            // Delete this node
            QueueFree();
        }

        /*
        Priortize parent transition:
        1. Get leaf state(s) (non-history)
        2. For each leaf state(s), duplicate transition, add it as first child
        3. Delete transition
        */

        // Find leaf states
        Node node = transition.GetParentOrNull<Node>();

        if (node is State state)
        {
            // Get leaf states
            List<State> leafStates = new List<State>();
            // state.GetLeafStates(states)

            // For each leaf state(s), duplicate transition, add it as first child
            foreach (State leaf in leafStates)
            {
                
            }
        }

        // Delete transition
        transition.QueueFree();
    }
}
