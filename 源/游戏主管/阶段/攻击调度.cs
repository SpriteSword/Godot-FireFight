using Godot;
using Godot.Collections;

//  攻守方同时进入，同时退出攻击调度
//  判断守方棋子能否被攻击，攻击动画时才判断攻方棋子能否攻击

public class 攻击调度 : 游戏阶段
{
    bool been_attacked = false;      //  守方有无被攻击过





    public override void Enter()
    {
        base.Enter();
        GD.Print("攻击调度");

        //  联机且是守方则啥也不干
        if (Global.联机调试 && !IsLocalPlayerActionable()) return;

        //  如果对方棋子不能被射击
        if (!game_mnger.defender.can_be_shot)
        {
            //  警告
            Warn("警告！目标不能被射击！");
            Back2ShotStage();
            FinishQ();
            return;
        }

        //  执行攻击调度。如果转场了，就return true，就不往下执行。
        if (ConductAttackScheduling()) { return; }


        //  全部攻击完
        FinishQ();      //++++++++++++++++++++++++++++++处理攻击完棋子结果
        Back2ShotStage();
    }
    public override void UpdatePhysicsProcess(float delta) { }
    public override void UpdateProcess(float delta) { }
    public override void HandleInput(InputEvent _event) { }
    public override void HandleUnhandledInput(InputEvent _event) { }
    public override void Exit()
    {
        base.Exit();        //  子类必须执行
    }

    //——————————————————————————————————————————————————————————————————————————
    //  回到攻击阶段
    void Back2ShotStage()
    {
        if (been_attacked)
            game_mnger.defender.can_be_shot = !been_attacked;
        been_attacked = false;

        if (game_mnger.CurrentStageIndex == GameMnger.Stage.直射)
            superior.ChangeTo<直射阶段>();
        else
            superior.ChangeTo<临机射击>();
    }


    #region 玩法

    //  执行攻击调度。返回是否转场到 攻击动画。没执行完攻击都会转场，执行完所有攻击了就返回到Enter()执行退出攻击调度。
    bool ConductAttackScheduling()
    {
        int c = game_mnger.attackers.Count;
        while (c > 0)       //  让可以攻击的棋子攻击。退出循环就是数组空了
        {
            Piece piece = game_mnger.attackers[c - 1];
            //  能否行动
            if (!piece.CanAct || piece.BeK || piece.BeKF)
            {
                Warn("警告！" + piece.id.ToString() + "不能执行任务！");        //  只给攻击方看到
                goto CanNotShoot;
            }

            //  满足条件可以攻击
            game_mnger.attacker = piece;
            game_mnger.attackers.RemoveAt(c - 1);       //  不用管c了，反正return
            been_attacked = true;
            EnterAttackAnimationQ();
            superior.ChangeTo<攻击动画>();
            return true;

        CanNotShoot:        //  正常不会执行到这里
            game_mnger.attackers.RemoveAt(c - 1);
            c = game_mnger.attackers.Count;
        }
        return false;
    }

    #endregion

    //——————————————————————————————————————————————————————————————————————————  网络

    //  RPC分发
    protected override void _RPC(int id, Dictionary content)
    {
        if (!content.Contains("func") || !content.Contains("params")) return;
        Array _params = content["params"] as Array;

        switch (content["func"])
        {
            case "EnterAttackAnimationA":
                EnterAttackAnimationA();
                break;
            case "FinishA":
                FinishA();
                break;
            default:
                GD.PrintErr("攻击调度：没有找到函数：", content["func"]);
                break;
        }
    }

    //  通知进入攻击动画
    void EnterAttackAnimationQ()
    {
        game_mnger.Send(Global.opposite_player_peer_id, NetworkMnger.Data2JSON("EnterAttackAnimationA", null));
    }

    //  收到通知进入攻击动画
    void EnterAttackAnimationA()
    {
        been_attacked = true;
        superior.ChangeTo<攻击动画>();
    }

    //  通知结束
    void FinishQ()
    {
        game_mnger.Send(Global.opposite_player_peer_id, NetworkMnger.Data2JSON("FinishA", null));
    }

    //  收到通知结束
    void FinishA()
    {
        Back2ShotStage();
    }

}
