using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;


namespace LGWCP.StatechartSharp
{

[GlobalClass, Icon("res://addons/statechart_sharp/icon/Transition.svg")]
public partial class Transition : StatechartComposition
{
    #region define signals

    [Signal] public delegate void GuardEventHandler(Transition t);
    [Signal] public delegate void InvokeEventHandler(Transition t);
    
    #endregion

    #region define properties

    // If transitEvent is null, transition is auto (checked on state enter)
    [Export] protected EventNameEnum TransitionEvent { get; set; } = EventNameEnum.PROCESS;
    [Export] protected StringName CustomEventName { get; set; }
    [Export] protected Array<State> TargetStatesArray;
    public StringName EventName { get; protected set; }
    protected List<State> TargetStates { get; set; }
    public State SourceState { get; protected set; }
    public State LcaState { get; protected set; }
    public SortedSet<State> EnterRegion  { get; protected set; }
    public SortedSet<State> EnterRegionEdge { get; protected set; }
    public SortedSet<State> DeducedEnterSet { get; protected set; }
    public bool IsEnabled { get; set; }
    public bool IsTargetless { get; protected set; }
    public bool IsAuto { get; protected set; }
    public double Delta
    {
        get { return SourceState.HostStatechart.Delta; }
    }
    public InputEvent GameInput
    {
        get { return SourceState.HostStatechart.GameInput; }
    }

    #endregion

    public override void _Ready()
    {
        // Convert GD collection to CS collection
        TargetStates = new List<State>(TargetStatesArray);

        // Init Sets
        EnterRegion = new SortedSet<State>(new StateComparer());
        EnterRegionEdge = new SortedSet<State>(new StateComparer());
        DeducedEnterSet = new SortedSet<State>(new StateComparer());
    }

    public override void Init(Statechart hostStatechart, ref int ancestorId)
    {
        base.Init(hostStatechart, ref ancestorId);

        // Get source-state
        Node parent = GetParent<Node>();
        if (parent != null && parent is State)
        {
            SourceState = parent as State;
        }

        // Handle event-name
        EventName = StatechartConfig.GetEventName(TransitionEvent, CustomEventName);
        IsAuto = EventName == null;
        if (TransitionEvent == EventNameEnum.CUSTOM && EventName == null)
        {
            #if DEBUG
            GD.PushError(Name, ": no event name for custom-event. For eventless, switch to Auto.");
            #endif
        }

        // Set targetless
        IsTargetless = TargetStates.Count == 0;
    }

    public override void PostInit()
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
        State iter = SourceState;
        List<State> srcToRoot = new List<State>();
        while (iter != null)
        {
            srcToRoot.Add(iter);
            iter = iter.ParentState;
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
                GD.PushWarning(Name, ": target states should under same statechart.");
                #endif
                continue;
            }

            // Record target-to-root
            tgtToRoot.Clear();
            iter = target;
            bool isConflict = false;
            while (iter != null)
            {
                // Check transition conflicts
                State iterParent = iter.ParentState;
                if (iterParent != null)
                {
                    if (iterParent.IsConflictToEnterRegion(iter, EnterRegion))
                    {
                        isConflict = true;
                        break;
                    }
                }
                tgtToRoot.Add(iter);
                iter = iterParent;
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
            for (int i = 0; i <= tgtToRoot.Count; i++)
            {
                EnterRegion.Add(tgtToRoot[^i]);
            }
        }

        // LCA
        LcaState = srcToRoot[^reversedLcaIdx];
        #if DEBUG
        GD.Print("First element in EnterStates is LCA: ", EnterRegion.First() == LcaState);
        #endif

        // Exclude states from root to LCA (include LCA)
        for (int i = 1; i <= reversedLcaIdx; ++i)
        {
            EnterRegion.Remove(srcToRoot[^i]);
        }

        /*
        Extend enter-region:
            1. For compond, add initial substate to region (recursively)
            2. For parallel, add substate not in region (recursively)
            3. For history, add to region-edge
        */
        SortedSet<State> extraEnterRegion = new SortedSet<State>(new StateComparer());
        LcaState.ExtendEnterRegion(EnterRegion, EnterRegionEdge, extraEnterRegion);
        /*
        foreach (State s in EnterRegion)
        {
            if (s.StateMode == StateModeEnum.Compond)
            {
                if (s.Substates.Count == 0) continue;
                EnterRegionEdge.Add(s);
            }
            else if (s.StateMode == StateModeEnum.Parallel)
            {
                foreach (State substate in s.Substates)
                {
                    if (EnterRegion.Contains(substate)) continue;
                    extraEnterRegion.Add(substate);
                    EnterRegionEdge.Add(substate);
                }
            }
            else if (s.StateMode == StateModeEnum.History)
            {
                EnterRegion.Remove(s);
                EnterRegionEdge.Add(s);
            }
        }*/
        EnterRegion.UnionWith(extraEnterRegion);
    }

    public void Check()
    {
        // Transition is enabled on default.
        IsEnabled = true;
        EmitSignal(SignalName.Guard, this);
    }

    public SortedSet<State> GetDeducedEnterSet()
    {
        DeducedEnterSet.Clear();
        foreach (State s in EnterRegionEdge)
        {
            s.DeduceDescendants(DeducedEnterSet);
        }
        return DeducedEnterSet;
    }
}

} // namespace