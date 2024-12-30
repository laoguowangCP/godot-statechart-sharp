using System;
using System.Collections.Generic;

namespace LGWCP.Godot.StatechartSharp.Nodeless;

public partial class Statechart<TDuct, TEvent>
    where TDuct : IStatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{

    public class Reaction : BuildComposition<Reaction>
    {
        public Reaction() {}

        public override object Clone()
        {
            // TODO: impl clone
            return new Reaction();
        }
    }
}
