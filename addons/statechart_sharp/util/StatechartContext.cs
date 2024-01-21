using System.Collections.Generic;
using Godot;
using Godot.Collections;


namespace LGWCP.StatechartSharp
{

public class StatechartContext
{
    public double Delta { get; internal set; }
    public double PhysicsDelta { get; internal set; }
    public InputEvent Input { get; internal set; }
    public InputEvent UnhandledInput { get; internal set; }

    public bool IsEnabled { set; internal get; }
}

} // end of namespace