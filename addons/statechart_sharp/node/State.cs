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


#region signal

	[Signal]
	public delegate void EnterEventHandler(StatechartDuct duct);
	[Signal]
	public delegate void ExitEventHandler(StatechartDuct duct);

#endregion


#region property

	[Export]
	public StateModeEnum StateMode
#if DEBUG
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
	private StateModeEnum _stateMode
#endif
		= StateModeEnum.Compound;

	[Export]
	public bool IsDeepHistory;
	[Export]
	public State InitialState
#if DEBUG
	{
		get => _initialState;
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
	private State _initialState
#endif
		;

	public State _ParentState;
	public State _CurrentState;
	public List<State> _Substates;
	public List<Transition> _AutoTransitions;
	protected StateImpl StateImpl;
	public State _LowerState;
	public State _UpperState;
	/// <summary>
	/// The index of this state enlisted in parent state.
	/// </summary>
	public int _SubstateIdx;
	/// <summary>
	/// The ID of this state in statechart.
	/// </summary>
	public int _StateId
	{
        set => StateImpl.StateId = value;
    }
	protected StatechartDuct Duct;
	public bool _IsHistory;

#endregion


#region func cache
	public Func<List<State>, bool> GetPromoteStates;
	public Func<bool> IsAvailableRootState;
	public Action<SortedSet<State>> SubmitActiveState;
	public Func<State, SortedSet<State>, bool> IsConflictToEnterRegion;
	public Action<SortedSet<State>, SortedSet<State>, SortedSet<State>, bool> ExtendEnterRegion;
	public Func<SortedSet<Transition>, bool, int> SelectTransitions;
	public Action<SortedSet<State>, bool, bool> DeduceDescendants;
	public Action<State> HandleSubstateEnter;
	public Action<SortedSet<Reaction>> SelectReactions;
	public Action<List<int>> SaveAllStateConfig;
	public Action<List<int>> SaveActiveStateConfig;
	public Func<int[], int, int> LoadAllStateConfig;
	public Func<int[], int, int> LoadActiveStateConfig;

#endregion


#region method

	public override void _Ready()
	{
		_Substates = new List<State>();
		_AutoTransitions = new List<Transition>();

		StateImpl = GetStateImpl(StateMode);
		_IsHistory = StateMode == StateModeEnum.History;

		// Cache methods
		if (StateImpl is not null)
		{
			GetPromoteStates = StateImpl.GetPromoteStates;
			IsAvailableRootState = StateImpl.IsAvailableRootState;
			SubmitActiveState = StateImpl.SubmitActiveState;
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

	public override void _Setup(Statechart hostStateChart, ref int parentOrderId)
	{
		base._Setup(hostStateChart, ref parentOrderId);
		Duct = _HostStatechart._Duct;
		_StateId = _HostStatechart.GetStateId();

		// Called from statechart node, root state substate index is 0
		StateImpl.Setup(hostStateChart, ref parentOrderId, 0);
	}

	public void Setup(Statechart hostStateChart, ref int parentOrderId, int substateIdx)
	{
		base._Setup(hostStateChart, ref parentOrderId);
		Duct = _HostStatechart._Duct;
		_StateId = _HostStatechart.GetStateId();

		StateImpl.Setup(hostStateChart, ref parentOrderId, substateIdx);
	}

	public override void _SetupPost()
	{
		StateImpl.SetupPost();
	}

	public void StateEnter()
	{
		_ParentState?.HandleSubstateEnter(this);
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
		int id = state._OrderId;

		// Leaf state
		if (_LowerState is null || _UpperState is null)
		{
			return false;
		}

		return id >= _LowerState._OrderId
			&& id <= _UpperState._OrderId;
	}

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
