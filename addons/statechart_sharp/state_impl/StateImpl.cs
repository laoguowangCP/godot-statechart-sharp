using Godot;
using System.Collections.Generic;


namespace LGWCP.Godot.StatechartSharp;


public class StateImpl
{
    protected State HostState;
    protected Statechart HostStatechart;
    protected State ParentState;
    protected List<State> Substates;
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
    public int StateId;
    protected Dictionary<StringName, List<Transition>> Transitions;
    protected List<Transition> AutoTransitions;
    protected Dictionary<StringName, List<Reaction>> Reactions;
    protected (List<Transition> Transitions, List<Reaction> Reactions)[] CurrentTAMap
    {
        get => HostStatechart.CurrentTAMap;
    }

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
        State parentState = HostState.GetParentOrNull<State>();
        if (parentState != null)
        {
            HostState.ParentState = parentState;
        }

        HostState.SubstateIdx = substateIdx;

        // Cache property
        ParentState = HostState.ParentState;
        HostStatechart = HostState.HostStatechart;
        Substates = HostState.Substates;
        Transitions = HostState.Transitions;
        AutoTransitions = HostState.AutoTransitions;
        Reactions = HostState.Reactions;
    }

    public virtual void PostSetup()
    {
        HostStatechart.SubmitGlobalTransitions(StateId, Transitions);
        HostStatechart.SubmitGlobalReactions(StateId, Reactions);
    }

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

    public virtual void SelectReactions(SortedSet<Reaction> enabledReactions, StringName eventName)
	{
        /*
		bool hasEventName = Reactions.TryGetValue(eventName, out var matched);
		if (!hasEventName)
		{
			return;
		}
        */
        
        if (CurrentTAMap is null)
        {
            return;
        }

        var matched = CurrentTAMap[StateId].Reactions;
        if (matched is null)
        {
            return;
        }

		foreach (Reaction reaction in matched)
		{
			enabledReactions.Add(reaction);
		}
	}

    public virtual void SaveAllStateConfig(List<int> snapshot) {}

    public virtual void SaveActiveStateConfig(List<int> snapshot) {}

    public virtual bool LoadAllStateConfig(int[] config, IntParser configIdx) { return true; }
    
    public virtual bool LoadActiveStateConfig(int[] config, IntParser configIdx) { return true; }

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
