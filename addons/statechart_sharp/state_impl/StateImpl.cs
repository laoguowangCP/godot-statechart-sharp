using Godot;
using System.Collections.Generic;
using System.Reflection.Metadata;


namespace LGWCP.Godot.StatechartSharp;


public class StateImpl
{
    protected State HostState;
    protected Statechart HostStatechart;
    protected State ParentState;
    protected List<State> Substates;
    protected State LowerState
    {
        get => HostState._LowerState;
        set => HostState._LowerState = value;
    }
    protected State UpperState
    {
        get => HostState._UpperState;
        set => HostState._UpperState = value;
    }
    public int StateId;
    // protected Dictionary<StringName, List<Transition>> Transitions;
    protected List<Transition> AutoTransitions;
    // protected Dictionary<StringName, List<Reaction>> Reactions;
    protected (List<Transition> Transitions, List<Reaction> Reactions)[] CurrentTAMap
    {
        get => HostStatechart._CurrentTAMap;
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
            HostState._ParentState = parentState;
        }

        HostState._SubstateIdx = substateIdx;

        // Cache property
        ParentState = HostState._ParentState;
        HostStatechart = HostState._HostStatechart;
        Substates = HostState._Substates;
        AutoTransitions = HostState._AutoTransitions;
    }

    public virtual void SetupPost() {}

    public virtual bool GetPromoteStates(List<State> states) { return false; }

    public virtual void SubmitActiveState(SortedSet<State> activeStates) {}

    public virtual bool IsConflictToEnterRegion(State substateToPend, SortedSet<State> enterRegionUnextended)
    {
        return false;
    }

    public virtual int SelectTransitions(SortedSet<Transition> enabledTransitions, bool isAuto)
    {
        return 1;
    }

    public virtual void ExtendEnterRegion(SortedSet<State> enterRegion, SortedSet<State> enterRegionEdge, SortedSet<State> extraEnterRegion, bool needCheckContain) {}

    public virtual void DeduceDescendants(SortedSet<State> deducedSet, bool isHistory, bool isEdgeState) {}

    public virtual void HandleSubstateEnter(State substate) {}

    public virtual void SelectReactions(SortedSet<Reaction> enabledReactions)
	{
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

    public virtual int LoadAllStateConfig(int[] config, int configIdx) { return configIdx; }

    public virtual int LoadActiveStateConfig(int[] config, int configIdx) { return configIdx; }

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
                isParentWarning = state._IsHistory;
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
