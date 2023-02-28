using Godot;

public class 试子类信号的行为 : PieceCard
{
    //  父类或子类有一个登记就可以了
    [Signal] delegate void SelectMe(试子类信号的行为 me, Vector2 mouse);        //  这参数是假的！看你实际传的什么来调用哪个函数

    public override void _Ready()
    {
        GD.Print("ha");		//  完了，this is TextureRect 是false
        Connect("SelectMe", this, "_on_TextureRect_SelectMe");      //  连接信号只看名字不看参数的！！！调用的时候看你实际传的什么来调用哪个函数
        EmitSignal("SelectMe", this, Vector2.Zero);
        EmitSignal("SelectMe", this);
    }


    //  父类试验用
    // protected virtual void _on_TextureRect_SelectMe(PieceCard me)
    // {
    //     GD.Print("父类", me.GetType());
    // }

    protected void _on_TextureRect_SelectMe(PieceCard me)
    {
        GD.Print("子类", me.GetType());
    }

    protected void _on_TextureRect_SelectMe(PieceCard me, Vector2 mouse)
    {
        GD.Print(me.GetType(), mouse);
    }
}

