using Godot;
using System;


namespace LGWCP.StatechartSharp;


[Flags]
public enum StatechartSnapshotFlagEnum
{
    IncludeInactiveState = 1,
    DelayedEvent = 2
}

public class StatechartSnapshot
{
    protected StatechartSnapshotFlagEnum SnapshotFlag;
    protected int[] stateConfiguration;
}

