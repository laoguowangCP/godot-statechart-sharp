using Godot;
using System;
using LGWCP.StatechartSharp;
using System.Collections.Generic;

public partial class ImPlayer : CharacterBody3D
{
	[ExportGroup("Player Move Property")]
	[Export] public PlayerMoveProp StandWalkProp;
	[Export] public PlayerMoveProp StandSprintProp;
	[Export] public PlayerMoveProp CrouchProp;
	[Export] public PlayerMoveProp AirProp;
	[Export] public PlayerMoveProp AirTopProp;
	
	public float MoveMaxSpeed = 5.0f;
	public float ForwardRatio = 1.0f;
	public float BackwardRatio = 1.0f;
	public float MoveAccel = 25.0f;
	public float MoveDecel = 75.0f;

	[ExportGroup("")]
	[Export] public float JumpVelocity = 6.0f;
	[Export] public float CamMaxSpeed = 0.06f;
	[Export] public float CamMaxPitch = 87.0f;
	[Export] public float AirTopIntervalVel = 1.0f;

	protected Node3D Head;
	protected CollisionShape3D BodyCol;
	protected List<CollisionShape3D> HeadCols;
	protected ShapeCast3D PoseCast;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	protected float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	protected Timer CoyoteTimer;
	protected bool IsCoyote = false;
	protected Timer PrejumpTimer;
	protected bool IsPrejump = false;
	protected Vector3 Vel;

    public override void _Ready()
    {
		Head = GetNode<Node3D>("Head");

		BodyCol = GetNode<CollisionShape3D>("CollisionBody");
		HeadCols = new List<CollisionShape3D>();
		for(int i = 1; i <= 8; ++i)
		{
			var head = GetNode<CollisionShape3D>("CollisionHead" + i.ToString());
			HeadCols.Add(head);
		}

		PoseCast = GetNode<ShapeCast3D>("PoseCast");

		CoyoteTimer = GetNode<Timer>("Timers/CoyoteTimer");
		PrejumpTimer = GetNode<Timer>("Timers/PrejumpTimer");

        Input.MouseMode = Input.MouseModeEnum.Captured;
		CamMaxSpeed = Mathf.DegToRad(CamMaxSpeed);
		CamMaxPitch = Mathf.DegToRad(CamMaxPitch);

		ApplyPlayerMoveProp(StandWalkProp);
		AirTopIntervalVel = MathF.Max(AirTopIntervalVel, 0.001f);
    }

	protected void ApplyPlayerMoveProp(PlayerMoveProp prop)
	{
		MoveMaxSpeed = prop.MaxSpeed;
		ForwardRatio = prop.ForwardRatio;
		BackwardRatio = prop.BackwardRatio;
		MoveAccel = prop.Accel;
		MoveDecel = prop.Decel;
	}

	protected void ApplyPlayerAirProp(PlayerMoveProp prop)
	{
		// Only change accel & decel
		MoveAccel = prop.Accel;
		MoveDecel = prop.Decel;
	}

	protected void ApplyPose(
		float height,
		float feetHeight,
		float headFromTop,
		float headColFromTop)
	{
		float bodyShapeHeight = height - feetHeight;

		// Body shape height
		var bodyShape = (CapsuleShape3D)BodyCol.Shape;
		bodyShape.Height = bodyShapeHeight;

		// BodyCol height
		var bodyColPos = BodyCol.Position;
		bodyColPos.Y = 0.5f * bodyShapeHeight + feetHeight;
		BodyCol.Position = bodyColPos;

		// HeadCol height
		foreach (var headCol in HeadCols)
		{
			var headColPos = headCol.Position;
			headColPos.Y = height + headColFromTop;
			headCol.Position = headColPos;
		}

		// Head height
		var headPos = Head.Position;
		headPos.Y = height + headFromTop;
		Head.Position = headPos;
	}

	protected bool IsPoseHasRoom(float height, float feetHeight)
	{
		float shapeHeight = height-feetHeight;
		PoseCast.Shape = new CapsuleShape3D() { Height = shapeHeight, Radius = 0.4f };
		// 0.5f * height + feetHeight
		var castPos = Vector3.Zero;
		castPos.Y += 0.5f * shapeHeight + feetHeight;
		PoseCast.Position = castPos;
		PoseCast.TargetPosition = Vector3.Zero;
		// static, player and rigid, 0b0111 => 0xb
		PoseCast.CollisionMask = 0x3;
		PoseCast.ForceShapecastUpdate();
		var res = PoseCast.CollisionResult;
		foreach (var item in res)
		{
			GD.Print(item);
		}
		// should not collide with any
		return res.Count == 0;
	}

