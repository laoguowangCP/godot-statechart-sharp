using Godot;
using System;
using System.Collections.Generic;


namespace LGWCP.StatechartSharp;


[Flags]
public enum SnapshotFlagEnum : int
{
    AllStateConfiguration = 1,
    ExitOnLoad = 2,
    EnterOnLoad = 4
}

[Tool]
public partial class StatechartSnapshot : Resource
{
    internal int Flag;
    internal int[] Configuration;
}
