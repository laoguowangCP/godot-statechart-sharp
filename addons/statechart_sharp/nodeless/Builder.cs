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
    }

    public abstract class BuildComposition<T> : IComposition, IDisposable
        where T : BuildComposition<T>
    {
        // Child nodes
        private LinkedList<IComposition> _comps;
        // LLN to parent _comps. If reparent, use it to delist from former parent.
        private LinkedListNode<IComposition> _compIdx;

        public BuildComposition()
        {
            _comps = new();
        }

        public T Add<TChild>(TChild child)
            where TChild : BuildComposition<TChild>
        {
            _comps.AddLast(child);
            return (T)this;
        }

        public void Dispose() {}
    }
}
