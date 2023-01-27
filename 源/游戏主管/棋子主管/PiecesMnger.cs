using Godot;
using Godot.Collections;

public class PiecesMnger : MyTileMap
{
    //  资源
    PackedScene scn_red_piece;
    PackedScene scn_blue_piece;
    PackedScene scn_shadow;

    //  子节点
    public Node pieces;     //  其下子节点是敌我双方棋子
    Node stacks;     //  其下子节点是各堆叠数据结构

    public PieceStack stack_focused;        //  正被鼠标指着的棋子堆叠
    public Array<Piece> pieces_focused = new Array<Piece>();
    // bool piece_is_focused = false;      //  鼠标停留在棋子上方，用来显示悬浮棋子信息栏


    public override void _Ready()
    {
        scn_red_piece = GD.Load<PackedScene>("res://源/棋子/red/RedPiece.tscn");
        scn_blue_piece = GD.Load<PackedScene>("res://源/棋子/blue/BluePiece.tscn");
        scn_shadow = GD.Load<PackedScene>("res://源/游戏主管/棋子主管/PieceStack.tscn");

        pieces = GetNode<Node>("Pieces");
        stacks = GetNode<Node>("Stack");
    }


    //——————————————————————————————————————————————————————————————————

    //  返回2个棋子间是否是敌对
    public static bool IsAgainst(GameMnger.Side side1, GameMnger.Side side2)
    {
        return side1 != side2;
    }

    //  根据id找棋子
    public Piece GetPiece(uint id)
    {
        foreach (Piece piece in pieces.GetChildren())
        {
            if (id == piece.id) return piece;
        }
        return null;
    }

    // //
    // public int GetPieceAmountInStack(PieceStack stack)
    // {
    //     return stack.pieces.Count;
    // }

    //  根据直角坐标找棋子。坐标是以PiecesMnger的坐标原点。
    public Piece GetTopPieceByRectPos(Vector2 rect_pos)
    {
        return GetPieceStackByRectPos(rect_pos).GetTopPiece();
    }

    //  根据直角坐标找堆叠。
    public PieceStack GetPieceStackByRectPos(Vector2 rect_pos)
    {
        return GetPieceStack(DetermineCellOfHexGrid(rect_pos));
    }

    //  根据cell_pos位置找堆叠
    public PieceStack GetPieceStack(Vector2 cell_pos)
    {
        foreach (PieceStack st in stacks.GetChildren())
        {
            if (cell_pos == st.cell_pos) return st;
        }
        return null;
    }

    //  添加棋子进堆叠，不会重复添加。不同side会报错！自动将新加入的置顶。
    public void AddPieceInStack(Piece piece)
    {
        PieceStack s = GetPieceStack(piece.CellPos);
        if (s == null)
        {
            s = scn_shadow.Instance<PieceStack>();
            s.cell_pos = piece.CellPos;
            s.side = piece.side;
            s.Position = HexGridCenter(s.cell_pos);
            stacks.AddChild(s);
        }

        if (s.side != piece.side) GD.PrintErr("堆叠的side与棋子不同！");

        s.AddPiece(piece);

        foreach (var itm in s.pieces)       //  置顶棋子
        {
            if (itm == piece) itm.ZIndex = (int)Piece.ZIndexInStack._top_;
            else itm.ZIndex = (int)Piece.ZIndexInStack._ordinary_;
        }
    }

    //  从堆叠移除棋子，并更新置顶棋子
    public void RemovePieceFromStack(Piece piece)
    {
        PieceStack s = GetPieceStack(piece.CellPos);
        if (s == null) return;

        if (s.pieces.Count > 1)
        {
            s.RemovePiece(piece);
            s.pieces[0].ZIndex = (int)Piece.ZIndexInStack._top_;
        }
        else        //  == 1
        {
            var r = s.RemovePiece(piece);
            if (!r) s.pieces[0].ZIndex = (int)Piece.ZIndexInStack._top_;        //  移除不成功则仍有棋子置顶
        }
    }

    //  将棋子图像置顶，必须首先确定棋子在堆叠中。top 动词。
    public void TopAPiece(Piece piece)
    {
        var stck = GetPieceStack(piece.CellPos);
        if (stck == null) return;

        foreach (var itm in stck.pieces)
        {
            if (itm == piece) itm.ZIndex = (int)Piece.ZIndexInStack._top_;
            else itm.ZIndex = (int)Piece.ZIndexInStack._ordinary_;
        }
    }




    //  添加棋子。游戏想定阶段调用。
    public void AddRedPiece(Vector2 hex_pos, uint id)
    {
        var p = scn_red_piece.Instance<Piece>();
        p.id = id;
        p.type = Piece.PieceType.人;
        p.model_name = "TM";        //+++++++++++++++++++++++++++++++

        pieces.AddChild(p);
        p.Connect("PlaceMe", this, "_PlacePiece");
        p.Connect("MouseIn", this, "_PieceFocusIn");
        p.Connect("MouseOut", this, "_PieceFocusOut");

        p.HexPos = hex_pos;

        AddPieceInStack(p);
    }
    public void AddBluePiece(Vector2 hex_pos, uint id)
    {
        var p = scn_blue_piece.Instance<Piece>();
        p.id = id;
        p.type = Piece.PieceType.人;
        p.model_name = "TM";

        pieces.AddChild(p);
        p.Connect("PlaceMe", this, "_PlacePiece");
        p.Connect("MouseIn", this, "_PieceFocusIn");
        p.Connect("MouseOut", this, "_PieceFocusOut");

        p.HexPos = hex_pos;

        AddPieceInStack(p);
    }




    //-------------------------------------------------------信号
    //  定位棋子到格子中央
    void _PlacePiece(Piece piece)
    {
        piece.Position = HexGridCenter(piece.CellPos);     //+++++++++++++++++++++++++++++==随时更改stack数据
    }

    //  鼠标正指向一个棋子
    void _PieceFocusIn(Piece piece)     //  好像是如果重叠，先进（结点排在前面）先出
    {
        pieces_focused.Add(piece);
        // stack_focused = GetPieceStack(piece.CellPos);        //  ++++++++++++++++++++++++++++++++++
    }
    void _PieceFocusOut(Piece piece)
    {
        pieces_focused.Remove(piece);
    }




    //——————————————————————————————————————————————————————————————————————————————————

}
