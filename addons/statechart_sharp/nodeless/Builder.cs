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
        protected Statechart<TDuct, TEvent> HostStatechart;
        public Builder(Statechart<TDuct, TEvent> statechart)
        {
            HostStatechart = statechart;
        }

        /*
        public State GetNewState(StateModeEnum mode)
        {
            return new State(mode);
        }

        public Transition GetNewTransition()
        {
            return new Transition();
        }

        public Transition GetNewReaction()
        {
            return new Reaction();
        }
        */
    }

    public abstract class BuildComposition<T> : IComposition, IDisposable
        where T : BuildComposition<T>
    {
        private LinkedList<IComposition> _comps;
        public BuildComposition()
        {
            _comps = new();
        }

        public void Add<TChild>(TChild child)
            where TChild : BuildComposition<TChild>
        {
            _comps.AddLast(child);
        }

        public void Dispose() {}
    }
}
