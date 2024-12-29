using System;
using System.Collections.Generic;

namespace LGWCP.Godot.StatechartSharp.Nodeless;

public partial class Statechart<TDuct, TEvent>
    where TDuct : IStatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{
    protected class TransitionInt : Composition<TransitionInt>
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

    public class Transition : BuildComposition<Transition>
    {

        public Transition()
        {}
        
        public Transition(TEvent @event, State[] targets=null, bool isAuto=false, Action<TDuct>[] guards=null, Action<TDuct>[] invokes = null)
        {
        }

        public override object Clone()
        {
            // TODO: impl clone
            return new Transition();
        }
    }
}
