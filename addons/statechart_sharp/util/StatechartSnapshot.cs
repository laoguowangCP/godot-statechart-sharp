using Godot;


namespace LGWCP.Godot.StatechartSharp;


[Tool]
[GlobalClass]
public partial class StatechartSnapshot : Resource
{
    [Export] public bool IsAllStateConfig;
    [Export] public int[] Config;
}
