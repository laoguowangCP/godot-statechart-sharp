using System;
using System.Collections.Generic;
using LGWCP.Godot.StatechartSharp.Nodeless.Internal;

namespace LGWCP.Godot.StatechartSharp.Nodeless;

public enum StateModeEnum : int
{
    Compound,
    Parallel,
    History,
    DeepHistory
}

public partial class StatechartBuilder<TDuct, TEvent>
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

        public virtual bool SubmitPromoteStates(Action<State> submit)
        {
            // Used by transition promoter
            if (Mode is StateModeEnum.Compound)
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
            else if (Mode is StateModeEnum.Parallel)
            {
                bool isPromote = true;
                foreach (var comp in _Comps)
                {
                    if (comp is State state)
                    {
                        if (state.SubmitPromoteStates(submit))
                        {
                            isPromote = false;
                            break; // Parallel diff
                        }
                    }
                }
                if (isPromote)
                {
                    submit(this);
                }
                // Make sure promoted
                return true;
            }
            // EXT: required if extend state mode
            else // History
            {
                return false;
            }
        }

        public override State Duplicate()
        {
            // TODO: impl clone
            throw new NotImplementedException();
        }
    }
}
