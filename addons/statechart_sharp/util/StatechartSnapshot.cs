using Godot;
using System;


namespace LGWCP.StatechartSharp;


[Flags]
public enum SnapshotFlagEnum : int
{
    AllStateConfiguration = 1,
    ExitOnLoad = 2,
    EnterOnLoad = 4,
    CancelDelayed = 8
}


public partial class StatechartSnapshot : Resource
{
    public int SnapshotFlag;
    public int[] stateConfiguration;
}

