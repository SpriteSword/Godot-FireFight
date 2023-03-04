using Godot;

public class 回合显示栏 : Panel
{
	Label round_label;
	Label stage_label;
	Label active_side_label;


	public override void _Ready()
	{
		round_label = GetNode<Label>("HBoxContainer/Round/Text");
		stage_label = GetNode<Label>("HBoxContainer/Stage/Text");
		active_side_label = GetNode<Label>("HBoxContainer/ActionSide/Text");
	}

	//  设置当前行动方标签
	public void SetActiveSideLabel(GameMnger.Side active_side)
	{
		StyleBoxFlat style = null;
		if (active_side_label.Get("custom_styles/normal") is StyleBoxFlat st)
		{
			style = st;
		}

		if (active_side == GameMnger.Side.蓝)
		{
			active_side_label.Text = "蓝方";
			if (style == null) return;
			style.BgColor = new Color(0.16f, 0.16f, 0.8f);
		}
		else if (active_side == GameMnger.Side.红)//'bg_color''custom_styles/normal'
		{
			active_side_label.Text = "红方";
			if (style == null) return;
			style.BgColor = new Color(0.8f, 0.16f, 0.16f);
		}
	}

	//  设置当前阶段标签
	public void SetStageLabel(GameMnger.Stage stage)
	{
		string txt = stage.ToString();
		if (stage == GameMnger.Stage.临机射击)
			stage_label.Text = stage.ToString();
		else stage_label.Text = txt + "阶段";
	}

	//  设置当前回合标签
	public void SetRoundLabel(int round)
	{
		round_label.Text = round.ToString("000");
	}

}
