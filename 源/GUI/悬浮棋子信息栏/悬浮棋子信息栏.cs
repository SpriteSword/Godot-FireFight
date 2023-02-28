using Godot;

//  仅在点击敌方棋子时显示，用来（查看信息和）选择攻击某个棋子。选择攻击某个棋子时，如果只有单个棋子则不显示。

public class 悬浮棋子信息栏 : Panel
{
	PackedScene card_scn;

	GUI gui;
	GridContainer container;


	public override void _EnterTree()       //  资源，进入树就要加载
	{
		card_scn = GD.Load<PackedScene>("res://源/GUI/悬浮棋子信息栏/简要棋子信息卡.tscn");
	}

	public override void _Ready()
	{
		container = GetNode<GridContainer>("ScrollContainer/GridContainer");

		gui = GetNode<GUI>("..");

		Visible = false;

	}

	//  显示，框体悬浮在右上方。screen_pos 是信息栏的左下角坐标。
	public void Show(Vector2 screen_pos, PieceStack stack)
	{
		Visible = true;
		if (screen_pos.x + RectSize.x > OS.WindowSize.x) { screen_pos.x = OS.WindowSize.x - RectSize.x; }
		if (screen_pos.y - RectSize.y < 0) { screen_pos.y = RectSize.y; }

		RectPosition = new Vector2(screen_pos.x, screen_pos.y - RectSize.y);

		foreach (Node item in container.GetChildren()) item.QueueFree();
		if (stack == null) return;

		Visible = true;

		foreach (Piece piece in stack.pieces)
		{
			if (piece.BeK) continue;

			简要棋子信息卡 card = card_scn.Instance<简要棋子信息卡>();
			container.AddChild(card);

			card.UpdatePieceInfo(piece);
			gui.game_mnger.pieces_mnger.Assign(card.piece_sprite, piece.sprite);
			card.Connect("SelectMe", this, "_ACardIsSelected");
		}
	}



	//------------------------------------------------------------------------信号
	//  信号。信息卡被选中传来
	void _ACardIsSelected(简要棋子信息卡 card)
	{
		gui.game_mnger.PieceInfoCardSelectedInFBar(card.piece);
		Visible = false;
	}
}
