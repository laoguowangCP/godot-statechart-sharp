using System;


namespace LGWCP.Godot.StatechartSharp.Nodeless;

public partial class Statechart<TDuct, TEvent>
    where TDuct : StatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{

public class Reaction : Composition
{
    public Action<TDuct>[] _Invokes;
    public TEvent _Event;

    public Reaction(
        Statechart<TDuct, TEvent> statechart,
        TEvent @event,
        Action<TDuct>[] invokes) : base(statechart)
    {
        _Event = @event;
        _Invokes = invokes;
    }

    public override void _Setup(ref int parentOrderId)
    {
        base._Setup(ref parentOrderId);
        _Invokes ??= Array.Empty<Action<TDuct>>();
    }


    public void _ReactionInvoke(TDuct duct)
    {
        for (int i = 0; i < _Invokes.Length; ++i)
        {
            _Invokes[i](duct);
        }
    }

    public override Composition Duplicate(bool isDeepDuplicate)
    {
        return new Reaction(_HostStatechart, _Event, _Invokes);
    }
}

}
