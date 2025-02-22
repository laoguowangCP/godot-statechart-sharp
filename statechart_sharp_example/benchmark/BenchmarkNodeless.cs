using Godot;
using System.Diagnostics;
using LGWCP.Godot.StatechartSharp.Nodeless;

public partial class BenchmarkNodeless : Node
{
    public int TransCnt = 0;
    protected Statechart<StatechartDuct, string> Statechart;
    protected string GoEvent = "go";

    public override void _Ready()
    {
        BuildStatechart();
        Stopwatch sw = new();
        int iterCnt = 100000;
        sw.Start();
        for(int i = 0; i < iterCnt; ++i)
        {
            Statechart.Step(GoEvent);
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
        var a = Statechart.GetCompound();
        var x = Statechart.GetParallel();
        var x1 = Statechart.GetCompound();
        var x1t1 = Statechart.GetTransition(GoEvent);
        var x1t2 = Statechart.GetTransition(GoEvent);
        var x1r1 = Statechart.GetReaction(GoEvent);
        var x1r2 = Statechart.GetReaction(GoEvent);

        x1
            .Append(x1t1)
            .Append(x1t2)
            .Append(x1r1)
            .Append(x1r2);

        var x2 = x1.Duplicate(true);

        x
            .Append(x1)
            .Append(x2);
        
        var y = x.Duplicate(true);
        
        a
            .Append(x)
            .Append(y);

        var b = a.Duplicate(true);

        root
            .Append(a)
            .Append(b);

        // getting dirty
        var ax1_y2 = Statechart.GetTransition(GoEvent, invokes: new[] { TI_AddTransCnt });
        var y2 = (Statechart<StatechartDuct, string>.State)y._Comps[1];
        ax1_y2.SetTargetState(new[] { y2 });
        x1._Comps.Insert(0, ax1_y2);
        ax1_y2._BeAppended(x1);

        var ay2_x1 = Statechart.GetTransition(GoEvent, invokes: new[] { TI_AddTransCnt });
        ay2_x1.SetTargetState(new[] { x1 });
        y2._Comps.Insert(0, ay2_x1);
        ay2_x1._BeAppended(y2);

        var bx1_y2 = Statechart.GetTransition(GoEvent, invokes: new[] { TI_AddTransCnt });
        var bx = b._Comps[0];
        var bx1 = (Statechart<StatechartDuct, string>.State)bx._Comps[0];
        var by = b._Comps[1];
        var by2 = (Statechart<StatechartDuct, string>.State)by._Comps[1];
        bx1_y2.SetTargetState(new[] { by2 });
        bx1._Comps.Insert(0, bx1_y2);
        bx1_y2._BeAppended(bx1);

        var by2_x1 = Statechart.GetTransition(GoEvent, invokes: new[] { TI_AddTransCnt });
        by2_x1.SetTargetState(new[] { bx1 });
        by2._Comps.Insert(0, by2_x1);
        by2_x1._BeAppended(by2);

        Statechart.Ready(root);
    }
}
