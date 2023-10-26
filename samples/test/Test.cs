using Godot;
using LGWCP.GodotPlugin.StateChartSharp;
using System;

public partial class Test : Node2D
{
	protected StateChart stateChart;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		stateChart = GetNode<StateChart>("StateChart");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("Space"))
		{
			stateChart.Step();
		}
	}

	public void OnRootEnter()
	{
		GD.Print("Root_p Enter...");
	}
	
	public void OnTopEnter()
	{
		GD.Print("Top Enter...");
	}

	public void OnStateAEnter()
	{
		GD.Print("StateA Enter...");
	}

	public void OnStateBEnter()
	{
		GD.Print("StateB Enter...");
	}

	public void OnStateCEnter()
	{
		GD.Print("StateC Enter...");
	}

	public void OnStateDEnter()
	{
		GD.Print("StateD Enter...");
	}

	public void OnStateEEnter()
	{
		GD.Print("StateE Enter...");
	}

	public void OnStateFEnter()
	{
		GD.Print("StateF Enter...");
	}

	public void CheckCtoD(Transition t)
	{
		GD.Print("Check CtoD");
		t.SetChecked(true);
	}
}
