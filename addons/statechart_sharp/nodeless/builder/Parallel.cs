using System;
using System.Collections.Generic;
using LGWCP.Godot.StatechartSharp.Nodeless.Internal;

namespace LGWCP.Godot.StatechartSharp.Nodeless;


public partial class StatechartBuilder<TDuct, TEvent>
    where TDuct : StatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{
    public class Parallel : State
    {
        public Parallel(
            Action<TDuct>[] enters,
            Action<TDuct>[] exits)
        {
            Enters = enters;
            Exits = exits;
        }

        public override BuildComposition Duplicate()
        {
            return new Parallel(Enters, Exits);
        }
    }
}
