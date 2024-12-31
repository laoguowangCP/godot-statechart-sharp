using System;
using System.Collections.Generic;
using LGWCP.Godot.StatechartSharp.Nodeless.Internal;

namespace LGWCP.Godot.StatechartSharp.Nodeless;

public partial class StatechartBuilder<TDuct, TEvent>
    where TDuct : IStatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{
    // TODO: builder is necessary to provide typed composition
    public static (Statechart<TDuct, TEvent>, Builder) GetStatechartAndBuilder()
    {
        var statechart = new Statechart<TDuct, TEvent>();
        return (statechart, new Builder(statechart));
    }

    public class Builder
    {
        protected Statechart<TDuct, TEvent> Statechart;
        protected List<Action> BuildActions = new();

        public Builder(Statechart<TDuct, TEvent> statechart)
        {
            Statechart = statechart;
        }

        // TODO: fill params
        public State NewState()
        {
            var comp = new State() { _Statechart = Statechart };
            return comp;
        }

        public Transition NewTransition()
        {
            var comp = new Transition() { _Statechart = Statechart };
            return comp;
        }

        public Reaction NewReaction()
        {
            var comp = new Reaction() { _Statechart = Statechart };
            return comp;
        }

        // TODO: impl TransitionPromoter
        /*
        public TransitionPromoter NewTransitionPromoter()
        {
            var comp = new TransitionPromoter() { Statechart = Statechart };
            return comp;
        }
        */

        public bool Commit(Statechart<TDuct, TEvent> statechart, State rootState)
        {
            if (rootState is null)
            {
                return false;
            }
            // TODO: commit comps to statechart

            // TODO: process build comps (TransitionPromoter)
            // rootState.Build(); // ?
            rootState.SubmitBuildAction(BuildActions.Add);
            foreach (var buildAction in BuildActions)
            {
                buildAction();
            }

            var rootStateInt = Statechart<TDuct, TEvent>.GetStateInt(rootState);
            statechart.RootState = rootStateInt;

            // TODO: Setup comps
            // CommitRecur(statechart, rootState, rootStateInt); // ?
            // rootStateInt.Setup(rootState);
            // rootStateInt.SetupPost();
            return true;
        }

        protected void SubmitBuildAction(Action buildAction)
        {
            BuildActions.Add(buildAction);
        }
    }
}
