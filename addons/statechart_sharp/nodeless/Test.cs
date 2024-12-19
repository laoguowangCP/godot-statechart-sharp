using System;
using System.Collections.Generic;
using LGWCP.Godot.StatechartSharp.Nodeless;

namespace LGWCP.Godot.StatechartSharp.Test;

public class Test
{
    public Test()
    {
        var (sc, sb) = Statechart<BaseStatechartDuct, string>.GetStatechartAndBuilder();
        sb.GetNewState().SetAsRootState(sc);
    }
}

