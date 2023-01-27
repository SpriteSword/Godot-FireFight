using Godot;
using Godot.Collections;

//  行动方等待动画完成，另一方判断是否进行临机射击，临机射击完成后同步结果

public class 移动动画 : 游戏阶段
{
    Vector2 next_cell_pos;
    Vector2 next_pos;
    bool anim_finished;     //  担心对方已完成了我还没完成
    ///+++++++++++++++++++++++++++++++++++++++++++++临机射击被击毁又如何？

    public override void Enter()
    {
        base.Enter();
        GD.Print("移动动画");

        game_mnger.tween.Connect("tween_all_completed", this, "_TweenAllCompleted");

        if (Global.联机调试)
        {
            //  非移动方
            if (!IsLocalPlayerActionable()) return;
        }

        InitAnimation();
    }
    public override void UpdatePhysicsProcess(float delta) { }
    public override void UpdateProcess(float delta) { }
    public override void HandleInput(InputEvent _event) { }
    public override void HandleUnhandledInput(InputEvent _event) { }
    public override void Exit()
    {
        base.Exit();
        game_mnger.tween.Disconnect("tween_all_completed", this, "_TweenAllCompleted");
    }


    //-------------------------------------------------------------------------

    //  动画初始化
    void InitAnimation()
    {
        int ind = game_mnger.path_node_index;
        int c = game_mnger.path.Count;
        if (ind + 1 >= c)
        {
            Back2MoveStageQ();
            game_mnger.mover.CanAct = false;
            superior.ChangeTo<移动阶段>();
            return;
        }
        game_mnger.path_node_index++;
        next_cell_pos = game_mnger.path[game_mnger.path_node_index];
        next_pos = game_mnger.mark.HexGridCenter(next_cell_pos);
        game_mnger.mover.ZIndex = (int)Piece.ZIndexInStack._act_;       //  在动画期间调为2，使其高于所有棋子。进入堆叠后会调回1。
        game_mnger.pieces_mnger.RemovePieceFromStack(game_mnger.mover);

        //  同步下一位置
        SynPiecePosQ();     //  反正没联机也不会发出去

        anim_finished = false;
        game_mnger.tween.InterpolateProperty(game_mnger.mover, "position", game_mnger.mover.Position, next_pos, 0.2f);
        game_mnger.tween.Start();
    }

    //  临机询问发送给询问框。若本地已经结束阶段了，不需要临机询问！
    void PromptInquire()        //++++++++++++++++++++++++++++=我已经结束阶段了，不需要临机询问！
    {
        if (Global.联机调试)
        {
            if (IsLocalPlayerEndStage()) DecideNoShot();
            else Inquire("是否进行临机射击？", IC._i_临机射击);
        }
        else
        {
            if (IsNoOneEndStage()) Inquire("是否进行临机射击？", IC._i_临机射击);
            else DecideNoShot();
        }
    }

    //  决定不进行临机射击
    void DecideNoShot()
    {
        if (!Global.联机调试)
        {
            InitAnimation();
            return;
        }
        NoShotQ();
    }
    //  决定进行临机射击
    void Decide2Shoot()
    {
        superior.ChangeTo<临机射击>();
        YesShotQ();
    }

    //------------------------------------------------------------
    //  临机射击询问Yes。询问框返回的
    public override void InquiryBoxYes(IC command)
    {
        switch (command)
        {
            case IC._i_临机射击:
                Decide2Shoot();
                break;
        }
    }
    //  临机射击询问No
    public override void InquiryBoxNo(IC command)
    {
        switch (command)
        {
            case IC._i_临机射击:
                DecideNoShot();
                break;
        }
    }

    //————————————————————————————————————————————————————————————————————————————————————————————————————  信号

    //  补间动画完成
    void _TweenAllCompleted()
    {
        anim_finished = true;
        game_mnger.mover.CellPos = next_cell_pos;
        game_mnger.pieces_mnger.AddPieceInStack(game_mnger.mover);        //  z index 自动为1。

        if (!Global.联机调试)
        {
            PromptInquire();
            return;
        }

        //  非移动方进行临机询问。如果本地玩家已经结束阶段了，就不必询问了
        if (!IsLocalPlayerActionable())
        {
            PromptInquire();
            return;
        }
        //  移动方
        GD.Print("等待对方确定是否临机射击...");

    }



    //————————————————————————————————————————————————————————————————————————————————————————————————————  网络

    //  RPC分发
    protected override void _RPC(int id, Dictionary content)
    {
        if (!content.Contains("func") || !content.Contains("params")) return;
        Array _params = content["params"] as Array;

        switch (content["func"])
        {
            case "SynPiecePosA":
                SynPiecePosA(id, _params);
                break;
            case "NoShotA":
                NoShotA();
                break;
            case "YesShotA":
                YesShotA();
                break;
            case "Back2MoveStageA":
                Back2MoveStageA();
                break;
            default:
                GD.PrintErr("移动动画：没有找到函数：", content["func"]);
                break;
        }
    }

    //  同步下一位置，发送 Q
    void SynPiecePosQ()
    {
        Array _params = new Array();
        _params.Add(next_cell_pos.x);
        _params.Add(next_cell_pos.y);
        game_mnger.Send(Global.opposite_player_peer_id, NetworkMnger.Data2JSON("SynPiecePosA", _params));
    }

    //  同步下一位置，RPC，接收 A。_params[x, y]
    void SynPiecePosA(int id, Array _params)
    {
        if (_params == null || _params.Count != 2) return;

        next_cell_pos.x = (float)_params[0];
        next_cell_pos.y = (float)_params[1];
        next_pos = game_mnger.mark.HexGridCenter(next_cell_pos);

        game_mnger.tween.InterpolateProperty(game_mnger.mover, "position", game_mnger.mover.Position, next_pos, 0.2f);
        game_mnger.tween.Start();
    }

    //  告知移动方不进行临机射击
    void NoShotQ()
    {
        game_mnger.Send(Global.opposite_player_peer_id, NetworkMnger.Data2JSON("NoShotA", null));
    }

    //  移动方收到不进行临机射击
    void NoShotA()
    {
        InitAnimation();
    }

    //  告知对方进行临机射击
    void YesShotQ()
    {
        game_mnger.Send(Global.opposite_player_peer_id, NetworkMnger.Data2JSON("YesShotA", null));
    }

    //  收到对方进行临机射击
    void YesShotA()
    {
        superior.ChangeTo<临机射击>();
    }

    //  告知非移动方回到移动阶段
    void Back2MoveStageQ()       //  move 名词
    {
        game_mnger.Send(Global.opposite_player_peer_id, NetworkMnger.Data2JSON("Back2MoveStageA", null));
    }

    void Back2MoveStageA()
    {
        game_mnger.mover.CanAct = false;
        superior.ChangeTo<移动阶段>();
    }
}