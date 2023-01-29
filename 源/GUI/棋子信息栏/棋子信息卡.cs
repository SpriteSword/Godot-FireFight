using Godot;
using System;


public class 棋子信息卡 : Panel
{
    [Signal] delegate void SelectMe(棋子信息卡 me);      //  传给 棋子信息栏
    [Signal] delegate void Close(棋子信息卡 me);     //  -> 棋子信息栏

    public StyleBoxFlat style;
    Label id_label;
    Label state_label;

    bool selected = false;
    public Piece piece;     //  指向的棋子

    //  棋子信息
    // uint p_id;

    //  是否正在被选，即点选棋子会反应到信息卡上
    public bool Selected
    {
        get { return selected; }
        set
        {
            selected = value;
            ChangeStyle();
        }
    }


    public override void _Ready()
    {
        style = Get("custom_styles/panel") as StyleBoxFlat;     //  写死
        style.ResourceLocalToScene = true;

        id_label = GetNode<Label>("IdLabel");
        state_label = GetNode<Label>("StateLabel");
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event.IsActionPressed("click_left"))
        {
            EmitSignal("SelectMe", this);
        }
    }


    //  被选中后的效果。同时玩家视角会回到棋子上
    public void ChangeStyle()
    {
        if (selected)
        {
            style.BorderColor = new Color(1, 1, 0);
            style.BorderWidthBottom = 3;
            style.BorderWidthLeft = 3;
            style.BorderWidthRight = 3;
            style.BorderWidthTop = 3;
            return;
        }
        style.BorderColor = new Color(1, 1, 1);
        style.BorderWidthBottom = 1;
        style.BorderWidthLeft = 1;
        style.BorderWidthRight = 1;
        style.BorderWidthTop = 1;
    }

    //  更新棋子信息
    public void UpdatePieceInfo(Piece p)
    {
        piece = p;
        id_label.Text = "id: " + p.id.ToString();

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

