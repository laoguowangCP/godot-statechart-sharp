using System;
using LGWCP.Godot.StatechartSharp.Nodeless.Internal;

namespace LGWCP.Godot.StatechartSharp.Nodeless;

public enum StateModeEnum : int
{
    Compound,
    Parallel,
    History,
    DeepHistory
    // EXT: new state mode
}

public partial class StatechartBuilder<TDuct, TEvent>
    where TDuct : IStatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{
    public interface IState
    {
        public bool SubmitPromoteStates(Action<IBuildComposition> submit)
        {
            // EXT: required if extend state mode
            // return false by default;
            return false;
        }
    }
    
    public class State<TState, TStateInt> : BuildComposition<State<TState, TStateInt>>, IState
        where TState : State<TState, TStateInt>
        where TStateInt : StateInt<TDuct, TEvent>
    {
        public Action<TDuct>[] Enters;
        public Action<TDuct>[] Exits;
        public State Initial;

        public State() {}

        public State(
            Action<TDuct>[] enters,
            Action<TDuct>[] exits,
            State initial = null)
        {
            Enters = enters;
            Exits = exits;
            Initial = initial;
        }

        public override TState Duplicate()
        {
            // TODO: impl clone
            throw new NotImplementedException();
        }
    }
}
