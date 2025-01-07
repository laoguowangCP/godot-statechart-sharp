using System;
using System.Collections.Generic;

namespace LGWCP.Godot.StatechartSharp.Nodeless.Internal;

public partial class StatechartInt<TDuct, TEvent>
    where TDuct : IStatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{
    public class ReactionInt : Composition
    {
        protected delegate void InvokeEvent(TDuct duct);
        protected event InvokeEvent Invoke;

        public void ReactionInvoke(TDuct duct)
        {
            Invoke.Invoke(duct);
        }
    }
}
