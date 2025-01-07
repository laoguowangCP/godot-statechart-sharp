using System;
using LGWCP.Godot.StatechartSharp.Nodeless.Internal;

namespace LGWCP.Godot.StatechartSharp.Nodeless;


public partial class StatechartBuilder<TDuct, TEvent>
    where TDuct : IStatechartDuct, new()
    where TEvent : IEquatable<TEvent>
{
    public class History : State<History, HistoryInt<TDuct, TEvent>>
    {
        
    }
}
