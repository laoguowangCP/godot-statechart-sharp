using Godot;
using System.Collections.Generic;
using System.Linq;


namespace LGWCP.Godot.StatechartSharp;

[Tool]
[GlobalClass]
[Icon("res://addons/statechart_sharp/icon/Statechart.svg")]
public partial class Statechart : StatechartComposition
{

#region property

	[Export(PropertyHint.Range, "0,32,")]
	public int MaxInternalEventCount = 8;
	[Export(PropertyHint.Range, "0,32,")]
	public int MaxAutoTransitionRound = 8;
	[Export(PropertyHint.Flags, "Process,Physics Process,Input,Shortcut Input,UnhandledKey Input,Unhandled Input")]
	public EventFlagEnum EventFlag = 0;
	[Export]
	public bool IsWaitParentReady = true;
	protected bool IsRunning;
	protected int EventCount;
	protected State RootState;
	protected SortedSet<State> ActiveStates;
	protected Queue<StringName> QueuedEvents;
	protected SortedSet<Transition> EnabledTransitions;
	protected SortedSet<Transition> EnabledFilteredTransitions;
	protected SortedSet<State> ExitSet;
	protected SortedSet<State> EnterSet;
	protected SortedSet<Reaction> EnabledReactions;
	protected List<int> SnapshotConfig;
	protected StringName LastStepEventName = "_";

	// Total count of states in statechart
	protected int StateLength = 0;

	public StatechartDuct _Duct;

	// Global transition/reaction hashmap
	public Dictionary<StringName, (List<Transition>, List<Reaction>)[]> _GlobalEventTAMap;
	public (List<Transition>, List<Reaction>)[] _CurrentTAMap;

#endregion


#region method

	public override void _Ready()
	{
		_Duct = new StatechartDuct(this);

		IsRunning = false;
		EventCount = 0;

		ActiveStates = new SortedSet<State>(new StatechartComparer<State>());
		QueuedEvents = new Queue<StringName>();
		EnabledTransitions = new SortedSet<Transition>(new StatechartComparer<Transition>());
		EnabledFilteredTransitions = new SortedSet<Transition>(new StatechartComparer<Transition>());
		ExitSet = new SortedSet<State>(new StatechartReversedComparer<State>());
		EnterSet = new SortedSet<State>(new StatechartComparer<State>());
		EnabledReactions = new SortedSet<Reaction>(new StatechartComparer<Reaction>());

		_GlobalEventTAMap = new Dictionary<StringName, (List<Transition>, List<Reaction>)[]>();

		SnapshotConfig = new List<int>();

#if TOOLS
		if (!Engine.IsEditorHint())
		{
#endif

		// Statechart setup async
		StartSetUp();

#if TOOLS
		}
		else
		{
			UpdateConfigurationWarnings();
		}
#endif
	}

	protected async void StartSetUp()
	{
		if (IsWaitParentReady)
		{
			Node parentNode = GetParentOrNull<Node>();
			if (parentNode is not null
				&& !parentNode.IsNodeReady())
			{
				await ToSignal(parentNode, Node.SignalName.Ready);
			}
		}

		_Setup();
		_SetupPost();
	}

	public override void _Setup()
	{
		_HostStatechart = this;
		foreach (Node child in GetChildren())
		{
			if (child is State state)
			{
				if (state._IsAvailableRootState())
				{
					RootState = state;
					break;
				}
			}
		}

		// Statechart node order id = -1
		_OrderId = -1;
		if (RootState is not null)
		{
			// Root state node order id = 0
			int orderId = 0;
			RootState._Setup(this, ref orderId);
		}
	}

	public override void _SetupPost()
	{
		// Get and enter active states
		if (RootState is not null)
		{
			RootState._SubmitActiveState(ActiveStates);
			RootState._SetupPost();
		}

		foreach (State state in ActiveStates)
		{
			state._StateEnter();
		}

		// Set node process according to flags
		SetProcess(
			EventFlag.HasFlag(EventFlagEnum.Process));
		SetPhysicsProcess(
			EventFlag.HasFlag(EventFlagEnum.PhysicsProcess));
		SetProcessInput(
			EventFlag.HasFlag(EventFlagEnum.Input));
		SetProcessShortcutInput(
			EventFlag.HasFlag(EventFlagEnum.ShortcutInput));
		SetProcessUnhandledKeyInput(
			EventFlag.HasFlag(EventFlagEnum.UnhandledKeyInput));
		SetProcessUnhandledInput(
			EventFlag.HasFlag(EventFlagEnum.UnhandledInput));
	}

	public int _GetStateId()
	{
		int id = StateLength;
		++StateLength;
		return id;
	}

	public void _RegistGlobalTransition(int stateId, StringName eventName, Transition trans)
	{
		(List<Transition> Transitions, List<Reaction> _)[] globalTA;
		bool isEventInTable = _GlobalEventTAMap.TryGetValue(eventName, out globalTA);
		if (isEventInTable)
		{
			if (globalTA[stateId].Transitions is null)
			{
				globalTA[stateId].Transitions = new() { trans };
			}
			else
			{
				globalTA[stateId].Transitions.Add(trans);
			}
		}
		else
		{
			globalTA = new (List<Transition>, List<Reaction>)[StateLength];
			globalTA[stateId].Transitions = new() { trans };
			_GlobalEventTAMap.Add(eventName, globalTA);
		}
	}

