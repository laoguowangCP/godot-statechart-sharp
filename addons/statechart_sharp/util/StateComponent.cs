using Godot;
using System.Collections.Generic;


namespace LGWCP.StatechartSharp;

public class StateComponent
{
    protected State HostState;
    protected Statechart HostStatechart { get => HostState.HostStatechart; }
    protected State ParentState
    {
        get => HostState.ParentState;
        set => HostState.ParentState = value;
    }
    protected List<State> Substates { get => HostState.Substates; }
    protected State LowerState
    {
        get => HostState.LowerState;
        set => HostState.LowerState = value;
    }
    protected State UpperState
    {
        get => HostState.UpperState;
        set => HostState.UpperState = value;
    }
    protected List<Transition> Transitions { get => HostState.Transitions; }
    protected List<Reaction> Reactions { get => HostState.Reactions; }

    public StateComponent(State state)
    {
        HostState = state;
    }

    internal virtual void Setup(Statechart hostStateChart, ref int parentOrderId)
    {
        // Get parent state
        Node parent = HostState.GetParentOrNull<Node>();
        if (parent != null && parent is State)
        {
            ParentState = parent as State;
        }
    }

    internal virtual void PostSetup() {}

    internal virtual bool GetPromoteStates(List<State> states) { return false; }

    internal virtual void RegisterActiveState(SortedSet<State> activeStates) {}

    internal virtual bool IsConflictToEnterRegion(State substateToPend, SortedSet<State> enterRegionUnextended)
    {
        return false;
    }

    internal virtual int SelectTransitions(SortedSet<Transition> enabledTransitions, StringName eventName)
    {
        return 1;
    }
    
    internal virtual void ExtendEnterRegion(SortedSet<State> enterRegion, SortedSet<State> enterRegionEdge, SortedSet<State> extraEnterRegion, bool needCheckContain) {}

    internal virtual void DeduceDescendants(SortedSet<State> deducedSet, bool isHistory, bool isEdgeState) {}

    internal virtual void HandleSubstateEnter(State substate) {}

    #if TOOLS
    internal virtual void GetConfigurationWarnings(List<string> warnings)
    {
        // Check parent
        bool isParentWarning = true;
        bool isRootState = false;
        Node parent = HostState.GetParentOrNull<Node>();
        if (parent != null)
        {
            if (parent is Statechart)
            {
                isParentWarning = false;
                isRootState = true;
            }
            else if (parent is State state)
            {
                isParentWarning = state.IsHistory;
            }
        }

        // Root state should not have transition
        if (isRootState)
        {
            foreach (Node child in HostState.GetChildren())
            {
                if (child is Transition)
                {
                    warnings.Add("Root state should not have transition");
                    break;
                }
            }
        }

        if (isParentWarning)
        {
            warnings.Add("State should be child to statechart or non-history state.");
        }
    }
    #endif
}
