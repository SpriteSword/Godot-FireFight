using Godot;
using Godot.Collections;

public class PiecesMnger : HexTileMap
{
    //  各型号的属性
    public readonly Dictionary _model_attribute_ = new Dictionary{
        {Piece.PieceType.人, new Array<string>{"TM", "TM+"}},
        {Piece.PieceType.坦克, new Array<string>{"T62", "M60"}},
        {Piece.PieceType.APC, new Array<string>{"TOW"}}

    };

    //  资源
    Texture txtur_p_sprite_bg_red;
    Texture txtur_p_sprite_bg_blue;
    Texture txtur_p_sprite_body_man;
    Texture txtur_p_sprite_body_APC;
    Texture txtur_p_sprite_body_tank;

    PackedScene scn_shadow;
    PackedScene scn_piece;
    PackedScene scn_piece_sprite;

    //  子节点
    public Node pieces;     //  其下子节点是敌我双方棋子
    Node stacks;     //  其下子节点是各堆叠数据结构

    public PieceStack stack_focused;        //  正被鼠标指着的棋子堆叠
    // public Array<Piece> pieces_focused = new Array<Piece>();


    public override void _EnterTree()       //  资源，进入树就要加载
    {
        txtur_p_sprite_bg_red = GD.Load<Texture>("res://assets/棋子/红背景.png");
        txtur_p_sprite_bg_blue = GD.Load<Texture>("res://assets/棋子/蓝背景.png");
        txtur_p_sprite_body_man = GD.Load<Texture>("res://assets/棋子/步兵.png");
        txtur_p_sprite_body_APC = GD.Load<Texture>("res://assets/棋子/APC.png");
        txtur_p_sprite_body_tank = GD.Load<Texture>("res://assets/棋子/坦克.png");

        scn_piece = GD.Load<PackedScene>("res://源/棋子/Piece.tscn");
        scn_piece_sprite = GD.Load<PackedScene>("res://源/棋子/PieceSprite.tscn");
        scn_shadow = GD.Load<PackedScene>("res://源/棋子主管/PieceStack.tscn");
    }

    public override void _Ready()
    {
        pieces = GetNode<Node>("Pieces");
        stacks = GetNode<Node>("Stack");


        // var p_s = InstancePieceSprite(GameMnger.Side.红, "T62");
        // p_s.RectPosition = (new Vector2(100, 100));
        // AddChild(p_s);
        // AddChild(p_s);      //  add_child:无法将子“PieceSprite”添加到“PiecesMnger”，已具有父“PiecesMnger”。原来就是个指针！！

        // p_s.QueueFree();
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
            if (!r) { s.pieces[0].ZIndex = (int)Piece.ZIndexInStack._top_; }        //  移除不成功则仍有棋子置顶
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
    public void AddPiece(uint id, Vector2 cell_pos, GameMnger.Side side, Piece.PieceType piece_type, string model_name = "TM", bool visible = true)
    {
        var p = scn_piece.Instance<Piece>();
        p.id = id;
        p.side = side;
        p.type = piece_type;
        p.ModelName = model_name;
        p.Visible = visible;

        p.txtur_sprite_bg = GetPieceSprBgTxturBy(side);
        p.txtur_sprite_body = GetPieceSprBodyTxturBy((int)piece_type);

        pieces.AddChild(p);
        p.Connect("PlaceMe", this, "_PlacePiece");
        p.CellPos = cell_pos;

        AddPieceInStack(p);
    }

    //  使棋子sprite与side 、型号名相适应
    public void AdaptPieceSpriteTo(PieceSprite piece_sprite, GameMnger.Side side, string model_name)
    {
        var ms = model_name.Split(" ");     //  分离出大型号
        int p_type = GetPieceTypeBy(ms[0]);
        if (p_type == -1) GD.PrintErr("AdaptPieceSpriteTo(): big_model_name不合法！");

        piece_sprite.body.Texture = GetPieceSprBodyTxturBy(p_type);
        piece_sprite.bg.Texture = GetPieceSprBgTxturBy(side);
        piece_sprite.label.Text = model_name;
    }

    //  根据型号名返回棋子类型。没找到返回 -1.
    int GetPieceTypeBy(string big_model_name)
    {
        foreach (int key in _model_attribute_.Keys)
        {
            if (_model_attribute_[key] is Array arr)        ///  不能写Array<string>!
            {
                if (arr.Contains(big_model_name)) { return key; }
            }
        }
        return -1;
    }

    //  根据棋子类型索引返回sprite的body 纹理
    Texture GetPieceSprBodyTxturBy(int piece_type)
    {
        if (piece_type == (int)Piece.PieceType.人) { return txtur_p_sprite_body_man; }
        else if (piece_type == (int)Piece.PieceType.APC) { return txtur_p_sprite_body_APC; }
        else if (piece_type == (int)Piece.PieceType.坦克) { return txtur_p_sprite_body_tank; }

        return null;
    }

    //  根据棋子类型索引返回sprite的背景 纹理
    Texture GetPieceSprBgTxturBy(GameMnger.Side side)
    {
        if (side == GameMnger.Side.红) { return txtur_p_sprite_bg_red; }
        else { return txtur_p_sprite_bg_blue; }
    }

    //  用一个PieceSprite给另一个赋值，为了其子节点的图像资源相同，当然target需要有子节点的引用，没有直接报错！
    public void Assign(PieceSprite target, PieceSprite resource)
    {
        if (target == null || resource == null) return;

        target.bg.Texture = resource.bg.Texture;
        target.body.Texture = resource.body.Texture;
        target.label.Text = resource.label.Text;
        target.symbol.Texture = resource.symbol.Texture;
    }



    //-------------------------------------------------------信号
    //  定位棋子到格子中央
    void _PlacePiece(Piece piece)
    {
        piece.Position = HexGridCenter(piece.CellPos);     //+++++++++++++++++++++++++++++==随时更改stack数据
    }

    // //  鼠标正指向一个棋子
    // void _PieceFocusIn(Piece piece)     //  好像是如果重叠，先进（结点排在前面）先出
    // {
    //     pieces_focused.Add(piece);
    //     // stack_focused = GetPieceStack(piece.CellPos);        //  ++++++++++++++++++++++++++++++++++
    // }
    // void _PieceFocusOut(Piece piece)
    // {
    //     pieces_focused.Remove(piece);
    // }




    //——————————————————————————————————————————————————————————————————————————————————

}
