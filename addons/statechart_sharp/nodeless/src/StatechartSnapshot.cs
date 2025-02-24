using System;
using System.Linq;


namespace LGWCP.Godot.StatechartSharp.Nodeless;


public class StatechartSnapshot : IEquatable<StatechartSnapshot>
{
    public bool IsAllStateConfig;
    public int[] Config;

    public bool Equals(StatechartSnapshot other)
    {
        if (other == null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (IsAllStateConfig != other.IsAllStateConfig)
        {
            return false;
        }

        return Config.SequenceEqual(other.Config);
    }
}
