using Godot;
using System.Collections.Generic;
using System.Linq;

namespace LGWCP.StatechartSharp;

[Tool]
[GlobalClass, Icon("res://addons/statechart_sharp/icon/Statechart.svg")]
public partial class Statechart : StatechartComposition
{
    [Export(PropertyHint.Range, "0,32,")]
    protected int MaxInternalEventCount = 8;
    [Export(PropertyHint.Range, "0,32,")]
    protected int MaxAutoTransitionRound = 8;
    [Export(PropertyHint.Flags, "Process,Physics Process,Input,Unhandled Input")]
    protected EventFlagEnum EventFlag { get; set; } = 0;
    internal bool IsRunning { get; private set; }
    protected int EventCount;
    internal State RootState { get; set; }
    protected SortedSet<State> ActiveStates { get; set; }
    protected Queue<StringName> QueuedEvents { get; set; }
    protected SortedSet<Transition> EnabledTransitions { get; set; }
    protected SortedSet<Transition> EnabledFilteredTransitions { get; set; }
    protected SortedSet<State> ExitSet { get; set; }
    protected SortedSet<State> EnterSet { get; set; }
    protected SortedSet<Reaction> EnabledReactions { get; set; }
    internal StatechartDuct Duct { get; private set; }

    public override void _Ready()
    {
        Duct = new StatechartDuct { HostStatechart = this };

        IsRunning = false;
        EventCount = 0;

        ActiveStates = new SortedSet<State>(new StateComparer());
        QueuedEvents = new Queue<StringName>();
        EnabledTransitions = new SortedSet<Transition>(new TransitionComparer());
        EnabledFilteredTransitions = new SortedSet<Transition>(new TransitionComparer());
        ExitSet = new SortedSet<State>(new ReversedStateComparer());
        EnterSet = new SortedSet<State>(new StateComparer());
        EnabledReactions = new SortedSet<Reaction>(new ReactionComparer());

        #if TOOLS
        if (!Engine.IsEditorHint())
        {
        #endif

        // Statechart setup
        Setup();
        PostSetup();

        #if TOOLS
        }
        else
        {
            UpdateConfigurationWarnings();
        }
        #endif
    }

    internal override void Setup()
    {
        OrderId = 0;
        HostStatechart = this;
        foreach (Node child in GetChildren())
        {
            if (child is State rootState)
            {
                if (!rootState.IsHistory)
                {
                    RootState = rootState;
                    break;
                }
            }
        }
        
        if (RootState != null)
        {
            int ancestorId = 0;
            RootState.Setup(this, ref ancestorId);
        }
    }

    internal override void PostSetup()
    {
        if (RootState != null)
        {
            // Get activeStates
            RootState.RegisterActiveState(ActiveStates);
            RootState.PostSetup();
        }

        // Enter active-state
        foreach (State state in ActiveStates)
        {
            state.StateEnter();
        }

        // Set node process according to flags
        SetProcess(EventFlag.HasFlag(EventFlagEnum.Process));
        SetPhysicsProcess(EventFlag.HasFlag(EventFlagEnum.PhysicsProcess));
        SetProcessInput(EventFlag.HasFlag(EventFlagEnum.Input));
        SetProcessShortcutInput(EventFlag.HasFlag(EventFlagEnum.ShortcutInput));
        SetProcessUnhandledKeyInput(EventFlag.HasFlag(EventFlagEnum.UnhandledKeyInput));
        SetProcessUnhandledInput(EventFlag.HasFlag(EventFlagEnum.UnhandledInput));
    }

    public void Step(StringName eventName)
    {
        if (eventName == null || eventName == "")
        {
            return;
        }

        /*
        If is running
            if <= max round
                queue event
            return
        Else not running:
            queue event
        */
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

    /// <summary>
    /// Macro step
    /// </summary>
    /// <param name="eventName"></param>
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

        // 3. Select and do auto transitions
        for (int i = 1; i <= MaxAutoTransitionRound; ++i)
        {
            RootState.SelectTransitions(EnabledTransitions);

            // Active-states are stable
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

        foreach (Reaction react in EnabledReactions)
        {
            react.ReactionInvoke();
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
        foreach (Transition trans in EnabledTransitions)
        {
            // Targetless checks no conflicts, and do no set operations.
            if (trans.IsTargetless)
            {
                EnabledFilteredTransitions.Add(trans);
                continue;
            }

            /*
            Check confliction
                1. If this source is descendant to other LCA.
                    <=> source is in exit set
                2. Else, if other source descendant to this LCA
                    <=> Any other source's most anscetor state in set is also descendant to this LCA (or it will be case 1)
                    <=> Any state in exit set is descendant to this LCA
            */

            if (ExitSet.Contains(trans.SourceState))
            {
                continue;
            }
            else
            {
                bool IsDescendantInExit = ExitSet.Any<State>(
                    s => trans.LcaState.IsAncestorOf(s));
                if (IsDescendantInExit)
                {
                    continue;
                }
            }

            EnabledFilteredTransitions.Add(trans);

            SortedSet<State> exitStates = ActiveStates.GetViewBetween(
                trans.LcaState.LowerState, trans.LcaState.UpperState);
            ExitSet.UnionWith(exitStates);
        }

        ActiveStates.ExceptWith(ExitSet);
        foreach (State state in ExitSet)
        {
            state.StateExit();
        }

        // 2. Invoke transitions
        foreach (Transition trans in EnabledFilteredTransitions)
        {
            trans.TransitionInvoke();
        }

        // 3. Deduce and merge enter set
        foreach (Transition trans in EnabledFilteredTransitions)
        {
            // If transition is targetless, enter-region is null.
            if (trans.IsTargetless)
            {
                continue;
            }

            SortedSet<State> enterRegion = trans.EnterRegion;
            SortedSet<State> deducedEnterStates = trans.GetDeducedEnterStates();
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
        Step(StatechartConfig.EVENT_PROCESS);
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
        Step(StatechartConfig.EVENT_PHYSICS_PROCESS);
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
        Step(StatechartConfig.EVENT_INPUT);
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
        Step(StatechartConfig.EVENT_SHORTCUT_INPUT);
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
        Step(StatechartConfig.EVENT_UNHANDLED_KEY_INPUT);
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
        Step(StatechartConfig.EVENT_UNHANDLED_INPUT);
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
                    // We already have a root state
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
            warnings.Add("Statechart needs exactly 1 non-history root state.");
        }

        return warnings.ToArray();
    }
    #endif
}
