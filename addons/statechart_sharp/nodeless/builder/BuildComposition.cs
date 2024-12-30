using System;
using System.Collections.Generic;

namespace LGWCP.Godot.StatechartSharp.Nodeless;

public partial class Statechart<TDuct, TEvent>
    where TDuct : IStatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{
    public interface IBuildComposition : IDisposable
    {
        public void SubmitBuildAction(Action<Action> submit) {}
    }

    public abstract class BuildComposition<T> : IBuildComposition
        where T : BuildComposition<T>
    {
        public Statechart<TDuct, TEvent> _Statechart;
        // Child nodes
        public LinkedList<IBuildComposition> _Comps { get; protected set; } = new();
        // LLN to parent _comps. If reparent, use it to delist from former parent.
        protected LinkedListNode<IBuildComposition> CompIdx;
        // Parent comp
        public IBuildComposition PComp;

        public BuildComposition() {}

        public T Add<TChild>(TChild child)
            where TChild : BuildComposition<TChild>
        {
            // Add default add last
            _Comps.AddLast(child);
            return (T)this;
        }

        public T AddFirst<TChild>(TChild child)
            where TChild : BuildComposition<TChild>
        {
            _Comps.AddFirst(child);
            return (T)this;
        }

        public void Detach()
        {
            // Detach this comp from parent
            var pComps = CompIdx.List;
            pComps.Remove(CompIdx);
        }

        // TODO: scratch
        public virtual void SubmitBuildAction(Action<Action> submit) {}

        public void Dispose() {}

        public abstract T Duplicate();
    }
}
