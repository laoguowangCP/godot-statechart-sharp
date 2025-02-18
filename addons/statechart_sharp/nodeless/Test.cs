using System;
using System.Collections.Generic;
using LGWCP.Godot.StatechartSharp.Nodeless;

public class Test
{
    protected Statechart<StatechartDuct, string> Statechart;

    public Test()
    {
        var sc = new Statechart<StatechartDuct, string>();

        // sb.GetNewState().SetAsRootState(sc);
        var rootState = sc.GetCompound();
        var s1 = sc.GetParallel();
        var s2 = sc.GetCompound();

        var t1 = sc.GetTransition("go");
        var t2 = sc.GetAutoTransition();

        var a1 = sc.GetReaction("go");
        var a2 = sc.GetReaction("stop");

        rootState
            .Append(s1
                .Append(a1)
                .Append(t1))
            .Append(s2
                .Append(t2)
                .Append(a2));
        sc.Ready(rootState);
        Statechart = sc;

        Statechart.Step("go");
    }
}

