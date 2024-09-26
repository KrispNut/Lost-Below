using Godot;
using System;

public partial class Button : Godot.Button
{
	public override void _Pressed()
	{
		GetTree().ChangeSceneToFile("res://Scenes/menu.tscn");
	}
}
