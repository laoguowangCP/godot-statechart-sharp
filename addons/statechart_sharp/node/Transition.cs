using Godot;
using Godot.Collections;
using System.Collections.Generic;


namespace LGWCP.Godot.StatechartSharp;

[Tool]
[GlobalClass]
[Icon("res://addons/statechart_sharp/icon/Transition.svg")]
public partial class Transition : StatechartComposition
{

#region signal

    [Signal]
    public delegate void GuardEventHandler(StatechartDuct duct);
    [Signal]
    public delegate void InvokeEventHandler(StatechartDuct duct);

#endregion


#region property

    [Export]
    public TransitionEventNameEnum TransitionEvent
#if DEBUG
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
    private TransitionEventNameEnum _transitionEvent
#endif
        = TransitionEventNameEnum.Process;

    [Export]
    public StringName CustomEventName
#if DEBUG
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
    private StringName _customEventName
#endif
        ;

    [Export]
    public Array<State> TargetStatesArray
#if DEBUG
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
    private Array<State> _targetStatesArray
#endif
        = new();

    public string _EventName;
    protected List<State> TargetStates;
    public State _SourceState;
    public State _LcaState;
    public SortedSet<State> _EnterRegion;
    protected SortedSet<State> EnterRegionEdge;
    protected SortedSet<State> DeducedEnterStates;
    protected StatechartDuct Duct;
    public bool _IsTargetless;
    public bool _IsAuto;
    protected bool IsValid;

#endregion


#region method

    public override void _Ready()
    {
        // Convert GD collection to CS collection
        TargetStates = new List<State>(TargetStatesArray);

        // Init Sets
        _EnterRegion = new SortedSet<State>(new StatechartComparer<State>());
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

    public override void _Setup(Statechart hostStatechart, ref int parentOrderId)
    {
        base._Setup(hostStatechart, ref parentOrderId);
		Duct = _HostStatechart._Duct;

        // Get source state
        Node parent = GetParentOrNull<Node>();
        if (parent != null && parent is State)
        {
            _SourceState = parent as State;
        }

        // Handle event name
        if (TransitionEvent == TransitionEventNameEnum.Custom &&  CustomEventName == null)
        {
#if DEBUG
            GD.PushError(GetPath(), ": no event name for custom event.");
#endif
            IsValid = false;
        }
        _EventName = StatechartEventName.GetTransitionEventName(TransitionEvent, CustomEventName);
        _IsAuto = TransitionEvent == TransitionEventNameEnum.Auto;

        // Set targetless
        _IsTargetless = TargetStates.Count == 0;

        if (_IsTargetless && _IsAuto)
        {
            IsValid = false;
#if DEBUG
            GD.PushWarning(GetPath(),
                ": targetless auto transition is invalid. This may cause loop transition.");
#endif
        }
    }

    public override void _SetupPost()
    {
        /*
        Post setup:
        1. Find LCA(Least Common Ancestor) of source and targets
        2. Record path from LCA to targets
        3. Update enter region
        */

        // Targetless transition's LCA is source's parent state as defined
        if (_IsTargetless)
        {
            _LcaState = _SourceState._ParentState;
            return;
        }

        // Record source-to-root
        State iterState = _SourceState;
        List<State> srcToRoot = new();
        while (iterState != null)
        {
            srcToRoot.Add(iterState);
            iterState = iterState._ParentState;
        }

        // Init LCA as source state, the first state in srcToRoot
        int reversedLcaIdx = srcToRoot.Count;
        List<State> tgtToRoot = new();
        foreach (State target in TargetStates)
        {
            // Check under same statechart
            if (!_IsCommonHost(target, _SourceState))
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
            bool isConflict = _EnterRegion.Contains(target);

            iterState = target;
            bool isReachEnterRegion = false;
            while (!isConflict && iterState != null)
            {
                // Check target conflicts
                State iterParent = iterState._ParentState;

                if (!isReachEnterRegion && iterParent != null)
                {
                    isReachEnterRegion = _EnterRegion.Contains(iterParent);
                    if (isReachEnterRegion)
                    {
                        isConflict = iterParent._IsConflictToEnterRegion(
                            iterState, _EnterRegion);
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
                _EnterRegion.Add(tgtToRoot[^i]);
            }
        }

        // LCA
        _LcaState = srcToRoot[^reversedLcaIdx];

        // Extend enter region under LCA
        SortedSet<State> extraEnterRegion = new(new StatechartComparer<State>());
        _LcaState._ExtendEnterRegion(_EnterRegion, EnterRegionEdge, extraEnterRegion, true);
        _EnterRegion.UnionWith(extraEnterRegion);

        // Remove states from root to LCA (include LCA)
        for (int i = 1; i <= reversedLcaIdx; ++i)
        {
            _EnterRegion.Remove(srcToRoot[^i]);
        }

        // Check auto-transition loop case
        if (_IsAuto && _EnterRegion.Contains(_SourceState))
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

    public bool _Check()
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

    public void _TransitionInvoke()
    {
        CustomTransitionInvoke(Duct);
    }

    protected virtual void CustomTransitionInvoke(StatechartDuct duct)
    {
        // Use signal by default
        duct.CompositionNode = this;
        EmitSignal(SignalName.Invoke, duct);
    }

    public SortedSet<State> _GetDeducedEnterStates()
    {
        DeducedEnterStates.Clear();
        foreach (State edgeState in EnterRegionEdge)
        {
            edgeState._DeduceDescendants(DeducedEnterStates);
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
            isParentWarning = !state._IsValidState();
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
