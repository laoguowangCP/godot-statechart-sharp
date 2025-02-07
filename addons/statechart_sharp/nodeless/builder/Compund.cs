using System;
using LGWCP.Godot.StatechartSharp.Nodeless.Internal;

namespace LGWCP.Godot.StatechartSharp.Nodeless;


public partial class StatechartBuilder<TDuct, TEvent>
    where TDuct : StatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{

public class Compound : State
{
    public State Initial;

    public Compound(
        Action<TDuct>[] enters,
        Action<TDuct>[] exits,
        State initial = null)
    {
        Enters = enters;
        Exits = exits;
        Initial = initial;
    }

    public override BuildComposition Duplicate()
    {
        return new Compound(Enters, Exits, Initial);
    }

    public override StatechartInt<TDuct, TEvent>.Composition _GetInternalComposition()
    {
        return new StatechartInt<TDuct, TEvent>.CompoundInt(this);
    }
}

}