	public void _RegistGlobalReaction(int stateId, StringName eventName, Reaction react)
	{
		(List<Transition> _, List<Reaction> Reactions)[] globalTA;
		bool isEventInTable = _GlobalEventTAMap.TryGetValue(eventName, out globalTA);
		if (isEventInTable)
		{
			if (globalTA[stateId].Reactions is null)
			{
				globalTA[stateId].Reactions = new() { react };
			}
			else
			{
				globalTA[stateId].Reactions.Add(react);
			}
		}
		else
		{
			globalTA = new (List<Transition>, List<Reaction>)[StateLength];
			globalTA[stateId].Reactions = new() { react };
			_GlobalEventTAMap.Add(eventName, globalTA);
		}
	}

	public void Step(StringName eventName)
	{
		if (eventName == null) // Empty StringName is not constructed
		{
			return;
		}

		if (IsRunning)
		{
			if (EventCount <= MaxInternalEventCount)
			{
				++EventCount;
				QueuedEvents.Enqueue(eventName);
			}
			return;
		}

		// Else is not running
		++EventCount;
		QueuedEvents.Enqueue(eventName);

		IsRunning = true;

		while (QueuedEvents.Count > 0)
		{
			StringName nextEvent = QueuedEvents.Dequeue();
			HandleEvent(nextEvent);
		}

		IsRunning = false;
		EventCount = 0;
	}

	public StatechartSnapshot Save(bool isAllStateConfig = false)
	{
		if (IsRunning)
		{
#if DEBUG
			GD.PushWarning(GetPath(), "Statechart is running, abort save.");
#endif
			return null;
		}

		StatechartSnapshot snapshot = new()
		{
			IsAllStateConfig = isAllStateConfig
		};

		if (isAllStateConfig)
		{
			RootState._SaveAllStateConfig(SnapshotConfig);
		}
		else
		{
			RootState._SaveActiveStateConfig(SnapshotConfig);
		}
		snapshot.Config = SnapshotConfig.ToArray();
		SnapshotConfig.Clear();
		return snapshot;
	}

	public bool Load(StatechartSnapshot snapshot, bool isExitOnLoad = false, bool isEnterOnLoad = false)
	{
		if (IsRunning)
		{
#if DEBUG
			GD.PushWarning(GetPath(), "Statechart is running, abort load.");
#endif
			return false;
		}

		if (snapshot is null)
		{
#if DEBUG
			GD.PushWarning(GetPath(), "Snapshot is null, abort load.");
#endif
			return false;
		}

		/*
		1. Iterate state configuration:
		  - Set current idx
		  - Add to statesToLoad
		  - If config not aligned, abort
		2. Deal enter/exit on load:
		  - Get differed states
		  - Exit old states
		  - Enter new states
		3. Update active states
		*/

		List<State> statesToLoad = new();
		var config = snapshot.Config;
		if (config.Length == 0)
		{
#if DEBUG
			GD.PushWarning(GetPath(), "Configuration is null, abort load.");
#endif
			return false;
		}

		int loadIdxResult;
		if (snapshot.IsAllStateConfig)
		{
			loadIdxResult = RootState._LoadAllStateConfig(config, 0);
		}
		else
		{
			loadIdxResult = RootState._LoadActiveStateConfig(config, 0);
		}

		if (loadIdxResult == -1)
		{
#if DEBUG
			GD.PushWarning(GetPath(), "Load failed, configuration not aligned.");
#endif
			return false;
		}

		// Exit on load
		if (isExitOnLoad)
		{
			foreach (State state in ActiveStates.Reverse())
			{
				state._StateExit();
			}
		}

		ActiveStates.Clear();
		RootState._SubmitActiveState(ActiveStates);

		// Enter on load
		if (isEnterOnLoad)
		{
			foreach (State state in ActiveStates)
			{
				state._StateEnter();
			}
		}

		return true;
	}

	protected void HandleEvent(StringName eventName)
	{
		if (RootState == null)
		{
			return;
		}
		/*
		Handle event:
		1. Select transitions
		2. Do transitions
		3. While iter < MAX_AUTO_ROUND:
			- Select auto-transition
			- Do auto-transition
			- Break if no queued auto-transition
		4. Do reactions
		*/

		// Set global transitions/reactions, null if no matched event
		if (LastStepEventName != eventName)
		{
			_GlobalEventTAMap.TryGetValue(eventName, out _CurrentTAMap);
			LastStepEventName = eventName;
		}
		// Use last query in GlobalEventTAMap if eventname not changed
		// GlobalEventTAMap.TryGetValue(eventName, out CurrentTAMap);

		// 1. Select transitions
		RootState._SelectTransitions(EnabledTransitions, false);

		// 2. Do transitions
		DoTransitions();

		// 3. Select and do automatic transitions
		for (int i = 1; i <= MaxAutoTransitionRound; ++i)
		{
			RootState._SelectTransitions(EnabledTransitions, true);

			// Stop if active states are stable
			if (EnabledTransitions.Count == 0)
			{
				break;
			}
			DoTransitions();
		}

		// 4. Select and do reactions
		foreach (State state in ActiveStates)
		{
			state._SelectReactions(EnabledReactions);
		}

		foreach (Reaction reaction in EnabledReactions)
		{
			reaction._ReactionInvoke();
		}

		EnabledReactions.Clear();
	}

