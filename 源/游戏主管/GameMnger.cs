using Godot;
using Godot.Collections;
using System;

public class GameMnger : Node2D
{
	public enum Side : byte { 无, 红, 蓝 };        //  无 用来标记哪方打算结束本阶段。暂时 红色代表苏联，蓝色代表美国
	public enum Stage : byte { 直射, 移动, 解除压制, 间射, 临机射击 };

	//  攻击结果。NULL 无影响；K 被杀死；S 被压制suppressed；KF 失去火力；KM 失去移动力；KMS 失去导弹，间射火力才有！。
	public enum AttackResult : byte { _null_, K, S, KF, KM, KMS };      //  专有名词不用管命名

	//  必要的游戏战斗判定数据。++++++++++++++++++++++++++++++++++各种型号真的很烦人！我还要解析他们？大、小型号。没有小型号，就代表所有该大型号的都通用
	public Dictionary _anti_vehicle_result_;       //  反车辆战斗结果表（2军通用）。{色子:{攻击等级:结果}}   还是先 等级 后 色子 理解更好点
	public Dictionary _anti_personnel_result_;     //  反人员战斗结果表（2军通用）。{色子:{攻击等级:结果}}
	public Dictionary _blue_attack_red_vehicles_level_;       //  美军对 苏军车辆 攻击等级表。{美军开火单位:{ 苏军目标单位:{距离:等级} }}。  上级别< 距离(m) <= 下1级别
	public Dictionary _blue_attack_red_personnel_level_;      //  美军对 苏军人员 攻击等级表。{美军单位:{距离:等级}}
	public Dictionary _red_attack_blue_vehicles_level_;       //  苏军对 美军车辆 攻击等级表。{苏军单位:{ 美军目标单位:{距离:等级} }}。
	public Dictionary _red_attack_blue_personnel_level_;      //  苏军对 美军人员 攻击等级表。{苏军单位:{距离:等级}}

	public readonly Array<int> _distance_level_ =       //  攻击距离分级。上级别< 距离(m) <= 下1级别，返回 下1级别
					new Array<int> { 0, 50, 100, 150, 200, 250, 300, 350, 400, 450, 500, 750, 1000, 1500, 2000, 2500, 3000 };       //  第一个0是自己加的，原规则无，为了好算。


	//  六边形的6个方向。共占6位bit，正右方是第1位，顺时针依次排下去。
	public enum DirectionIndex : byte { E = 1 << 0, SE = 1 << 1, SW = 1 << 2, W = 1 << 3, NW = 1 << 4, NE = 1 << 5 }

	//  6个方向向量。用的是 hex 坐标。
	public readonly Dictionary _directions_ = new Dictionary {
			{ Vector2.Right, DirectionIndex.E }, { Vector2.One, DirectionIndex.SE }, { Vector2.Down, DirectionIndex.SW },
			{ Vector2.Left, DirectionIndex.W }, { -Vector2.One, DirectionIndex.NW }, { Vector2.Up, DirectionIndex.NE }};

	//  道路瓦片的索引，数组下标是图块号，存的是对应的方向。
	//  [9, 18, 36, 33, 6, 24, 3, 12, 48, 34, 5, 10, 20, 40, 17]
	public readonly Array<uint> _road_tile_direction_index_ = new Array<uint>{
				(uint)DirectionIndex.E | (uint)DirectionIndex.W, (uint)DirectionIndex.SE | (uint)DirectionIndex.NW,(uint)DirectionIndex.NE | (uint)DirectionIndex.SW,
				(uint)DirectionIndex.E | (uint)DirectionIndex.NE, (uint)DirectionIndex.SE | (uint)DirectionIndex.SW, (uint)DirectionIndex.NW | (uint)DirectionIndex.W,
				(uint)DirectionIndex.E | (uint)DirectionIndex.SE, (uint)DirectionIndex.W | (uint)DirectionIndex.SW, (uint)DirectionIndex.NE | (uint)DirectionIndex.NW,
				(uint)DirectionIndex.NE | (uint)DirectionIndex.SE, (uint)DirectionIndex.E | (uint)DirectionIndex.SW,  (uint)DirectionIndex.SE | (uint)DirectionIndex.W,
				(uint)DirectionIndex.NW | (uint)DirectionIndex.SW, (uint)DirectionIndex.NE | (uint)DirectionIndex.W, (uint)DirectionIndex.E | (uint)DirectionIndex.NW,
				};


