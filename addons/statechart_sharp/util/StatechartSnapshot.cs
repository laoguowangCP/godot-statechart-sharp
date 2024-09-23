using Godot;


namespace LGWCP.Godot.StatechartSharp;


[Tool]
[GlobalClass]
public partial class StatechartSnapshot : Resource
{
    [Export] public bool IsAllStateConfiguration;
    [Export] public int[] Configuration;
}
