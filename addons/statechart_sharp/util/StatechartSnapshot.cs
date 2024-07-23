using Godot;


namespace LGWCP.StatechartSharp;


[Tool]
[GlobalClass]
public partial class StatechartSnapshot : Resource
{
    [Export] public bool IsAllStateConfiguration;
    [Export] public int[] Configuration;
}
