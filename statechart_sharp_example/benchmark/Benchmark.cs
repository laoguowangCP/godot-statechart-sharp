using Godot;
using LGWCP.Godot.StatechartSharp;
using System;
using System.Diagnostics;

public partial class Benchmark : Node
{
    public int TransCnt = 0;
    protected Statechart Statechart;
    public override void _Ready()
    {
        Statechart = GetNodeOrNull<Statechart>("Statechart");
        Stopwatch sw = new();
        StringName goEvent = "go";
        int iterCnt = 100000;
        sw.Start();
        for(int i = 0; i < iterCnt; ++i)
        {
            Statechart.Step(goEvent);
        }
        sw.Stop();
        GD.Print("Step ", iterCnt, " times cost:");
        GD.Print(sw.ElapsedMilliseconds);
        GD.Print(TransCnt);
    }

    public void TI_AddTransCnt(StatechartDuct duct)
    {
        ++TransCnt;
    }
}

