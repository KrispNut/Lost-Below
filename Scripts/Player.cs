using Godot;
using System;
using Horror_Game.Scripts;

public partial class Player : CharacterBody3D
{
	public const float Speed = 4.5f;
	public const float CrouchedSpeed = 3.0f;
	public const float JumpVelocity = 4.5f;
	public const float Sensitivity = 3.0f;
	private RayCast3D HeadRaycast;
	public bool IsCrouched;
	public bool FlashlightOut;
	private AudioStreamPlayer footstepPlayer;
	private Timer footstepTimer;
	private bool isMoving = false;
	const float footstepInterval = 0.5f;
	const float crouchedFootstepInterval = 1.0f;
	const float footstepVolume = -20f;

	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	public override void _Ready()
	{
		base._Ready();
		Input.MouseMode = Input.MouseModeEnum.Captured;
		HeadRaycast = GetNode<Camera3D>("Camera3D").GetNode<RayCast3D>("RayCast3D");

		footstepPlayer = new AudioStreamPlayer();
		AddChild(footstepPlayer);
		footstepPlayer.Stream = GD.Load<AudioStream>("res://assets/Sounds/deep_walk.wav");
		footstepPlayer.VolumeDb = footstepVolume;
		footstepTimer = new Timer();
		AddChild(footstepTimer);
		footstepTimer.WaitTime = footstepInterval;
		footstepTimer.OneShot = false;
		footstepTimer.Connect("timeout", new Callable(this, nameof(OnFootstepTimerTimeout)));
	}

	public override void _PhysicsProcess(double delta)
	{
		if (Input.IsActionJustPressed("Quit"))
		{
			GetTree().Quit();
		}

		Vector3 velocity = Velocity;

		if (!IsOnFloor())
		{
			velocity.Y -= gravity * (float)delta;
		}		

		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
		}

		Vector2 inputDir = Input.GetVector("Left", "Right", "Up", "Down");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		float speed = Speed;

		if (Input.IsActionPressed("Crouch"))
		{
			if (!IsCrouched)
			{
				GetNode<AnimationPlayer>("AnimationPlayer").Play("Crouch");
				IsCrouched = true;
				footstepTimer.WaitTime = crouchedFootstepInterval;
			}
		}
		else
		{
			if (IsCrouched)
			{
				PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
				var result = spaceState.IntersectRay(new PhysicsRayQueryParameters3D()
				{
					From = Position,
					To = new Vector3(Position.X, Position.Y + 2, Position.Z),
					Exclude = new Godot.Collections.Array<Rid> { GetRid() }
				});
				if (result.Count <= 0)
				{
					GetNode<AnimationPlayer>("AnimationPlayer").Play("UnCrouch");
					IsCrouched = false;
					footstepTimer.WaitTime = footstepInterval;
				}
			}
		}

		if (IsCrouched)
		{
			speed = CrouchedSpeed;
		}

		if (Input.IsActionJustPressed("Flashlight"))
		{
			if (FlashlightOut)
			{
				GetNode<AnimationPlayer>("AnimationPlayer").Play("FlashlightHide");
			}
			else
			{
				GetNode<AnimationPlayer>("AnimationPlayer").Play("FlashlightShow");
			}
			FlashlightOut = !FlashlightOut;
		}

		if (HeadRaycast.IsColliding())
		{
			object obj = HeadRaycast.GetCollider();
			if (obj is Interactable)
			{
				Interactable interactable = obj as Interactable;
				if (Input.IsActionJustPressed("Interact"))
				{
					interactable.Interact();
				}
			}
		}

		bool wasMoving = isMoving;
		isMoving = direction != Vector3.Zero;

		if (isMoving)
		{
			velocity.X = direction.X * speed;
			velocity.Z = direction.Z * speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, speed);
		}

		if (isMoving && !wasMoving && IsOnFloor())
		{
			footstepTimer.Start();
		}
		else if (!isMoving && wasMoving)
		{
			footstepTimer.Stop();
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	private void OnFootstepTimerTimeout()
	{
		if (isMoving && IsOnFloor())
		{
			footstepPlayer.Play();
		}
	}

	public override void _Input(InputEvent @event)
	{
		base._Input(@event);
		if (@event is InputEventMouseMotion)
		{
			InputEventMouseMotion motion = @event as InputEventMouseMotion;
			Rotation = new Vector3(Rotation.X, Rotation.Y - motion.Relative.X / 1000 * Sensitivity, Rotation.Z);
			Camera3D camera = GetNode<Camera3D>("Camera3D");
			camera.Rotation = new Vector3(Mathf.Clamp(camera.Rotation.X - motion.Relative.Y / 1000 * Sensitivity, -1f, 1f), camera.Rotation.Y, camera.Rotation.Z);
		}
	}
}
