using Godot;
using System;

//  游戏开始，成所有的信息都在GameMnger，这里的图片跟其建立联系。经操作后（布子），生成棋子在 PieceMnger，棋子也是只有一张皮
public class 棋子部署系统 : Panel
{
	[Signal] delegate void MovePiece(uint id);
	[Signal] delegate void Finish();

	//  鼠标相对于棋子的偏移
	Vector2 mouse_offset;

	//  资源
	PackedScene scn_texture;

	//  节点
	GameMnger game_mnger;
	GridContainer container;
	public 预备部署棋子 selected_texture;
	TextureRect picked_up_texture;		//  用来附着在鼠标的棋子图像
	Button move_btn;
	Button finish_btn;

	public override void _Ready()
	{
		scn_texture = GD.Load<PackedScene>("res://源/游戏主管/GUI/棋子部署系统/预备部署棋子.tscn");

		picked_up_texture = GetNode<TextureRect>("PickedUpPieceTexture");
		container = GetNode<GridContainer>("ScrollContainer/GridContainer");
		move_btn = GetNode<Button>("MoveBtn");
		finish_btn = GetNode<Button>("FinishBtn");

		picked_up_texture.Visible = false;

		AddTexture(1);
		AddTexture(2);

	}

	public override void _Process(float delta)
	{
		if (HasPicked())
		{
			picked_up_texture.RectGlobalPosition = GetGlobalMousePosition() - mouse_offset;
		}
	}


	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("click_right"))
		{
			if (HasPicked())
			{
				picked_up_texture.Texture = null;

				ShowPicked(false);

				selected_texture = null;
				//+++++++ GUI显示信息
			}
		}
	}


	public override void _GuiInput(InputEvent @event)
	{
	}


	public bool HasPicked()
	{
		return picked_up_texture.Visible;
	}


	//  添加棋子图像。根据文件读入后，生成id、一览表
	void AddTexture(uint piece_id)
	{
		var t = scn_texture.Instance<预备部署棋子>();
		t.piece_id = piece_id;
		t.Connect("SelectMe", this, "_SelectTexture");

		container.AddChild(t);
	}


	void ShowPicked(bool picked)
	{
		picked_up_texture.Visible = picked;
		finish_btn.Disabled = picked;

		if (picked)
		{
			selected_texture.Modulate = new Color(0.2f, 0.2f, 0.2f);
			selected_texture.MouseFilter = MouseFilterEnum.Ignore;
			return;
		}

		selected_texture.Modulate = new Color(1, 1, 1);
		selected_texture.MouseFilter = MouseFilterEnum.Stop;

	}

	void FindByID(uint id)
	{
		foreach (预备部署棋子 itm in container.GetChildren())
			if (itm.piece_id == id)
			{
				selected_texture = itm;
				break;
			}
	}


	//-----------------------------------------------------------------------ctrl 信号




	void _ShowUI(bool show)
	{
		Visible = show;
	}

	void _Deployed()
	{
		picked_up_texture.Visible = false;
	}

	void _WantEditPiece(bool want, uint id = 0)
	{
		move_btn.Disabled = !want;
		FindByID(id);
		GD.Print(id);
	}



	//------------------------------------------------ 子节点信号
	private void _on_MoveBtn_pressed()
	{
		EmitSignal("MovePiece", selected_texture.piece_id);
		move_btn.Disabled = true;
		picked_up_texture.Texture = selected_texture.Texture;
		mouse_offset = new Vector2(18, 18);
		ShowPicked(true);
	}

	void _SelectTexture(预备部署棋子 who, Vector2 _mouse_offset)
	{
		selected_texture = who;
		GD.Print(selected_texture);

		picked_up_texture.Texture = who.Texture;
		mouse_offset = _mouse_offset;

		ShowPicked(true);
	}



	private void _on_FinishBtn_pressed()
	{
		EmitSignal("Finish");
	}



}


