using Godot;


//  在实例化时，会由棋子部署系统 增加一个PieceSprite子节点实例
public class 预备部署棋子 : PieceCard
{
	[Signal] delegate void SelectMe(预备部署棋子 me, Vector2 mouse_offset);


	public uint piece_id;       //  在GameMnger的棋子信息的索引。文件-> 读入，生成id -> 一览表 -> 地图上棋子


    protected override void EmitSignalSelectMe()
    {
		EmitSignal("SelectMe", this, GetLocalMousePosition());
    }
}


