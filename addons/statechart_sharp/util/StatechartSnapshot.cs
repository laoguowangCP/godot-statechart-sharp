using Godot;
using System;


namespace LGWCP.StatechartSharp;


[Flags]
public enum SnapshotFlagEnum : int
{
    AllStateConfiguration = 1,
    DelayedEvent = 2,
    ExitOnLoad = 4,
    EnterOnLoad = 8,
}


public partial class StatechartSnapshot : Resource
{
    public int SnapshotFlag;
    public int[] stateConfiguration;
}

