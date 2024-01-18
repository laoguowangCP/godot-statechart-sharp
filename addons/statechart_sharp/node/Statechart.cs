using Godot;
using System.Collections.Generic;

namespace LGWCP.StatechartSharp
{

[Tool]
[GlobalClass, Icon("res://addons/statechart_sharp/icon/Statechart.svg")]
public partial class Statechart : StatechartComposition
{
    [Export(PropertyHint.Range, "0,32,")]
    protected int MaxAutoTransitionRound = 8;
    [Export(PropertyHint.Flags, "Process,Physics Process,Input,Unhandled Input")]
    protected EventFlagEnum EventFlag { get; set; } = 0;
    protected bool IsRunning { get; set; }
    internal new double Delta { get; set; }
    internal new double PhysicsDelta { get; set; }
    internal new InputEvent Input { get; set; }
    internal new InputEvent UnhandledInput { get; set; }
    internal State RootState { get; set; }
    protected SortedSet<State> ActiveStates { get; set; }
    protected Queue<StringName> QueuedEvents { get; set; }
    protected SortedSet<Transition> EnabledTransitions { get; set; }
    protected SortedSet<Transition> EnabledFilteredTransitions { get; set; }
    protected SortedSet<State> ExitSet { get; set; }
    protected SortedSet<State> EnterSet { get; set; }
    protected SortedSet<Reaction> EnabledReactions { get; set; }
    
    public override void _Ready()
    {
        IsRunning = false;

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
                RootState = rootState;
                break;
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
        foreach (State s in ActiveStates)
        {
            s.StateEnter();
        }

        return;
    }

    public void Step(StringName eventName)
    {
        #if TOOLS
        if (Engine.IsEditorHint())
        {
            return;
        }
        #endif

        if (eventName == null || eventName == "")
        {
            return;
        }

        // Queue transition event
        QueuedEvents.Enqueue(eventName);
        if (IsRunning)
        {
            return;
        }
        
        IsRunning = true;

        while (QueuedEvents.Count > 0)
        {
            StringName nextEvent = QueuedEvents.Dequeue();
            HandleEvent(nextEvent);
        }

        IsRunning = false;
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
        foreach (State s in ActiveStates)
        {
            s.SelectReactions(EnabledReactions, eventName);
        }

        foreach (Reaction a in EnabledReactions)
        {
            a.ReactionInvoke();
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
        foreach (Transition t in EnabledTransitions)
        {
            // Targetless checks no conflicts, and do no set operations.
            if (t.IsTargetless)
            {
                EnabledFilteredTransitions.Add(t);
                continue;
            }

            /*
            Check confliction
                1. If source is in exit set (descendant to other LCA) already.
                2. Else, if any other source descendant to this LCA
                    <=> Any other source's most anscetor state is also descendant to this LCA (or it is case 1)
                    <=> Any state in exit set is descendant to this LCA
            */

            if (ExitSet.Contains(t.SourceState))
            {
                continue;
            }
            else
            {
                SortedSet<State> DescendantInExit = ExitSet.GetViewBetween(
                    t.LcaState.LowerState, t.LcaState.UpperState);
                if (DescendantInExit.Count > 0)
                {
                    continue;
                }
            }

            EnabledFilteredTransitions.Add(t);

            SortedSet<State> exitStates = ActiveStates.GetViewBetween(
                t.LcaState.LowerState, t.LcaState.UpperState);
            ExitSet.UnionWith(exitStates);
        }

        ActiveStates.ExceptWith(ExitSet);
        foreach (State s in ExitSet)
        {
            s.StateExit();
        }

        // 2. Invoke transitions
        foreach (Transition t in EnabledFilteredTransitions)
        {
            t.TransitionInvoke();
        }

        // 3. Deduce and merge enter set
        foreach (Transition t in EnabledFilteredTransitions)
        {
            // If transition is targetless, enter-region is null.
            if (t.IsTargetless)
            {
                continue;
            }

            SortedSet<State> enterRegion = t.EnterRegion;
            SortedSet<State> deducedEnterStates = t.GetDeducedEnterStates();
            EnterSet.UnionWith(enterRegion);
            EnterSet.UnionWith(deducedEnterStates);
        }

        ActiveStates.UnionWith(EnterSet);
        foreach (State s in EnterSet)
        {
            s.StateEnter();
        }

        // 4. Clear
        EnabledTransitions.Clear();
        ExitSet.Clear();
        EnterSet.Clear();
        EnabledFilteredTransitions.Clear();
    }

    public override void _Process(double delta)
    {
        if (!EventFlag.HasFlag(EventFlagEnum.Process))
        {
            return;
        }
        Delta = delta;
        Step(StatechartConfig.EVENT_PROCESS);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!EventFlag.HasFlag(EventFlagEnum.PhysicsProcess))
        {
            return;
        }
        PhysicsDelta = delta;
        Step(StatechartConfig.EVENT_PHYSICS_PROCESS);
    }

    public override void _Input(InputEvent @event)
    {
        if (!EventFlag.HasFlag(EventFlagEnum.Input))
        {
            return;
        }
        Input = @event;
        Step(StatechartConfig.EVENT_INPUT);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!EventFlag.HasFlag(EventFlagEnum.UnhandledInput))
        {
            return;
        }
        UnhandledInput = @event;
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

} // end of namespace