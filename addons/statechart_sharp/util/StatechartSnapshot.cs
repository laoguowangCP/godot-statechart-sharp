using Godot;
using System;
using System.Collections.Generic;


namespace LGWCP.StatechartSharp;


[Tool]
public partial class StatechartSnapshot : Resource
{
    internal bool IsAllStateConfiguration;
    internal int[] Configuration;
}
