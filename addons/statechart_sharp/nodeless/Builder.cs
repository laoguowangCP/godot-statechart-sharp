using System;
using System.Collections.Generic;
using System.Linq;

namespace LGWCP.Godot.StatechartSharp.Nodeless;

public partial class Statechart<TDuct, TEvent>
    where TDuct : IStatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{
    // TODO: builder is necessary to provide typed composition
    public static (Statechart<TDuct, TEvent>, Builder) GetStatechartAndBuilder()
    {
        var statechart = new Statechart<TDuct, TEvent>();
        return (statechart, new Builder(statechart));
    }

    public class Builder
    {
        protected Statechart<TDuct, TEvent> HostStatechart;
        public Builder(Statechart<TDuct, TEvent> statechart)
        {
            HostStatechart = statechart;
        }


        // TODO: fill params
        public State NewState()
        {
            return new State();
        }

        public Transition NewTransition()
        {
            return new Transition();
        }

        public Reaction NewReaction()
        {
            return new Reaction();
        }

        public bool Commit(Statechart<TDuct, TEvent> statechart, State rootState)
        {
            // TODO: commit comps to statechart

            // TODO: process build comps (TransitionPromoter)
            // rootState.Build(); // ?

            var rootStateInt = GetStateInt(rootState);
            statechart.RootState = rootStateInt;

            // TODO: Setup comps
            // CommitRecur(statechart, rootState, rootStateInt); // ?
            // rootStateInt.Setup(rootState);
            // rootStateInt.SetupPost();
            return true;
        }

        private void CommitRecur<TBuildComp, TComp>(
            Statechart<TDuct, TEvent> statechart,
            BuildComposition<TBuildComp> pBuildComp,
            Composition<TComp> pComp)
            where TBuildComp : BuildComposition<TBuildComp>
            where TComp : Composition<TComp>
        {
            // TODO: commit recursively
            foreach (var comp in pBuildComp.Comps)
            {
                if (comp is State s)
                {
                    var sInt = GetStateInt(s);
                }
            }
        }
    }

    public interface IBuildComposition : IDisposable, ICloneable {}

    public abstract class BuildComposition<T> : IBuildComposition
        where T : BuildComposition<T>
    {
        // Child nodes
        public LinkedList<IBuildComposition> Comps { get; protected set; }
        // LLN to parent _comps. If reparent, use it to delist from former parent.
        protected LinkedListNode<IBuildComposition> _compIdx;

        public BuildComposition()
        {
            Comps = new();
        }

        public T Add<TChild>(TChild child)
            where TChild : BuildComposition<TChild>
        {
            Comps.AddLast(child);
            return (T)this;
        }

        public void Dispose() {}

        public abstract object Clone();
    }
}
