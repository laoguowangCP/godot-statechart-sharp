using Godot;
using LGWCP.Godot.StatechartSharp;


public partial class StatechartBuildTest : Node
{
    public override void _Ready()
    {
        // Create statechart
        Statechart statechart = new()
        {
            Name = "Statechart",
            IsWaitParentReady = true,
            EventFlag = EventFlagEnum.Process
        };

        // Build some states
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
        
        // Add transition and promoter
        Transition transition = new()
        {
            Name = "A/X->A/Y+B/X->B/Y",
            TargetStatesArray = { stateAY, stateBY }
        };

        /*
        transition.Invoke += TransitionInvoke;
        += expression is not managed,
        -= is needed when statechart node is freed
        */
        transition.Connect(Transition.SignalName.Invoke, Callable.From<StatechartDuct>(TransitionInvoke));

        stateA.Append(
            transition.Append(
                new TransitionPromoter()));

        // Ship our statechart
        AddChild(statechart);
    }

    public void TransitionInvoke(StatechartDuct duct)
    {
        GD.Print(duct.CompositionNode.GetPath(), ": transition invoked.");
    }
}