	// const float _Hint_Wait_Time_ = 1;

	//  信号
	[Signal] delegate void Inquire(string qusetion, IC command);      // -> 询问框._PopUp
	[Signal] delegate void Warn(string text);       //  -> 警告框，面板连好

	// [Export(PropertyHint.File, "后缀名")] string s;


	//  坐标、几何
	public const float _hex_side_len_ = 26;       //  六边形边长
	public const float _cell_w_ = 45;
	public const float _cell_h_ = 39;
	public readonly static Vector2 _hex_center_offset_ = new Vector2(_cell_w_ / 2, _hex_side_len_);
	//  地图大小，格数
	public const uint _map_w_ = 54;
	public const uint _map_h_ = 38;



	//  --------------------------------------------------资源


	//----------------------------------------------------网络
	public bool i_ready = false;
	public bool client_ready = false;

	//-----------------------------------------------------------
	//  相机
	float view_zoom = 1;        //  zoom 变焦
	bool camera_drag = false;       //  相机是否拖拽
	Vector2 camera_old_pos;

	//  鼠标
	Vector2 mouse_pos;
	Vector2 mouse_screen_pos;
	Vector2 mouse_screen_old_pos;
	public Vector2 mouse_cell_pos;
	public Vector2 mouse_cell_old_pos;

	//---------------------------------------------------


	#region ————————————————————————————————————————————————————————  节点
	//  子节点
	public Camera2D camera;
	public PiecesMnger pieces_mnger;
	public Mark mark;
	public GUI gui;
	询问框 inquiry_box;
	public 游戏网络处理 network_handler;

	//  数据查询用
	public TileMap road;
	public TileMap train;

	public Tween tween;
	Timer hint_timer;

	//  当前阶段
	public 游戏阶段 current_stage;
	Stage current_stage_index;
	#endregion

	Side actionable_side;       //  当前活动的一方
	public Side local_player_side;      //  本地玩家属于哪一方。不联网就是 无
	public Side end_stage_side = Side.无;     //  用来标记哪方打算结束本阶段

	//  移动阶段的路径
	//  储存路径节点坐标，cell 坐标。起点与棋子位置相同
	public Array<PathPoint> path = new Array<PathPoint>();
	public int path_node_index = 0;        //  路径节点在数组中的索引


	//-----------------------------------------当全局变量用
	public Array<Piece> pieces_selected = new Array<Piece>();     //  被选中的棋子们
	public PieceStack stack_selected;

	public Piece attacker;
	public Array<Piece> attackers = new Array<Piece>();
	public Piece defender;
	public Piece mover;     //  进入移动动画时使用

	#region get set

	public Side ActionableSide
	{
		get { return actionable_side; }
		set
		{
			actionable_side = value;
			gui.round_display_bar.SetActiveSideLabel(actionable_side);
		}
	}

	public Stage CurrentStageIndex
	{
		get { return current_stage_index; }
		set
		{
			current_stage_index = value;
			gui.round_display_bar.SetStageLabel(value);
		}
	}

	#endregion

