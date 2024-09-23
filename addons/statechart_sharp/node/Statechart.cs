using Godot;
using System.Collections.Generic;
using System.Linq;


namespace LGWCP.Godot.StatechartSharp;

[Tool]
[GlobalClass]
[Icon("res://addons/statechart_sharp/icon/Statechart.svg")]
public partial class Statechart : StatechartComposition
{
	#region properties

	/// <summary>
	/// MaxInternalEventCount
	/// </summary>
	[Export(PropertyHint.Range, "0,32,")]
	public int MaxInternalEventCount = 8;
	[Export(PropertyHint.Range, "0,32,")]
	public int MaxAutoTransitionRound = 8;
	[Export(PropertyHint.Flags, "Process,Physics Process,Input,Shortcut Input,UnhandledKey Input,Unhandled Input")]
	public EventFlagEnum EventFlag { get; set; } = 0;
	[Export]
	public bool IsWaitParentReady = true;
	public bool IsRunning { get; private set; }
	protected int EventCount;
	protected State RootState { get; set; }
	protected SortedSet<State> ActiveStates { get; set; }
	protected Queue<StringName> QueuedEvents { get; set; }
	protected SortedSet<Transition> EnabledTransitions { get; set; }
	protected SortedSet<Transition> EnabledFilteredTransitions { get; set; }
	protected SortedSet<State> ExitSet { get; set; }
	protected SortedSet<State> EnterSet { get; set; }
	protected SortedSet<Reaction> EnabledReactions { get; set; }
	public StatechartDuct Duct { get; private set; }
	protected List<int> SnapshotConfiguration;

	#endregion


	#region methods

	public override void _Ready()
	{
		Duct = new StatechartDuct
		{
			HostStatechart = this
		};

		IsRunning = false;
		EventCount = 0;

		ActiveStates = new SortedSet<State>(new StatechartComparer<State>());
		QueuedEvents = new Queue<StringName>();
		EnabledTransitions = new SortedSet<Transition>(new StatechartComparer<Transition>());
		EnabledFilteredTransitions = new SortedSet<Transition>(new StatechartComparer<Transition>());
		ExitSet = new SortedSet<State>(new StatechartReversedComparer<State>());
		EnterSet = new SortedSet<State>(new StatechartComparer<State>());
		EnabledReactions = new SortedSet<Reaction>(new StatechartComparer<Reaction>());
		
		SnapshotConfiguration = new List<int>();

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

		Setup();
		PostSetup();
	}

	public override void Setup()
	{
		HostStatechart = this;
		foreach (Node child in GetChildren())
		{
			if (child is State state)
			{
				if (state.IsAvailableRootState())
				{
					RootState = state;
					break;
				}
			}
		}
		
		OrderId = 0;
		if (RootState is not null)
		{
			int parentOrderId = 0;
			RootState.Setup(this, ref parentOrderId);
		}
	}

