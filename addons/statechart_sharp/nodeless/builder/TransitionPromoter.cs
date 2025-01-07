using System;
using System.Collections.Generic;
using LGWCP.Godot.StatechartSharp.Nodeless.Internal;

namespace LGWCP.Godot.StatechartSharp.Nodeless;

public partial class StatechartBuilder<TDuct, TEvent>
    where TDuct : IStatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{
    public class TransitionPromoter : BuildComposition<TransitionPromoter>
    {
        public TransitionPromoter() {}

        public override void SubmitBuildAction(Action<Action> submit)
        {
            submit(Promote);
        }

        protected void Promote()
        {
            if (PComp is Transition hostTransition
                && hostTransition.PComp is IState hostState)
            {
                // No need extra flag to pend multiple promoter
                // Unlike handled in node tree
                // Promoted transition leaves comps immediately

                List<IBuildComposition> promoteStates = new();
                _ = hostState.SubmitPromoteStates(promoteStates.Add);

                foreach (var s in promoteStates)
                {
                    var t = hostTransition.Duplicate();
                    s.AddFirst(t);
                }

                hostTransition.Detach();
            }
        }

        public override TransitionPromoter Duplicate()
        {
            // TODO: impl clone
            throw new NotImplementedException();
        }
    }
}
