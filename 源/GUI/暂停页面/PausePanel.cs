using Godot;

public class PausePanel : ColorRect
{
	GUI gui;

	public override void _Ready()
	{
		gui = GetNode<GUI>("..");

		Visible = false;
	}


	private void _on_RtrnToGameBtn_pressed()
	{
		Visible = false;
	}

	private void _on_RtrnToMenuBtn_pressed()
	{
		gui.game_mnger.ExitGame();
	}


}
