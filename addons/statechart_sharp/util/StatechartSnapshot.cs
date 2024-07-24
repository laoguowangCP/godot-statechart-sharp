using Godot;


namespace LGWCP.StatechartSharp;


[Tool]
[GlobalClass]
public partial class StatechartSnapshot : Resource
{
    [Export] internal bool IsAllStateConfiguration;
    [Export] internal int[] Configuration;
}
