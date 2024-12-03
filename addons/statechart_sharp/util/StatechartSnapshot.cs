using System;
using System.Linq;
using Godot;


namespace LGWCP.Godot.StatechartSharp;


[Tool]
[GlobalClass]
public partial class StatechartSnapshot : Resource, IEquatable<StatechartSnapshot>
{
    [Export] public bool IsAllStateConfig;
    [Export] public int[] Config;

    public bool Equals(StatechartSnapshot other)
    {
        if (other == null)
        {
            return false;
        }
        
        if (IsAllStateConfig != other.IsAllStateConfig)
        {
            return false;
        }

        return Config.SequenceEqual(other.Config);
    }
}
