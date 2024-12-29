using System;


namespace LGWCP.Godot.StatechartSharp.Nodeless;

public partial class Statechart<TDuct, TEvent>
    where TDuct : IStatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{
    protected class CompoundInt : StateInt
    {
        public CompoundInt() {}

        public CompoundInt(State state) {}
    }
}