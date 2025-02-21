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
	protected StateImpl StateImpl;
	public State _LowerState;
	public State _UpperState;
	/// <summary>
	/// The index of this state enlisted in parent state.
	/// </summary>
	public int _SubstateIdx;
	protected StatechartDuct Duct;

#endregion


#region method

	public override void _Ready()
	{
		_Substates = new List<State>();

		StateImpl = GetStateImpl(StateMode);

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

	public bool _IsAncestorStateOfAny(SortedSet<State> states)
	{
		bool isAncestorStateOfAny = false;
		if (_LowerState == null || _UpperState == null)
		{
			return false;
		}
		int lcaLowerId = _LowerState._OrderId;
		int lcaUpperId = _UpperState._OrderId;
		foreach (var state in states)
		{
			int orderId = state._OrderId;
			if (orderId < lcaLowerId)
			{
				continue;
			}
			else if (orderId > lcaUpperId)
			{
				break;
			}
			else
			{
				isAncestorStateOfAny = true;
				break;
			}
		}

		return isAncestorStateOfAny;
	}
	public bool _IsAncestorStateOfAnyReversed(SortedSet<State> states)
	{
		bool isAncestorStateOfAny = false;
		int lcaLowerId = _LowerState._OrderId;
		int lcaUpperId = _UpperState._OrderId;
		foreach (var state in states)
		{
			int orderId = state._OrderId;
			if (orderId > lcaUpperId)
			{
				continue;
			}
			else if (orderId < lcaLowerId)
			{
				break;
			}
			else
			{
				isAncestorStateOfAny = true;
				break;
			}
		}

		return isAncestorStateOfAny;
	}

	public bool _IsConflictToEnterRegion(State substateToPend, SortedSet<State> enterRegionUnextended)
	{
		return StateImpl._IsConflictToEnterRegion(substateToPend, enterRegionUnextended);
	}

	public bool _SubmitPromoteStates(List<State> states)
	{
		return StateImpl._SubmitPromoteStates(states);
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
	
	public void _SubmitActiveState(SortedSet<State> states)
	{
		StateImpl._SubmitActiveState(states);
	}

	public void _ExtendEnterRegion(
		SortedSet<State> enterRegion,
		SortedSet<State> enterRegionEdge,
		SortedSet<State> extraEnterRegion,
		bool needCheckContain)
	{
		StateImpl._ExtendEnterRegion(enterRegion, enterRegionEdge, extraEnterRegion, needCheckContain);
	}

	public int _SelectTransitions(SortedSet<Transition> enabledTransitions, StringName eventName)
	{
		return StateImpl._SelectTransitions(enabledTransitions, eventName);
	}

	public void _DeduceDescendants(SortedSet<State> deducedSet)
	{
		StateImpl._DeduceDescendants(deducedSet);
	}

	public void _DeduceDescendantsRecur(SortedSet<State> deducedSet, DeduceDescendantsModeEnum deduceMode)
	{
		StateImpl._DeduceDescendantsRecur(deducedSet, deduceMode);
	}

	public void _HandleSubstateEnter(State substate)
	{
		StateImpl._HandleSubstateEnter(substate);
	}

    public void _SelectReactions(SortedSet<Reaction> enabledReactions, StringName eventName)
	{
		StateImpl._SelectReactions(enabledReactions, eventName);
	}

	public void _SaveAllStateConfig(List<int> snapshot)
	{
		StateImpl._SaveAllStateConfig(snapshot);
	}

	public void _SaveActiveStateConfig(List<int> snapshot)
	{
		StateImpl._SaveActiveStateConfig(snapshot);
	}

	public int _LoadAllStateConfig(int[] config, int configIdx)
	{
		return StateImpl._LoadAllStateConfig(config, configIdx);
	}

	public int _LoadActiveStateConfig(int[] config, int configIdx)
	{
		return StateImpl._LoadActiveStateConfig(config, configIdx);
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
