using Godot;
using System;

namespace Horror_Game.Scripts
{
	public partial class Escape : Node3D, Interactable
	{
		public void Interact()
		{
			GetTree().ChangeSceneToFile("res://Scenes/Game_Over.tscn");
		}
	}
}
