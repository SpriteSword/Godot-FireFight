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
		if (!Global.联机调试) return;
		gui.game_mnger.ExitGame();
	}

	private void _on_HelpBtn_pressed()
	{
		OS.Execute("CMD.exe", new string[] { "/C", OS.GetExecutablePath()+"/../火力战中文快速规则.pdf" }, true);
	}

}


