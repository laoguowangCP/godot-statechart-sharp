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
        get => HostState._LowerState;
        set => HostState._LowerState = value;
    }
    protected State UpperState
    {
        get => HostState._UpperState;
        set => HostState._UpperState = value;
    }
    public int _StateId;
    protected List<Transition> AutoTransitions;
    protected (List<Transition> Transitions, List<Reaction> Reactions)[] CurrentTAMap
    {
        get => HostStatechart._CurrentTAMap;
    }

    public StateImpl(State state)
    {
        HostState = state;
    }

    public virtual bool _IsValidState()
    {
        return true;
    }


    public virtual void _Setup(Statechart hostStateChart, ref int parentOrderId, int substateIdx)
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

        _StateId = HostStatechart._GetStateId();
    }

    public virtual void _SetupPost() {}

    public virtual bool _GetPromoteStates(List<State> states) { return false; }

    public virtual void _SubmitActiveState(SortedSet<State> activeStates) {}

    public virtual bool _IsConflictToEnterRegion(State substateToPend, SortedSet<State> enterRegionUnextended)
    {
        return false;
    }

    public virtual int _SelectTransitions(SortedSet<Transition> enabledTransitions, bool isAuto)
    {
        return 1;
    }

    public virtual void _ExtendEnterRegion(SortedSet<State> enterRegion, SortedSet<State> enterRegionEdge, SortedSet<State> extraEnterRegion, bool needCheckContain) {}

    public virtual void _DeduceDescendants(SortedSet<State> deducedSet) {}

    // TODO: rework deduce desendant
    public virtual void _DeduceDescendantsRecurr(SortedSet<State> deducedSet, DeduceDescendantsModeEnum deduceMode) {}

    public virtual void _HandleSubstateEnter(State substate) {}

    public virtual void _SelectReactions(SortedSet<Reaction> enabledReactions)
	{
        if (CurrentTAMap is null)
        {
            return;
        }

        var matched = CurrentTAMap[_StateId].Reactions;
        if (matched is null)
        {
            return;
        }

		foreach (Reaction reaction in matched)
		{
			enabledReactions.Add(reaction);
		}
	}

    public virtual void _SaveAllStateConfig(List<int> snapshot) {}

    public virtual void _SaveActiveStateConfig(List<int> snapshot) {}

    public virtual int _LoadAllStateConfig(int[] config, int configIdx) { return configIdx; }

    public virtual int _LoadActiveStateConfig(int[] config, int configIdx) { return configIdx; }

#if TOOLS
    public virtual void _GetConfigurationWarnings(List<string> warnings)
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
                isParentWarning = state._IsValidState();
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
