using System;
using System.Collections.Generic;
using LGWCP.Godot.StatechartSharp.Nodeless.Internal;

namespace LGWCP.Godot.StatechartSharp.Nodeless;

public partial class StatechartBuilder<TDuct, TEvent>
    where TDuct : StatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{

    public class Reaction : BuildComposition
    {
        public TEvent Event;
        public Action<TDuct>[] Invokes;

        public Reaction(
            TEvent @event,
            Action<TDuct>[] invokes=null)
        {
            Event = @event;
            Invokes = invokes;
        }

        public override Reaction Duplicate()
        {
            return new Reaction(Event, Invokes);
        }
    }
}
