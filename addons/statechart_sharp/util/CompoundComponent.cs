using System.Collections.Generic;
using System.Linq;
using Godot;


namespace LGWCP.StatechartSharp;

public class CompoundComponent : StateComponent
{
    private State CurrentState
    {
        get => HostState.CurrentState;
        set { HostState.CurrentState = value; }
    }
    private State InitialState
    {
        get => HostState.InitialState;
        set { HostState.InitialState = value; }
    }
    
    public CompoundComponent(State state) : base(state) {}

    internal override bool IsAvailableRootState()
    {
        return true;
    }

    internal override void Setup(Statechart hostStateChart, ref int parentOrderId, int substateIdx)
    {
        base.Setup(hostStateChart, ref parentOrderId, substateIdx);

        // Init & collect states, transitions, actions
        // Get lower state and upper state
        State lastSubstate = null;
        foreach (Node child in HostState.GetChildren())
        {
            if (child.IsQueuedForDeletion())
            {
                continue;
            }
            
            if (child is State s)
            {
                s.Setup(hostStateChart, ref parentOrderId, Substates.Count);
                Substates.Add(s);

                // First substate is lower-state
                if (LowerState is null)
                {
                    LowerState = s;
                }
                lastSubstate = s;
            }
            else if (child is Transition t)
            {
                // Root state should not have transition
                if (ParentState != null)
                {
                    t.Setup(hostStateChart, ref parentOrderId);
                    Transitions.Add(t);
                }
            }
            else if (child is Reaction a)
            {
                a.Setup(hostStateChart, ref parentOrderId);
                Reactions.Add(a);
            }
        }

        if (lastSubstate != null)
        {
            if (lastSubstate.UpperState != null)
            {
                // Last substate's upper is upper-state
                UpperState = lastSubstate.UpperState;
            }
            else
            {
                // Last substate is upper-state
                UpperState = lastSubstate;
            }
        }
        // Else state is atomic, lower and upper are both null.

        // Set initial state
        if (InitialState != null)
        {
            // Check selected initial-state is non-history substate
            if (InitialState.ParentState != HostState || InitialState.IsHistory)
            {
                #if DEBUG
                GD.PushWarning(
                    HostState.GetPath(),
                    ": initial-state should be a non-history substate.");
                #endif
                InitialState = null;
            }
        }

        // No assigned initial state, use first non-history substate
        if (InitialState == null)
        {
            foreach (State substate in Substates)
            {
                if (substate.StateMode != StateModeEnum.History)
                {
                    InitialState = substate;
                    break;
                }
            }
        }

        // Set current state
        CurrentState = InitialState;
    }

    internal override void PostSetup()
    {
        foreach (State state in Substates)
        {
            state.PostSetup();
        }

        foreach (Transition trans in Transitions)
        {
            trans.PostSetup();
        }

        foreach (Reaction react in Reactions)
        {
            react.PostSetup();
        }
    }

    internal override bool IsConflictToEnterRegion(
        State substateToPend,
        SortedSet<State> enterRegionUnextended)
    {
        // Conflicts if any substate is already exist in region
        return enterRegionUnextended.Any<State>(
            state => HostState.IsAncestorStateOf(state));
    }

    internal override void ExtendEnterRegion(
        SortedSet<State> enterRegion,
        SortedSet<State> enterRegionEdge,
        SortedSet<State> extraEnterRegion,
        bool needCheckContain)
    {
        /*
        if need check (state is in region, checked by parent):
            if any substate in region:
                extend this substate, still need check
            else (no substate in region)
                extend initial state, need no check
        else (need no check)
            add state to extra-region
            extend initial state, need no check
        */

        // Need no check, add to extra
        if (!needCheckContain)
        {
            extraEnterRegion.Add(HostState);
        }

        // Need check
        if (needCheckContain)
        {
            foreach (State substate in Substates)
            {
                if (enterRegion.Contains(substate))
                {
                    substate.ExtendEnterRegion(
                        enterRegion, enterRegionEdge, extraEnterRegion);
                    return;
                }
            }
        }

        // Need no check, or need check but no substate in region
        if (InitialState != null)
        {
            InitialState.ExtendEnterRegion(
                enterRegion, enterRegionEdge, extraEnterRegion, false);
        }
    }

    internal override bool GetPromoteStates(List<State> states)
    {
        bool isPromote = true;
        foreach (Node child in HostState.GetChildren())
        {
            if (child is State state)
            {
                bool isChildPromoted = state.GetPromoteStates(states);
                if (isChildPromoted)
                {
                    isPromote = false;
                }
            }
        }

        if (isPromote)
        {
            states.Add(HostState);
        }

        // Make sure promoted
        return true;
    }
    
    internal override void RegisterActiveState(SortedSet<State> activeStates)
    {
        activeStates.Add(HostState);
        CurrentState?.RegisterActiveState(activeStates);
    }

    internal override int SelectTransitions(
        SortedSet<Transition> enabledTransitions, StringName eventName)
    {
        int handleInfo = -1;
        if (HostState.CurrentState != null)
        {
            handleInfo = CurrentState.SelectTransitions(
                enabledTransitions, eventName);
        }

        /*
        Check source's transitions:
            - < 0, check any
            - == 0, only check targetless
            - > 0, do nothing
        */
        if (handleInfo > 0)
        {
            return handleInfo;
        }

        foreach (Transition transtion in Transitions)
        {
            // If == 0, only check targetless
            if (handleInfo == 0)
            {
                if (!transtion.IsTargetless)
                {
                    continue;
                }
            }

            bool isEnabled = transtion.Check(eventName);
            if (isEnabled)
            {
                enabledTransitions.Add(transtion);
                handleInfo = 1;
                break;
            }
        }

        return handleInfo;
    }

    internal override void DeduceDescendants(
        SortedSet<State> deducedSet, bool isHistory, bool isEdgeState)
    {
        /* 
        If is edge state:
            1. It is called from history substate
            2. IsHistory arg represents IsDeepHistory
        */
        if (isEdgeState)
        {
            if (CurrentState != null)
            {
                bool isDeepHistory = isHistory;
                CurrentState.DeduceDescendants(deducedSet, isDeepHistory);
            }
            return;
        }

        // Not edge state
        deducedSet.Add(HostState);
        State deducedSubstate = isHistory ? CurrentState : InitialState;

        if (deducedSubstate != null)
        {
            deducedSubstate.DeduceDescendants(deducedSet, isHistory);
        }
    }

    internal override void HandleSubstateEnter(State substate)
    {
        HostState.CurrentState = substate;
    }

    internal override bool Save(ref List<int> snapshot, bool isAllStateConfig)
    {
        // TODO: save
        if (isAllStateConfig)
        {
            // all config
        }
        else
        {
            // Active states only
            snapshot.Append(CurrentState.SubstateIdx);
        }
    }

    #if TOOLS
    internal override void GetConfigurationWarnings(List<string> warnings)
    {
        // Check parent
        base.GetConfigurationWarnings(warnings);

        // Check child
        if (InitialState != null)
        {
            var initialStateParent = InitialState.GetParentOrNull<Node>();
            if (initialStateParent is null
                || initialStateParent != HostState
                || InitialState.IsHistory)
            {
                warnings.Add("Initial state should be a non-history substate.");
            }
        }
    }
    #endif
}
