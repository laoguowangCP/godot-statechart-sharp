using System;
using System.Collections.Generic;
using LGWCP.Godot.StatechartSharp.Nodeless.Internal;

namespace LGWCP.Godot.StatechartSharp.Nodeless;

public partial class StatechartBuilder<TDuct, TEvent>
    where TDuct : StatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{
    public abstract class BuildComposition
    {
        // Child nodes
        public LinkedList<BuildComposition> _Comps { get; protected set; } = new();
        // LLN of this comp in parent comps. If reparent, use it to delist from former parent.
        protected LinkedListNode<BuildComposition> CompIdx;
        // Parent comp
        public BuildComposition _PComp;
        // Internal composition: handle initial state, transition targets
        public StatechartInt<TDuct, TEvent>.Composition _CompInt;

        public BuildComposition() {}

        public BuildComposition Add<TChild>(TChild child)
            where TChild : BuildComposition
        {
            // Add default add last
            CompIdx = _Comps.AddLast(child);
            return this;
        }

        public BuildComposition AddFirst<TChild>(TChild child)
            where TChild : BuildComposition
        {
            CompIdx = _Comps.AddFirst(child);
            return this;
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
        public virtual void _SubmitBuildAction(Action<Action> submit)
        {
            foreach (var comp in _Comps)
            {
                comp._SubmitBuildAction(submit);
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

        public abstract BuildComposition Duplicate();

        public abstract StatechartInt<TDuct, TEvent>.Composition _GetInternalComposition();
    }
}
