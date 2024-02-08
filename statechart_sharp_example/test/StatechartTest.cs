using Godot;
using LGWCP.StatechartSharp;

public partial class StatechartTest : Node
{
	[Export] protected StringName StepName;
	[Export] protected Statechart TestStatechart;

	protected int TestCnt = 1;

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
		// GD.Print("[", GetParent<Node>().Name, "]");
		GD.Print(">> Test Start: step ", TestCnt);
		TestStatechart?.Step(StepName);
		GD.Print(">> Finish\n");
		++TestCnt;
	}

	public void OnTransitionGuard(StatechartDuct duct)
	{
		PrintDelegateInfo(duct, "(Guard)");
	}

	public void OnTransitionInvoke(StatechartDuct duct)
	{
		PrintDelegateInfo(duct, "(Invoke)");
	}

	public void OnActionInvoke(StatechartDuct duct)
	{
		PrintDelegateInfo(duct, "(Invoke)");
	}

	public void OnStateEnter(StatechartDuct duct)
	{
		PrintDelegateInfo(duct, "(Enter)");
	}

	public void OnStateExit(StatechartDuct duct)
	{
		PrintDelegateInfo(duct, "(Exit)");
	}

	protected void PrintDelegateInfo(StatechartDuct duct, string delegateName)
	{
		Node compositionNode = duct.CompositionNode;
		NodePath path = TestStatechart.GetPathTo(compositionNode);
        string indentStr = "";
		for (int i=1; i<path.GetNameCount(); ++i)
		{
			indentStr += "— "; // "└─"
		}
		GD.Print(indentStr, delegateName, " ", path);
	}

	protected void TestStatechartQueueEvent(StatechartDuct duct)
	{
		TestStatechart.Step("test_queue_event");
	}
}
