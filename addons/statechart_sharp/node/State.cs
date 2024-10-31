using Godot;
using System;
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
				StateImpl = GetStateImpl(value);
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
	public State ParentState;
	public State CurrentState;
	public List<State> Substates;
	public Dictionary<StringName, List<Transition>> Transitions;
	public List<Transition> AutoTransitions;
	public Dictionary<StringName, List<Reaction>> Reactions;
	protected StateImpl StateImpl;
	public State LowerState;
	public State UpperState;
	/// <summary>
	/// The index of this state exists in parent state.
	/// </summary>
	public int SubstateIdx;
	/// <summary>
	/// The ID of this state in statechart.
	/// </summary>
	public int StateId
	{
        set => StateImpl.StateId = value;
    }
	protected StatechartDuct Duct;
	public bool IsHistory;
	
	#endregion


	#region methods

	public override void _Ready()
	{
		Substates = new List<State>();
		Transitions = new Dictionary<StringName, List<Transition>>();
		AutoTransitions = new List<Transition>();
		Reactions = new Dictionary<StringName, List<Reaction>>();

		StateImpl = GetStateImpl(StateMode);
		IsHistory = StateMode == StateModeEnum.History;

		// Cache methods
		if (StateImpl is not null)
		{
			GetPromoteStates = StateImpl.GetPromoteStates;
			IsAvailableRootState = StateImpl.IsAvailableRootState;
			RegisterActiveState = StateImpl.RegisterActiveState;
			IsConflictToEnterRegion = StateImpl.IsConflictToEnterRegion;
			ExtendEnterRegion = StateImpl.ExtendEnterRegion;
			SelectTransitions = StateImpl.SelectTransitions;
			DeduceDescendants = StateImpl.DeduceDescendants;
			HandleSubstateEnter = StateImpl.HandleSubstateEnter;
			SelectReactions = StateImpl.SelectReactions;

			SaveAllStateConfig = StateImpl.SaveAllStateConfig;
			SaveActiveStateConfig = StateImpl.SaveActiveStateConfig;
			LoadAllStateConfig = StateImpl.LoadAllStateConfig;
			LoadActiveStateConfig = StateImpl.LoadActiveStateConfig;
		}

		#if TOOLS
		if (Engine.IsEditorHint())
		{
			UpdateConfigurationWarnings();
		}
		#endif
	}

	protected StateImpl GetStateImpl(StateModeEnum mode) => mode switch
	{
		StateModeEnum.Compound => new CompoundImpl(this),
		StateModeEnum.Parallel => new ParallelImpl(this),
		StateModeEnum.History => new HistoryImpl(this),
		_ => new CompoundImpl(this)
	};

	public override void Setup(Statechart hostStateChart, ref int parentOrderId)
	{
		base.Setup(hostStateChart, ref parentOrderId);
		Duct = HostStatechart.Duct;
		StateId = HostStatechart.GetStateId();

		// Called from statechart node, root state substate index is 0
		StateImpl.Setup(hostStateChart, ref parentOrderId, 0);
	}

	public void Setup(Statechart hostStateChart, ref int parentOrderId, int substateIdx)
	{
		base.Setup(hostStateChart, ref parentOrderId);
		Duct = HostStatechart.Duct;
		StateId = HostStatechart.GetStateId();

		StateImpl.Setup(hostStateChart, ref parentOrderId, substateIdx);
	}

	public override void PostSetup()
	{
		StateImpl.PostSetup();
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

	public Func<List<State>, bool> GetPromoteStates;
	public Func<bool> IsAvailableRootState;
	public Action<SortedSet<State>> RegisterActiveState;
	public Func<State, SortedSet<State>, bool> IsConflictToEnterRegion;
	public Action<SortedSet<State>, SortedSet<State>, SortedSet<State>, bool> ExtendEnterRegion;
	public Func<SortedSet<Transition>, bool, int> SelectTransitions;
	public Action<SortedSet<State>, bool, bool> DeduceDescendants;
	public Action<State> HandleSubstateEnter;
	public Action<SortedSet<Reaction>, StringName> SelectReactions;
	public Action<List<int>> SaveAllStateConfig;
	public Action<List<int>> SaveActiveStateConfig;
	public Func<int[], IntParser, bool> LoadAllStateConfig;
	public Func<int[], IntParser, bool> LoadActiveStateConfig;

	#if TOOLS
    public override string[] _GetConfigurationWarnings()
	{
		List<string> warnings = new List<string>();
		StateImpl?.GetConfigurationWarnings(warnings);
		return warnings.ToArray();
	}
	#endif

	#endregion
}
