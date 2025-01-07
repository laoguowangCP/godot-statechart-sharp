using System;
using LGWCP.Godot.StatechartSharp.Nodeless.Internal;

namespace LGWCP.Godot.StatechartSharp.Nodeless;


public partial class StatechartBuilder<TDuct, TEvent>
    where TDuct : IStatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{
    public class Parallel : State<Parallel, ParallelInt<TDuct, TEvent>>
    {
        public bool SubmitPromoteStates(Action<IState> submit)
        {
            bool isPromote = true;
            foreach (var comp in _Comps)
            {
                if (comp is IState state
                    && state.SubmitPromoteStates(submit))
                {
                    isPromote = false;
                    break; // Parallel diff
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
