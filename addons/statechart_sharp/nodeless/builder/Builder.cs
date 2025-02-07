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

    public Compound GetCompound(
        Action<TDuct>[] enters,
        Action<TDuct>[] exits,
        State initial = null)
    {
        return new Compound(enters, exits, initial);
    }

    public Parallel GetParallel(
        Action<TDuct>[] enters,
        Action<TDuct>[] exits)
    {
        return new Parallel(enters, exits);
    }

    public History GetHistory()
    {
        return new History();
    }

    public DeepHistory GetDeepHistory()
    {
        return new DeepHistory();
    }

    public Transition GetTransition(
        TEvent @event,
        State[] targets=null,
        Action<TDuct>[] guards=null,
        Action<TDuct>[] invokes=null)
    {
        return new Transition(@event, targets, false, guards, invokes);
    }

    public Transition GetAutoTransition(
        State[] targets=null,
        Action<TDuct>[] guards=null,
        Action<TDuct>[] invokes=null)
    {
        return new Transition(default, targets, true, guards, invokes);
    }
}
