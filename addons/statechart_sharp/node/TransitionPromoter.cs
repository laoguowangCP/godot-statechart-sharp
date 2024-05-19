using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace LGWCP.StatechartSharp;

[Tool]
[GlobalClass, Icon("res://addons/statechart_sharp/icon/TransitionPromoter.svg")]
public partial class TransitionPromoter : StatechartComposition
{
    protected Transition HostTransition;
    protected State ParentState;

    public override void _Ready()
    {
        #if TOOLS
        if (Engine.IsEditorHint())
        {
            UpdateConfigurationWarnings();
            return;
        }
        #endif

        HostTransition = GetParentOrNull<Transition>();

        if (HostTransition is not null)
        {
            Node transitionParent = HostTransition.GetParentOrNull<Node>();

            if (transitionParent is not null
                && transitionParent is State parentState)
            {
                ParentState = parentState;
                PromoteTransition();
                return;
            }
        }

        QueueFree();
    }
    
    protected async void PromoteTransition()
    {
        // Wait transition is ready
        await ToSignal(HostTransition, Transition.SignalName.Ready);

        if (HostTransition.IsQueuedForDeletion())
        {
            return;
        }

        /*
        Priortize parent transition:
        1. Get leaf state(s) (non-history)
        2. For each leaf state(s), duplicate transition, add it as first child
        3. Delete transition
        */

        // Remove children
        foreach(var child in HostTransition.GetChildren())
        {
            HostTransition.RemoveChild(child);
        }

        // Find leaf states
        Node transitionParent = HostTransition.GetParentOrNull<Node>();

        // Get leaf states
        List<State> leafStates = new List<State>();
        ParentState.GetLeafStates(leafStates);

        // For each leaf state(s), duplicate transition, add it as first child
        foreach (State leafState in leafStates)
        {
            // Duplicated transition with signal
            Node t = HostTransition.Duplicate();

            // Add child
            leafState.AddChild(t);
            // Move child to top
            leafState.MoveChild(t, 0);
        }

        // Delete transition
        HostTransition.QueueFree();
    }
}
