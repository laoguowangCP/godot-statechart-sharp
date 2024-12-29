using System;
using System.Collections.Generic;
using LGWCP.Godot.StatechartSharp.Nodeless;

namespace LGWCP.Godot.StatechartSharp.Test;

public class Test
{
    public Test()
    {
        var (sc, sb) = Statechart<BaseStatechartDuct, string>.GetStatechartAndBuilder();
        // sb.GetNewState().SetAsRootState(sc);
        {
            using var rootState = sb.NewState();
            using var s1 = sb.NewState();
            using var s2 = sb.NewState();

            using var t1 = sb.NewTransition();
            using var t2 = sb.NewTransition();

            using var a1 = sb.NewReaction();
            using var a2 = sb.NewReaction();

            rootState
                .Add(s1
                    .Add(a1)
                    .Add(t1))
                .Add(s2
                    .Add(t2)
                    .Add(a2));
        }
    }
}

