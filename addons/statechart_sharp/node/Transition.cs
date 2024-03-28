using Godot;
using Godot.Collections;
using System.Collections.Generic;


namespace LGWCP.StatechartSharp;

[Tool]
[GlobalClass, Icon("res://addons/statechart_sharp/icon/Transition.svg")]
public partial class Transition : StatechartComposition
{
    #region define signals

    [Signal] public delegate void GuardEventHandler(StatechartDuct duct);
    [Signal] public delegate void InvokeEventHandler(StatechartDuct duct);
    
    #endregion

    #region define properties

    [Export] private TransitionEventNameEnum TransitionEvent
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
    [Export] private StringName CustomEventName
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
    [Export] private Array<State> TargetStatesArray
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
    private Array<State> _targetStatesArray = new Array<State>();
    private StringName EventName { get; set; }
    private List<State> TargetStates { get; set; }
    internal State SourceState { get; set; }
    internal State LcaState { get; private set; }
    internal SortedSet<State> EnterRegion  { get; private set; }
    private SortedSet<State> EnterRegionEdge { get; set; }
    private SortedSet<State> DeducedEnterStates { get; set; }
    protected StatechartDuct Duct { get => HostStatechart.Duct; }
    internal bool IsTargetless { get; private set; }
    internal bool IsAuto { get; private set; }
    // private bool IsEnabled { get => Duct.IsEnabled; set => Duct.IsEnabled = value; }
    private bool IsValid { get; set; }

    #endregion

    public override void _Ready()
    {
        // Convert GD collection to CS collection
        TargetStates = new List<State>(TargetStatesArray);

        // Init Sets
        EnterRegion = new SortedSet<State>(new StateComparer());
        EnterRegionEdge = new SortedSet<State>(new StateComparer());
        DeducedEnterStates = new SortedSet<State>(new StateComparer());

        IsValid = true;

        #if TOOLS
        if (Engine.IsEditorHint())
        {
            UpdateConfigurationWarnings();
        }
        #endif
    }

    internal override void Setup(Statechart hostStatechart, ref int ancestorId)
    {
        base.Setup(hostStatechart, ref ancestorId);

        // Get source-state
        Node parent = GetParent<Node>();
        if (parent != null && parent is State)
        {
            SourceState = parent as State;
        }

        // Handle event name
        if (TransitionEvent == TransitionEventNameEnum.Custom
            &&  (CustomEventName == null || CustomEventName == ""))
        {
            #if DEBUG
            GD.PushError(
                GetPath(),
                ": no event name for custom event.");
            #endif
            IsValid = false;
        }
        EventName = StatechartConfig.GetTransitionEventName(TransitionEvent, CustomEventName);
        IsAuto = TransitionEvent == TransitionEventNameEnum.Auto;

        // Set targetless
        IsTargetless = TargetStates.Count == 0;

        if (IsTargetless && IsAuto)
        {
            #if DEBUG
            GD.PushWarning(
                GetPath(),
                ": targetless auto transition is invalid. This will most likely cause loop transition.");
            #endif
            IsValid = false;
        }
    }

    internal override void PostSetup()
    {
        /*
        Process target states:
            1. Find LCA(Least Common Ancestor) of source and targets.
            2. Record path from LCA to targets.
            3. Update enter-region
        */

        // Though targetless has no exit set or enter set, still have LCA = source.parent as defined
        if (IsTargetless)
        {
            LcaState = SourceState.ParentState;
            return;
        }

        // Record source-to-root
        State iterState = SourceState;
        List<State> srcToRoot = new List<State>();
        while (iterState != null)
        {
            srcToRoot.Add(iterState);
            iterState = iterState.ParentState;
        }

        // Init LCA as SourceState, the first state in srcToRoot
        int reversedLcaIdx = srcToRoot.Count;
        List<State> tgtToRoot = new List<State>();
        foreach (State target in TargetStates)
        {
            // Check under same statechart
            if (!IsCommonHost(target, SourceState))
            {
                #if DEBUG
                GD.PushWarning(
                    GetPath(),
                    ": target ",
                    target.GetPath(),
                    " should under same statechart as source state.");
                #endif
                continue;
            }

            /*
            Record target-to-root with conflict check:
                1. Do conflict check once when iter state's parent first reach enter region.
                2. If target is already in enter region, it conflicts naturally.
            */
            tgtToRoot.Clear();
            bool isConflict = EnterRegion.Contains(target);

            iterState = target;
            bool isReachEnterRegion = false;
            while (!isConflict && iterState != null)
            {
                // Check transition conflicts
                State iterParent = iterState.ParentState;

                if (!isReachEnterRegion && iterParent != null)
                {
                    isReachEnterRegion = EnterRegion.Contains(iterParent);

                    // First reach enter region
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
                    SourceState.GetPath(),
                    ": target ",
                    target.GetPath(),
                    " conflicts with previous target(s).");
                #endif
                continue;
            }

            // local-LCA between src and 1 tgt
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
            Update enter-region, with entire tgtToRoot, for that:
                1. Following targets may check conflict in local-LCA's ancestor
                2. When extending, states need check substate in enter-region
            */
            for (int i = 1; i <= tgtToRoot.Count; ++i)
            {
                EnterRegion.Add(tgtToRoot[^i]);
            }
        }

        // LCA
        LcaState = srcToRoot[^reversedLcaIdx];

        // Extend enter region under LCA
        SortedSet<State> extraEnterRegion = new SortedSet<State>(new StateComparer());
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
            #if DEBUG
            GD.PushWarning(
                GetPath(),
                ": auto transition's enter region contains source state, ",
                "causes transition invalid, this will most likely cause loop transition.");
            #endif
            IsValid = false;
        }
    }

    internal bool Check(StringName eventName)
    {
        if (!IsValid)
        {
            return false;
        }

        if (EventName != eventName)
        {
            return false;
        }

        // else EventName == eventName, transition is enabled on default.
        // Duct.IsEnabled = true;
        // Duct.CompositionNode = this;
        // EmitSignal(SignalName.Guard, Duct);
        return CustomTransitionGuard(Duct);
    }

    protected virtual bool CustomTransitionGuard(StatechartDuct duct)
    {
        // Use signal by default
        duct.IsEnabled = true;
        duct.CompositionNode = this;
        EmitSignal(SignalName.Guard, duct);
        return duct.IsEnabled;
    }

    internal void TransitionInvoke()
    {
        // Duct.CompositionNode = this;
        // EmitSignal(SignalName.Invoke, Duct);
        CustomTransitionInvoke(Duct);
    }

    protected virtual void CustomTransitionInvoke(StatechartDuct duct)
    {
        // Use signal by default
        duct.CompositionNode = this;
        EmitSignal(SignalName.Invoke, duct);
    }

    internal SortedSet<State> GetDeducedEnterStates()
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
        Node parent = GetParent<Node>();

        if (parent != null && parent is State state)
        {
            isParentWarning = state.IsHistory;
        }

        if (isParentWarning)
        {
            warnings.Add("Transition should be child to non-history state.");
        }

        // Check children
        if (GetChildren().Count > 0)
        {
            warnings.Add("Transition should not have child.");
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
}
