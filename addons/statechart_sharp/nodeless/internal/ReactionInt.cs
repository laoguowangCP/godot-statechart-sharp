using System;
using System.Collections.Generic;

namespace LGWCP.Godot.StatechartSharp.Nodeless.Internal;

public partial class StatechartInt<TDuct, TEvent>
    where TDuct : StatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{
    public class ReactionInt : Composition
    {
        protected Action<TDuct>[] Invokes;
        public TEvent Event;

        public void ReactionInvoke(TDuct duct)
        {
            for (int i = 0; i < Invokes.Length; ++i)
            {
                Invokes[i](duct);
            }
        }
    }
}