	public override void PostSetup()
	{
		// Get and enter active states
		if (RootState is not null)
		{
			RootState.RegisterActiveState(ActiveStates);
			RootState.PostSetup();
		}

		foreach (State state in ActiveStates)
		{
			state.StateEnter();
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

	public void Step(StringName eventName)
	{
		if (eventName == null || eventName == "")
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

	public StatechartSnapshot Save(bool isAllStateConfiguration = false)
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
			IsAllStateConfiguration = isAllStateConfiguration
		};
		
		if (isAllStateConfiguration)
		{
			RootState.SaveAllStateConfig(ref SnapshotConfiguration);
		}
		else
		{
			RootState.SaveActiveStateConfig(ref SnapshotConfiguration);
		}
		snapshot.Configuration = SnapshotConfiguration.ToArray();
		SnapshotConfiguration.Clear();
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
		int[] config = snapshot.Configuration;
		if (config.Length == 0)
		{
			#if DEBUG
			GD.PushWarning(GetPath(), "Configuration is null, abort load.");
			#endif
			return false;
		}

		bool isLoadSuccess;
		int configIdx = 0;
		if (snapshot.IsAllStateConfiguration)
		{
			isLoadSuccess = RootState.LoadAllStateConfig(
				ref config, ref configIdx);
		}
		else
		{
			isLoadSuccess = RootState.LoadActiveStateConfig(
				ref config, ref configIdx);
		}

		if (!isLoadSuccess)
		{
			#if DEBUG
			GD.PushWarning(
				GetPath(),
				"Load failed, configuration not aligned."
			);
			#endif

			return false;
		}

		// Exit on load
		if (isExitOnLoad)
		{
			foreach (State state in ActiveStates.Reverse())
			{
				state.StateExit();
			}
		}

		ActiveStates.Clear();
		RootState.RegisterActiveState(ActiveStates);

		// Enter on load
		if (isEnterOnLoad)
		{
			foreach (State state in ActiveStates)
			{
				state.StateEnter();
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

		// 1. Select transitions
		RootState.SelectTransitions(EnabledTransitions, eventName);

		// 2. Do transitions
		DoTransitions();

		// 3. Select and do automatic transitions
		for (int i = 1; i <= MaxAutoTransitionRound; ++i)
		{
			RootState.SelectTransitions(EnabledTransitions);

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
			state.SelectReactions(EnabledReactions, eventName);
		}

		foreach (Reaction reaction in EnabledReactions)
		{
			reaction.ReactionInvoke();
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
			if (transition.IsTargetless)
			{
				EnabledFilteredTransitions.Add(transition);
				continue;
			}

			bool hasConfliction = ExitSet.Contains(transition.SourceState)
				|| ExitSet.Any<State>(
					state => transition.LcaState.IsAncestorStateOf(state));

			if (hasConfliction)
			{
				continue;
			}

			EnabledFilteredTransitions.Add(transition);

			SortedSet<State> exitStates = ActiveStates.GetViewBetween(
				transition.LcaState.LowerState, transition.LcaState.UpperState);
			ExitSet.UnionWith(exitStates);
		}

		ActiveStates.ExceptWith(ExitSet);
		foreach (State state in ExitSet)
		{
			state.StateExit();
		}

		// 2. Invoke transitions
		// 3. Deduce and merge enter set
		foreach (Transition transition in EnabledFilteredTransitions)
		{
			transition.TransitionInvoke();
			
			// If transition is targetless, enter-region is null.
			if (transition.IsTargetless)
			{
				continue;
			}

			SortedSet<State> enterRegion = transition.EnterRegion;
			SortedSet<State> deducedEnterStates = transition.GetDeducedEnterStates();
			EnterSet.UnionWith(enterRegion);
			EnterSet.UnionWith(deducedEnterStates);
		}

		ActiveStates.UnionWith(EnterSet);
		foreach (State state in EnterSet)
		{
			state.StateEnter();
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

		Duct.Delta = delta;
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
		
		Duct.PhysicsDelta = delta;
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
		
		Duct.Input = @event;
		Step(StatechartEventName.EVENT_INPUT);
	}

	public override void _ShortcutInput(InputEvent @event)
	{
		#if TOOLS
		if (Engine.IsEditorHint())
		{
			return;
		}
		#endif
		
		Duct.ShortcutInput = @event;
		Step(StatechartEventName.EVENT_SHORTCUT_INPUT);
	}

	public override void _UnhandledKeyInput(InputEvent @event)
	{
		#if TOOLS
		if (Engine.IsEditorHint())
		{
			return;
		}
		#endif
		
		Duct.UnhandledKeyInput = @event;
		Step(StatechartEventName.EVENT_UNHANDLED_KEY_INPUT);
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		#if TOOLS
		if (Engine.IsEditorHint())
		{
			return;
		}
		#endif
		
		Duct.UnhandledInput = @event;
		Step(StatechartEventName.EVENT_UNHANDLED_INPUT);
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
				if (state.IsHistory)
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
