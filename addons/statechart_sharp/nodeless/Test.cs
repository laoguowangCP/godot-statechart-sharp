using System;
using System.Collections.Generic;
using LGWCP.Godot.StatechartSharp.Nodeless;

public class Test
{
    protected Statechart<StatechartDuct, string> Statechart;

    public Test()
    {
        var builder = new StatechartBuilder<StatechartDuct, string>();

        // sb.GetNewState().SetAsRootState(sc);
        {
            var rootState = builder.NewState();
            var s1 = builder.NewState();
            var s2 = builder.NewState();

            var t1 = builder.NewTransition();
            var t2 = builder.NewTransition();

            var a1 = builder.NewReaction();
            var a2 = builder.NewReaction();

            rootState
                .Add(s1
                    .Add(a1)
                    .Add(t1))
                .Add(s2
                    .Add(t2)
                    .Add(a2));
            Statechart = builder.Commit(rootState);
        }

        Statechart.Step("go");
    }
}

