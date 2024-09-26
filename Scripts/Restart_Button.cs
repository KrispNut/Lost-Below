using Godot;
using System;

public partial class Restart_Button : Button
{
	public override void _Ready()
	{
	}
	public override void _Process(double delta)
	{
	}
	public override void _Pressed()
	{
		GetTree().ChangeSceneToFile("res://scenes/Game.tscn");
	}
}
