using Godot;
using System.Collections.Generic;


namespace LGWCP.Godot.StatechartSharp;

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
	
	[Signal]
	public delegate void EnterEventHandler(StatechartDuct duct);
	[Signal]
	public delegate void ExitEventHandler(StatechartDuct duct);
	
	#endregion


	#region properties

	[Export]
	public StateModeEnum StateMode
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
	[Export]
	public bool IsDeepHistory { get; private set; }
	[Export]
	public State InitialState
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
	public State ParentState { get; set; }
	public State CurrentState { get; set; }
	public List<State> Substates { get; set; }
	public Dictionary<StringName, List<Transition>> Transitions { get; set; }
	public List<Transition> AutoTransitions { get; set; }
	public Dictionary<StringName, List<Reaction>> Reactions { get; set; }
	protected StateImpl StateComponent { get; set; }
	public State LowerState { get; set; }
	public State UpperState { get; set; }
	/// <summary>
	/// The index of this state exists in parent state.
	/// </summary>
	public int SubstateIdx;
	protected StatechartDuct Duct { get => HostStatechart.Duct; }
	public bool IsHistory { get => StateMode == StateModeEnum.History; }
	
	#endregion


	#region methods

	public override void _Ready()
	{
		Substates = new List<State>();
		Transitions = new Dictionary<StringName, List<Transition>>();
		AutoTransitions = new List<Transition>();
		Reactions = new Dictionary<StringName, List<Reaction>>();

		StateComponent = GetStateComponent(StateMode);

		#if TOOLS
		if (Engine.IsEditorHint())
		{
			UpdateConfigurationWarnings();
		}
		#endif
	}

	protected StateImpl GetStateComponent(StateModeEnum mode) => mode switch
	{
		StateModeEnum.Compound => new CompoundImpl(this),
		StateModeEnum.Parallel => new ParallelImpl(this),
		StateModeEnum.History => new HistoryImpl(this),
		_ => null
	};

	public bool GetPromoteStates(List<State> states)
	{
		return StateComponent.GetPromoteStates(states);
	}

	public bool IsAvailableRootState()
	{
		return StateComponent.IsAvailableRootState();
	}

	public override void Setup(Statechart hostStateChart, ref int parentOrderId)
	{
		base.Setup(hostStateChart, ref parentOrderId);
		// Called from statechart node, root state substate index is 0
		StateComponent.Setup(hostStateChart, ref parentOrderId, 0);
	}

	public void Setup(Statechart hostStateChart, ref int parentOrderId, int substateIdx)
	{
		base.Setup(hostStateChart, ref parentOrderId);
		StateComponent.Setup(hostStateChart, ref parentOrderId, substateIdx);
	}

	public override void PostSetup()
	{
		StateComponent.PostSetup();
	}

	public void StateEnter()
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

	public void StateExit()
	{
		CustomStateExit(Duct);
	}

	protected virtual void CustomStateExit(StatechartDuct duct)
	{
		// Use signal by default
		duct.CompositionNode = this;
		EmitSignal(SignalName.Exit, duct);
	}

	public void RegisterActiveState(SortedSet<State> activeStates)
	{
		StateComponent.RegisterActiveState(activeStates);
	}

	public bool IsConflictToEnterRegion(State substateToPend, SortedSet<State> enterRegionUnextended)
	{
		return StateComponent.IsConflictToEnterRegion(substateToPend, enterRegionUnextended);
	}

	public bool IsAncestorStateOf(State state)
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

	public void ExtendEnterRegion(SortedSet<State> enterRegion, SortedSet<State> enterRegionEdge, SortedSet<State> extraEnterRegion, bool needCheckContain = true)
	{
		StateComponent.ExtendEnterRegion(enterRegion, enterRegionEdge, extraEnterRegion, needCheckContain);
	}

	public int SelectTransitions(SortedSet<Transition> enabledTransitions, StringName eventName = null)
	{
		return StateComponent.SelectTransitions(enabledTransitions, eventName);
	}

	public void DeduceDescendants(SortedSet<State> deducedSet, bool isHistory = false, bool isEdgeState = false)
	{
		StateComponent.DeduceDescendants(deducedSet, isHistory, isEdgeState);
	}

	public void HandleSubstateEnter(State substate)
	{
		StateComponent.HandleSubstateEnter(substate);
	}

	public void SelectReactions(SortedSet<Reaction> enabledReactions, StringName eventName)
	{
		bool hasEventName = Reactions.TryGetValue(eventName, out var matchedReactions);
		if (!hasEventName)
		{
			return;
		}

		foreach (Reaction reaction in matchedReactions)
		{
			enabledReactions.Add(reaction);
		}
	}

	public void SaveAllStateConfig(ref List<int> config)
	{
		StateComponent.SaveAllStateConfig(ref config);
	}

	public void SaveActiveStateConfig(ref List<int> config)
	{
		StateComponent.SaveActiveStateConfig(ref config);
	}

	public bool LoadAllStateConfig(ref int[] config, ref int configIdx)
	{
		return StateComponent.LoadAllStateConfig(ref config, ref configIdx);
	}

	public bool LoadActiveStateConfig(ref int[] config, ref int configIdx)
	{
		return StateComponent.LoadActiveStateConfig(ref config, ref configIdx);
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
