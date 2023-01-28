using Godot;
using Godot.Collections;

public class Global : Node
{
    public readonly static bool 联机调试 = false;		//  这个纯粹是调试用！

    public static int opposite_player_peer_id;      //  对手的客户端的id



}



//  Interactive Command 交互命令。_i_代表询问，
public enum IC
{
    _none_,
    //  游戏
    结束行动,
    结束阶段,

    _i_结束行动,
    _i_结束阶段,
    _i_临机射击,
    _i_结束想定,

    //  联机
    _i_选择对手,


};//  Interactive Command 交互命令



//————————————————————————————————————————  键盘快捷键
/*
F5  结束小回合
F6  结束大回合

*/