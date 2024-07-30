using Godot;
using LGWCP.StatechartSharp;


public partial class StatechartBuildTest : Node
{
    public override void _Ready()
    {
        Statechart statechart = new()
        {
            Name = "Statechart",
            IsWaitParentReady = true,
            EventFlag = EventFlagEnum.Process
        };
        State root = new() { Name = "Root", StateMode = StateModeEnum.Parallel };
        State stateA = new() { Name = "A" };
        State stateB = new() { Name = "B" };
        State stateAX = new() { Name = "X" };
        State stateAY = new() { Name = "Y" };
        State stateBX = new() { Name = "X" };
        State stateBY = new() { Name = "Y" };
        statechart.Append(
            root.Append(
                stateA.Append(
                    stateAX).Append(
                    stateAY)).Append(
                stateB.Append(
                    stateBX).Append(
                    stateBY)));
        Transition transition = new()
        {
            Name = "A/X->A/Y+B/X->B/Y",
            TargetStatesArray = { stateAY, stateBY }
        };
        transition.Invoke += TransitionInvoke;
        stateAX.Append(transition);
        AddChild(statechart);
    }

    public void TransitionInvoke(StatechartDuct duct)
    {
        GD.Print(duct.CompositionNode.GetPath(), ": transition invoked.");
    }
}