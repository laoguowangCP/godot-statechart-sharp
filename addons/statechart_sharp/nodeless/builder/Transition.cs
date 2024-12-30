using System;
using System.Collections.Generic;

namespace LGWCP.Godot.StatechartSharp.Nodeless;

public partial class Statechart<TDuct, TEvent>
    where TDuct : IStatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{
    public class Transition : BuildComposition<Transition>
    {

        public Transition() {}
        
        public Transition(TEvent @event, State[] targets=null, bool isAuto=false, Action<TDuct>[] guards=null, Action<TDuct>[] invokes = null)
        {
        }

        public override Transition Duplicate()
        {
            // TODO: impl clone
            throw new NotImplementedException();
        }
    }
}
