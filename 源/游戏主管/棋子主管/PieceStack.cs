using Godot;
using Godot.Collections;

//  棋子堆。主要用来显示阴影。
public class PieceStack : Node2D
{
    //  子节点
    Sprite shadow;

    public Vector2 cell_pos;
    public GameMnger.Side side;
    public Array<Piece> pieces = new Array<Piece>();        //  ++++++好像怎么做也防止不了外面随意add、remove

    public bool ShadowVisible
    {
        get { return shadow.Visible; }
        set { shadow.Visible = value; }
    }

    public override void _Ready()
    {
        shadow = GetNode<Sprite>("阴影");

        ShadowVisible = false;
    }

    //  加进棋子，只是加进数组并改变阴影。重复则什么也不做。
    public void AddPiece(Piece piece)
    {
        foreach (var item in pieces)      //  看看是否棋子已经记录过了
        {
            if (item == piece) return;      //  可以2个引用进行比较的！
        }
        pieces.Add(piece);
        if (pieces.Count > 1) ShadowVisible = true;
    }

    //  移除棋子，只是从数组移除并改变阴影。返回是否成功。
    public bool RemovePiece(Piece piece)
    {
        var r = pieces.Remove(piece);
        if (pieces.Count < 2) ShadowVisible = false;
        if (pieces.Count == 0) { QueueFree(); }     //++++++++++++=  万一这里还没销毁，其它地方又需要这个位置的堆叠又如何？
        return r;
    }

    //  获取顶部棋子。不可能返回null，否则就是有bug
    public Piece GetTopPiece()
    {
        foreach (var item in pieces)
        {
            if (item.ZIndex == (int)Piece.ZIndexInStack._top_) return item;
        }
        return null;
    }
}
