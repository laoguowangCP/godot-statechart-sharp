using System;


namespace LGWCP.Godot.StatechartSharp.NodelessV2;

public class Transition<TDuct, TEvent>
    where TDuct : StatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{
}
