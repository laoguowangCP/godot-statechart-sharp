using Godot;
using LGWCP.StatechartSharp;
using System;

public partial class Test : Node2D
{
	protected Statechart statechart;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		statechart = GetNode<Statechart>("Statechart");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("Jump"))
		{
            GD.Print("---- Statechart Step ----");
		}
	}

	public void CheckCtoF(Transition t)
	{
		// Beware transition is enabled at default
		bool isEnabled = Input.IsActionJustPressed("Jump");
		if (isEnabled)
		{
			GD.Print("- CtoF is enabled");
		}
		t.IsEnabled = isEnabled;
	}
}
