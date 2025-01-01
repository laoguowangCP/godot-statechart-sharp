using System;
using System.Collections.Generic;
using System.Linq;
using LGWCP.Godot.StatechartSharp.Nodeless.Internal;


namespace LGWCP.Godot.StatechartSharp.Nodeless;

public partial class Statechart<TDuct, TEvent>
    where TDuct : IStatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{
    protected StatechartInt<TDuct, TEvent> StatechartInt;
    public Action<TEvent> Step;

    public Statechart(StatechartInt<TDuct, TEvent> statechartInt)
    {
        StatechartInt = statechartInt;
        Step = statechartInt.Step;
    }
}
