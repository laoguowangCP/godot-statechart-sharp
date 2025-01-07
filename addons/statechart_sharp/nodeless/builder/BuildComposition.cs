using System;
using System.Collections.Generic;
using LGWCP.Godot.StatechartSharp.Nodeless.Internal;

namespace LGWCP.Godot.StatechartSharp.Nodeless;

public partial class StatechartBuilder<TDuct, TEvent>
    where TDuct : IStatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{
    public interface IBuildComposition : IDisposable
    {
        public void Add(IBuildComposition child);
        public void AddFirst(IBuildComposition child);
        public void Detach();
        public void SubmitBuildAction(Action<Action> submit);
    }

    public interface IBuildComposition<TComposition>
        where TComposition : Composition<TDuct, TEvent>
    {
        public TComposition GetComposition();
    }

    public abstract class BuildComposition<TSelf> : IBuildComposition
        where TSelf : BuildComposition<TSelf>
    {
        public StatechartInt<TDuct, TEvent> _Statechart;
        // Child nodes
        public LinkedList<IBuildComposition> _Comps { get; protected set; } = new();
        // LLN of this comp in parent comps. If reparent, use it to delist from former parent.
        protected LinkedListNode<IBuildComposition> CompIdx;
        // Parent comp
        public IBuildComposition PComp;

        public BuildComposition() {}

        public TSelf Add<TChild>(TChild child)
            where TChild : BuildComposition<TChild>
        {
            // Add default add last
            CompIdx = _Comps.AddLast(child);
            return (TSelf)this;
        }

        public TSelf AddFirst<TChild>(TChild child)
            where TChild : BuildComposition<TChild>
        {
            CompIdx = _Comps.AddFirst(child);
            return (TSelf)this;
        }

        public void Detach()
        {
            // Detach this comp from parent
            if (CompIdx is null)
            {
                return; // Orphan comp
            }
            var pComps = CompIdx.List;
            pComps.Remove(CompIdx);
            CompIdx = null;
        }

        // TODO: use BuildActionDetachInvalid in SubmitBuildAction
        public virtual void SubmitBuildAction(Action<Action> submit)
        {
            foreach (var comp in _Comps)
            {
                comp.SubmitBuildAction(submit);
            }
        }

        protected virtual void BuildActionDetachInvalid()
        {
            // TODO: impl BuildActionDetachInvalid
            // TODO: use BuildActionDetachInvalid in SubmitBuildAction
            // Detach if build comp is invalid
            // Detach by default
            Detach();
        }

        public void Dispose() {}

        public abstract TSelf Duplicate();

        public void Add(IBuildComposition child)
        {
            CompIdx = _Comps.AddLast(child);
        }

        public void AddFirst(IBuildComposition child)
        {
            CompIdx = _Comps.AddFirst(child);
        }
    }
}
