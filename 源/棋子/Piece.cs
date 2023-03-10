using Godot;

public class Piece : Node2D
{
	public enum PieceType : byte { 人, 坦克, APC };        //  该死的结算表还分 坦克/APC
	public enum ZIndexInStack : byte { _ordinary_, _top_ = 1, _act_ = 2 };      //  普通棋子ZIndex==0，置顶的=1。在移动动画期间调为2，使其高于所有棋子。进入堆叠后会调回1。

	[Signal] delegate void PlaceMe(Piece me);     //  如果坐标改变，由PiecesMnger定位到正确格子上。-->PiecesMnger


	//-------------------------------------------------------

	public Texture txtur_sprite_bg;       //  piece manager的资源的引用
	public Texture txtur_sprite_body;

	//  坐标
	Vector2 hex_pos;       //  人用的
	Vector2 cell_pos;       //  tilemap用

	//  唯一标识
	public uint id;

	//  哪边
	[Export] public GameMnger.Side side;        //  导出枚举不管你的标记PropertyHint

	[Export] public PieceType type;     //  单位类型
	string model_name;       //  包括大小型号。像 "M60 A3"，M60大型号，A3小型号。

	//  移动点数 movement point
	public float m_p = 10;


	//  在 1 个回合内或移动或射击，只能执行 1 次行动（短停射击除外）
	//  每个单位 每个阶段 只能被直射 1 次
	//  射击
	bool can_act = true;        //  可以行动
	public bool can_be_shot = true;        //  可以被敌方射击

	//  被攻击的结果。GameMnger.AttackResult 的值减一，null不算，分别为 是否被杀死；是否被压制，是否失去火力；是否失去移动力；是否失去导弹。
	public bool[] be_attacked_result = new bool[5] { false, false, false, false, false };

	//  子节点
	public PieceSprite sprite;
	AnimationPlayer anim_player;

	#region ————————————————————————————————————————————————————————————————————————  Get Set

	#region 坐标get set
	[Export]
	public Vector2 HexPos
	{
		get { return hex_pos; }
		set
		{
			hex_pos = value;
			cell_pos = Math.Hex2CellCoord(hex_pos);
			EmitSignal("PlaceMe", this);
		}
	}
	[Export]
	public Vector2 CellPos
	{
		get { return cell_pos; }
		set
		{
			cell_pos = value;
			hex_pos = Math.Cell2HexCoord(cell_pos);
			EmitSignal("PlaceMe", this);
		}
	}
	#endregion

	//  能否行动
	public bool CanAct
	{
		get { return can_act; }
		set
		{
			can_act = value;
			if (can_act) { Modulate = new Color(1, 1, 1); }
			else
			{
				if (!BeK) { Modulate = new Color(0.5f, 0.5f, 0.5f); }
				else { Modulate = new Color(1, 1, 1); }
			}
		}
	}

	//  型号名
	public string ModelName
	{
		get { return model_name; }
		set
		{
			model_name = value;
		}
	}

	//  是否被杀死
	public bool BeK
	{
		get { return be_attacked_result[(int)GameMnger.AttackResult.K - 1]; }
		set
		{
			be_attacked_result[(int)GameMnger.AttackResult.K - 1] = value;
		}
	}
	//  是否被压制
	public bool BeS
	{
		get { return be_attacked_result[(int)GameMnger.AttackResult.S - 1]; }
		set
		{
			be_attacked_result[(int)GameMnger.AttackResult.S - 1] = value;
		}
	}
	//  是否失去火力
	public bool BeKF
	{
		get { return be_attacked_result[(int)GameMnger.AttackResult.KF - 1]; }
		set
		{
			be_attacked_result[(int)GameMnger.AttackResult.KF - 1] = value;
		}
	}
	//  是否失动
	public bool BeKM
	{
		get { return be_attacked_result[(int)GameMnger.AttackResult.KM - 1]; }
		set
		{
			be_attacked_result[(int)GameMnger.AttackResult.KM - 1] = value;
		}
	}
	//  是否失去导弹
	public bool BeKMS
	{
		get { return be_attacked_result[(int)GameMnger.AttackResult.KMS - 1]; }
		set
		{
			be_attacked_result[(int)GameMnger.AttackResult.KMS - 1] = value;
		}
	}

	#endregion

	public override void _Ready()
	{
		sprite = GetNode<PieceSprite>("PieceSprite");
		anim_player = GetNode<AnimationPlayer>("AnimationPlayer");

		InitSprite();
	}


	//————————————————————————————————————————————————————————————————————————
	//  设置be_attacked_result
	void SetBeAttackedResultTrue(GameMnger.AttackResult result)
	{
		be_attacked_result[(int)result - 1] = true;
	}

	//  设置攻击结果
	public void SetBeAttackedResult(GameMnger.AttackResult result)
	{
		if (result == GameMnger.AttackResult._null_) return;

		if (result == GameMnger.AttackResult.K) { Die(); return; }

		if (this.type == PieceType.人)
		{
			if (this.BeS && result == GameMnger.AttackResult.S) { Die(); }
		}
		else
		{
			if ((this.BeKM && result == GameMnger.AttackResult.KM) ||
				this.BeKF && result == GameMnger.AttackResult.KF) { Die(); }
		}

		SetBeAttackedResultTrue(result);
	}

	//  死亡
	void Die()
	{
		anim_player.Play("die");
		SetBeAttackedResultTrue(GameMnger.AttackResult.K);
		CanAct = false;
	}

	//  初始化Sprite
	void InitSprite()
	{
		sprite.SetTexture(model_name, txtur_sprite_bg, txtur_sprite_body);
	}

	//  清理出地图。queue_free
	public void CleanUp()
	{
		anim_player.Play("be_cleaned");
	}


}







