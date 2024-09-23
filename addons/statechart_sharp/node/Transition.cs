using Godot;
using Godot.Collections;
using System.Collections.Generic;


namespace LGWCP.Godot.StatechartSharp;

[Tool]
[GlobalClass]
[Icon("res://addons/statechart_sharp/icon/Transition.svg")]
public partial class Transition : StatechartComposition
{
    #region signals

    [Signal]
    public delegate void GuardEventHandler(StatechartDuct duct);
    [Signal]
    public delegate void InvokeEventHandler(StatechartDuct duct);

    #endregion


    #region properties

    [Export]
    public TransitionEventNameEnum TransitionEvent
    {
        get => _transitionEvent;
        set
        {
            _transitionEvent = value;

            #if TOOLS
            if (Engine.IsEditorHint())
            {
                UpdateConfigurationWarnings();
            }
            #endif
        }
    }
    private TransitionEventNameEnum _transitionEvent = TransitionEventNameEnum.Process;
    [Export]
    public StringName CustomEventName
    {
        get => _customEventName;
        set
        {
            _customEventName = value;

            #if TOOLS
            if (Engine.IsEditorHint())
            {
                UpdateConfigurationWarnings();
            }
            #endif
        }
    }
    private StringName _customEventName;
    [Export]
    public Array<State> TargetStatesArray
    {
        get => _targetStatesArray;
        set
        {
            _targetStatesArray = value;

            #if TOOLS
            if (Engine.IsEditorHint())
            {
                UpdateConfigurationWarnings();
            }
            #endif
        }
    }
    private Array<State> _targetStatesArray = new();
    public StringName EventName { get; set; }
    private List<State> TargetStates { get; set; }
    public State SourceState { get; set; }
    public State LcaState { get; private set; }
    public SortedSet<State> EnterRegion  { get; private set; }
    private SortedSet<State> EnterRegionEdge { get; set; }
    private SortedSet<State> DeducedEnterStates { get; set; }
    protected StatechartDuct Duct { get => HostStatechart.Duct; }
    public bool IsTargetless { get; private set; }
    public bool IsAuto { get; private set; }
    private bool IsValid { get; set; }

    #endregion


    #region methods

    public override void _Ready()
    {
        // Convert GD collection to CS collection
        TargetStates = new List<State>(TargetStatesArray);

        // Init Sets
        EnterRegion = new SortedSet<State>(new StatechartComparer<State>());
        EnterRegionEdge = new SortedSet<State>(new StatechartComparer<State>());
        DeducedEnterStates = new SortedSet<State>(new StatechartComparer<State>());

        IsValid = true;

        #if TOOLS
        if (Engine.IsEditorHint())
        {
            UpdateConfigurationWarnings();
        }
        #endif
    }

    public override void Setup(Statechart hostStatechart, ref int parentOrderId)
    {
        base.Setup(hostStatechart, ref parentOrderId);

        // Get source state
        Node parent = GetParentOrNull<Node>();
        if (parent != null && parent is State)
        {
            SourceState = parent as State;
        }

        // Handle event name
        if (TransitionEvent == TransitionEventNameEnum.Custom
            &&  (CustomEventName == null || CustomEventName == ""))
        {
            #if DEBUG
            GD.PushError(GetPath(), ": no event name for custom event.");
            #endif
            IsValid = false;
        }
        EventName = StatechartEventName.GetTransitionEventName(TransitionEvent, CustomEventName);
        IsAuto = TransitionEvent == TransitionEventNameEnum.Auto;

        // Set targetless
        IsTargetless = TargetStates.Count == 0;

        if (IsTargetless && IsAuto)
        {
            IsValid = false;

            #if DEBUG
            GD.PushWarning(GetPath(),
                ": targetless auto transition is invalid. This may cause loop transition.");
            #endif
        }
    }

