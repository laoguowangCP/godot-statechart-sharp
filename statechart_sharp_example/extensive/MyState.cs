using Godot;
using LGWCP.StatechartSharp;

public partial class MyState : State
{
    protected override void CustomStateEnter(StatechartDuct duct)
    {
        base.CustomStateEnter(duct);
        GD.Print("Custom state enter");
    }

    protected override void CustomStateExit(StatechartDuct duct)
    {
        base.CustomStateExit(duct);
        GD.Print("Custom state exit");
    }
}
