using System;
using System.Collections.Generic;
using LGWCP.Godot.StatechartSharp.Nodeless.Internal;

namespace LGWCP.Godot.StatechartSharp.Nodeless;


public partial class StatechartBuilder<TDuct, TEvent>
    where TDuct : IStatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{
    public class Compound : State
    {
        public override bool SubmitPromoteStates(Action<State> submit)
        {
            bool isPromote = true;
            foreach (var comp in _Comps)
            {
                if (comp is State state
                    && state.SubmitPromoteStates(submit))
                {
                    isPromote = false;
                }
            }
            if (isPromote)
            {
                submit(this);
            }
            // Make sure promoted
            return true;
        }
    }
}
