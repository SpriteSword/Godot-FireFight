using Godot;
using Godot.Collections;


public class 移动阶段 : 游戏阶段
{
    public const GameMnger.Stage stage_index = GameMnger.Stage.移动;

    bool start = false;


    public override void Enter()
    {
        base.Enter();
        GD.Print("移动阶段");
        game_mnger.CurrentStageIndex = stage_index;       //  是游戏规则里的都要写！

        DeselectPiece();

        if (!start)
        {
            start = true;       //  ++++++++++在转入游戏下一阶段时设置为false
            game_mnger.end_stage_side = GameMnger.Side.无;
            //++++++++++++++++++++++++++++将所有棋子的状态恢复

            //  主机确定先手并发送给客户端
            if (Global.联机调试)
            {
                if (game_mnger.IsAsServer())
                {
                    DetermineFirstPlayer();
                    SynActiveSideQ();
                }
            }
            else { DetermineFirstPlayer(); }
        }
    }
    public override void UpdatePhysicsProcess(float delta) { }
    public override void UpdateProcess(float delta) { }
    public override void HandleInput(InputEvent _event) { }
    public override void HandleUnhandledInput(InputEvent _event)
    {
        if (_event is InputEventMouse)
        {
            mouse_pos = game_mnger.GetGlobalMousePosition();

            UpdatePath();

            if (_event is InputEventMouseButton)        //  鼠标按键
            {
                if (_event.IsActionPressed("click_left"))
                {
                }
                else if (_event.IsActionReleased("click_left"))
                {
                    HandleSelectOwnSide();
                }
                else if (_event.IsActionPressed("click_right"))
                {
                    HandleSelectTarget();
                }
            }
        } //  鼠标InputEventMouse
    }
    public override void Exit()
    {
        base.Exit();

        ClearPiecesSelected();
        DrawPathLine(false);
    }

    //————————————————————————————————————————————————————————————————————————
    #region 规划路径
    //  处理选择己方单位
    void HandleSelectOwnSide()
    {
        ClearPiecesSelected();
        game_mnger.path.Clear();

        var stack = game_mnger.pieces_mnger.GetPieceStackByRectPos(mouse_pos);
        if (stack != null)
        {
            Piece piece = stack.GetTopPiece();

            //  GUI显示信息+++++++++++敌我双方信息都显示
            GD.Print("棋子能否行动：", piece.CanAct);

            //  如果是本地玩家棋子，则进行下去，画路径线
            if ((!Global.联机调试 && game_mnger.ActionableSide == stack.side) || (Global.联机调试 && piece.side == game_mnger.local_player_side))
            {
                byte ind = 0;

                if (piece.CanAct && !piece.BeKM && !piece.BeK)      //++++++++++++++++++++++++beK不用判断。步兵被压制也不能移动
                {
                    GD.Print("选择：", piece, " ", piece.id, " s: ", piece.side);

                    game_mnger.pieces_selected.Add(piece);
                    game_mnger.path.Add(piece.CellPos);
                    ind = 1;

                    MarkPiecesSelected();
                    DrawPathLine(true);
                }

                game_mnger.gui.piece_info_bar.UpdateListByPieceStack(stack, ind);
                return;
            }
        }

        //  凡是没选到的都执行
        DeselectPiece();
        game_mnger.GrayPieceInfoBar();
    }

    //  处理确定移动目标
    void HandleSelectTarget()
    {
        if (Global.联机调试)
        {
            if (!IsLocalPlayerActionable()) return;
        }

        // if (game_mnger.local_player_side != game_mnger.ActiveSide) return;      //  可不写？
        if (game_mnger.pieces_selected.Count != 1) return;

        //  如果路径不合格
        if (!PathIsQualified())
        {
            Warn("路径错误！");
            DeselectPiece();
            return;
        }

        game_mnger.mover = game_mnger.pieces_selected[0];
        game_mnger.path_node_index = 0;

        EnterAnimationQ();      //  告诉对面也进入动画
        superior.ChangeTo<移动动画>();
    }

    //  更新路径线
    void UpdatePath()
    {
        if (game_mnger.pieces_selected.Count != 1) return;      //+++++++++++++++++

        Vector2 mou_pos_cell = game_mnger.mark.WorldToMap(mouse_pos);
        int ind = game_mnger.path.IndexOf(mou_pos_cell);
        if (ind >= 0)
        {
            game_mnger.path.Resize(ind);
        }
        game_mnger.path.Add(mou_pos_cell);

        // DrawPathLine(true);
        // GD.Print(game_mnger.path);

    }

