using System;
using System.Collections.Generic;
using LGWCP.Godot.StatechartSharp.Nodeless.Internal;

namespace LGWCP.Godot.StatechartSharp.Nodeless;

public partial class StatechartBuilder<TDuct, TEvent>
    where TDuct : StatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{
    // TODO: builder is necessary to provide typed composition
    // protected StatechartInt<TDuct, TEvent> Statechart;
    protected List<Action> BuildActions = new();

    public StatechartBuilder() {}


    // TODO: impl TransitionPromoter
    /*
    public TransitionPromoter NewTransitionPromoter()
    {
        var comp = new TransitionPromoter() { Statechart = Statechart };
        return comp;
    }
    */

    public Statechart<TDuct, TEvent> Commit(State rootState)
    {
        if (rootState is null)
        {
            return null;
        }

        // TODO: process build comps (TransitionPromoter)
        // rootState.Build(); // ?
        rootState._SubmitBuildAction(BuildActions.Add);
        foreach (var buildAction in BuildActions)
        {
            buildAction();
        }

        var statechartInt = StatechartInt<TDuct, TEvent>.Commit(rootState);

        // rootStateInt.SetupPost();
        return new Statechart<TDuct, TEvent>(statechartInt);
    }

    protected void SubmitBuildAction(Action buildAction)
    {
        BuildActions.Add(buildAction);
    }
}
