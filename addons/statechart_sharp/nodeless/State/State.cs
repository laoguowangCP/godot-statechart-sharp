using System;
using System.Collections.Generic;

namespace LGWCP.Godot.StatechartSharp.Nodeless;

public enum StateModeEnum : int
{
    Compound,
    Parallel,
    History,
    DeepHistory
}

public partial class Statechart<TDuct, TEvent>
    where TDuct : IStatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{
    public class State : BuildComposition<State>
    {
        public StateModeEnum Mode;
        public Action<TDuct>[] Enters;
        public Action<TDuct>[] Exits;
        public State Initial;

        public State() {}

        public State(
            StateModeEnum mode,
            Action<TDuct>[] enters,
            Action<TDuct>[] exits,
            State initial = null)
        {
            Mode = mode;
            Enters = enters;
            Exits = exits;
            Initial = initial;
        }

        public override object Clone()
        {
            // TODO: impl clone
            return new State();
        }
    }

    protected static StateInt GetStateInt(State state) => state.Mode switch
    {
        StateModeEnum.Compound => new CompoundInt(),
        // StateModeEnum.Parallel => new ParallelInt(),
        // StateModeEnum.History => new HistoryInt(),
        _ => null
    };
}
