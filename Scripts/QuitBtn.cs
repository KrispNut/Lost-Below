using Godot;
using System;

public partial class QuitBtn : Button
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("Quit"))
		{
			GetTree().Quit();
		}
	}
	public override void _Pressed()
	{
		GetTree().Quit();
	}
}
