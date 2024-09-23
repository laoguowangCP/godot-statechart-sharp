using Godot;
using System;
using LGWCP.Godot.StatechartSharp;

public partial class NodeToBeFreed : Node
{
    public StringName MyName;
    public override void _Ready()
    {
        MyName = "NodeToBeFreed";
        // We have connection in editor, here we add some delegates
        State state = GetNodeOrNull<State>("../Statechart/Root/State");
        Transition transition = GetNodeOrNull<Transition>("../Statechart/Root/State/Transition");
        Reaction reaction = GetNodeOrNull<Reaction>("../Statechart/Root/State/Reaction");

        state.Connect(State.SignalName.Enter, Callable.From<StatechartDuct>(Bar));
        transition.Connect(Transition.SignalName.Guard, Callable.From<StatechartDuct>(Bar));
        transition.Connect(Transition.SignalName.Invoke, Callable.From<StatechartDuct>(Bar));
        reaction.Connect(Reaction.SignalName.Invoke, Callable.From<StatechartDuct>(Bar));
    }

    public void Foo(StatechartDuct duct)
    {
        GD.Print("Foo called from: ", duct.CompositionNode.Name);
    }

    public void Bar(StatechartDuct duct)
    {
        GD.Print("Bar called from: ", duct.CompositionNode.Name);
    }
}
