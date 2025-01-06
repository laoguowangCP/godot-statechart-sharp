using System;
using System.Collections.Generic;
using LGWCP.Godot.StatechartSharp.Nodeless;

public class Test
{
    protected Statechart<BaseStatechartDuct, string> Statechart;

    public Test()
    {
        var builder = new StatechartBuilder<BaseStatechartDuct, string>();

        // sb.GetNewState().SetAsRootState(sc);
        {
            using var rootState = builder.NewState();
            using var s1 = builder.NewState();
            using var s2 = builder.NewState();

            using var t1 = builder.NewTransition();
            using var t2 = builder.NewTransition();

            using var a1 = builder.NewReaction();
            using var a2 = builder.NewReaction();

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

