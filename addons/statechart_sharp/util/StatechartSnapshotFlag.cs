using Godot;
using System;


namespace LGWCP.StatechartSharp;


[Flags]
public enum SnapshotFlagEnum : int
{
    AllStateConfiguration = 1,
    ExitOnLoad = 2,
    EnterOnLoad = 4
}
