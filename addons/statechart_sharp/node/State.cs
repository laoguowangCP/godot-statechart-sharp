using Godot;
using System;
using System.Collections.Generic;


namespace LGWCP.Godot.StatechartSharp;

public enum StateModeEnum : int
{
	Compound,
	Parallel,
	History,
	DeepHistory
}

public enum DeduceDescendantsModeEnum : int
{
	Initial,
	History,
	DeepHistory
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
        set => StateImpl._StateId = value;
    }
	protected StatechartDuct Duct;

#endregion


#region func cache
	public Action<SortedSet<State>> _SubmitActiveState;
	public Action<SortedSet<State>, SortedSet<State>, SortedSet<State>, bool> _ExtendEnterRegion;
	public Func<SortedSet<Transition>, bool, int> _SelectTransitions;
	public Action<SortedSet<State>> _DeduceDescendants;
	public Action<SortedSet<State>, DeduceDescendantsModeEnum> _DeduceDescendantsRecurr;
	public Action<State> _HandleSubstateEnter;
	public Action<SortedSet<Reaction>> _SelectReactions;
	public Action<List<int>> _SaveAllStateConfig;
	public Action<List<int>> _SaveActiveStateConfig;
	public Func<int[], int, int> _LoadAllStateConfig;
	public Func<int[], int, int> _LoadActiveStateConfig;

#endregion


#region method

	public override void _Ready()
	{
		_Substates = new List<State>();
		_AutoTransitions = new List<Transition>();

		StateImpl = GetStateImpl(StateMode);

		// Cache methods
		if (StateImpl is not null)
		{
			_SubmitActiveState = StateImpl._SubmitActiveState;
			_ExtendEnterRegion = StateImpl._ExtendEnterRegion;
			_SelectTransitions = StateImpl._SelectTransitions;
			_DeduceDescendants = StateImpl._DeduceDescendants;
			_DeduceDescendantsRecurr = StateImpl._DeduceDescendantsRecurr;
			_HandleSubstateEnter = StateImpl._HandleSubstateEnter;
			_SelectReactions = StateImpl._SelectReactions;

			_SaveAllStateConfig = StateImpl._SaveAllStateConfig;
			_SaveActiveStateConfig = StateImpl._SaveActiveStateConfig;
			_LoadAllStateConfig = StateImpl._LoadAllStateConfig;
			_LoadActiveStateConfig = StateImpl._LoadActiveStateConfig;
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
		StateModeEnum.DeepHistory => new DeepHistoryImpl(this),
		_ => new CompoundImpl(this)
	};

	public override void _Setup(Statechart hostStateChart, ref int parentOrderId)
	{
		base._Setup(hostStateChart, ref parentOrderId);
		Duct = _HostStatechart._Duct;

		// Called from statechart node, root state substate index is 0
		StateImpl._Setup(hostStateChart, ref parentOrderId, 0);
	}

	public void _Setup(Statechart hostStateChart, ref int parentOrderId, int substateIdx)
	{
		base._Setup(hostStateChart, ref parentOrderId);
		Duct = _HostStatechart._Duct;
		_StateId = _HostStatechart._GetStateId();

		StateImpl._Setup(hostStateChart, ref parentOrderId, substateIdx);
	}

	public override void _SetupPost()
	{
		StateImpl._SetupPost();
	}

	public void _StateEnter()
	{
		_ParentState?._HandleSubstateEnter(this);
		CustomStateEnter(Duct);
	}

	protected virtual void CustomStateEnter(StatechartDuct duct)
	{
		// Use signal by default
		duct.CompositionNode = this;
		EmitSignal(SignalName.Enter, duct);
	}

	public void _StateExit()
	{
		CustomStateExit(Duct);
	}

	protected virtual void CustomStateExit(StatechartDuct duct)
	{
		// Use signal by default
		duct.CompositionNode = this;
		EmitSignal(SignalName.Exit, duct);
	}

	public bool _IsAncestorStateOf(State state)
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

	public bool _IsConflictToEnterRegion(State substateToPend, SortedSet<State> enterRegionUnextended)
	{
		return StateImpl._IsConflictToEnterRegion(substateToPend, enterRegionUnextended);
	}

	public bool _GetPromoteStates(List<State> states)
	{
		return StateImpl._GetPromoteStates(states);
	}

	public bool _IsValidState()
	{
		/*
		Valid state is virtual concept to divide compound and parallel from history. Being valid, the state:

		- Can have transition and reaction as child.
		- Can be active state.
		- Can be initial state.
		- Can be root state.
		- Is stable, meaning if targeted in transition, enter region is determined.
		- For now, non-history state.
		*/
		return StateImpl._IsValidState();
	}

#if TOOLS
    public override string[] _GetConfigurationWarnings()
	{
		List<string> warnings = new List<string>();
		StateImpl?._GetConfigurationWarnings(warnings);
		return warnings.ToArray();
	}
#endif

#endregion

}
