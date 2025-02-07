using System;
using System.Collections.Generic;
using LGWCP.Godot.StatechartSharp.Nodeless.Internal;

namespace LGWCP.Godot.StatechartSharp.Nodeless;


public partial class StatechartBuilder<TDuct, TEvent>
    where TDuct : StatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{
    public class History : State
    {
        public History() {}

        public override BuildComposition Duplicate()
        {
            return new History();
        }

        public override StatechartInt<TDuct, TEvent>.Composition _GetInternalComposition()
        {
            return new StatechartInt<TDuct, TEvent>.HistoryInt(this);
        }
    }
}
