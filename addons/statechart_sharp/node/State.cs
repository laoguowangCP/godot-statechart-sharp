using Godot;
using System.Collections.Generic;


namespace LGWCP.StatechartSharp;

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
    [Signal] public delegate void EnterEventHandler(StatechartDuct duct);
    [Signal] public delegate void ExitEventHandler(StatechartDuct duct);
    
    #endregion

    [Export] internal StateModeEnum StateMode
    {
        get => _stateMode;
        set
        {
            _stateMode = value;
            #if TOOLS
            if (Engine.IsEditorHint())
            {
                StateComponent = GetStateComponent(value);
                UpdateConfigurationWarnings();
            }
            #endif
        }
    }
    private StateModeEnum _stateMode = StateModeEnum.Compound;
    [Export] internal bool IsDeepHistory { get; private set; }
    [Export] internal State InitialState
    {
        get { return _initialState; }
        set
        {
            _initialState = value;
            #if TOOLS
            if (Engine.IsEditorHint())
            {
                UpdateConfigurationWarnings();
            }
            #endif
        }
    }
    private State _initialState;
    internal State ParentState { get; set; }
    internal State CurrentState { get; set; }
    internal List<State> Substates { get; set; }
    internal List<Transition> Transitions { get; set; }
    internal List<Reaction> Reactions { get; set; }
    protected StateComponent StateComponent { get; set; }
    internal State LowerState { get; set; }
    internal State UpperState { get; set; }
    protected StatechartDuct Duct { get => HostStatechart.Duct; }
    internal bool IsHistory { get => StateMode == StateModeEnum.History; }
    
    public override void _Ready()
    {
        Substates = new List<State>();
        Transitions = new List<Transition>();
        Reactions = new List<Reaction>();

        StateComponent = GetStateComponent(StateMode);

        #if TOOLS
        if (Engine.IsEditorHint())
        {
            UpdateConfigurationWarnings();
        }
        #endif
    }

    public StateComponent GetStateComponent(StateModeEnum mode) => mode switch
    {
        StateModeEnum.Compound => new CompoundComponent(this),
        StateModeEnum.Parallel => new ParallelComponent(this),
        StateModeEnum.History => new HistoryComponent(this),
        _ => null
    };

    internal override void Setup(Statechart hostStateChart, ref int ancestorId)
    {
        base.Setup(hostStateChart, ref ancestorId);
        StateComponent.Setup(hostStateChart, ref ancestorId);
    }

    internal override void PostSetup()
    {
        StateComponent.PostSetup();
    }

    internal void StateEnter()
    {
        if (ParentState != null)
        {
            ParentState.HandleSubstateEnter(this);
        }
        
        // Duct.CompositionNode = this;
        // EmitSignal(SignalName.Enter, Duct);
        CustomStateEnter(Duct);
    }

    protected virtual void CustomStateEnter(StatechartDuct duct)
    {
        // Use signal by default
        duct.CompositionNode = this;
        EmitSignal(SignalName.Enter, duct);
    }

    internal void StateExit()
    {
        // Duct.CompositionNode = this;
        // EmitSignal(SignalName.Exit, Duct);
        CustomStateExit(Duct);
    }

    protected virtual void CustomStateExit(StatechartDuct duct)
    {
        // Use signal by default
        duct.CompositionNode = this;
        EmitSignal(SignalName.Exit, duct);
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

    internal int SelectTransitions(SortedSet<Transition> enabledTransitions, StringName eventName = null)
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

    internal void SelectReactions(SortedSet<Reaction> enabledReactions, StringName eventName)
    {
        foreach (Reaction a in Reactions)
        {
            if (a.Check(eventName))
            {
                enabledReactions.Add(a);
            }
        }
    }

    #if TOOLS
    public override string[] _GetConfigurationWarnings()
    {
        List<string> warnings = new List<string>();

        if (StateComponent != null)
        {
            StateComponent.GetConfigurationWarnings(warnings);
        }

        return warnings.ToArray();
    }
    #endif
}
