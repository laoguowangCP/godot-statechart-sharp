using Godot;
using System;
using LGWCP.Godot.StatechartSharp;

public partial class TestSignalDisconnect : Node
{
    NodeToBeFreed NodeToBeFreed;
    public override void _Ready()
    {
        NodeToBeFreed = GetNodeOrNull<NodeToBeFreed>("NodeToBeFreed");
        State state = GetNodeOrNull<State>("Statechart/Root/State");
        RemoveChild(NodeToBeFreed);
        // state.
        NodeToBeFreed.QueueFree();
    }
}
