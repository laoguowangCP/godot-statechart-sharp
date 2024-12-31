using System;
using System.Collections.Generic;
using LGWCP.Godot.StatechartSharp.Nodeless.Internal;

namespace LGWCP.Godot.StatechartSharp.Nodeless;

public partial class StatechartBuilder<TDuct, TEvent>
    where TDuct : IStatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{

    public class Reaction : BuildComposition<Reaction>
    {
        public Reaction() {}

        public override Reaction Duplicate()
        {
            // TODO: impl clone
            throw new NotImplementedException();
        }
    }
}
