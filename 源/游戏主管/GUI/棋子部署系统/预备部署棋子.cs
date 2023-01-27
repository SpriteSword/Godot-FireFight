using Godot;
using System;

public class 预备部署棋子 : TextureRect
{
    [Signal] delegate void SelectMe(预备部署棋子 me, Vector2 mouse_offset);


    public uint piece_id;       //  在GameMnger的棋子信息的索引。文件-> 读入，生成id -> 一览表 -> 地图上棋子
    public Piece piece_pointed;     //  指向在地图上的棋子

    bool mouse_in = false;

    public override void _Ready()
    {
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event.IsActionPressed("click_left"))
        {
            if (mouse_in)
                EmitSignal("SelectMe", this, GetLocalMousePosition());
        }
    }



    private void _on_mouse_entered()
    {
        mouse_in = true;
    }

    private void _on_mouse_exited()
    {
        mouse_in = false;
    }


}


