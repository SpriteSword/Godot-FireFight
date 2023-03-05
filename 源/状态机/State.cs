using Godot;
using Godot.Collections;

public abstract class State : Node
{
    //  不是原始的SM ！！！
    public GameMngerStateMachine superior = null;      //  统一由父节点初始化！！

    // public virtual void Enter(params object[] args) { GD.Print("基类.Enter(params object[] args)"); }
    public virtual void Enter() { }
    public virtual void UpdatePhysicsProcess(float delta) { }
    public virtual void UpdateProcess(float delta) { }
    public virtual void HandleInput(InputEvent _event) { }
    public virtual void HandleUnhandledInput(InputEvent _event) { }
    public virtual void Exit() { }
}


public abstract class 游戏阶段 : State
{
    public GameMnger game_mnger;

    protected Vector2 mouse_pos;

    public override void Enter()        //  子类必须执行
    {
        Connect2NetworkHandler();
    }
    public override void UpdatePhysicsProcess(float delta) { }
    public override void UpdateProcess(float delta) { }
    public override void HandleInput(InputEvent _event) { }
    public override void HandleUnhandledInput(InputEvent _event) { }

    public override void Exit()     //  子类必须执行
    {
        DisconnectNetworkHandler();
    }

    //————————————————————————————————————————————————————————————————————————  信号

    //  远程调用，由NetworkHandler信号传来
    protected virtual void _RPC(int id, Dictionary content) { }


    //———————————————————————————————————————————————————————————————————————— GUI
    #region 面板

    //  input event 输入事件，game_mnger调用.
    public virtual void HandleInputKey(uint scancode) { }     //  键盘。鼠标太复杂了不用集到一起了

    //  询问框传信号到game_mnger，game_mnger再调用
    public virtual void InquiryBoxYes(IC command) { }
    public virtual void InquiryBoxNo(IC command) { }
    protected void Inquire(string question, IC command)      //  询问
    {
        game_mnger.EmitSignal("Inquire", question, command);
    }

    //  警告框
    protected void Warn(string text)
    {
        game_mnger.EmitSignal("Warn", text);
    }

    //  棋子信息栏中的棋子信息卡被关闭。由game_mnger调用
    public virtual void PieceInfoCardClosed(Piece p_selected) { }

    //  处理棋子信息栏中的信息卡被选，关联到棋子。由game_mnger调用
    public virtual void PieceInfoCardSelected(Piece p_selected) { }

    //  清理gui操作残留
    protected virtual void ClearOperationResidues() { }

    #endregion


    #region 地图

    //  清除pieces_selected数组
    protected void ClearPiecesSelected()
    {
        game_mnger.pieces_mnger.Clear();
        game_mnger.pieces_selected.Clear();
    }

    //  添加棋子被选到时的标记，标记在所在格。pieces_mnger绘制，图层在比棋子低，比地图高。
    public void MarkPiecesSelected()
    {
        game_mnger.pieces_mnger.Clear();

        foreach (Piece p in game_mnger.pieces_selected)
        {
            game_mnger.pieces_mnger.SetCellv(p.CellPos, 1);
        }
    }

    #endregion

    //————————————————————————————————————————————————————————————————————————  控制
    //  确定先手，主机一方确定
    protected void DetermineFirstPlayer()
    {
        if (Math.ThrowDice() % 2 == 0)
        {
            game_mnger.ActionableSide = GameMnger.Side.红;
            return;
        }
        game_mnger.ActionableSide = GameMnger.Side.蓝;
    }

    //  红蓝对调
    public static GameMnger.Side SwapRedBlue(GameMnger.Side curr_active_side)
    {
        return (curr_active_side == GameMnger.Side.红) ? GameMnger.Side.蓝 : GameMnger.Side.红;
    }


    #region 结束行动/阶段

    //  是否仍然没人结束阶段
    protected bool IsNoOneEndStage()
    {
        return game_mnger.end_stage_side == GameMnger.Side.无;
    }

    //  是否是本地玩家回合(拥有行动权)
    protected bool IsLocalPlayerActionable()
    {
        return game_mnger.ActionableSide == game_mnger.local_player_side;
    }