	protected float MaxSpeedMultipler(Vector2 dir, float forwardRatio, float backwardRatio)
	{
		// A function like oval to noramlize magnitude of direction

		// dir.Y -> origin Z direction, <0 is forward
		float r = dir.Y < 0.0f ? forwardRatio : backwardRatio;

		float dirXSqr = dir.X * dir.X;
		float dirMagSqr = dirXSqr + dir.Y * dir.Y;

		if (dirMagSqr == 0.0f)
		{
			// X == Y == 0 , multipler is 1.0f
			return 1.0f;
		}

		float cosSqr = dirXSqr / dirMagSqr;
		float mul = r + (1 - r) * cosSqr;
		return mul;
	}

	protected float GetAccelOrDecel(float from, float to, float accel, float decel)
	{
		bool isAccel;

		if (from == 0.0f)
		{
			// from stillness (from == 0.0f), use accel
			isAccel = true;
		}
		else
		{
			// to stillness (to == 0.0f), use decel
			isAccel = from * to > 0.0f
				&& MathF.Abs(from) < MathF.Abs(to);
		}
		
		return isAccel ? accel : decel;
	}

    public void RI_ViewRotate(StatechartDuct duct)
    {
		var @event = duct.Input;
		if (@event is InputEventMouseMotion mouseMotion)
		{
			RotateY(-mouseMotion.Relative.X * CamMaxSpeed);
			Vector3 headRotation = Head.Rotation;
			headRotation.X = Mathf.Clamp(
				headRotation.X - mouseMotion.Relative.Y * CamMaxSpeed,
				-CamMaxPitch, CamMaxPitch);
			Head.Rotation = headRotation;
		}
    }
	
    public void RI_StandWalk(StatechartDuct duct)
	{
		var delta = (float)(duct.PhysicsDelta);
		Vel = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
			Vel.Y -= gravity * delta;

		// Handle Jump.
		if ((Input.IsActionJustPressed("Space") || IsPrejump) && IsOnFloor())
		{
			Vel.Y = JumpVelocity;
		}

		/*
		Get the input direction
			1. Get vector (local) from input
			2. Local to world
			3. Modify
		*/
		Vector2 inputDir = Input.GetVector("Leftward", "Rightward", "Forward", "Backward");
		inputDir /= Mathf.Max(1.0f, inputDir.Length());
		inputDir *= MaxSpeedMultipler(inputDir, ForwardRatio,BackwardRatio);
		Vector3 direction = Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y);

		// Target vel is clamped under max speed
		Vector3 targetVel = direction * MoveMaxSpeed;
		Vel.X = Mathf.MoveToward(Vel.X, targetVel.X,
			delta * GetAccelOrDecel(Vel.X, targetVel.X, MoveAccel, MoveDecel));
		Vel.Z = Mathf.MoveToward(Vel.Z, targetVel.Z,
			delta * GetAccelOrDecel(Vel.Z, targetVel.Z, MoveAccel, MoveDecel));
		
