using System;


namespace LGWCP.Godot.StatechartSharp.NodelessV2;

public class Reaction<TDuct, TEvent>
    where TDuct : StatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{

}
