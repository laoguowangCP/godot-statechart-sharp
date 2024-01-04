using Godot;
using Godot.Collections;
using LGWCP.StatechartSharp;

public partial class StatechartTest : Node
{
	[Export] protected StringName StepName;
	[Export] protected Statechart TestStatechart;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (TestStatechart == null)
		{
			TestStatechart = GetChild<Statechart>(0);
		}
		
		if (TestStatechart == null)
		{
			GD.PrintErr(GetPath(), "Test statechart not set.");
		}
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
			else if (child is Reaction action)
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
		GD.Print("[", GetParent<Node>().Name, "]");
		GD.Print("-------- Test step start --------");
		TestStatechart?.Step(StepName);
		GD.Print("-------- Test step finish --------");
		GD.Print();
	}

	public void OnTransitionGuard(Transition transition)
	{
		PrintDelegateInfo(transition, "Guard");
	}

	public void OnTransitionInvoke(Transition transition)
	{
		PrintDelegateInfo(transition, "Invoke");
	}

	public void OnActionInvoke(Reaction reaction)
	{
		PrintDelegateInfo(reaction, "Invoke");
	}

	public void OnStateEnter(State state)
	{
		PrintDelegateInfo(state, "Enter");
	}

	public void OnStateExit(State state)
	{
		PrintDelegateInfo(state, "Exit");
	}

	protected void PrintDelegateInfo(Node callee, string delegateName)
	{
		NodePath path = TestStatechart.GetPathTo(callee);
        string indentStr = "";
		for (int i=1; i<path.GetNameCount(); ++i)
		{
			indentStr += "> "; // "└─"
		}
		GD.Print(indentStr, delegateName, ": ", path);
	}
}
