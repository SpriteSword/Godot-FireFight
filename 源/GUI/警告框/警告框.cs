using Godot;

public class 警告框 : Panel
{
	Label text;
	AnimationPlayer anim_player;
	Timer timer;

	public override void _Ready()
	{
		text = GetNode<Label>("Text");
		anim_player = GetNode<AnimationPlayer>("AnimationPlayer");
		timer = GetNode<Timer>("Timer");

		Visible = false;
	}

	void _PopUp(string txt)
	{
		text.Text = txt;
		anim_player.Play("fade_in");
		timer.Start();
	}



	private void _on_Timer_timeout()
	{
		anim_player.Play("fade_out");
	}


}


