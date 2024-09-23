using Godot;
using System.Collections.Generic;


namespace LGWCP.Godot.StatechartSharp;


public class StateImpl
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
    protected Dictionary<StringName, List<Transition>> Transitions { get => HostState.Transitions; }
    protected List<Transition> AutoTransitions { get => HostState.AutoTransitions; }
    protected Dictionary<StringName, List<Reaction>> Reactions { get => HostState.Reactions; }

    public StateImpl(State state)
    {
        HostState = state;
    }

    public virtual bool IsAvailableRootState()
    {
        return false;
    }

    public virtual void Setup(Statechart hostStateChart, ref int parentOrderId, int substateIdx)
    {
        // Get parent state
        Node parent = HostState.GetParentOrNull<Node>();
        if (parent != null && parent is State)
        {
            ParentState = parent as State;
        }

        HostState.SubstateIdx = substateIdx;
    }

    public virtual void PostSetup() {}

    public virtual bool GetPromoteStates(List<State> states) { return false; }

    public virtual void RegisterActiveState(SortedSet<State> activeStates) {}

    public virtual bool IsConflictToEnterRegion(State substateToPend, SortedSet<State> enterRegionUnextended)
    {
        return false;
    }

    public virtual int SelectTransitions(SortedSet<Transition> enabledTransitions, StringName eventName)
    {
        return 1;
    }
    
    public virtual void ExtendEnterRegion(SortedSet<State> enterRegion, SortedSet<State> enterRegionEdge, SortedSet<State> extraEnterRegion, bool needCheckContain) {}

    public virtual void DeduceDescendants(SortedSet<State> deducedSet, bool isHistory, bool isEdgeState) {}

    public virtual void HandleSubstateEnter(State substate) {}

    public virtual void SaveAllStateConfig(ref List<int> snapshot) {}

    public virtual void SaveActiveStateConfig(ref List<int> snapshot) {}

    public virtual bool LoadAllStateConfig(ref int[] config, ref int configIdx) { return true; }
    
    public virtual bool LoadActiveStateConfig(ref int[] config, ref int configIdx) { return true; }

    #if TOOLS
    public virtual void GetConfigurationWarnings(List<string> warnings)
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
