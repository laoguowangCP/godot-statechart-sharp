using Godot;
using System.Collections.Generic;
using System.Linq;

namespace LGWCP.StatechartSharp
{

public enum StateModeEnum : int
{
    Compound,
    Parallel,
    History
}

[Tool]
[GlobalClass, Icon("res://addons/statechart_sharp/icon/State.svg")]
public partial class State : StatechartComposition
{
    #region define signals
    [Signal] public delegate void EnterEventHandler(State state);
    [Signal] public delegate void ExitEventHandler(State state);
    
    #endregion

    [Export] internal StateModeEnum StateMode { get; private set; } = StateModeEnum.Compound;
    [Export] internal bool IsDeepHistory { get; private set; }
    [Export] internal State InitialState
    {
        get { return _initialState; }
        set
        {
            _initialState = value;
            #if TOOLS
            UpdateConfigurationWarnings();
            #endif
        }
    }
    private State _initialState;
    internal State ParentState { get; set; }
    internal State CurrentState { get; set; }
    internal List<State> Substates { get; set; }
    internal List<Transition> Transitions { get; set; }
    internal List<Reaction> Actions { get; set; }
    protected StateComponent StateComponent { get; set; }
    internal State LowerState { get; set; }
    internal State UpperState { get; set; }
    internal bool IsHistory { get => StateMode == StateModeEnum.History; }

    public override void _Ready()
    {
        Substates = new List<State>();
        Transitions = new List<Transition>();
        Actions = new List<Reaction>();
        switch (StateMode)
        {
            case StateModeEnum.Compound:
                StateComponent = new CompoundComponent(this);
                break;
            case StateModeEnum.Parallel:
                StateComponent = new ParallelComponent(this);
                break;
            case StateModeEnum.History:
                StateComponent = new HistoryComponent(this);
                break;
        }
    }

    internal override void Setup(Statechart hostStateChart, ref int ancestorId)
    {
        base.Setup(hostStateChart, ref ancestorId);
        StateComponent.Init(hostStateChart, ref ancestorId);
    }

    internal override void PostSetup()
    {
        StateComponent.PostInit();
    }

    internal void StateEnter()
    {
        if (ParentState != null)
        {
            ParentState.HandleSubstateEnter(this);
        }
        
        EmitSignal(SignalName.Enter, this);
    }

    internal void StateExit()
    {
        EmitSignal(SignalName.Exit, this);
    }

    internal void StateInvoke(StringName eventName)
    {
        foreach (Reaction a in Actions)
        {
            a.ReactionInvoke(eventName);
        }
    }

    internal void RegisterActiveState(SortedSet<State> activeStates)
    {
        StateComponent.RegisterActiveState(activeStates);
    }

    internal bool IsConflictToEnterRegion(State substate, SortedSet<State> enterRegion)
    {
        return StateComponent.IsConflictToEnterRegion(substate, enterRegion);
    }

    internal void ExtendEnterRegion(SortedSet<State> enterRegion, SortedSet<State> enterRegionEdge, SortedSet<State> extraEnterRegion, bool needCheckContain = true)
    {
        StateComponent.ExtendEnterRegion(enterRegion, enterRegionEdge, extraEnterRegion, needCheckContain);
    }

    internal bool SelectTransitions(List<Transition> enabledTransitions, StringName eventName = null)
    {
        return StateComponent.SelectTransitions(enabledTransitions, eventName);
    }

    internal void DeduceDescendants(SortedSet<State> deducedSet, bool isHistory = false, bool isEdgeState = false)
    {
        StateComponent.DeduceDescendants(deducedSet, isHistory, isEdgeState);
    }

    internal void HandleSubstateEnter(State substate)
    {
        StateComponent.HandleSubstateEnter(substate);
    }

    #if TOOLS
    public override string[] _GetConfigurationWarnings()
    {
        var warnings = new List<string>();

        if (InitialState != null
            && (InitialState.GetParent() != this || InitialState.IsHistory))
        {
            warnings.Add("Initial state should be a non-history substate.");
        }

        return warnings.ToArray();
    }
    #endif
}

} // end of namespace