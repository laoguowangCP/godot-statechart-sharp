using System.Collections.Generic;
using Godot;
using Godot.Collections;


namespace LGWCP.StatechartSharp
{

[Tool]
[GlobalClass, Icon("res://addons/statechart_sharp/icon/Transition.svg")]
public partial class Transition : StatechartComposition
{
    #region define signals

    [Signal] public delegate void GuardEventHandler(Transition transition);
    [Signal] public delegate void InvokeEventHandler(Transition transition);
    
    #endregion

    #region define properties

    [Export] private TransitionEventNameEnum TransitionEvent
    {
        get => _transitionEvent;
        set
        {
            _transitionEvent = value;
            #if TOOLS
            UpdateConfigurationWarnings();
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
            UpdateConfigurationWarnings();
            #endif
        }
    }
    private StringName _customEventName;
    [Export] private Array<State> TargetStatesArray = new Array<State>();
    private StringName EventName { get; set; }
    private List<State> TargetStates { get; set; }
    internal State SourceState { get; set; }
    internal State LcaState { get; private set; }
    internal SortedSet<State> EnterRegion  { get; private set; }
    private SortedSet<State> EnterRegionEdge { get; set; }
    private SortedSet<State> DeducedEnterStates { get; set; }
    internal bool IsTargetless { get; private set; }
    internal bool IsAuto { get; private set; }
    public bool IsEnabled { private get; set; }
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

        // Handle event-name
        if (TransitionEvent == TransitionEventNameEnum.Custom
            &&  (CustomEventName == null || CustomEventName == ""))
        {
            #if DEBUG
            GD.PushError(
                GetPath(),
                ": no event name for custom event. For eventless, switch to Auto.");
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
            GD.PushError(
                GetPath(),
                ": targetless auto transition is invalid. This will most likely causes loop transition.");
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

        // Check auto transition target
        if (IsAuto)
        {
            State sourceParent = SourceState.ParentState;
            foreach (State targetState in TargetStates)
            {
                bool isDescToParent = false;
                bool isDescToSource = false;
                State targetAscendant = targetState.ParentState;
                while (targetAscendant != null)
                {
                    if (targetAscendant == SourceState)
                    {
                        isDescToSource = true;
                    }
                    if (targetAscendant == sourceParent)
                    {
                        isDescToParent = true;
                        break;
                    }
                    targetAscendant = targetAscendant.ParentState;
                }

                #if DEBUG
                if (!isDescToParent)
                {
                    GD.PushWarning(
                        GetPath(),
                        ": target ",
                        targetState.GetPath(),
                        " is not descendant to source's parent, this may cause unintended loop transition.");
                }
                #endif

                if (sourceParent.StateMode == StateModeEnum.Compound)
                {
                    if (isDescToSource || targetState == SourceState)
                    {
                        #if DEBUG
                        GD.PushWarning(
                            GetPath(),
                            ": target ",
                            targetState.GetPath(),
                            " causes transition invalid. ",
                            "When source's parent is compound, auto transition's target should not be source state or its descendant, this will most likely lead to loop transition.");
                        #endif
                        IsValid = false;
                        break;
                    }
                }
                else if (sourceParent.StateMode == StateModeEnum.Parallel)
                {
                    if (isDescToParent || targetState == sourceParent)
                    {
                        #if DEBUG
                        GD.PushWarning(
                            GetPath(),
                            ": target ",
                            targetState.GetPath(),
                            " causes transition invalid. ",
                            "When source's parent is parallel, auto transition's target should not be source's parent or its descendant, this will most likely lead to loop transition.");
                        #endif
                        IsValid = false;
                        break;
                    }
                }
            }
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

            // Record target-to-root
            tgtToRoot.Clear();
            iterState = target;
            bool isConflict = false;
            while (iterState != null)
            {
                // Check transition conflicts
                State iterParent = iterState.ParentState;
                if (iterParent != null)
                {
                    if (iterParent.IsConflictToEnterRegion(iterState, EnterRegion))
                    {
                        isConflict = true;
                        break;
                    }
                }
                tgtToRoot.Add(iterState);
                iterState = iterParent;
            }

            // Transition t conflicts, not available
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
            int maxIdx = 1; // Reversed
            int maxCountToRoot = srcToRoot.Count < tgtToRoot.Count
                ? srcToRoot.Count : tgtToRoot.Count;
            for (int i = 1; i <= maxCountToRoot; ++i)
            {
                if (srcToRoot[^i] == tgtToRoot[^i])
                {
                    maxIdx = i;
                }
                else
                {
                    // Last state is LCA
                    break;
                }
            }

            // If local-LCA is ancestor of current LCA, update LCA
            if (maxIdx < reversedLcaIdx)
            {
                reversedLcaIdx = maxIdx;
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

        // Extend enter-region under LCA
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
                ": auto transition's enter region contains source state, target states invalid.");
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

        // Targetless transition should not be selected as eventless
        if (eventName == null && IsTargetless)
        {
            return false;
        }

        // Transition is enabled on default.
        if (EventName == eventName)
        {
            IsEnabled = true;
            EmitSignal(SignalName.Guard, this);
        }

        return IsEnabled;
    }

    internal void TransitionInvoke()
    {
        EmitSignal(SignalName.Invoke, this);
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

        if (TransitionEvent == TransitionEventNameEnum.Custom
            && (CustomEventName == null || CustomEventName == ""))
        {
            warnings.Add("No event name for custom event. For eventless, switch to Auto.");
        }

        if (TransitionEvent == TransitionEventNameEnum.Auto
            && (TargetStatesArray == null || TargetStatesArray.Count == 0))
        {
            warnings.Add("Targetless auto transition is invalid.");
        }

        return warnings.ToArray();
    }
    #endif
}

} // end of namespace