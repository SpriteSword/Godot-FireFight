using Godot;


//  棋子信息卡的基类
public class PieceCard : Control
{
    //  父类懒得写信号，子类写了就行

    public Piece piece;     //  指向的棋子

    //  子节点
    public PieceSprite piece_sprite;


    public override void _Ready()
    {
        ReferenceChildNode();
    }
    public override void _GuiInput(InputEvent @event)
    {
        if (@event.IsActionReleased("click_left"))
        {
            EmitSignalSelectMe();
        }
    }


    //  引用子节点，使自己的变量初始化
    public virtual void ReferenceChildNode()
    {
        piece_sprite = GetNode<PieceSprite>("PieceSprite");
    }

    //  发送 SelectMe 信号
    protected virtual void EmitSignalSelectMe()
    {
        EmitSignal("SelectMe", this);
    }
}