	//----------------------------------------------------------
	public override void _Ready()
	{

		camera = GetNode<Camera2D>("Camera2D");
		pieces_mnger = GetNode<PiecesMnger>("PiecesMnger");
		mark = GetNode<Mark>("Mark");
		gui = GetNode<GUI>("画布层/GUI");
		inquiry_box = GetNode<询问框>("画布层/GUI/询问框");
		network_handler = GetNode<游戏网络处理>("游戏网络处理");
		road = GetNode<TileMap>("Map/Road");
		train = GetNode<TileMap>("Map/Train");
		tween = GetNode<Tween>("Tween");
		hint_timer = GetNode<Timer>("HintTimer");

		// road.Visible = false;
		// train.Visible = false;


		InitSignal();
		ReadNecessaryFile();


		// pieces_mnger.AddRedPiece(new Vector2(0, 2), 1);
		// pieces_mnger.AddBluePiece(new Vector2(0, 3), 2);
		// pieces_mnger.AddRedPiece(new Vector2(0, 2), 3);
		// pieces_mnger.AddBluePiece(new Vector2(0, 0), 4);



		//  发送准备好了
		i_ready = true;
		network_handler.Try2Start();

	}
	public override void _Process(float delta)
	{
		if (camera_drag)
		{
			camera.Position = camera_old_pos - (mouse_screen_pos - mouse_screen_old_pos) * view_zoom;
		}
	}
	public override void _UnhandledInput(InputEvent @event)     //  通用的写这里
	{
		if (@event is InputEventMouse m)
		{
			mouse_pos = GetGlobalMousePosition();        //  用gui的get_global_mouse_position()会以其画布层为原点！GetLocalMousePosition(), GetGlobalMousePosition()现在是一样的
			mouse_screen_pos = m.Position;
			mouse_cell_old_pos = mouse_cell_pos;
			mouse_cell_pos = mark.DetermineCellOfHexGrid(mouse_pos);

			HandleHintTimer();

			if (m is InputEventMouseButton mb)        //  鼠标按键
			{
				switch (mb.ButtonIndex)
				{
					case (int)ButtonList.WheelDown:     //  滚轮向下(后)缩小
						if (mb.Pressed && view_zoom < 2.5)
						{
							view_zoom += 0.1f;
							Zoom(mouse_pos, view_zoom);
						}
						break;

					case (int)ButtonList.WheelUp:       //  向上(前)放大
						if (mb.Pressed && view_zoom > 0.6)
						{
							view_zoom -= 0.1f;
							Zoom(mouse_pos, view_zoom);
						}
						break;

					case (int)ButtonList.Middle:        //  滚轮按下拖拽视角。坐标移动在_Process
						if (mb.Pressed)
						{
							camera_drag = true;
							mouse_screen_old_pos = mouse_screen_pos;
							camera_old_pos = camera.Position;
						}
						else { camera_drag = false; }
						break;
				}
			}
		}//  InputEventMouse
		else if (@event is InputEventKey key)
		{
			if (key.Pressed)
			{
				current_stage.HandleInputKey(key.Scancode);
			}
		} //  键盘InputEventKey
	}

	//——————————————————————————————————————————————————————————————————————————————  信号
	//  询问框
	protected void _InquiryBoxYes(IC command)
	{
		switch (command)
		{
			// case IC._None_:
			// 	break;
			default:
				current_stage.InquiryBoxYes(command);
				break;
		}
	}
	protected void _InquiryBoxNo(IC command)
	{
		switch (command)
		{
			// case IC._None_:
			// 	break;
			default:
				current_stage.InquiryBoxNo(command);
				break;
		}
	}

	private void _on_HintTimer_timeout()
	{
		// gui.floating_piece_info_bar.Show(mouse_screen_pos);
	}




	//——————————————————————————————————————————————————————————————————————————————
	//  信号初始化
	void InitSignal()
	{
		Connect("Inquire", inquiry_box, "_PopUp");
		inquiry_box.Connect("Yes", this, "_InquiryBoxYes");
		inquiry_box.Connect("No", this, "_InquiryBoxNo");
	}

	//  相机对焦
	void Zoom(Vector2 mou_pos, float zoom_)
	{
		camera.Zoom = new Vector2(zoom_, zoom_);
		camera.Position += (mou_pos - GetGlobalMousePosition());
		camera.ForceUpdateScroll();
	}

	//—————————————————————————————————————————————————————————————————————————————  GUI
	#region 棋子信息栏

	//  棋子信息栏变灰。棋子信息卡丧失选择
	public void GrayPieceInfoBar()
	{
		gui.piece_info_bar.GrayPieceInfoBar();
	}

	//  处理信息卡被选择。被棋子信息栏调用
	public void PieceInfoCardSelected(Piece p_selected)
	{
		camera.Position = p_selected.Position;
		current_stage.PieceInfoCardSelected(p_selected);
	}

	//  信息卡关闭，取消选择这个信息卡代表的棋子。被棋子信息栏调用
	public void PieceInfoCardClosed(Piece p_selected)
	{
		pieces_selected.Remove(p_selected);     //  失败也没事
		current_stage.PieceInfoCardClosed(p_selected);
	}

	#endregion
	#region 悬浮棋子信息栏