    public override void PostSetup()
    {
        /*
        Post setup:
        1. Find LCA(Least Common Ancestor) of source and targets
        2. Record path from LCA to targets
        3. Update enter region
        */

        // Targetless transition's LCA is source's parent state as defined
        if (IsTargetless)
        {
            LcaState = SourceState.ParentState;
            return;
        }

        // Record source-to-root
        State iterState = SourceState;
        List<State> srcToRoot = new();
        while (iterState != null)
        {
            srcToRoot.Add(iterState);
            iterState = iterState.ParentState;
        }

        // Init LCA as source state, the first state in srcToRoot
        int reversedLcaIdx = srcToRoot.Count;
        List<State> tgtToRoot = new();
        foreach (State target in TargetStates)
        {
            // Check under same statechart
            if (!IsCommonHost(target, SourceState))
            {
                #if DEBUG
                GD.PushWarning(
                    GetPath(), ": target ", target.GetPath(), " is not under same statechart as source state.");
                #endif

                continue;
            }

            /*
            Record target-to-root with conflict check:
            1. Do conflict check once when iterated state's parent first reach enter region
            2. If target is already in enter region, conflicts
            */
            tgtToRoot.Clear();
            bool isConflict = EnterRegion.Contains(target);

            iterState = target;
            bool isReachEnterRegion = false;
            while (!isConflict && iterState != null)
            {
                // Check target conflicts
                State iterParent = iterState.ParentState;

                if (!isReachEnterRegion && iterParent != null)
                {
                    isReachEnterRegion = EnterRegion.Contains(iterParent);
                    if (isReachEnterRegion)
                    {
                        isConflict = iterParent.IsConflictToEnterRegion(
                            iterState, EnterRegion);
                    }
                }

                tgtToRoot.Add(iterState);
                iterState = iterParent;
            }

            // Target conflicts, disposed from enter region.
            if (isConflict)
            {
                #if DEBUG
                GD.PushWarning(
                    GetPath(), ": target ", target.GetPath(), " conflicts with previous target(s).");
                #endif

                continue;
            }

            // Local LCA between src and 1 tgt
            int maxReversedIdx = 1; // Reversed
            int maxCountToRoot = srcToRoot.Count < tgtToRoot.Count
                ? srcToRoot.Count : tgtToRoot.Count;
            for (int i = 1; i < maxCountToRoot; ++i)
            {
                if (srcToRoot[^i] == tgtToRoot[^i])
                {
                    maxReversedIdx = i;
                }
                else
                {
                    // Last state is LCA
                    break;
                }
            }

            // If local-LCA is ancestor of current LCA, update LCA
            if (maxReversedIdx < reversedLcaIdx)
            {
                reversedLcaIdx = maxReversedIdx;
            }

            /*
            Add tgtToRoot to enter region, because:
            1. Later target(s) need check conflict in local-LCA's ancestor
            2. When extending, states need check if their substate in enter region
            */
            for (int i = 1; i <= tgtToRoot.Count; ++i)
            {
                EnterRegion.Add(tgtToRoot[^i]);
            }
        }

        // LCA
        LcaState = srcToRoot[^reversedLcaIdx];

        // Extend enter region under LCA
        SortedSet<State> extraEnterRegion = new(new StatechartComparer<State>());
        LcaState.ExtendEnterRegion(EnterRegion, EnterRegionEdge, extraEnterRegion);
        EnterRegion.UnionWith(extraEnterRegion);

        // Remove states from root to LCA (include LCA)
        for (int i = 1; i <= reversedLcaIdx; ++i)
        {
            EnterRegion.Remove(srcToRoot[^i]);
        }

        // Check auto-transition loop case
        if (IsAuto && EnterRegion.Contains(SourceState))
        {
            IsValid = false;

            #if DEBUG
            GD.PushWarning(
                GetPath(),
                ": auto transition's enter region contains source state ",
                "causes transition invalid, this may cause loop transition.");
            #endif
        }
    }

    public bool Check()
    {
        if (!IsValid)
        {
            return false;
        }

        return CustomTransitionGuard(Duct);
    }

    protected virtual bool CustomTransitionGuard(StatechartDuct duct)
    {
        // Use signal by default
        duct.IsTransitionEnabled = true;
        duct.CompositionNode = this;
        EmitSignal(SignalName.Guard, duct);
        return duct.IsTransitionEnabled;
    }

    public void TransitionInvoke()
    {
        CustomTransitionInvoke(Duct);
    }

    protected virtual void CustomTransitionInvoke(StatechartDuct duct)
    {
        // Use signal by default
        duct.CompositionNode = this;
        EmitSignal(SignalName.Invoke, duct);
    }

    public SortedSet<State> GetDeducedEnterStates()
    {
        DeducedEnterStates.Clear();
        foreach (State edgeState in EnterRegionEdge)
        {
            edgeState.DeduceDescendants(DeducedEnterStates, isEdgeState: true);
        }
        return DeducedEnterStates;
    }

    #if TOOLS
    public override string[] _GetConfigurationWarnings()
    {
        var warnings = new List<string>();
        
        // Check parent
        bool isParentWarning = true;
        Node parent = GetParentOrNull<Node>();

        if (parent != null && parent is State state)
        {
            isParentWarning = state.IsHistory;
        }

        if (isParentWarning)
        {
            warnings.Add("Transition should be child to non-history state.");
        }

        // Check children
        bool hasPromoter = false;
        foreach (Node child in GetChildren())
        {
            if (child is TransitionPromoter)
            {
                if (hasPromoter)
                {
                    warnings.Add("Transition should not have multiple TransitionPromoter as child.");
                }
                else
                {
                    hasPromoter = true;
                }
            }
            else
            {
                warnings.Add("Transition should only have TransitionPromoter as child.");
            }
        }

        // Check event
        if (TransitionEvent == TransitionEventNameEnum.Custom
            && (CustomEventName == null || CustomEventName == ""))
        {
            warnings.Add("No event name for custom event.");
        }

        if (TransitionEvent == TransitionEventNameEnum.Auto)
        {
            if (TargetStatesArray == null || TargetStatesArray.Count == 0)
            {
                warnings.Add("Target is required for auto transition.");
            }
        }

        return warnings.ToArray();
    }
    #endif

    #endregion
}
