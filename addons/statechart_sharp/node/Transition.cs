using System.Collections.Generic;
using Godot;
using Godot.Collections;


namespace LGWCP.StatechartSharp
{

[GlobalClass, Icon("res://addons/statechart_sharp/icon/Transition.svg"), Tool]
public partial class Transition : StatechartComposition
{
    #region define signals

    [Signal] public delegate void GuardEventHandler(Transition transition);
    [Signal] public delegate void InvokeEventHandler(Transition transition);
    
    #endregion

    #region define properties

    [Export] private TransitionEventNameEnum TransitionEvent { get; set; } = TransitionEventNameEnum.PROCESS;
    [Export] private StringName CustomEventName { get; set; }
    [Export] private Array<State> TargetStatesArray;
    private StringName EventName { get; set; }
    private List<State> TargetStates { get; set; }
    private State SourceState { get; set; }
    internal State LcaState { get; private set; }
    internal SortedSet<State> EnterRegion  { get; private set; }
    private SortedSet<State> EnterRegionEdge { get; set; }
    private SortedSet<State> DeducedEnterStates { get; set; }
    internal bool IsTargetless { get; private set; }
    internal bool IsAuto { get; private set; }
    public bool IsEnabled { private get; set; }

    public double Delta { get => HostStatechart.Delta; }
    public double PhysicsDelta { get=>HostStatechart.PhysicsDelta; }
    public InputEvent GameInput { get => HostStatechart.GameInput; }
    public InputEvent GameUnhandledInput { get => HostStatechart.GameUnhandledInput; }

    #endregion

    public override void _Ready()
    {
        // Convert GD collection to CS collection
        TargetStates = new List<State>(TargetStatesArray);

        // Init Sets
        EnterRegion = new SortedSet<State>(new StateComparer());
        EnterRegionEdge = new SortedSet<State>(new StateComparer());
        DeducedEnterStates = new SortedSet<State>(new StateComparer());
    }

    internal override void Init(Statechart hostStatechart, ref int ancestorId)
    {
        base.Init(hostStatechart, ref ancestorId);

        // Get source-state
        Node parent = GetParent<Node>();
        if (parent != null && parent is State)
        {
            SourceState = parent as State;
        }

        // Handle event-name
        if (TransitionEvent == TransitionEventNameEnum.CUSTOM && CustomEventName == null)
        {
            #if DEBUG
            GD.PushError(Name, ": no event name for custom-event. For eventless, switch to Auto.");
            #endif
        }
        EventName = StatechartConfig.GetTransitionEventName(TransitionEvent, CustomEventName);
        IsAuto = TransitionEvent == TransitionEventNameEnum.AUTO;

        // Set targetless
        IsTargetless = TargetStates.Count == 0;
    }

    internal override void PostInit()
    {
        if (IsTargetless)
        {
            return;
        }
        
        /*
        Process target states:
            1. Find LCA(Least Common Ancestor) of source and targets.
            2. Record path from LCA to targets.
            3. Update enter-region
        */

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
                    ": target states should under same statechart.");
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
                GD.PushWarning(SourceState.GetPath(), ": target-state conflict, name: ", target.Name);
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
    }

    internal bool Check(StringName eventName)
    {
        // Transition is enabled on default.
        if (EventName == eventName)
        {
            IsEnabled = true;
            EmitSignal(SignalName.Guard, this);
        }
        else
        {
            IsEnabled = false;
        }
        return IsEnabled;
    }

    internal void TransitionInvoke()
    {
        EmitSignal(SignalName.Invoke);
    }

    internal SortedSet<State> GetDeducedEnterStates()
    {
        DeducedEnterStates.Clear();
        foreach (State s in EnterRegionEdge)
        {
            s.DeduceDescendants(DeducedEnterStates);
        }
        return DeducedEnterStates;
    }
}

} // namespace