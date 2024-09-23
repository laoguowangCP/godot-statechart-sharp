using Godot;
using LGWCP.Godot.StatechartSharp;

// Use Tool attribute so editor can check configuration
[Tool]
public partial class MyTransition : Transition
{
    protected override void CustomTransitionInvoke(StatechartDuct duct)
    {
        base.CustomTransitionInvoke(duct);
        GD.Print("Custom transition invoke");
    }

    protected override bool CustomTransitionGuard(StatechartDuct duct)
    {
        GD.Print("Custom transition guard");
        return base.CustomTransitionGuard(duct);
    }
}
