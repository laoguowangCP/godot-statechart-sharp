
using System;

namespace LGWCP.Godot.StatechartSharp.Nodeless;

public partial class Statechart<TDuct, TEvent> : StatechartComposition<Statechart<TDuct, TEvent>>
    where TDuct : IStatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{

}