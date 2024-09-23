using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace LGWCP.Godot.StatechartSharp;

[Tool]
[GlobalClass]
[Icon("res://addons/statechart_sharp/icon/TransitionPromoter.svg")]
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
        await ToSignal(ParentState, State.SignalName.Ready);

        if (HostTransition.IsQueuedForDeletion())
        {
            return;
        }

        /*
        Priortize parent transition:
        1. Get promote state(s)
        2. For each leaf state(s), duplicate transition, add it as first child
        3. Delete transition
        */

        // Remove children
        foreach(var child in HostTransition.GetChildren())
        {
            HostTransition.RemoveChild(child);
        }

        // Get promote state(s)
        List<State> leafStates = new();
        ParentState.GetPromoteStates(leafStates);

        // Duplicate
        foreach (State leafState in leafStates)
        {
            Node t = HostTransition.Duplicate();
            leafState.AddChild(t);
            leafState.MoveChild(t, 0);
        }

        // Delete transition
        HostTransition.QueueFree();
    }
}
