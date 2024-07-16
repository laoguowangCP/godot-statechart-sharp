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
[GlobalClass]
[Icon("res://addons/statechart_sharp/icon/State.svg")]
public partial class State : StatechartComposition
{
    #region signals
    
    [Signal] public delegate void EnterEventHandler(StatechartDuct duct);
    [Signal] public delegate void ExitEventHandler(StatechartDuct duct);
    
    #endregion


    #region properties

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
    /// <summary>
    /// The index of this state exists in parent state.
    /// </summary>
    internal int SubstateIdx;
    protected StatechartDuct Duct { get => HostStatechart.Duct; }
    internal bool IsHistory { get => StateMode == StateModeEnum.History; }
    
    #endregion


    #region methods

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

    internal bool GetPromoteStates(List<State> states)
    {
        return StateComponent.GetPromoteStates(states);
    }

    internal bool IsAvailableRootState()
    {
        return StateComponent.IsAvailableRootState();
    }

    internal override void Setup(Statechart hostStateChart, ref int parentOrderId)
    {
        base.Setup(hostStateChart, ref parentOrderId);
        // Called from statechart node, root state substate index is 0
        StateComponent.Setup(hostStateChart, ref parentOrderId, 0);
    }

    internal void Setup(Statechart hostStateChart, ref int parentOrderId, int substateIdx)
    {
        base.Setup(hostStateChart, ref parentOrderId);
        StateComponent.Setup(hostStateChart, ref parentOrderId, substateIdx);
    }

    internal override void PostSetup()
    {
        StateComponent.PostSetup();
    }

    internal void StateEnter()
    {
        ParentState?.HandleSubstateEnter(this);
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

    internal bool IsConflictToEnterRegion(State substateToPend, SortedSet<State> enterRegionUnextended)
    {
        return StateComponent.IsConflictToEnterRegion(substateToPend, enterRegionUnextended);
    }

    internal bool IsAncestorStateOf(State state)
    {
        int id = state.OrderId;

        // Leaf state
        if (LowerState is null || UpperState is null)
        {
            return false;
        }

        return id >= LowerState.OrderId
            && id <= UpperState.OrderId;
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
        foreach (Reaction reaction in Reactions)
        {
            if (reaction.Check(eventName))
            {
                enabledReactions.Add(reaction);
            }
        }
    }

    internal void SaveAllStateConfig(ref List<int> config)
    {
        StateComponent.SaveAllStateConfig(ref config);
    }

    internal void SaveActiveStateConfig(ref List<int> config)
    {
        StateComponent.SaveActiveStateConfig(ref config);
    }

    internal bool LoadAllStateConfig(ref int[] config)
    {
        return StateComponent.LoadAllStateConfig(ref config);
    }

    #if TOOLS
    public override string[] _GetConfigurationWarnings()
    {
        List<string> warnings = new List<string>();
        StateComponent?.GetConfigurationWarnings(warnings);
        return warnings.ToArray();
    }
    #endif

    #endregion
}
