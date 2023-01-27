using Godot;
using System;

public class Piece : Node2D
{
    public enum PieceType : byte { 人, 坦克, APC };        //  该死的结算表还分 坦克/APC
    public enum ZIndexInStack : byte { _ordinary_, _top_ = 1, _act_ = 2 };		//  普通棋子ZIndex==0，置顶的=1。在移动动画期间调为2，使其高于所有棋子。进入堆叠后会调回1。

    // [Signal] delegate void SelectMe(Piece me);		//  被鼠标选中。-->
    [Signal] delegate void MouseIn(Piece me);       //  -->PiecesMnger
    [Signal] delegate void MouseOut(Piece me);
    [Signal] delegate void PlaceMe(Piece me);     //  如果坐标改变，由PiecesMnger定位到正确格子上。-->PiecesMnger

    // bool mouse_in = false;

    // public const _Z_Index_Ordinary_

    //  坐标
    Vector2 hex_pos;       //  人用的
    Vector2 cell_pos;       //  tilemap用

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

    //-------------------------------------------------------
    //  唯一标识
    public uint id;

    //  哪边
    [Export] public GameMnger.Side side;        //  导出枚举不管你的标记PropertyHint

    [Export] public PieceType type;     //  单位类型
    public string model_name;       //  包括大小型号。像 "M60 A3"，M60大型号，A3小型号。


    //  在 1 个回合内或移动或射击，只能执行 1 次行动（短停射击除外）
    //  每个单位 每个阶段 只能被直射 1 次
    //  射击
    // bool can_shoot = true;      //  可以射击敌方
    bool can_act = true;        //  可以行动
    public bool can_be_shot = true;        //  可以被敌方射击

    //  子节点
    public Node2D sprite;

    //  get set
    public bool CanAct
    {
        get { return can_act; }
        set
        {
            can_act = value;
            if (can_act) Modulate = new Color(1, 1, 1);
            else Modulate = new Color(0.5f, 0.5f, 0.5f);
        }
    }


    public override void _Ready()
    {
        sprite = GetNode<Node2D>("Sprite");
    }



    //-------------------------------------------------------------------GUI


    private void _on_Area2D_mouse_entered()
    {
        EmitSignal("MouseIn", this);
    }

    private void _on_Area2D_mouse_exited()
    {
        EmitSignal("MouseOut", this);
    }


}







