using Godot;
using System;
using Godot.Collections;

//  只显示己方的棋子
//  2状态：1、棋子正在被选，信息卡亮；2、棋子被选后，又取消了，信息卡灰；3、信息卡关闭，棋子取消被选。

public class 棋子信息栏 : Panel		//+++++++++++++++++++++++++++++= 虽然不是自己行动的回合，但不能查看自己一方的信息，体验太差！
{
	PackedScene card_scn;

	GUI gui;
	ScrollContainer scroll;
	VBoxContainer container;

	Vector2 card_size;      //  卡片大小
	int card_separation;        //  卡片的间隔


    public override void _EnterTree()       //  资源，进入树就要加载
    {
		card_scn = GD.Load<PackedScene>("res://源/GUI/棋子信息栏/棋子信息卡.tscn");
	}

	public override void _Ready()
	{
		scroll = GetNode<ScrollContainer>("ScrollContainer");
		container = GetNode<VBoxContainer>("ScrollContainer/VBoxContainer");
		gui = GetNode<GUI>("..");

		card_separation = (int)container.Get("custom_constants/separation");
		Visible = false;
	}

	public override void _GuiInput(InputEvent @event)       //  _GuiInput没用的！
	{
	}


	//  选到新棋子，更新棋子列表，用pieces_selected。+++++++++++++++++++++++++++++++如果棋子数值变化了该如何？
	public void UpdateList(Array<Piece> pieces_selected)
	{
		foreach (Node item in container.GetChildren()) item.QueueFree();

		if (pieces_selected.Count == 0)
		{
			Visible = false;
			return;
		}
		Visible = true;

		float container_h = 0;      //  容器高

		foreach (Piece piece in pieces_selected)
		{
			棋子信息卡 card = card_scn.Instance<棋子信息卡>();
			container.AddChild(card);
			card.UpdatePieceInfo(piece);
			card.Selected = true;
			card.Connect("SelectMe", this, "_ACardIsSelected");
			card.Connect("Close", this, "_ACardClose");

			container_h += card.RectSize.y + card_separation;
		}
		container_h -= card_separation;

		AdjustHeight(container_h);
	}

	//  更新棋子列表，用棋子堆叠中的棋子。index指数，0：所有都灰；1：顶部棋子高亮，其余灰；2：所有棋子高亮
	public void UpdateListByPieceStack(PieceStack stack, byte index)
	{
		foreach (Node item in container.GetChildren()) item.QueueFree();

		if (stack == null) return;

		Visible = true;     //
		float container_h = 0;      //  容器高

		foreach (Piece piece in stack.pieces)
		{
			棋子信息卡 card = card_scn.Instance<棋子信息卡>();
			container.AddChild(card);
			card.UpdatePieceInfo(piece);

			if ((index == 2) || (index == 1 && piece.ZIndex == (int)Piece.ZIndexInStack._top_))
			{
				card.Selected = true;
			}

			card.Connect("SelectMe", this, "_ACardIsSelected");
			card.Connect("Close", this, "_ACardClose");

			container_h += card.RectSize.y + card_separation;
		}
		container_h -= card_separation;

		AdjustHeight(container_h);
	}



	//  棋子信息栏变灰。棋子信息卡丧失选择
	public void GrayPieceInfoBar()
	{
		foreach (棋子信息卡 item in container.GetChildren())
			item.Selected = false;
	}

	//  调整高度
	void AdjustHeight(float container_height)
	{
		if (container_height < OS.WindowSize.y)
		{
			scroll.RectSize = new Vector2(scroll.RectSize.x, container_height);
			return;
		}

		scroll.RectSize = new Vector2(scroll.RectSize.x, OS.WindowSize.y);
	}


	//------------------------------------------------------------------------信号
	//  信号。信息卡被选中传来
	void _ACardIsSelected(棋子信息卡 card)
	{
		gui.game_mnger.PieceInfoCardSelected(card.piece);

		card.Selected = true;
		foreach (棋子信息卡 item in container.GetChildren())
		{
			if (item != card) item.Selected = false;
		}
	}

	//  信号。卡关闭时传来。主控把该棋子从列表中清除及与之相应的效果
	void _ACardClose(棋子信息卡 card)
	{
		scroll.RectSize = new Vector2(scroll.RectSize.x, scroll.RectSize.y - card.RectSize.y - card_separation);
		gui.game_mnger.PieceInfoCardClosed(card.piece);
	}




}
