using Godot;
using Godot.Collections;

//  攻方要等守方动画完成后才能回到攻击调度，守方动画完成后自己回到攻击调度

public class 攻击动画 : 游戏阶段
{
    Vector2 attack_vector;
    bool i_finished;
    bool dfnder_finshed;        //  Defender守方结束

    public override void Enter()
    {
        base.Enter();
        GD.Print("攻击动画");

        i_finished = false;
        dfnder_finshed = false;
        game_mnger.tween.Connect("tween_all_completed", this, "_TweenAllCompleted");

        //  若联机且是守方则啥也不干
        if (Global.联机调试 && !IsLocalPlayerActionable()) return;

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


    //-----------------------------------------------------------

    //  动画初始化
    void InitAnimation()
    {
        InitAnimationQ();
        StartAnim();
    }

    void StartAnim()
    {
        attack_vector = (game_mnger.defender.Position - game_mnger.attacker.Position).Normalized();
        game_mnger.tween.InterpolateProperty(game_mnger.attacker, "position",
                                            game_mnger.attacker.Position, game_mnger.attacker.Position + attack_vector * 18,
                                            0.15f, Tween.TransitionType.Quart, Tween.EaseType.Out);
        game_mnger.tween.InterpolateProperty(game_mnger.attacker, "position",
                                            game_mnger.attacker.Position + attack_vector * 18, game_mnger.attacker.Position,
                                            0.2f, Tween.TransitionType.Linear, Tween.EaseType.InOut, 0.15f);
        game_mnger.tween.Start();
        game_mnger.attacker.ZIndex = (int)Piece.ZIndexInStack._act_;
    }

    //  返回攻击调度
    void Back2AttackSchedule()
    {
        game_mnger.attacker.CanAct = false;
        superior.ChangeTo<攻击调度>();
    }

    //--------------------------------------------  信号

    void _TweenAllCompleted()
    {
        i_finished = true;
        game_mnger.pieces_mnger.TopAPiece(game_mnger.attacker);

        if (Global.联机调试)
        {
            //  守方
            if (!IsLocalPlayerActionable())
            {
                AnimationFinishedQ();
                Back2AttackSchedule();
                return;
            }
            //  攻方
            if (dfnder_finshed)      //  一定要等守方完成动画后才结束！
            {
                Back2AttackSchedule();
            }
            return;
        }
        //  单机
        Back2AttackSchedule();
    }

    //————————————————————————————————————————————————————————————————————  网络

    //  RPC分发
    protected override void _RPC(int id, Dictionary content)
    {
        if (!content.Contains("func") || !content.Contains("params")) return;
        Array _params = content["params"] as Array;

        switch (content["func"])
        {
            case "InitAnimationA":
                InitAnimationA(id, _params);
                break;
            case "AnimationFinishedA":
                AnimationFinishedA();
                break;
            case "Back2AttackScheduleA":
                Back2AttackScheduleA();
                break;
            default:
                GD.PrintErr("攻击动画：没有找到函数：", content["func"]);
                break;
        }
    }

    //  通知动画初始化
    void InitAnimationQ()
    {
        //  参数：攻击者id，守者id，计算结果……
        Array _params = new Array();
        _params.Add(game_mnger.attacker.id);
        _params.Add(game_mnger.defender.id);
        //+++++++++++++++++++++
        game_mnger.Send(Global.opposite_player_peer_id, NetworkMnger.Data2JSON("InitAnimationA", _params));
    }

    //  收到通知动画初始化。这里才确定攻守的2个棋子
    void InitAnimationA(int id, Array _params)
    {
        if (_params == null || _params.Count != 2)
        {
            GD.PrintErr("InitAnimationA参数不合");
            return;
        }

        if (_params[0] is float p_id0)
        {
            Piece p = game_mnger.pieces_mnger.GetPiece((uint)p_id0);
            if (p == null)
            {
                GD.PrintErr("棋子id错误！"); return;
            }
            game_mnger.attacker = p;
        }

        if (_params[1] is float p_id1)
        {
            Piece p = game_mnger.pieces_mnger.GetPiece((uint)p_id1);
            if (p == null)
            {
                GD.PrintErr("棋子id错误！"); return;
            }
            game_mnger.defender = p;
        }
        GD.Print("攻守的棋子id: ", game_mnger.attacker.id, " ", game_mnger.attacker.HexPos, " | ", game_mnger.defender.id, " ", game_mnger.defender.HexPos);

        StartAnim();
    }

    //  通知攻方动画完成，守方使用
    void AnimationFinishedQ()
    {
        game_mnger.Send(Global.opposite_player_peer_id, NetworkMnger.Data2JSON("AnimationFinishedA", null));
    }

    //  收到守方动画完成，攻方使用
    void AnimationFinishedA()
    {
        dfnder_finshed = true;
        if (i_finished) Back2AttackSchedule();
    }

    //  通知守方返回攻击调度，攻方使用
    void Back2AttackScheduleQ()
    {
        game_mnger.Send(Global.opposite_player_peer_id, NetworkMnger.Data2JSON("Back2AttackScheduleA", null));
    }

    //  收到攻方通知返回攻击调度
    void Back2AttackScheduleA()
    {
        Back2AttackSchedule();
    }

}