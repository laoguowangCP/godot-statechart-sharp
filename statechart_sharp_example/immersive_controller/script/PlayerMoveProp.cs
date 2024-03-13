using Godot;
using System;

[Tool]
[GlobalClass]
public partial class PlayerMoveProp : Resource
{
    [Export] public float MaxSpeed = 5.0f;
	[Export] public float ForwardRatio = 1.0f;
	[Export] public float BackwardRatio = 1.0f;
	[Export] public float Accel = 25.0f;
	[Export] public float Decel = 75.0f;
}
