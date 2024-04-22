using Godot;
using LGWCP.StatechartSharp;

// Use Tool attribute so editor can check configuration
[Tool]
public partial class MyReaction : Reaction
{
    protected override void CustomReactionInvoke(StatechartDuct duct)
    {
        base.CustomReactionInvoke(duct);
        GD.Print("Custom reation invoke");
    }
}
