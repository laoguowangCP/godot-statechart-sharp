using System;
using System.Collections.Generic;

namespace LGWCP.Godot.StatechartSharp.Nodeless.Internal;

public partial class StatechartInt<TDuct, TEvent>
    where TDuct : StatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{
    public class TransitionInt : Composition
    {
        protected Action<TDuct>[] Guards;
        protected Action<TDuct>[] Invokes;

        public TEvent Event;
        protected List<StateInt> TargetStates;
        public StateInt SourceState;
        public StateInt LcaState;
        public SortedSet<StateInt> EnterRegion;
        protected SortedSet<StateInt> EnterRegionEdge;
        protected SortedSet<StateInt> DeducedEnterStates;
        public bool IsTargetless;
        public bool IsAuto;

        public void TransitionInvoke(TDuct duct)
        {
            for (int i = 0; i < Invokes.Length; ++i)
            {
                Invokes[i](duct);
            }
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
