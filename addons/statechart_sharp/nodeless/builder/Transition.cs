using System;
using System.Collections.Generic;
using LGWCP.Godot.StatechartSharp.Nodeless.Internal;

namespace LGWCP.Godot.StatechartSharp.Nodeless;

public partial class StatechartBuilder<TDuct, TEvent>
    where TDuct : StatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{
    public class Transition : BuildComposition
    {
        public TEvent Event;
        public State[] Targets;
        public bool IsAuto;
        public Action<TDuct>[] Guards;
        public Action<TDuct>[] Invokes;

        public Transition(
            TEvent @event,
            State[] targets=null,
            bool isAuto=false,
            Action<TDuct>[] guards=null,
            Action<TDuct>[] invokes=null)
        {
            Event = @event;
            Targets = targets;
            IsAuto = isAuto;
            Guards = guards;
            Invokes = invokes;
        }

        public override Transition Duplicate()
        {
            return new Transition(Event, Targets, IsAuto, Guards, Invokes);
        }

        public override StatechartInt<TDuct, TEvent>.Composition _GetInternalComposition()
        {
            return new StatechartInt<TDuct, TEvent>.TransitionInt(this);
        }
    }
}
