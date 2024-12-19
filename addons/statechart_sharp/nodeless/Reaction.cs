using System;
using System.Collections.Generic;

namespace LGWCP.Godot.StatechartSharp.Nodeless;

public partial class Statechart<TDuct, TEvent>
    where TDuct : IStatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{
    protected class ReactionInt : StatechartComposition<ReactionInt>
    {
        protected delegate void InvokeEvent(TDuct duct);
        protected event InvokeEvent Invoke;

        public void ReactionInvoke(TDuct duct)
        {
            Invoke.Invoke(duct);
        }
    }

    public class Reaction
    {
        private ReactionInt _a;
        public Reaction()
        {

        }
    }
}
