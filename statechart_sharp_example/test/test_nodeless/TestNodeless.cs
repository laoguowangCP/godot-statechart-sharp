using Godot;
using System;
using LGWCP.Godot.StatechartSharp.Nodeless;
using System.Collections.Generic;
using System.Text;

public partial class TestNodeless : Node
{
    public List<SnapshotTest<StatechartDuct, string>> Tests = new();

    public override void _Ready()
    {
        BuildStatechartTest();

        // Test
        bool isAllClear = true;
        foreach (var test in Tests)
        {
            GD.PrintRich("Test [b][u]", test.Name, "[/u][/b] start:");

            if (test.Test())
            {
                GD.PrintRich("[color=green]Success![/color]\n");
            }
            else
            {
                isAllClear = false;
                var currIdx = test.CurrIdx;
                var snapshot = test.Snapshots[currIdx];
                GD.PrintRich("[color=red]Snapshot ", currIdx, " is not expected.");
                GD.PrintRich("[color=red]  Expected snapshot:[/color]");
                GD.PrintRich("[color=red]  - IsAllStateConfig: ", snapshot.IsAllStateConfig, "[/color]");
                GD.PrintRich("[color=red]  - Config: ", IntArrayToStr(snapshot.Config), "[/color]");
                var realSnapshot = test.RealSnapshot;
                GD.PrintRich("[color=red]  Current snapshot:[/color]");
                GD.PrintRich("[color=red]  - IsAllStateConfig: ", realSnapshot.IsAllStateConfig, "[/color]");
                GD.PrintRich("[color=red]  - Config: ", IntArrayToStr(realSnapshot.Config), "[/color]\n");
            }
        }

        if (isAllClear)
        {
            GD.PrintRich("[color=green]Pass![/color]");
        }
        else
        {
            GD.PrintRich("[color=red]Failed![/color]");
        }
    }

