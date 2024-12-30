using System;
using System.Collections.Generic;

namespace LGWCP.Godot.StatechartSharp.Nodeless;

public partial class Statechart<TDuct, TEvent>
    where TDuct : IStatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{
    public interface IBuildComposition : IDisposable, ICloneable {}

    public abstract class BuildComposition<T> : IBuildComposition
        where T : BuildComposition<T>
    {
        public Statechart<TDuct, TEvent> Statechart;
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