    //  检查路径是否合格
    bool PathIsQualified()
    {
        //  检查起点。不用检查 棋子==null 了
        if (game_mnger.pieces_selected[0].CellPos != game_mnger.path[0]) return false;      //+++++++++++++

        //  检查中间点是否都连贯
        int c = game_mnger.path.Count;
        for (int i = 0; i < c; i++)
        {
            int n = i + 1;
            if (n >= c) break;

            // GD.Print("cell: ", game_mnger.path[n], " hex: ", Math.Cell2HexCoord(game_mnger.path[n]));

            Vector2 d = Math.Cell2HexCoord(game_mnger.path[n]) - Math.Cell2HexCoord(game_mnger.path[i]);

            if (d == Vector2.Right || d == Vector2.One || d == Vector2.Down ||
                d == Vector2.Left || d == -Vector2.One || d == Vector2.Up) { }
            else return false;
        }
        return true;
    }

    //-----------------------------------------------
    //  画路径线
    void DrawPathLine(bool draw)
    {
        game_mnger.mark.draw_path_line = draw;
    }

    #endregion

    //  取消选择棋子
    void DeselectPiece()
    {
        ClearPiecesSelected();
        game_mnger.path.Clear();
        DrawPathLine(false);
    }

    #region ——————————————————————————————————————————————————————————————  GUI

    //  处理键盘输入。game_mnger调用
    public override void HandleInputKey(uint scancode)
    {
        switch (scancode)
        {
            case (uint)KeyList.F5:      //  F5 结束行动，不停摁也只是更新一下文字而已
                InquireEndAction();
                break;
            case (uint)KeyList.F6:      //  F6 结束本阶段
                InquireEndStage();
                break;
        }
    }


    //--------------------------------------------------  棋子信息卡
    //  棋子信息卡被关闭。由game_mnger调用
    public override void PieceInfoCardClosed(Piece p_selected)
    {
        DeselectPiece();
    }

    //  棋子信息卡被选。由game_mnger调用
    public override void PieceInfoCardSelected(Piece p_selected)
    {
        if (game_mnger.ActionableSide == p_selected.side && p_selected.CanAct)
        {
            ClearPiecesSelected();
            game_mnger.pieces_selected.Add(p_selected);
            game_mnger.pieces_mnger.TopAPiece(p_selected);
            MarkPiecesSelected();
            game_mnger.path.Clear();
            game_mnger.path.Add(p_selected.CellPos);
            DrawPathLine(true);
            GD.Print("选择：", p_selected, " ", p_selected.id, " s: ", p_selected.side);
            return;
        }
        DeselectPiece();
    }

    //----------------------------------------------------------  交互

    //  临机射击询问Yes。询问框返回的
    public override void InquiryBoxYes(IC command)
    {
        switch (command)
        {
            case IC._i_结束行动:
                EndAction();
                break;
            case IC._i_结束阶段:
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

    //  进入下一阶段
    protected override void EnterNextStage()
    {
        superior.ChangeTo<间射阶段>();
        start = false;
    }

    //  清理gui操作残留
    protected override void ClearOperationResidues()
    {
        DeselectPiece();
    }

    #endregion

    //++++++++++++++++++++++++++++++++++结束回合


    //————————————————————————————————————————————————————————————————————————————————————————————————————————————  网络
    //  RPC分发
    protected override void _RPC(int id, Dictionary content)
    {
        if (!content.Contains("func") || !content.Contains("params")) return;
        Array _params = content["params"] as Array;

        switch (content["func"])
        {
            case "EnterAnimationA":
                EnterAnimationA(id, _params);       //  进入动画
                break;

            case "SynActiveSideA":
                SynActiveSideA(_params);        //  继承自父类
                break;
            case "EnterNextStageA":
                EnterNextStageA();      //  进入下阶段，继承
                break;
            case "SynEndStageSideA":
                SynEndStageSideA(_params);      // 同步结束阶段的标记， 继承
                break;
            default:
                GD.PrintErr("移动阶段：没有找到函数：", content["func"]);
                break;
        }
    }

    //  告诉对方进入动画，Q & A 问者 与 答者
    void EnterAnimationQ()
    {
        Array _params = new Array();
        _params.Add(game_mnger.mover.id);       //  移动者的id

        game_mnger.Send(Global.opposite_player_peer_id, NetworkMnger.Data2JSON("EnterAnimationA", _params));        //  传Vector会被解析成string！！
    }

    //  收到进入动画的通知。A 回应者执行
    void EnterAnimationA(int id, Array _params)
    {
        if (_params == null) return;
        if (_params.Count != 1) return;

        if (_params[0] is float mover_id)
        {
            Piece p = game_mnger.pieces_mnger.GetPiece((uint)mover_id);
            if (p == null)
            {
                GD.PrintErr("移动阶段：EnterAnimationA：棋子id错误！");
                return;
            }

            GD.Print("移动的棋子id: ", p.id, " ", p.HexPos);
            game_mnger.mover = p;
        }

        game_mnger.path_node_index = 0;
        superior.ChangeTo<移动动画>();
    }
}