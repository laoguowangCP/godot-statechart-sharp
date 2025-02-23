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

        // 204 State Enter & Exit
        {
            var sc = new Statechart<StatechartDuct, string>();
            var root = sc.GetCompound();
            var a = sc.GetParallel();
            var b = sc.GetParallel();
            var t = sc.GetTransition(stepEvent, targetStates: new[] { b });
            a
                .Append(t)
                .Append(sc.GetCompound())
                .Append(sc.GetCompound());
            b
                .Append(sc.GetCompound())
                .Append(sc.GetCompound());
            root
                .Append(a)
                .Append(b);
            sc.Ready(root);
            
            var snapshotTest = new SnapshotTest<StatechartDuct, string>(
                "204 State Enter & Exit",
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
                    },
                }
            );
            Tests.Add(snapshotTest);
        }

        // 205 State History 1
        {
            var sc = new Statechart<StatechartDuct, string>();
            var root = sc.GetCompound();
            var init = sc.GetCompound();
            var a = sc.GetCompound();
            var ax = sc.GetCompound();
            var ay = sc.GetCompound();
            var ay1 = sc.GetCompound();
            var ay2 = sc.GetCompound();
            var aHistory = sc.GetHistory();
            var b = sc.GetCompound();
            var bx = sc.GetCompound();
            var by = sc.GetCompound();
            var bHistory = sc.GetHistory();

            var init_2_ay2 = sc.GetTransition(stepEvent, targetStates: new[] { ay2 });
            var a_2_bHistory = sc.GetTransition(stepEvent, targetStates: new[] { bHistory });
            var b_2_aHistory = sc.GetTransition(stepEvent, targetStates: new[] { aHistory });

            root
                .Append(init
                    .Append(init_2_ay2))
                .Append(a
                    .Append(a_2_bHistory)
                    .Append(ax)
                    .Append(ay
                        .Append(ay1)
                        .Append(ay2))
                    .Append(aHistory))
                .Append(b
                    .Append(bx)
                    .Append(by)
                    .Append(bHistory)
                    .Append(b_2_aHistory));
            
            sc.Ready(root);

            var snapshotTest = new SnapshotTest<StatechartDuct, string>(
                "205 State History 1",
                sc,
                stepEvent,
                new() {
                    new StatechartSnapshot() {
                        IsAllStateConfig = true,
                        Config = new int[] { 0, 0, 0, 0 }
                    },
                    new StatechartSnapshot() {
                        IsAllStateConfig = true,
                        Config = new int[] { 1, 1, 1, 0 }
                    },
                    new StatechartSnapshot() {
                        IsAllStateConfig = true,
                        Config = new int[] { 2, 1, 1, 0 }
                    },
                    new StatechartSnapshot() {
                        IsAllStateConfig = true,
                        Config = new int[] { 1, 1, 0, 0 }
                    }
                }
            );
            Tests.Add(snapshotTest);
        }

        // 206 State History 2
        {
            var sc = new Statechart<StatechartDuct, string>();
            var root = sc.GetParallel();
            var init = sc.GetCompound();
            var a = sc.GetParallel();
            var ax = sc.GetCompound();
            var ay = sc.GetCompound();
            var aHistory = sc.GetHistory();
            var init_2_aHistory = sc.GetTransition(stepEvent, targetStates: new[] { aHistory });

            root
                .Append(init
                    .Append(init_2_aHistory))
                .Append(a
                    .Append(ax)
                    .Append(ay)
                    .Append(aHistory));
            
            sc.Ready(root);

            var snapshotTest = new SnapshotTest<StatechartDuct, string>(
                "206 State History 2",
                sc,
                stepEvent,
                new() {
                    new StatechartSnapshot() {
                        IsAllStateConfig = true,
                        Config = Array.Empty<int>()
                    },
                    new StatechartSnapshot() {
                        IsAllStateConfig = true,
                        Config = Array.Empty<int>()
                    }
                }
            );
            Tests.Add(snapshotTest);
        }

        // 207 State Deep Hsitory 1
        {
            var sc = new Statechart<StatechartDuct, string>();
            var root = sc.GetCompound();
            var init = sc.GetCompound();
            var a = sc.GetCompound();
            var ax = sc.GetCompound();
            var ay = sc.GetCompound();
            var ay1 = sc.GetCompound();
            var ay2 = sc.GetCompound();
            var aDeepHistory = sc.GetDeepHistory();
            var b = sc.GetCompound();
            var bx = sc.GetCompound();
            var by = sc.GetCompound();
            var by1 = sc.GetCompound();
            var by2 = sc.GetCompound();
            var bDeepHistory = sc.GetDeepHistory();

            var init_2_ay2 = sc.GetTransition(stepEvent, targetStates: new[] { ay2 });
            var a_2_bDeepHistory = sc.GetTransition(stepEvent, targetStates: new[] { bDeepHistory });
            var b_2_aDeepHistory = sc.GetTransition(stepEvent, targetStates: new[] { aDeepHistory });

            root
                .Append(init
                    .Append(init_2_ay2))
                .Append(a
                    .Append(ax)
                    .Append(ay
                        .Append(ay1)
                        .Append(ay2))
                    .Append(aDeepHistory)
                    .Append(a_2_bDeepHistory))
                .Append(b
                    .SetInitialState(by)
                    .Append(bx)
                    .Append(by
                        .SetInitialState(by2)
                        .Append(by1)
                        .Append(by2
                            .Append(b_2_aDeepHistory)))
                    .Append(bDeepHistory));

            sc.Ready(root);

            var snapshotTest = new SnapshotTest<StatechartDuct, string>(
                "207 State Deep Hsitory 1",
                sc,
                stepEvent,
                new() {
                    new StatechartSnapshot() {
                        IsAllStateConfig = true,
                        Config = new int[] { 0, 0, 0, 1, 1 }
                    },
                    new StatechartSnapshot() {
                        IsAllStateConfig = true,
                        Config = new int[] { 1, 1, 1, 1, 1 }
                    },
                    new StatechartSnapshot() {
                        IsAllStateConfig = true,
                        Config = new int[] { 2, 1, 1, 1, 1 }
                    },
                    new StatechartSnapshot() {
                        IsAllStateConfig = true,
                        Config = new int[] { 1, 1, 1, 1, 1 }
                    }
                }
            );
            Tests.Add(snapshotTest);

        }

        // 208 State Deep History 2
        {
            var sc = new Statechart<StatechartDuct, string>();
            var root = sc.GetCompound();
            var init = sc.GetCompound();
            var a = sc.GetParallel();
            var ax = sc.GetCompound();
            var ax1 = sc.GetCompound();
            var ax2 = sc.GetCompound();
            var ay = sc.GetCompound();
            var ay1 = sc.GetCompound();
            var ay2 = sc.GetCompound();
            var aDeepHistory = sc.GetDeepHistory();
            var b = sc.GetParallel();
            var bx = sc.GetCompound();
            var bx1 = sc.GetCompound();
            var bx2 = sc.GetCompound();
            var by = sc.GetCompound();
            var by1 = sc.GetCompound();
            var by2 = sc.GetCompound();
            var bDeepHistory = sc.GetDeepHistory();

            var init_2_ax2_ay2 = sc.GetTransition(stepEvent, targetStates: new[] { ax2, ay2 });
            var a_2_bDeepHistory = sc.GetTransition(stepEvent, targetStates: new[] { bDeepHistory });
            var b_2_aDeepHistory = sc.GetTransition(stepEvent, targetStates: new[] { aDeepHistory });

            root
                .Append(init
                    .Append(init_2_ax2_ay2))
                .Append(a
                    .Append(ax
                        .Append(ax1)
                        .Append(ax2))
                    .Append(ay
                        .Append(ay1)
                        .Append(ay2))
                    .Append(aDeepHistory)
                    .Append(a_2_bDeepHistory))
                .Append(b
                    .Append(bx
                        .SetInitialState(bx2)
                        .Append(bx1)
                        .Append(bx2))
                    .Append(by
                        .SetInitialState(by2)
                        .Append(by1)
                        .Append(by2))
                    .Append(bDeepHistory)
                    .Append(b_2_aDeepHistory));

            sc.Ready(root);

            var snapshotTest = new SnapshotTest<StatechartDuct, string>(
                "208 State Deep History 2",
                sc,
                stepEvent,
                new() {
                    new StatechartSnapshot() {
                        IsAllStateConfig = true,
                        Config = new int[] { 0, 0, 0, 1, 1 }
                    },
                    new StatechartSnapshot() {
                        IsAllStateConfig = true,
                        Config = new int[] { 1, 1, 1, 1, 1 }
                    },
                    new StatechartSnapshot() {
                        IsAllStateConfig = true,
                        Config = new int[] { 2, 1, 1, 1, 1 }
                    },
                    new StatechartSnapshot() {
                        IsAllStateConfig = true,
                        Config = new int[] { 1, 1, 1, 1, 1 }
                    }
                }
            );
            Tests.Add(snapshotTest);
        }

        // 209 State Root Transition
        {
            var sc = new Statechart<StatechartDuct, string>();
            var root = sc.GetCompound();
            var a = sc.GetCompound();
            var b = sc.GetCompound();
            var t = sc.GetTransition(stepEvent, targetStates: new[] { b });
            root
                .Append(t)
                .Append(a)
                .Append(b);
            sc.Ready(root);
            var snapshotTest = new SnapshotTest<StatechartDuct, string>(
                "209 State Root Transition",
                sc,
                stepEvent,
                new() {
                    new StatechartSnapshot() {
                        IsAllStateConfig = true,
                        Config = new int[] { 0 }
                    },
                    new StatechartSnapshot() {
                        IsAllStateConfig = true,
                        Config = new int[] { 0 }
                    }
                }
            );
            Tests.Add(snapshotTest);
        }

        // 303 Transition Automatic
        {
            var sc = new Statechart<StatechartDuct, string>();
            var root = sc.GetCompound();
            var a = sc.GetCompound();
            var ax = sc.GetCompound();
            var ay = sc.GetCompound();
            var b = sc.GetCompound();
            var bx = sc.GetCompound();
            var by = sc.GetCompound();

            var ax_2_ay_auto = sc.GetAutoTransition(targetStates: new[] { ay });
            var bx_2_by_auto = sc.GetAutoTransition(targetStates: new[] { by });
            var a_2_b = sc.GetTransition(stepEvent, targetStates: new[] { b });
            var b_2_a = sc.GetTransition(stepEvent, targetStates: new[] { a });

            root
                .Append(a
                    .Append(ax
                        .Append(ax_2_ay_auto))
                    .Append(ay)
                    .Append(a_2_b))
                .Append(b
                    .Append(bx
                        .Append(bx_2_by_auto))
                    .Append(by)
                    .Append(b_2_a));

            sc.Ready(root);

            var snapshotTest = new SnapshotTest<StatechartDuct, string>(
                "303 Transition Automatic",
                sc,
                stepEvent,
                new() {
                    new StatechartSnapshot() {
                        IsAllStateConfig = true,
                        Config = new int[] { 0, 0, 0 }
                    },
                    new StatechartSnapshot() {
                        IsAllStateConfig = true,
                        Config = new int[] { 1, 0, 1 }
                    },
                    new StatechartSnapshot() {
                        IsAllStateConfig = true,
                        Config = new int[] { 0, 1, 1 }
                    }
                }
            );
            Tests.Add(snapshotTest);
        }

        // 304 Transition Targetless
        {
            var sc = new Statechart<StatechartDuct, string>();
            var root = sc.GetCompound();
            var a = sc.GetCompound();
            var t = sc.GetTransition(stepEvent);

            root
                .Append(a
                    .Append(t));
            
            sc.Ready(root);

            var snapshotTest = new SnapshotTest<StatechartDuct, string>(
                "304 Transition Targetless",
                sc,
                stepEvent,
                new() {
                    new StatechartSnapshot() {
                        IsAllStateConfig = true,
                        Config = new int[] { 0 }
                    },
                    new StatechartSnapshot() {
                        IsAllStateConfig = true,
                        Config = new int[] { 0 }
                    }
                }
            );
            Tests.Add(snapshotTest);
        }

        // 305 Transition Target Confliction
        {
            var sc = new Statechart<StatechartDuct, string>();
            var root = sc.GetCompound();
            var a = sc.GetCompound();
            var b = sc.GetCompound();
            var bx = sc.GetCompound();
            var by = sc.GetCompound();

            var a_2_bx_by = sc.GetTransition(stepEvent, targetStates: new[] { by, bx });

            root
                .Append(a
                    .Append(a_2_bx_by))
                .Append(b
                    .Append(bx)
                    .Append(by));

            sc.Ready(root);

            var snapshotTest = new SnapshotTest<StatechartDuct, string>(
                "305 Transition Target Confliction",
                sc,
                stepEvent,
                new() {
                    new StatechartSnapshot() {
                        IsAllStateConfig = true,
                        Config = new int[] { 0, 0 }
                    },
                    new StatechartSnapshot() {
                        IsAllStateConfig = true,
                        Config = new int[] { 1, 1 }
                    }
                }
            );
            Tests.Add(snapshotTest);
        }

        // 306 Transition Multiple Targets 1
        {
            var sc = new Statechart<StatechartDuct, string>();
            var root = sc.GetCompound();
            var a = sc.GetCompound();
            var ax = sc.GetCompound();
            var b = sc.GetParallel();
            var bx = sc.GetCompound();
            var bx1 = sc.GetCompound();
            var bx2 = sc.GetCompound();
            var by = sc.GetCompound();
            var by1 = sc.GetCompound();
            var by2 = sc.GetCompound();
            var bz = sc.GetCompound();

            var a_2_bx2_by_by2 = sc.GetTransition(stepEvent, targetStates: new[] { bx2, by, by2 });

            root
                .Append(a
                    .Append(ax)
                    .Append(a_2_bx2_by_by2))
                .Append(b
                    .Append(bx
                        .Append(bx1)
                        .Append(bx2))
                    .Append(by
                        .Append(by1)
                        .Append(by2))
                    .Append(bz));
            
            sc.Ready(root);

            var snapshotTest = new SnapshotTest<StatechartDuct, string>(
                "306 Transition Multiple Targets 1",
                sc,
                stepEvent,
                new() {
                    new StatechartSnapshot() {
                        IsAllStateConfig = true,
                        Config = new int[] { 0, 0, 0, 0 }
                    },
                    new StatechartSnapshot() {
                        IsAllStateConfig = true,
                        Config = new int[] { 1, 0, 1, 1 }
                    }
                }
            );
            Tests.Add(snapshotTest);
        }

        // 307 Transition Multiple Targets 2
        {
            var sc = new Statechart<StatechartDuct, string>();
            var root = sc.GetParallel();
            var a = sc.GetCompound();
            var b = sc.GetCompound();
            var b1 = sc.GetCompound();
            var b2 = sc.GetCompound();
            var c = sc.GetCompound();
            var c1 = sc.GetCompound();
            var c2 = sc.GetCompound();

            var a_2_b2_c2 = sc.GetTransition(stepEvent, targetStates: new[] { b2, c2 });

            root
                .Append(a
                    .Append(a_2_b2_c2))
                .Append(b
                    .Append(b1)
                    .Append(b2))
                .Append(c
                    .Append(c1)
                    .Append(c2));
            
            sc.Ready(root);

            var snapshotTest = new SnapshotTest<StatechartDuct, string>(
                "307 Transition Multiple Targets 2",
                sc,
                stepEvent,
                new() {
                    new StatechartSnapshot() {
                        IsAllStateConfig = true,
                        Config = new int[] { 0, 0 }
                    },
                    new StatechartSnapshot() {
                        IsAllStateConfig = true,
                        Config = new int[] { 1, 1 }
                    }
                }
            );
            Tests.Add(snapshotTest);
        }

        // 308 Transition Select Case
        {
            var sc = new Statechart<StatechartDuct, string>();
            var root = sc.GetParallel();
            var a = sc.GetCompound();
            var ax = sc.GetParallel();
            var ax1 = sc.GetCompound();
            var ax1Leaf = sc.GetCompound();
            var ax2 = sc.GetCompound();
            var ax2Leaf = sc.GetCompound();
            var ax3 = sc.GetCompound();

            var ax1_2_self = sc.GetTransition(stepEvent, targetStates: new[] { ax1 });
            var ax2_2_self = sc.GetTransition(stepEvent, targetStates: new[] { ax2 });
            var ax2Leaf_2_ = sc.GetTransition(stepEvent);
            var ax_2_self = sc.GetTransition(stepEvent, targetStates: new[] { ax });
            var a_2_ = sc.GetTransition(stepEvent);

            root
                .Append(a
                    .Append(ax
                        .Append(ax1
                            .Append(ax1Leaf)
                            .Append(ax1_2_self))
                        .Append(ax2
                            .Append(ax2_2_self)
                            .Append(ax2Leaf
                                .Append(ax2Leaf_2_)))
                        .Append(ax3)
                        .Append(ax_2_self))
                    .Append(a_2_));
            
            sc.Ready(root);

            var snapshotTest = new SnapshotTest<StatechartDuct, string>(
                "308 Transition Select Case",
                sc,
                stepEvent,
                new() {
                    new StatechartSnapshot() {
                        IsAllStateConfig = true,
                        Config = new int[] { 0, 0, 0 }
                    },
                    new StatechartSnapshot() {
                        IsAllStateConfig = true,
                        Config = new int[] { 0, 0, 0 }
                    }
                }
            );
            Tests.Add(snapshotTest);
        }

        // 309 Transition Filter
        {
            var sc = new Statechart<StatechartDuct, string>();
            var root = sc.GetParallel();
            var a = sc.GetParallel();
            var ax = sc.GetCompound();
            var ay = sc.GetCompound();
            var b = sc.GetParallel();
            var bx = sc.GetCompound();
            var bx1 = sc.GetCompound();
            var bx2 = sc.GetCompound();
            var by = sc.GetCompound();

            var ax_2_ay = sc.GetTransition(stepEvent, targetStates: new[] { ay });
            var ay_2_ax = sc.GetTransition(stepEvent, targetStates: new[] { ax });
            var bx1_2_bx2 = sc.GetTransition(stepEvent, targetStates: new[] { bx2 });
            var by_2_bx = sc.GetTransition(stepEvent, targetStates: new[] { bx });

            root
                .Append(a
                    .Append(ax
                        .Append(ax_2_ay))
                    .Append(ay
                        .Append(ay_2_ax)))
                .Append(b
                    .Append(bx
                        .Append(bx1
                            .Append(bx1_2_bx2))
                        .Append(bx2))
                    .Append(by
                        .Append(by_2_bx)));
            
            sc.Ready(root);

            var snapshotTest = new SnapshotTest<StatechartDuct, string>(
                "309 Transition Filter",
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
                    },
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
            Tests.Add(snapshotTest);
        }

        // 501 promoter not impl yet :p
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


