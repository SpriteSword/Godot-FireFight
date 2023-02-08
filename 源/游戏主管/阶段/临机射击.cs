using Godot;
using Godot.Collections;

//  可以多打一，但被射击者只能被射击一次，射击完成后直接将行动权交还给对方，不用确认。

public class 临机射击 : 射击阶段Base
{
    public const GameMnger.Stage stage_index = GameMnger.Stage.临机射击;


    public override void Enter()
    {
        base.Enter();

        GD.Print("临机射击");
        game_mnger.CurrentStageIndex = stage_index;

        game_mnger.defender = game_mnger.mover;     //  守方一定是移动的棋子

        own_side = SwapRedBlue(game_mnger.mover.side);
        game_mnger.ActionableSide = own_side;       //+++++++++++++++++++++++++++结束了再反回来
    }

    // public override void Exit() { base.Exit(); }     //  可不用写

    //——————————————————————————————————————————————————————————————————————————————————
    //  处理选择正在移动的敌人
    protected override void HandleSelectEnemySide()
    {
        if (Global.联机调试)
        {   //  本地玩家非行动方
            if (!IsLocalPlayerActionable()) return;
        }


        if (game_mnger.pieces_mnger.pieces_focused.Count > 0 && game_mnger.pieces_selected.Count > 0)
        {
            if (!IsSightLineQualified()) { GD.Print("视线不合格"); return; }

            var stack = game_mnger.pieces_mnger.GetPieceStackByRectPos(mouse_pos);

            if (stack.pieces.Contains(game_mnger.mover)) { HandleSelectedEnemySide(game_mnger.mover); }
        }
    }

    //  临机射击询问Yes。询问框返回的
    public override void InquiryBoxYes(IC command)
    {
        switch (command)
        {
            case IC._i_结束行动:        //  放弃射击
                GD.Print("临机的结束行动");
                EndAction();
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
        }
    }

    //  结束行动，end动词。回到对方的移动阶段
    protected override void EndAction()
    {
        game_mnger.Send(Global.opposite_player_peer_id, NetworkMnger.Data2JSON("EndActionA", null));
        game_mnger.ActionableSide = SwapRedBlue(game_mnger.ActionableSide);
        superior.ChangeTo<移动动画>();
    }

    //  尝试结束阶段。除 临机射击 不能结束阶段
    protected override void InquireEndStage() { }




    //——————————————————————————————————————————————————————————————————————————————————————————————————————————  网络

    //  通知进入攻击调度。需要告诉其攻击者的id
    protected override void EnterAttackScheduleQ()
    {
        GD.Print("linji的通知进入调度Q");
        Array _params = new Array();
        foreach (Piece piece in game_mnger.attackers)
        {
            _params.Add(piece.id);
        }
        game_mnger.Send(Global.opposite_player_peer_id, NetworkMnger.Data2JSON("EnterAttackScheduleA", _params));
    }

    //  收到通知进入攻击调度
    protected override void EnterAttackScheduleA(int id, Array _params)
    {
        GD.Print("linji的通知进入调度A");

        if (_params == null) { GD.PrintErr("临机射击：EnterAttackScheduleA：参数不对"); return; }
        foreach (float p_id in _params)
        {
            Piece piece = game_mnger.pieces_mnger.GetPiece((uint)p_id);
            game_mnger.attackers.Add(piece);
        }

        superior.ChangeTo<攻击调度>();
    }

    //  收到结束行动
    protected override void EndActionA()
    {
        game_mnger.ActionableSide = SwapRedBlue(game_mnger.ActionableSide);
        superior.ChangeTo<移动动画>();
    }

}