    //  是否是本地玩家结束了本阶段
    protected bool IsLocalPlayerEndStage()
    {
        return game_mnger.end_stage_side == game_mnger.local_player_side;
    }

    //  询问是否结束行动
    protected virtual void InquireEndAction()
    {
        //  1、联机时不是行动方；2、对方已结束了 不会询问结束行动
        if (Global.联机调试 && !IsLocalPlayerActionable()) return;
        if (!IsNoOneEndStage()) return;

        Inquire("是否结束本次行动？", IC._i_结束行动);
    }

    //  结束行动。简单对调一下行动方就好
    protected virtual void EndAction()
    {
        ClearOperationResidues();
        game_mnger.ActionableSide = SwapRedBlue(game_mnger.ActionableSide);     //+++++++++++++++++++++++gui操作的残留清理！
        SynActiveSideQ();
    }

    //  尝试结束阶段。除 临机射击 不能结束阶段
    protected virtual void InquireEndStage()
    {
        //  联机时不是行动方不能结束行动
        if (Global.联机调试 && !IsLocalPlayerActionable()) return;
        Inquire("是否结束本阶段？当对手也结束后，将会进入下一阶段。", IC._i_结束阶段);
    }

    //  结束本阶段，同步end_stage_side
    protected virtual void EndStage()
    {
        ClearOperationResidues();

        if (IsNoOneEndStage())
        {
            game_mnger.end_stage_side = game_mnger.ActionableSide;
            SynEndStageSideQ(game_mnger.ActionableSide);

            game_mnger.ActionableSide = SwapRedBlue(game_mnger.ActionableSide);
            SynActiveSideQ();
            return;
        }
        //  对方已结束本阶段了
        game_mnger.Send(Global.opposite_player_peer_id, NetworkMnger.Data2JSON("EnterNextStageA", null));
        EnterNextStage();
    }

    //  进入下一阶段。具体执行
    protected virtual void EnterNextStage() { }
    #endregion



    //————————————————————————————————————————————————————————————————————————————————  网络。记得在子类的switch里写！

    //  连接网络处理节点
    void Connect2NetworkHandler()
    {
        game_mnger.network_handler.Connect("Give2External", this, "_RPC");
    }

    //  与网络处理节点断开
    void DisconnectNetworkHandler()
    {
        game_mnger.network_handler.Disconnect("Give2External", this, "_RPC");
    }


    //  同步当前行动方。确定先手时由服务器发送
    protected void SynActiveSideQ()
    {
        Array _params = new Array();
        _params.Add(game_mnger.ActionableSide);
        game_mnger.Send(Global.opposite_player_peer_id, NetworkMnger.Data2JSON("SynActiveSideA", _params));
    }

    //  同步当前行动方，RPC。确定先手时客户端执行
    protected virtual void SynActiveSideA(Array _params)
    {
        if (_params == null) return;
        if (_params.Count == 0) return;

        if (_params[0] is float side)       //  所有数字都是float！
        {
            game_mnger.ActionableSide = (GameMnger.Side)side;
            GD.Print("client: active_side: ", game_mnger.ActionableSide);
        }
    }

    //  结束行动
    protected virtual void EndActionA() { }     //  临机射击才用

    //  收到 结束本阶段，进入下阶段
    protected virtual void EnterNextStageA()
    {
        EnterNextStage();
    }


    #region 同步 结束阶段的标记 Q A game_mnger.end_stage_side
    protected void SynEndStageSideQ(GameMnger.Side side)
    {
        Array _params = new Array();
        _params.Add(side);
        game_mnger.Send(Global.opposite_player_peer_id, NetworkMnger.Data2JSON("SynEndStageSideA", _params));
    }
    protected void SynEndStageSideA(Array _params)
    {
        if (_params == null) return;
        if (_params.Count != 1) return;

        if (_params[0] is float side)       //  所有数字都是float！
        {
            game_mnger.end_stage_side = (GameMnger.Side)side;
            GD.Print("end_stage_side: ", game_mnger.end_stage_side);
        }
    }
    #endregion

}
