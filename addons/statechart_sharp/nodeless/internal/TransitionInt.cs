using System;
using System.Collections.Generic;

namespace LGWCP.Godot.StatechartSharp.Nodeless.Internal;

public partial class StatechartInt<TDuct, TEvent>
    where TDuct : IStatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{
    public class TransitionInt : Composition
    {
        protected delegate void GuardEvent(TDuct duct);
        protected event GuardEvent Guard;
        protected delegate void InvokeEvent(TDuct duct);
        protected event InvokeEvent Invoke;

        public TEvent Event;
        protected List<StateInt> TargetStates;
        public StateInt SourceState;
        public StateInt LcaState;
        public SortedSet<StateInt> EnterRegion;
        protected SortedSet<StateInt> EnterRegionEdge;
        protected SortedSet<StateInt> DeducedEnterStates;
        public bool IsTargetless;

        public void TransitionInvoke(TDuct duct)
        {
            Invoke.Invoke(duct);
        }

        public SortedSet<StateInt> GetDeducedEnterStates()
        {
            DeducedEnterStates.Clear();
            foreach (var edgeState in EnterRegionEdge)
            {
                edgeState.DeduceDescendants(DeducedEnterStates, false, true);
            }
            return DeducedEnterStates;
        }
    }

}