	protected void DoTransitions()
	{
		/*
		Execute transitions:
			1. Process exit set (with filter)
			2. Invoke transitions
			3. Process enter set
		*/

		// 1. Deduce and merge exit set
		foreach (Transition transition in EnabledTransitions)
		{
			/*
			Check confliction
			1. If this source is descendant to other LCA.
				<=> source is in exit set
			2. Else, if other source descendant to this LCA
				<=> Any other source's most anscetor state in set is also descendant to this LCA (or it will be case 1)
				<=> Any state in exit set is descendant to this LCA
			*/

			// Targetless has no confliction
			if (transition._IsTargetless)
			{
				EnabledFilteredTransitions.Add(transition);
				continue;
			}

			bool hasConfliction = ExitSet.Contains(transition._SourceState)
				|| ExitSet.Any<State>(
					state => transition._LcaState._IsAncestorStateOf(state));

			if (hasConfliction)
			{
				continue;
			}

			EnabledFilteredTransitions.Add(transition);

			var exitStates = ActiveStates.GetViewBetween(
				transition._LcaState._LowerState, transition._LcaState._UpperState);
			ExitSet.UnionWith(exitStates);
		}

		ActiveStates.ExceptWith(ExitSet);
		foreach (State state in ExitSet)
		{
			state._StateExit();
		}

		// 2. Invoke transitions
		// 3. Deduce and merge enter set
		foreach (Transition transition in EnabledFilteredTransitions)
		{
			transition._TransitionInvoke();

			// If transition is targetless, enter-region is null.
			if (transition._IsTargetless)
			{
				continue;
			}

			SortedSet<State> enterRegion = transition._EnterRegion;
			SortedSet<State> deducedEnterStates = transition._GetDeducedEnterStates();
			EnterSet.UnionWith(enterRegion);
			EnterSet.UnionWith(deducedEnterStates);
		}

		ActiveStates.UnionWith(EnterSet);
		foreach (State state in EnterSet)
		{
			state._StateEnter();
		}

		// 4. Clear
		EnabledTransitions.Clear();
		EnabledFilteredTransitions.Clear();
		ExitSet.Clear();
		EnterSet.Clear();
	}

	public override void _Process(double delta)
	{
#if TOOLS
		if (Engine.IsEditorHint())
		{
			return;
		}
#endif

		_Duct.Delta = delta;
		Step(StatechartEventName.EVENT_PROCESS);
	}

	public override void _PhysicsProcess(double delta)
	{
#if TOOLS
		if (Engine.IsEditorHint())
		{
			return;
		}
#endif

		_Duct.PhysicsDelta = delta;
		Step(StatechartEventName.EVENT_PHYSICS_PROCESS);
	}

	public override void _Input(InputEvent @event)
	{
#if TOOLS
		if (Engine.IsEditorHint())
		{
			return;
		}
#endif

		using(@event)
		{
			_Duct.Input = @event;
			Step(StatechartEventName.EVENT_INPUT);
		}
	}

	public override void _ShortcutInput(InputEvent @event)
	{
#if TOOLS
		if (Engine.IsEditorHint())
		{
			return;
		}
#endif

		using(@event)
		{
			_Duct.ShortcutInput = @event;
			Step(StatechartEventName.EVENT_SHORTCUT_INPUT);
		}
	}

	public override void _UnhandledKeyInput(InputEvent @event)
	{
#if TOOLS
		if (Engine.IsEditorHint())
		{
			return;
		}
#endif

		using(@event)
		{
			_Duct.UnhandledKeyInput = @event;
			Step(StatechartEventName.EVENT_UNHANDLED_KEY_INPUT);
		}
	}

	public override void _UnhandledInput(InputEvent @event)
	{
#if TOOLS
		if (Engine.IsEditorHint())
		{
			return;
		}
#endif


		using(@event)
		{
			_Duct.UnhandledInput = @event;
			Step(StatechartEventName.EVENT_UNHANDLED_INPUT);
		}
	}

#if TOOLS
    public override string[] _GetConfigurationWarnings()
	{
		var warnings = new List<string>();

		bool hasRootState = false;
		bool hasOtherChild = false;
		foreach (Node child in GetChildren())
		{
			if (child is State state)
			{
				if (state._IsHistory)
				{
					hasOtherChild = true;
					continue;
				}
				if (hasRootState)
				{
					hasOtherChild = true;
				}
				hasRootState = true;
			}
			else
			{
				hasOtherChild = true;
			}
		}

		if (!hasRootState || hasOtherChild)
		{
			warnings.Add("Statechart should have exactly 1 non-history root state.");
		}

		return warnings.ToArray();
	}
#endif

#endregion

}