    public void BuildStatechartTest()
    {
        string stepEvent = "test";

        // 000 No Test Here
        {
            var sc = new Statechart<StatechartDuct, string>();
            var root = sc.GetCompound();
            sc.Ready(root);
            var test = new SnapshotTest<StatechartDuct, string>(
                "000 No Test Here",
                sc,
                stepEvent,
                new() {}
            );
            Tests.Add(test);
        }

        // 102 Statechart Empty
        {
            var sc = new Statechart<StatechartDuct, string>();
            var root = sc.GetCompound();
            root.Append(sc.GetReaction(stepEvent));
            sc.Ready(root);
            var test = new SnapshotTest<StatechartDuct, string>(
                "102 Statechart Empty",
                sc,
                stepEvent,
                new() {
                    new StatechartSnapshot() {
                        IsAllStateConfig = true,
                        Config = Array.Empty<int>()
                    }
                }
            );
            Tests.Add(test);
        }

        // 104 Statechart Max Auto Transition Round
        {
            var sc = new Statechart<StatechartDuct, string>(maxAutoTransitionRound: 1);
            var root = sc.GetCompound();
            var s1 = sc.GetCompound();
            var s2 = sc.GetCompound();
            var t1 = sc.GetAutoTransition().SetTargetState(new[] { s2 });
            var t2 = sc.GetAutoTransition().SetTargetState(new[] { s1 });
            root
                .Append(s1
                    .Append(t1))
                .Append(s2
                    .Append(t2));
            sc.Ready(root);
            var test = new SnapshotTest<StatechartDuct, string>(
                "104 Statechart Max Auto Transition Round",
                sc,
                stepEvent,
                new() {
                    new StatechartSnapshot() {
                        IsAllStateConfig = true,
                        Config = new int[] { 0 }
                    },
                    new StatechartSnapshot() {
                        IsAllStateConfig = true,
                        Config = new int[] { 1 }
                    }
                }
            );
            Tests.Add(test);
        }

        // 202 State Active States
        {
            var sc = new Statechart<StatechartDuct, string>();
            var root = sc.GetParallel();
            var compound = sc.GetCompound();
            {
                var a = sc.GetCompound()
                    .Append(sc.GetReaction(stepEvent));
                var b = sc.GetCompound()
                    .Append(sc.GetReaction(stepEvent));
                compound
                    .Append(a)
                    .Append(b)
                    .Append(sc.GetReaction(stepEvent));
            }
            var parallel = sc.GetParallel();
            {
                var a = sc.GetCompound()
                    .Append(sc.GetReaction(stepEvent));
                var b = sc.GetCompound()
                    .Append(sc.GetReaction(stepEvent));
                var c = sc.GetCompound()
                    .Append(sc.GetReaction(stepEvent));
                compound
                    .Append(a)
                    .Append(b)
                    .Append(c)
                    .Append(sc.GetReaction(stepEvent));
            }
            var history = sc.GetHistory()
                .Append(sc.GetReaction(stepEvent));
            
            root
                .Append(compound)
                .Append(parallel)
                .Append(history)
                .Append(sc.GetReaction(stepEvent));
            
            sc.Ready(root);
            var test = new SnapshotTest<StatechartDuct, string>(
                "202 State Active States",
                sc,
                stepEvent,
                new() {
                    new StatechartSnapshot() {
                        IsAllStateConfig = true,
                        Config = new int[] { 0 }
                    }
                }
            );
            Tests.Add(test);
        }

        // 203 State Initial State
        {
            var sc = new Statechart<StatechartDuct, string>();
            var root = sc.GetCompound();
            var history = sc.GetHistory();
            var init = sc.GetCompound();
            var test = sc.GetCompound();
            var a = sc.GetCompound();
            var b = sc.GetCompound();
            var x = sc.GetCompound();
            var y = sc.GetCompound();
            var bHistory = sc.GetHistory();
            var t = sc.GetTransition(stepEvent, targetStates: new[] { test });
            root
                .Append(history)
                .Append(init
                    .Append(t))
                .Append(test
                    .SetInitialState(b)
                    .Append(a)
                    .Append(b
                        .SetInitialState(bHistory)
                        .Append(x)
                        .Append(y)
                        .Append(bHistory)));
            sc.Ready(root);
            
            var snapshotTest = new SnapshotTest<StatechartDuct, string>(
                "203 State Initial State",
                sc,
                stepEvent,
                new() {
                    new StatechartSnapshot() {
                        IsAllStateConfig = true,
                        Config = new int[] { 1, 1, 0 }
                    },
                    new StatechartSnapshot() {
                        IsAllStateConfig = true,
                        Config = new int[] { 2, 1, 0 }
                    },
                }
            );
            Tests.Add(snapshotTest);
        }


    }

    public void Foo(StatechartDuct duct)
    {
        GD.Print("foo");
    }

    public class SnapshotTest<TDuct, TEvent>
        where TDuct : StatechartDuct, new()
        where TEvent : IEquatable<TEvent>
    {
        public string Name;
        public Statechart<TDuct, TEvent> Statechart;
        public TEvent StepEvent;
        public List<StatechartSnapshot> Snapshots;
        public int CurrIdx = 0;
        public StatechartSnapshot RealSnapshot;

        public SnapshotTest(
            string name,
            Statechart<TDuct, TEvent> statechart,
            TEvent stepEvent,
            List<StatechartSnapshot> snapshots)
        {
            Name = name;
            Statechart = statechart;
            StepEvent = stepEvent;
            Snapshots = snapshots;
        }

        public bool Test()
        {
            bool isSnapshotEqual = true;

            for (CurrIdx = 0; CurrIdx < Snapshots.Count; ++CurrIdx)
            {
                var snapshot = Snapshots[CurrIdx];
                RealSnapshot = Statechart.Save(snapshot.IsAllStateConfig);
                if (!RealSnapshot.Equals(snapshot))
                {
                    isSnapshotEqual = false;
                    break;
                }
                Statechart.Step(StepEvent);
            }

            return isSnapshotEqual;
        }
    }

    public string IntArrayToStr(int[] config)
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


