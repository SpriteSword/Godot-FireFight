using Godot;


public class 棋子信息卡 : PieceCard
{
    [Signal] delegate void SelectMe(棋子信息卡 me);      //  传给 棋子信息栏
    [Signal] delegate void Close(棋子信息卡 me);     //  -> 棋子信息栏

    // public StyleBoxFlat style;
    Label id_label;
    Label state_label;

    bool selected = false;

    //  是否正在被选，即点选棋子会反应到信息卡上
    public bool Selected
    {
        get { return selected; }
        set
        {
            selected = value;
            Update();
        }
    }


    public override void _Ready()
    {
        // style = Get("custom_styles/panel") as StyleBoxFlat;     //  写死
        // style.ResourceLocalToScene = true;

        ReferenceChildNode();
    }
    public override void _Draw()
    {
        if (selected)
        {
            DrawLine(new Vector2(0,0), new Vector2(RectSize.x, 0), Colors.Yellow, 2);
            DrawLine(new Vector2(1,0), new Vector2(1, RectSize.y), Colors.Yellow, 2);
            DrawLine(new Vector2(0, RectSize.y), RectSize, Colors.Yellow, 2);
            DrawLine(new Vector2(RectSize.x, 0), RectSize, Colors.Yellow, 2);
        }
    }


    //  EmitSignalSelectMe() 用父类的

    //  引用子节点，使自己的变量初始化
    public override void ReferenceChildNode()
    {
        base.ReferenceChildNode();
        id_label = GetNode<Label>("IdLabel");
        state_label = GetNode<Label>("StateLabel");
    }

    //  更新棋子信息
    public void UpdatePieceInfo(Piece p)
    {
        piece = p;
        id_label.Text = "#" + p.id.ToString();

        if (piece.BeK) { state_label.Text = "已阵亡！"; }
        else
        {
            state_label.Text = "";
            if (piece.BeS) { state_label.Text += "被压制 "; }
            if (piece.BeKF) { state_label.Text += "失去火力 "; }
            if (piece.BeKM) { state_label.Text += "失去移动 "; }
            if (piece.BeKMS) { state_label.Text += "失去导弹 "; }

            if (!piece.CanAct) { state_label.Text += "\n行动完毕"; }
        }

    }

    //  关闭。并且告诉主控把该棋子从列表中清除及与之相应的效果
    private void _on_CloseBtn_pressed()
    {
        EmitSignal("Close", this);
        QueueFree();
    }


}

