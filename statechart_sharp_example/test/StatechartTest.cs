using Godot;
using LGWCP.StatechartSharp;

public partial class StatechartTest : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void OnTransitionGuard(Transition transition)
	{
		GD.Print(transition.Name, ": transition guard.");
	}

	public void OnTransitionInvoke(Transition transition)
	{
		GD.Print(transition.Name, ": transition invoke.");
	}

	public void OnActionInvoke(Action action)
	{
		GD.Print(action.Name, ": action invoke.");
	}

	public void OnStateEnter(State state)
	{
		GD.Print(state.Name, ": state enter.");
	}

	public void OnStateEixt(State state)
	{
		GD.Print(state.Name, ": state exit.");
	}
}
