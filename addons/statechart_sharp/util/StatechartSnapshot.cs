using Godot;


namespace LGWCP.StatechartSharp;


[Tool]
[GlobalClass]
public partial class StatechartSnapshot : Resource
{
    internal bool IsAllStateConfiguration;
    internal int[] Configuration;
}
