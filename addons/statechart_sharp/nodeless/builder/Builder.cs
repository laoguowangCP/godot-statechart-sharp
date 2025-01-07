using System;
using System.Collections.Generic;
using LGWCP.Godot.StatechartSharp.Nodeless.Internal;

namespace LGWCP.Godot.StatechartSharp.Nodeless;

public partial class StatechartBuilder<TDuct, TEvent>
    where TDuct : IStatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{
    // TODO: builder is necessary to provide typed composition
    // protected StatechartInt<TDuct, TEvent> Statechart;
    protected List<Action> BuildActions = new();

    public StatechartBuilder() {}

    // TODO: fill params
    public State NewState(StateModeEnum mode)
    {
        var comp = GetState(mode);
        return comp;

        static State GetState(StateModeEnum mode) => mode switch
        {
            StateModeEnum.Compound => new Compound(),
            StateModeEnum.Parallel => new Parallel(),
            StateModeEnum.History => new History(),
            StateModeEnum.DeepHistory => new DeepHistory(),
            // EXT: new state mode
            _ => null
        };
    }

    public Transition NewTransition()
    {
        var comp = new Transition();
        return comp;
    }

    public Reaction NewReaction()
    {
        var comp = new Reaction();
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

    public Statechart<TDuct, TEvent> Commit(State rootState)
    {
        var statechartInt = new StatechartInt<TDuct, TEvent>();
        if (rootState is null)
        {
            return null;
        }
        // TODO: commit comps to statechart

        // TODO: process build comps (TransitionPromoter)
        // rootState.Build(); // ?
        rootState.SubmitBuildAction(BuildActions.Add);
        foreach (var buildAction in BuildActions)
        {
            buildAction();
        }

        var rootStateInt = StatechartInt<TDuct, TEvent>.GetStateInt(rootState);
        statechartInt.RootState = rootStateInt;

        // TODO: Setup comps
        // CommitRecur(statechart, rootState, rootStateInt); // ?
        // rootStateInt.Setup(rootState);
        // rootStateInt.SetupPost();
        return new Statechart<TDuct, TEvent>(statechartInt);
    }

    protected void SubmitBuildAction(Action buildAction)
    {
        BuildActions.Add(buildAction);
    }
}
