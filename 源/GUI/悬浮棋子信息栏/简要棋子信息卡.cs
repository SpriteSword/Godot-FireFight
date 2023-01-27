using Godot;

public class 简要棋子信息卡 : Panel
{
	[Signal] delegate void SelectMe(简要棋子信息卡 me);      //  传给 信息栏

	Label id_label;

	public Piece piece;


	public override void _Ready()
	{
		id_label = GetNode<Label>("IdLabel");
	}

	public override void _GuiInput(InputEvent @event)
	{
		if (@event.IsActionReleased("click_left"))
		{
			EmitSignal("SelectMe", this);
		}
	}


	//  更新棋子信息
	public void UpdatePieceInfo(Piece p)
	{
		piece = p;
		id_label.Text = "id: " + p.id.ToString();
	}
}
