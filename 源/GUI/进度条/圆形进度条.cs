using Godot;
using System;

public class 圆形进度条 : TextureProgress
{
	AnimationPlayer anima_player;

	public override void _Ready()
	{
		anima_player = GetNode<AnimationPlayer>("AnimationPlayer");

		Visible = false;
	}


	public void Show(bool show, string text)
	{
		if(show)
		{
			Visible = true;
			GetNode<Label>("Text").Text = text;
			anima_player.Play("loading");
			return;
		}

		Visible = false;
		anima_player.Stop();
	}

}

