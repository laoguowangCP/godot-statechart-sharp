using System;
using System.Collections.Generic;
using System.Text;
using Godot;
using Godot.Collections;
using LGWCP.Godot.StatechartSharp;


// Assert snapshot before each step
public partial class StatechartStepAsserter : Node
{
    [Export]
    public StringName TestName;
    [Export]
    public bool IsStepAllOnReady = true;
    [Export]
    protected StatechartProxy StatechartProxy;
    [Export]
    protected StringName StepName = "test";
    /// <summary>
    /// Snapshots to be asserted before each step, the first asserter inspect statechart's initial config.
    /// </summary>
    [Export]
    protected Array<StatechartSnapshot> SnapshotBeforeSteps;
    protected List<StatechartSnapshot> Snapshots;
    protected bool IsAllStepClear = true;
    protected int CurrIdx = 0;


    public override async void _Ready()
    {
        if (TestName is null)
        {
            TestName = Name;
        }

        Snapshots = new List<StatechartSnapshot>(SnapshotBeforeSteps);
        if (!StatechartProxy.IsNodeReady())
        {
            await ToSignal(StatechartProxy, StatechartProxy.SignalName.Ready);
        }

        GD.PrintRich("Test [b][u]", TestName, "[/u][/b] start:");

        if (IsStepAllOnReady)
        {
            StepAllAssert();
        }
        else
        {
            // If not, assert once
            StepAssert();
        }
    }

    public void TestStep()
    {
        StatechartProxy.Step(StepName);
    }

    protected bool GetNextSnapshotAsserter(out SnapshotAsserter asserter)
    {
        if (Snapshots.Count == 0)
        {
            GD.PushWarning("SnapshotBeforeSteps is empty");
            asserter = null;
            return false;
        }

        if (Snapshots.Count <= CurrIdx)
        {
            GD.PushWarning("SnapshotBeforeSteps is consumed");
            asserter = null;
            return false;
        }

        asserter = new(StatechartProxy.Statechart, Snapshots[CurrIdx]);
        ++CurrIdx;
        return true;
    }

    protected void StepAssert()
    {
        if (GetNextSnapshotAsserter(out var asserter))
        {
            if (asserter.Assert())
            {
                // Success
            }
            else
            {
                IsAllStepClear = false;
                GD.PrintRich("[color=red]Snapshot asserted in step ", CurrIdx - 1, " is not expected. Current snapshot:[/color]");
                GD.PrintRich("[color=red]- IsAllStateConfig: ", asserter.CurrSnapshot.IsAllStateConfig, "[/color]");
                GD.PrintRich("[color=red]- Config: ", ConfigArrayToStr(asserter.CurrSnapshot.Config), "[/color]");
            }
        }
    }

    protected void StepAllAssert()
    {
        while(CurrIdx < Snapshots.Count)
        {
            StepAssert();
            StatechartProxy.Step(StepName);
        }
        if (IsAllStepClear)
        {
            GD.PrintRich("[color=green]Success![/color]");
        }

        // Leave a line
        GD.Print();
    }

    protected String ConfigArrayToStr(int[] config)
    {
        StringBuilder sb = new("[");
        foreach(int x in config)
        {
            sb.Append(x);
            sb.Append(", ");
        }
        sb.Append(']');

        return sb.ToString();
    }
}
