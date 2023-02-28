using Godot;

//  游戏开始，所有的信息都在GameMnger，这里的图片跟其建立联系。经操作后（布子），生成棋子在 PieceMnger，棋子也是只有一张皮
public class 棋子部署系统 : Panel
{
	//  鼠标相对于棋子的偏移
	Vector2 mouse_offset;

	//  资源
	PackedScene scn_texture;

	//  节点
	public GUI gui;
	public 想定 controller;       //  为了方便的引用。由 想定 初始化
	GridContainer container;
	Button move_btn;
	Button finish_btn;
	public 预备部署棋子 piece_in_bar;     //  用来指着当前编辑的棋子的图像
	PieceSprite piece_spr_on_mouse;     //  用来附着在鼠标的棋子图像


	public override void _EnterTree()
	{
		scn_texture = GD.Load<PackedScene>("res://源/GUI/棋子部署系统/预备部署棋子.tscn");
	}
	public override void _Ready()
	{
		piece_spr_on_mouse = GetNode<PieceSprite>("PieceSpriteOnMouse");

		gui = GetNode<GUI>("..");
		container = GetNode<GridContainer>("ScrollContainer/GridContainer");
		move_btn = GetNode<Button>("MoveBtn");
		finish_btn = GetNode<Button>("FinishBtn");

		piece_spr_on_mouse.Visible = false;
		Hide();
	}
	public override void _Process(float delta)
	{
		if (IsHolding())
		{
			piece_spr_on_mouse.RectGlobalPosition = GetGlobalMousePosition() - mouse_offset;
		}
	}
	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("click_right"))
		{
			if (IsHolding())
			{
				piece_spr_on_mouse.Hide();

				ShowPiecePicked(false);

				piece_in_bar = null;
				//+++++++ GUI显示信息
			}
		}
	}

	//  添加棋子图像。根据文件读入后，生成id、一览表。PieceSprite 是个节点，
	public void AddPieceInBar(uint piece_id, GameMnger.Side side, string model_name)
	{
		var t = scn_texture.Instance<预备部署棋子>();
		container.AddChild(t);      //  AddChild 执行完即进入树且ready了。让它先进树就不用在这里手动 ReferenceChildNode() 了

		t.piece_id = piece_id;
		t.Connect("SelectMe", this, "_SelectTexture");
		gui.game_mnger.pieces_mnger.AdaptPieceSpriteTo(t.piece_sprite, side, model_name);
	}

	//  是否拾起了棋子
	public bool IsHolding()
	{
		return piece_spr_on_mouse.Visible;
	}

	//  显示栏中的棋子被拾取。true：拾取在鼠标上，false: 还在栏中
	void ShowPiecePicked(bool picked)
	{
		piece_spr_on_mouse.Visible = picked;

		finish_btn.Disabled = picked;

		if (picked)
		{
			piece_in_bar.Modulate = new Color(0.2f, 0.2f, 0.2f);
			piece_in_bar.MouseFilter = MouseFilterEnum.Ignore;
			return;
		}

		piece_in_bar.Modulate = new Color(1, 1, 1);
		piece_in_bar.MouseFilter = MouseFilterEnum.Stop;
	}

	//  根据id在栏中找棋子
	void FindByID(uint id)
	{
		foreach (预备部署棋子 itm in container.GetChildren())
		{
			if (itm.piece_id == id) { piece_in_bar = itm; return; }
		}
	}

	//  清除栏中所有棋子
	public void ClearAll()
	{
		foreach (预备部署棋子 itm in container.GetChildren()) { itm.QueueFree(); }
	}





	#region  ————————————————————————————————————————————————————————————————  控制器 调用

	//  已经将棋子放到地图上了
	public void HavePlacedPiece()
	{
		piece_spr_on_mouse.Hide();
		finish_btn.Disabled = false;
	}

	//  当点击地图上的棋子时，显示UI
	public void ShowUIIfClickPieceOnMap(bool want, uint id = 0)
	{
		move_btn.Disabled = !want;
		FindByID(id);
	}

	#endregion

	//------------------------------------------------ 子节点信号
	//  点击 移动 按钮
	private void _on_MoveBtn_pressed()
	{
		controller.Redeploy(piece_in_bar.piece_id);

		move_btn.Disabled = true;
		gui.game_mnger.pieces_mnger.Assign(piece_spr_on_mouse, piece_in_bar.piece_sprite);

		mouse_offset = new Vector2(18, 18);
		ShowPiecePicked(true);
	}

	//  点选栏中的图像（棋子）后
	void _SelectTexture(预备部署棋子 who, Vector2 _mouse_offset)
	{
		if (IsHolding()) return;		//  防止拾起了棋子又去点击栏中的

		piece_in_bar = who;
		gui.game_mnger.pieces_mnger.Assign(piece_spr_on_mouse, who.piece_sprite);
		mouse_offset = _mouse_offset;
		ShowPiecePicked(true);
	}

	//  结束按钮
	private void _on_FinishBtn_pressed()
	{
		controller.InquireFinish();
	}



}