	//  处理信息卡被选择。被信息栏调用
	public void PieceInfoCardSelectedInFBar(Piece p_selected)
	{
		if (current_stage is 射击阶段Base shoot) { shoot.PieceInfoCardSelectedInFBar(p_selected); }
	}

	#endregion

	//  处理悬浮信息的timer
	void HandleHintTimer()
	{
		if (mouse_cell_old_pos == mouse_cell_pos)
		{
			if (hint_timer.IsStopped() && !camera_drag) { hint_timer.Start(hint_timer.WaitTime); }
		}
		else { hint_timer.Stop(); }
	}


	#region 玩法相关
	#endregion
	#region 文件

	//  必要文件读取
	void ReadNecessaryFile()        //+++++++++++++++++++++++++++++++++++++++++如果失败了，游戏又将如何退出？
	{
		File file = new File();
		file.Open("res://assets/data/反车辆战斗结果表.json", File.ModeFlags.Read);      //  反车辆战斗结果表
		var s = file.GetAsText();
		var json_rslt = JSON.Parse(s);

		if (json_rslt.Error == Error.Ok && json_rslt.Result is Dictionary)
		{
			_anti_vehicle_result_ = (Dictionary)json_rslt.Result;
		}
		else GD.PrintErr("读取文件失败: 反车辆战斗结果表.json");
		file.Close();


		file.Open("res://assets/data/反人员战斗结果表.json", File.ModeFlags.Read);      //  反人员战斗结果表
		s = file.GetAsText();
		json_rslt = JSON.Parse(s);

		if (json_rslt.Error == Error.Ok && json_rslt.Result is Dictionary)
		{
			_anti_personnel_result_ = (Dictionary)json_rslt.Result;
		}
		else GD.PrintErr("读取文件失败: 反人员战斗结果表.json");
		file.Close();


		file.Open("res://assets/data/美军对苏军车辆攻击等级表.json", File.ModeFlags.Read);      //  美军对苏军车辆攻击等级表
		s = file.GetAsText();
		json_rslt = JSON.Parse(s);

		if (json_rslt.Error == Error.Ok && json_rslt.Result is Dictionary)
		{
			_blue_attack_red_vehicles_level_ = (Dictionary)json_rslt.Result;
		}
		else GD.PrintErr("读取文件失败: 美军对苏军车辆攻击等级表.json");
		file.Close();


		file.Open("res://assets/data/美军对苏军人员攻击等级表.json", File.ModeFlags.Read);      //  美军对苏军人员攻击等级表
		s = file.GetAsText();
		json_rslt = JSON.Parse(s);

		if (json_rslt.Error == Error.Ok && json_rslt.Result is Dictionary)
		{
			_blue_attack_red_personnel_level_ = (Dictionary)json_rslt.Result;
		}
		else GD.PrintErr("读取文件失败: 美军对苏军人员攻击等级表.json");
		file.Close();


		file.Open("res://assets/data/苏军对美军车辆攻击等级表.json", File.ModeFlags.Read);      //  苏军对美军车辆攻击等级表
		s = file.GetAsText();
		json_rslt = JSON.Parse(s);

		if (json_rslt.Error == Error.Ok && json_rslt.Result is Dictionary)
		{
			_red_attack_blue_vehicles_level_ = (Dictionary)json_rslt.Result;
		}
		else GD.PrintErr("读取文件失败: 苏军对美军车辆攻击等级表.json");
		file.Close();


		file.Open("res://assets/data/苏军对美军人员攻击等级表.json", File.ModeFlags.Read);      //  苏军对美军人员攻击等级表
		s = file.GetAsText();
		json_rslt = JSON.Parse(s);

		if (json_rslt.Error == Error.Ok && json_rslt.Result is Dictionary)
		{
			_red_attack_blue_personnel_level_ = (Dictionary)json_rslt.Result;
		}
		else GD.PrintErr("读取文件失败: 苏军对美军人员攻击等级表.json");
		file.Close();


	}

	#endregion


	//—————————————————————————————————————————————————————————————————————————————— 网络

	//  是否是主机
	public bool IsAsServer()
	{
		return network_handler.network_mnger.IsAsServer();
	}

	//  发送信息
	public void Send(int tar_id, string data)
	{
		network_handler.network_mnger.Send(tar_id, data);
	}


}

