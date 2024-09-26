using Godot;
using System;
using System.Collections.Generic;

public partial class Enemy : CharacterBody3D
{
	public enum States
	{
		Patrol,
		Chasing,
		Hunting,
		Waiting,
	}
	public States CurrentState;
	private float PatrolSpeed = 15;
	private float ChaseSpeed = 20;
	private Timer PatrolTimer;
	public NavigationAgent3D NavigationAgent;
	private List<Marker3D> waypoints = new List<Marker3D>();
	private int waypointIndex;
	private Vector3 lastLookingDirection;
	private bool playerInEarShotFar;
	private bool playerInEarShotClose;
	private bool playerInSightFar;
	private bool playerInSightClose;
	private Player player;

	public override void _Ready()
	{
		NavigationAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");
		CurrentState = States.Patrol;
		PatrolTimer = GetNode<Timer>("waitTimer");
		player = GetTree().GetNodesInGroup("Player")[0] as Player;
		Godot.Collections.Array<Node> nodesArray = GetTree().GetNodesInGroup("EnemyWaypoint");
		waypoints = new List<Marker3D>();
		foreach (Node node in nodesArray)
		{
			if (node is Marker3D marker)
			{
				waypoints.Add(marker);
			}
		}
		NavigationAgent.TargetPosition = waypoints[0].GlobalPosition;
	}

	public override void _Process(double delta)
	{
		switch (CurrentState)
		{
			case States.Patrol:
				if (NavigationAgent.IsNavigationFinished())
				{
					PatrolTimer.Start();
					CurrentState = States.Waiting;
					return;
				}
				moveTowardPoint(delta, PatrolSpeed);
				break;
			case States.Chasing:
				if (NavigationAgent.IsNavigationFinished())
				{
					GD.Print("Attacking!");
					CurrentState = States.Waiting;
					PatrolTimer.Start();
					return;
				}
				NavigationAgent.TargetPosition = player.GlobalPosition;
				moveTowardPoint(delta, ChaseSpeed);
				break;
			case States.Hunting:
				if (NavigationAgent.IsNavigationFinished())
				{
					CurrentState = States.Waiting;
					PatrolTimer.Start();
					return;
				}
				moveTowardPoint(delta, PatrolSpeed);
				break;
			case States.Waiting:
				break;
			default:
				break;
		}
	}

	private void moveTowardPoint(double delta, float speed)
	{
		var targetPos = NavigationAgent.TargetPosition;
		var direction = GlobalPosition.DirectionTo(targetPos);
		Vector3 lookingDirection = lastLookingDirection.Lerp(targetPos, .6f);
		
		if (!GlobalPosition.IsEqualApprox(targetPos))
		{
			LookAt(new Vector3(lookingDirection.X, GlobalPosition.Y, lookingDirection.Z), Vector3.Up);
			lastLookingDirection = lookingDirection;
		}
		
		Velocity = direction * speed;
		MoveAndSlide();
		if (playerInEarShotFar)
		{
			checkForPlayer();
		}
	}

	private void checkForPlayer()
	{
		PhysicsDirectSpaceState3D spaceState3D = GetWorld3D().DirectSpaceState;
		var result = spaceState3D.IntersectRay(new PhysicsRayQueryParameters3D()
		{
			From = GetNode<Node3D>("Head").GlobalPosition,
			To = player.GetNode<Camera3D>("Camera3D").GlobalPosition,
			Exclude = new Godot.Collections.Array<Rid> { GetRid() }
		});

		if (result.Keys.Count > 0)
		{
			Node3D node = (Node3D)result["collider"];

			if (node is Player)
			{
				Player p = node as Player;
				if (playerInEarShotClose)
				{
					var audioPlayerClose = GetNode<AudioStreamPlayer3D>("AudioStreamPlayer3D2");
					if (!audioPlayerClose.Playing)
					{
						audioPlayerClose.Stream = GD.Load<AudioStream>("res://assets/Sounds/Caught.mp3");
						audioPlayerClose.Play();
					}
					CurrentState = States.Chasing;
					GD.Print("Player In Earshot Close");
				}
				if (playerInEarShotFar)
				{
					var audioPlayerClose = GetNode<AudioStreamPlayer3D>("AudioStreamPlayer3D2");
					if (!audioPlayerClose.Playing)
					{
						audioPlayerClose.Stream = GD.Load<AudioStream>("res://assets/Sounds/suspense_chase.mp3");
						audioPlayerClose.Play();
					}
					if (!p.IsCrouched)
					{
						CurrentState = States.Hunting;
						NavigationAgent.TargetPosition = player.GlobalPosition;
					}
					GD.Print("Player In Earshot Far");
				}
				if (playerInSightClose)
				{
					GetTree().ChangeSceneToFile("res://Scenes/Game_Over.tscn");
					CurrentState = States.Chasing;
					GD.Print("Player In Sight Close");
				}
				if (playerInSightFar)
				{
					if (!p.IsCrouched)
					{
						CurrentState = States.Hunting;
						NavigationAgent.TargetPosition = player.GlobalPosition;
					}
					GD.Print("Player In Sight Far");
				}
			}
		}
	}

	private void _on_wait_timer_timeout()
	{
		waypointIndex += 1;
		if (waypointIndex >= waypoints.Count)
		{
			waypointIndex = 0;
		}
		NavigationAgent.TargetPosition = waypoints[waypointIndex].GlobalPosition;
		CurrentState = States.Patrol;
	}

	private void _on_close_hearing_body_entered(Node3D obj)
	{
		if (obj is Player)
		{
			playerInEarShotClose = true;
		}
	}

	private void _on_close_hearing_body_exited(Node3D obj)
	{
		if (obj is Player)
		{
			playerInEarShotClose = false;
		}
	}

	private void _on_far_hearing_body_entered(Node3D obj)
	{
		if (obj is Player)
		{
			GD.Print("Player is in far Hearing");
			playerInEarShotFar = true;
		}
	}

	private void _on_far_hearing_body_exited(Node3D obj)
	{
		if (obj is Player)
		{
			playerInEarShotFar = false;
		}
	}

	private void _on_close_sight_body_entered(Node3D obj)
	{
		if (obj is Player)
		{
			playerInSightClose = true;
		}
	}

	private void _on_close_sight_body_exited(Node3D obj)
	{
		if (obj is Player)
		{
			playerInSightClose = false;
		}
	}

	private void _on_far_sight_body_entered(Node3D obj)
	{
		if (obj is Player)
		{
			playerInSightFar = true;
		}
	}

	private void _on_far_sight_body_exited(Node3D obj)
	{
		if (obj is Player)
		{
			playerInSightFar = false;
		}
	}
}
