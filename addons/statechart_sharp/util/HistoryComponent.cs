using Godot;
using System.Collections.Generic;

namespace LGWCP.StatechartSharp
{
    public class HistoryComponent : StateComponent
    {
        public HistoryComponent(State state) : base(state) {}

        public override void Init(Statechart hostStateChart, ref int ancestorId)
        {
            base.Init(hostStateChart, ref ancestorId);
            
            // No substates, transitions, or actions
            #if DEBUG
            foreach (Node child in HostState.GetChildren())
            {
                GD.PushWarning(HostState.GetPath(), ": History state should not have child.");
                break;
            }
            #endif
        }

        public override void ExtendEnterRegion(
            SortedSet<State> enterRegion,
            SortedSet<State> enterRegionEdge,
            SortedSet<State> extraEnterRegion,
            bool needCheckContain)
        {
            enterRegion.Remove(HostState);
            enterRegionEdge.Add(HostState);
        }

        public override void RegisterActiveState(SortedSet<State> activeStates)
        {
            // History should not register active
            #if DEBUG
            GD.PushError(HostState.GetPath(), ": history state should not be touched when stable.");
            #endif
        }
        
        public override bool SelectTransitions(List<Transition> enabledTransitions, StringName eventName)
        {
            #if DEBUG
            GD.PushError("Should not select transition in history state.");
            #endif
            return false;
        }

        public override void DeduceDescendants(SortedSet<State> deducedSet, bool isHistory)
        {
            // TODO:impl
        }
    }
}