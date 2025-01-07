using System;

namespace LGWCP.Godot.StatechartSharp.Nodeless.Internal;


public class ReactionInt<TDuct, TEvent> : Composition<TDuct, TEvent>
    where TDuct : IStatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{
    protected delegate void InvokeEvent(TDuct duct);
    protected event InvokeEvent Invoke;

    public void ReactionInvoke(TDuct duct)
    {
        Invoke.Invoke(duct);
    }
}
