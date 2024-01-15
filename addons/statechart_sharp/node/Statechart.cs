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
    
    public override void _Ready()
    {
        IsRunning = false;

        ActiveStates = new SortedSet<State>(new StateComparer());
        QueuedEvents = new Queue<StringName>();
        EnabledTransitions = new SortedSet<Transition>(new TransitionComparer());
        EnabledFilteredTransitions = new SortedSet<Transition>(new TransitionComparer());
        ExitSet = new SortedSet<State>(new ReversedStateComparer());
        EnterSet = new SortedSet<State>(new StateComparer());

        #if TOOLS
        if (!Engine.IsEditorHint())
        {
        #endif

        // Collect states, activate initial-states
        Setup();

        // Init Transitions
        // Enter active states, document order
        PostSetup();

        #if TOOLS
        }

        UpdateConfigurationWarnings();
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
            4. Do action of active-states

        */
        // 1. Select transitions
        RootState.SelectTransitions(EnabledTransitions, eventName);

        // 2. Do transitions
        DoTransitions(EnabledTransitions);

        // 3. Auto transition
        for (int i = 1; i <= MaxAutoTransitionRound; ++i)
        {
            RootState.SelectTransitions(EnabledTransitions);

            // Active-states are stable
            if (EnabledTransitions.Count == 0)
            {
                break;
            }
            DoTransitions(EnabledTransitions); 
        }

        // 4. Invoke action of active-states
        foreach (State s in ActiveStates)
        {
            s.StateInvoke(eventName);
        }
    }

    protected void DoTransitions(SortedSet<Transition> enabledTransitions)
    {
        /*
        Batch:
            1. Process exit-set (with filter)
            2. Invoke transitions
            3. Process enter-set (update current)
        */

        // 1. Deduce and merge exit set
        foreach (Transition t in enabledTransitions)
        {
            if (ExitSet.Contains(t.SourceState))
            {
                continue;
            }

            EnabledFilteredTransitions.Add(t);

            // Targetless is filtered by LCA as well, but do no set operation.
            if (t.IsTargetless)
            {
                continue;
            }

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
        enabledTransitions.Clear();
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
            if (child is State)
            {
                hasRootState = true;
            }
            else
            {
                hasOtherChild = true;
            }
        }

        if (!hasRootState || hasOtherChild)
        {
            warnings.Add("Statechart need 1 root state.");
        }

        return warnings.ToArray();
    }
    #endif
}

} // end of namespace