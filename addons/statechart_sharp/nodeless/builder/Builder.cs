using System;
using System.Collections.Generic;
using System.Linq;

namespace LGWCP.Godot.StatechartSharp.Nodeless;

public partial class Statechart<TDuct, TEvent>
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
            // TODO: commit comps to statechart

            // TODO: process build comps (TransitionPromoter)
            // rootState.Build(); // ?
            rootState.SubmitBuildAction(BuildActions.Add);
            foreach (var buildAction in BuildActions)
            {
                buildAction();
            }

            var rootStateInt = GetStateInt(rootState);
            statechart.RootState = rootStateInt;

            // TODO: Setup comps
            // CommitRecur(statechart, rootState, rootStateInt); // ?
            // rootStateInt.Setup(rootState);
            // rootStateInt.SetupPost();
            return true;
        }


        // TODO: may not use it
        private void CommitRecur<TBuildComp, TComp>(
            Statechart<TDuct, TEvent> statechart,
            BuildComposition<TBuildComp> pBuildComp,
            Composition<TComp> pComp)
            where TBuildComp : BuildComposition<TBuildComp>
            where TComp : Composition<TComp>
        {
            // TODO: commit recursively
            foreach (var comp in pBuildComp._Comps)
            {
                if (comp is State s)
                {
                    var sInt = GetStateInt(s);
                }
            }
        }

        protected void SubmitBuildAction(Action buildAction)
        {
            BuildActions.Add(buildAction);
        }
    }
}
