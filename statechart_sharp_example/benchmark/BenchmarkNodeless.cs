using Godot;
using System;
using System.Diagnostics;
using LGWCP.Godot.StatechartSharp.Nodeless;

public partial class BenchmarkNodeless : Node
{
    public int TransCnt = 0;
    protected Statechart<StatechartDuct, string> Statechart;

    public override void _Ready()
    {
        Stopwatch sw = new();
        string goEvent = "go";
        int iterCnt = 100000;
        sw.Start();
        for(int i = 0; i < iterCnt; ++i)
        {
            Statechart.Step(goEvent);
        }
        sw.Stop();
        GD.Print("Step ", iterCnt, " times cost: ", sw.ElapsedMilliseconds, "ms");
        GD.Print(TransCnt, " transitions invoked.");
    }

    public void TI_AddTransCnt(StatechartDuct duct)
    {
        ++TransCnt;
    }

    public void BuildStatechart()
    {
        Statechart = new();
        var root = Statechart.GetParallel();
    }
}