		Velocity = Vel;
		MoveAndSlide();
	}

	public void RI_AirMove(StatechartDuct duct)
	{
		var delta = (float)(duct.PhysicsDelta);
		Vel = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
			Vel.Y -= gravity * delta;

		// Handle Jump.
		if (Input.IsActionJustPressed("Space") && IsCoyote)
		{
			Vel.Y = JumpVelocity;
		}

		/*
		Get the input direction
			1. Get vector (local) from input
			2. Local to world
			3. Modify
		*/
		Vector2 inputDir = Input.GetVector("Leftward", "Rightward", "Forward", "Backward");
		inputDir /= Mathf.Max(1.0f, inputDir.Length());
		inputDir *= MaxSpeedMultipler(inputDir, ForwardRatio,BackwardRatio);
		Vector3 direction = Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y);

		// Target vel is clamped under max speed
		Vector3 targetVel = direction * MoveMaxSpeed;
		Vel.X = Mathf.MoveToward(Vel.X, targetVel.X,
			delta * GetAccelOrDecel(Vel.X, targetVel.X, MoveAccel, MoveDecel));
		Vel.Z = Mathf.MoveToward(Vel.Z, targetVel.Z,
			delta * GetAccelOrDecel(Vel.Z, targetVel.Z, MoveAccel, MoveDecel));

		// GD.Print(velocity.Length());

		Velocity = Vel;
		MoveAndSlide();
	}

	public void RI_MoveWithImpulse(StatechartDuct duct)
	{
		float delta = (float)(duct.PhysicsDelta);
		for (int i = 0; i < GetSlideCollisionCount(); i++)
		{
			var collision = GetSlideCollision(i);
			if (collision.GetCollider() is RigidBody3D body)
			{
				if (body.GetCollisionLayerValue(3))
				{
					GD.Print("Detect layer 3");
					var impluseDir = -collision.GetNormal();
					var implseForce = Math.Max(impluseDir.Dot(Vel), 0.0f) * 400.0f * delta;
					body.ApplyCentralImpulse(impluseDir * implseForce);
				}
			}
		}
	}

	public void RI_CoyoteEnabled(StatechartDuct duct)
	{
		GD.Print(CoyoteTimer.TimeLeft);
		IsCoyote = CoyoteTimer.TimeLeft > 0.0f;
	}
	public void RI_Prejump(StatechartDuct duct)
	{
		if (IsPrejump)
		{
			GD.Print(PrejumpTimer.TimeLeft);
			IsPrejump = PrejumpTimer.TimeLeft > 0;
			return;
		}

		// Else if not prejump
		if (Input.IsActionJustPressed("Space"))
		{
			IsPrejump = true;
			PrejumpTimer.Start();
		}
	}

	public void Enter_Walk(StatechartDuct duct)
	{
		// Beware of initial state enter, _Ready() is not done yet
		if (!IsNodeReady())
		{
			return;
		}

		ApplyPlayerMoveProp(StandWalkProp);

		float walkHeight = 1.7f;
		float feetHeight = 0.1f;
		float headFromTop = -0.3f;
		float headColFromTop = -0.4f;

		ApplyPose(walkHeight, feetHeight, headFromTop, headColFromTop);
	}

	public void Enter_Sprint(StatechartDuct duct)
	{
		ApplyPlayerMoveProp(StandSprintProp);

		float walkHeight = 1.7f;
		float feetHeight = 0.1f;
		float headFromTop = -0.3f;
		float headColFromTop = -0.4f;

		ApplyPose(walkHeight, feetHeight, headFromTop, headColFromTop);
	}

	public void Enter_AirDescend(StatechartDuct duct)
	{
		GD.Print("Enter Descend");
		ApplyPlayerAirProp(AirProp);
	}

	public void Enter_AirAscend(StatechartDuct duct)
	{
		GD.Print("Enter Ascend");
		ApplyPlayerAirProp(AirProp);
	}

	public void Enter_AirTop(StatechartDuct duct)
	{
		GD.Print("Enter Top");
		ApplyPlayerAirProp(AirTopProp);
	}

	public void Enter_CoyoteEnabled(StatechartDuct duct)
	{
		GD.Print("Enter Coyote Enabled");
		CoyoteTimer.Start();
	}

	public void Enter_Prejump(StatechartDuct duct)
	{
		GD.Print("Enter Prejump");
		IsPrejump = false;
	}

	public void Enter_Crouch(StatechartDuct duct)
	{
		ApplyPlayerMoveProp(CrouchProp);

		float crouchHeight = 1.3f;
		float feetHeight = 0.1f;
		float headFromTop = -0.3f;
		float headColFromTop = -0.4f;

		ApplyPose(crouchHeight, feetHeight, headFromTop, headColFromTop);
	}

	public void Exit_CoyoteEnabled(StatechartDuct duct)
	{
		GD.Print("Enter Coyote Disabled");
		CoyoteTimer.Stop();
		IsCoyote = false;
	}

	public void Exit_Prejump(StatechartDuct duct)
	{
		GD.Print("Exit Prejump");
		PrejumpTimer.Stop();
	}

	public void TG_WalkToSprint(StatechartDuct duct)
	{
		bool forwardTest = Input.GetAxis("Forward", "Backward") < -0.1f;
		duct.IsEnabled = Input.IsActionPressed("Shift")
			&& forwardTest;
	}

	public void TG_SprintToWalk(StatechartDuct duct)
	{
		duct.IsEnabled = Input.IsActionJustReleased("Shift")
			|| Input.IsActionJustReleased("Forward");
	}

	public void TG_GroundToAir(StatechartDuct duct)
	{
		duct.IsEnabled = !IsOnFloor();
	}

	public void TG_AirToGround(StatechartDuct duct)
	{
		duct.IsEnabled = IsOnFloor();
	}

	public void TG_AirIsAscend(StatechartDuct duct)
	{
		duct.IsEnabled = !IsOnFloor() && Velocity.Y > AirTopIntervalVel;
	}
	public void TG_AirIsDescend(StatechartDuct duct)
	{
		duct.IsEnabled = !IsOnFloor() && Velocity.Y < -AirTopIntervalVel;
	}
	public void TG_AirIsTop(StatechartDuct duct)
	{
		duct.IsEnabled = !IsOnFloor() && Velocity.Y <= AirTopIntervalVel && Velocity.Y >= -AirTopIntervalVel;
	}
	public void TG_IsCoyote(StatechartDuct duct)
	{
		duct.IsEnabled = !IsCoyote;
	}

	public void TG_WalkToCrouch(StatechartDuct duct)
	{
		duct.IsEnabled = Input.IsActionJustPressed("Ctrl");
	}

	public void TG_CrouchToWalk(StatechartDuct duct)
	{
		duct.IsEnabled = Input.IsActionJustPressed("Ctrl") && IsPoseHasRoom(1.7f, 0.1f);
	}

	public void TI_GroundToAir(StatechartDuct duct)
	{
		IsCoyote = true;
	}
}
