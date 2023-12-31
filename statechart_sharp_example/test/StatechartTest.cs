using Godot;
using LGWCP.StatechartSharp;

public partial class StatechartTest : Node
{
	[Export] StringName StepName;
	[Export] Statechart TestStatechart;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ConnectToTest(TestStatechart);
	}

	protected void ConnectToTest(Node node)
	{
		foreach (Node child in node.GetChildren())
		{
			if (child is State state)
			{
				state.Enter += OnStateEnter;
				state.Exit += OnStateExit;
			}
			else if (child is Transition transition)
			{
				transition.Guard += OnTransitionGuard;
				transition.Invoke += OnTransitionInvoke;
			}
			else if (child is Action action)
			{
				action.Invoke += OnActionInvoke;
			}
			else
			{
				continue;
			}

			ConnectToTest(child);
		}
	}

	public void OnTestButtonPressed()
	{
		GD.Print("-------- Test started --------");
		TestStatechart?.Step(StepName);
		GD.Print("-------- Test finished --------");
	}

	public void OnTransitionGuard(Transition transition)
	{
		NodePath path = TestStatechart.GetPathTo(transition);
        string indentStr = "";
		for (int i=0; i<path.GetNameCount(); ++i)
		{
			indentStr += ">";
		}
		GD.Print(indentStr, " Guard: ", path);
	}

	public void OnTransitionInvoke(Transition transition)
	{
		NodePath path = TestStatechart.GetPathTo(transition);
        string indentStr = "";
		for (int i=0; i<path.GetNameCount(); ++i)
		{
			indentStr += ">";
		}
		GD.Print(indentStr, " Invoke: ", path);
	}

	public void OnActionInvoke(Action action)
	{
		NodePath path = TestStatechart.GetPathTo(action);
        string indentStr = "";
		for (int i=0; i<path.GetNameCount(); ++i)
		{
			indentStr += ">";
		}
		GD.Print(indentStr, " Invoke: ", path);
	}

	public void OnStateEnter(State state)
	{
		NodePath path = TestStatechart.GetPathTo(state);
        string indentStr = "";
		for (int i=0; i<path.GetNameCount(); ++i)
		{
			indentStr += ">";
		}
		GD.Print(indentStr, " Enter: ", path);
	}

	public void OnStateExit(State state)
	{
		NodePath path = TestStatechart.GetPathTo(state);
        string indentStr = "";
		for (int i=0; i<path.GetNameCount(); ++i)
		{
			indentStr += ">";
		}
		GD.Print(indentStr, " Exit: ", path);
	}
}
