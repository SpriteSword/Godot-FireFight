using Godot;
using Godot.Collections;

public class Main : Node
{
	PackedScene scn_game;

	联机页面 online_page;
	GameMnger game;

	public Dictionary players_name_id = new Dictionary();


	public override void _EnterTree()
	{
		scn_game = GD.Load<PackedScene>("res://源/游戏主管/GameMnger.tscn");
	}
	public override void _Ready()
	{
		online_page = GetNode<联机页面>("联机页面");

		if (!Global.联机调试) { EnterGame(); }
	}

	//  正常进入
	public void EnterGame()
	{
		game = scn_game.Instance<GameMnger>();
		AddChild(game);

		online_page.Visible = false;
	}

	//  正常退出游戏
	public void ExitGame()
	{
		game.QueueFree();
		online_page.BackHere();
	}

	//  异常退出游戏
	public void ExitGameAbnormally()
	{
		game.QueueFree();
		online_page.BackHere();
		online_page.UpdatePlayerLabels();
		online_page.ShowUIByState(false);
		online_page.host_btn.Visible = true;
	}

}
