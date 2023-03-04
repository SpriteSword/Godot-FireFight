using Godot;
using Godot.Collections;

public class 直射阶段 : 射击阶段Base
{
    public const GameMnger.Stage stage_index = GameMnger.Stage.直射;
    //-----------------------------------------------

    bool start = false;


    public override void Enter()
    {
        GD.Print("直射阶段");
        base.Enter();
        game_mnger.CurrentStageIndex = stage_index;       //  都要写！

        if (!start)
        {
            start = true;       //  ++++++++++在转入游戏下一阶段时设置为false
            game_mnger.end_stage_side = GameMnger.Side.无;
            //++++++++++++++++++++++++++++将所有棋子的状态恢复

            //  主机确定先手并发送给客户端
            if (Global.联机调试)
            {
                if (game_mnger.IsAsServer())        //  不是服务器啥也不干
                {
                    DetermineFirstPlayer();
                    SynActiveSideQ();
                    own_side = game_mnger.ActionableSide;
                }
            }
            else
            {
                DetermineFirstPlayer();
                own_side = game_mnger.ActionableSide;
            }
        }
    }
    // public override void Exit() { base.Exit(); }     //  可不写

    //————————————————————————————————————————————————————————————————————————————

    //  临机射击询问Yes。询问框返回的
    public override void InquiryBoxYes(IC command)
    {
        switch (command)
        {
            case IC._i_结束行动:
                EndAction();
                break;
            case IC._i_结束阶段:        //++++++++++++++++++++++++++++++++++++++++++若对方也结束，则双方都进入下一阶段；若对方还没，则这个阶段就不能行动了
                EndStage();
                break;
        }
    }

    //  临机射击询问No
    public override void InquiryBoxNo(IC command)
    {
        switch (command)
        {
            case IC._i_结束行动:
                break;
            case IC._i_结束阶段:
                break;
        }
    }

    //  结束行动，end动词。直接对调并同步一下行动方就好了。
    protected override void EndAction()
    {
        base.EndAction();
        own_side = game_mnger.ActionableSide;
    }

    //  结束本阶段，同步end_stage_side
    protected override void EndStage()
    {
        base.EndStage();
        own_side = game_mnger.ActionableSide;
    }

    protected override void EnterNextStage()
    {
        superior.ChangeTo<移动阶段>();
        start = false;
    }



    //——————————————————————————————————————————————————————————————————————  网络

    //  收到 同步活动方
    protected override void SynActiveSideA(Array _params)
    {
        base.SynActiveSideA(_params);
        own_side = game_mnger.ActionableSide;
    }


}