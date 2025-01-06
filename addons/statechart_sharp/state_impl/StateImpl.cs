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
	protected Dictionary<string, List<Transition>> Transitions = new();
    protected List<Transition> AutoTransitions = new();
	protected Dictionary<string, List<Reaction>> Reactions = new();

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
    }

    public virtual void _SetupPost() {}

    public virtual bool _SubmitPromoteStates(List<State> states) { return false; }

    public virtual void _SubmitActiveState(SortedSet<State> activeStates) {}

    public virtual bool _IsConflictToEnterRegion(State substateToPend, SortedSet<State> enterRegionUnextended)
    {
        return false;
    }

    public virtual int _SelectTransitions(SortedSet<Transition> enabledTransitions, string eventName)
    {
        return 1;
    }

    public virtual void _ExtendEnterRegion(SortedSet<State> enterRegion, SortedSet<State> enterRegionEdge, SortedSet<State> extraEnterRegion, bool needCheckContain) {}

    public virtual void _DeduceDescendants(SortedSet<State> deducedSet) {}

    public virtual void _DeduceDescendantsRecur(SortedSet<State> deducedSet, DeduceDescendantsModeEnum deduceMode) {}

    public virtual void _HandleSubstateEnter(State substate) {}

    public virtual void _SelectReactions(SortedSet<Reaction> enabledReactions, string eventName)
    {
        if (Reactions.TryGetValue(eventName, out var eventReactions))
        {
            foreach (Reaction a in eventReactions)
            {
                enabledReactions.Add(a);
            }
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
                isParentWarning = !state._IsValidState();
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
