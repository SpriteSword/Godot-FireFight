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
    public override void HandleUnhandledInput(InputEvent _event)
    {
        if (_event is InputEventMouse)
        {
            mouse_pos = game_mnger.GetGlobalMousePosition();

            UpdatePath();

            if (_event is InputEventMouseButton)        //  鼠标按键
            {
                if (_event.IsActionPressed("click_left")) { }
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

    #region ————————————————————————————————————————————————————————————————  规划路径
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

            //  如果是本地玩家棋子，则进行下去，画路径线
            if ((!Global.联机调试 && game_mnger.ActionableSide == stack.side) || (Global.联机调试 && piece.side == game_mnger.local_player_side))
            {
                byte ind = 0;       //  信息栏的参数

                if (piece.CanAct && !piece.BeKM && !piece.BeK)      //++++++++++++++++++++++++beK不用判断。步兵被压制也不能移动
                {
                    GD.Print("选择：", piece, " ", piece.id, " s: ", piece.side);

                    game_mnger.pieces_selected.Add(piece);

                    PathPoint pp = new PathPoint(piece.CellPos, 0);
                    game_mnger.path.Add(pp);
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
            Warn("路径不合格！");
            DeselectPiece();
            return;
        }

        //  裁剪掉移动点数<0的点
        for (int i = 0; i < game_mnger.path.Count; i++)
        {
            if (game_mnger.path[i].remaining_m_p < 0)
            {
                game_mnger.path.Resize(i);      //  i 已经超前1位了！
            }
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

        Vector2 mou_cell_pos = game_mnger.mark.DetermineCellOfHexGrid(mouse_pos);

        int ind = -1;
        for (int i = 0; i < game_mnger.path.Count; i++)
        {
            if (game_mnger.path[i].cell_pos == mou_cell_pos) { ind = i; break; }
        }
        //  路径中已有
        if (ind >= 0) { game_mnger.path.Resize(ind); }      //  ==0也裁剪掉

        if (game_mnger.path.Count > 0)
        {
            //  地形损耗
            float loss = CalcMPLoss(game_mnger.pieces_selected[0], GetLastPPOf(game_mnger.path).cell_pos, mou_cell_pos);

            game_mnger.path.Add(new PathPoint(mou_cell_pos, GetLastPPOf(game_mnger.path).remaining_m_p - loss));
        }
        else { game_mnger.path.Add(new PathPoint(mou_cell_pos, game_mnger.pieces_selected[0].m_p)); }

    }

    //  检查路径是否合格
    bool PathIsQualified()
    {
        //  检查起点。不用检查 棋子==null 了
        if (game_mnger.pieces_selected[0].CellPos != game_mnger.path[0].cell_pos) return false;

        int c = game_mnger.path.Count;
        for (int i = 0; i < c; i++)
        {
            int n = i + 1;      //  next
            if (n >= c) break;

            Vector2 p_i = game_mnger.path[i].cell_pos;
            Vector2 p_n = game_mnger.path[n].cell_pos;

            //  检查中间点是否都连贯
            // GD.Print("cell: ", game_mnger.path[n], " hex: ", Math.Cell2HexCoord(game_mnger.path[n]));
            Vector2 d = Math.Cell2HexCoord(p_n) - Math.Cell2HexCoord(p_i);
            if (!IsDirectionValid(d)) { return false; }

            //  检查路径上是否有敌人。有的规则允许有敌人。
            var s = game_mnger.pieces_mnger.GetPieceStack(p_n);
            if (s != null)
            {
                if (PiecesMnger.IsAgainst(s.side, game_mnger.pieces_selected[0].side)) return false;
            }

            //++++++++++++++++++++++++++++++=检查路径损耗是否正确
        }
        return true;
    }


    //  计算移动点数损耗。用cell坐标。必须相邻2格。      +++++++++++++++++++++++++++++++++++++++++++++++++
    float CalcMPLoss(Piece mover, Vector2 from, Vector2 to)
    {
        if (mover.type == Piece.PieceType.人) { return 1; }

        //  --------  车
        //  有道路可无视地形
        if (IsWayInterconnected(game_mnger.road, from, to)) { return 0.5f; }        //  公路
        if (IsWayInterconnected(game_mnger.railway, from, to)) { return 1; }        //  铁路

        float loss = (MPLossByGroundFeature(from) + MPLossByGroundFeature(to)) / 2;
        if (IsCrossRiver(game_mnger.river.data, from, to)) { loss += 1; }

        return loss;
    }

    //  检查2个格是否道路互通。map 可以是公路图或铁路图，from、to 用cell坐标。必须是相邻2格的。
    bool IsWayInterconnected(TileMap way_map, Vector2 from, Vector2 to)
    {
        int from_tile = way_map.GetCellv(from);
        int to_tile = way_map.GetCellv(to);

        if (from_tile >= 0 && to_tile >= 0)
        {
            //  检查移动方向在这2格里面有无对应的。不是相邻的都通过不了。
            Vector2 d = Math.Cell2HexCoord(to) - Math.Cell2HexCoord(from);
            if (game_mnger._directions_.Contains(d))
            {
                uint d_ind = (uint)(int)game_mnger._directions_[d];     //  无语了，一次转还不行
                uint n_d_ind = (uint)(int)game_mnger._directions_[-d];       //  负方向
                uint f_ind = game_mnger._road_tile_direction_index_[from_tile];
                uint t_ind = game_mnger._road_tile_direction_index_[to_tile];

                if ((d_ind & f_ind) > 0 && ((n_d_ind) & t_ind) > 0) { return true; }
            }
        }
        return false;
    }

    //  根据方向的索引查找道路tile的索引。没找到返回 -1.
    int GetRoadTileIndexByDirection(int direction_index)
    {
        for (int i = 0; i < game_mnger._road_tile_direction_index_.Count; i++)
        {
            if (game_mnger._road_tile_direction_index_[i] == direction_index) { return i; }
        }
        return -1;
    }

    //  是否过河。都用cell坐标
    bool IsCrossRiver(Array<Array<int>> river, Vector2 from, Vector2 to)
    {
        foreach (var itm in river)
        {
            if (itm.Count == 4)
            {
                Vector2 c1 = new Vector2(itm[0], itm[1]);
                Vector2 c2 = new Vector2(itm[2], itm[3]);
                if ((c1 == from && c2 == to) || (c1 == to && c2 == from)) { return true; }
            }
        }
        return false;
    }

    //  1格的地形对移动点数的损耗
    float MPLossByGroundFeature(Vector2 cell_pos)
    {
        int ind = game_mnger.ground_feature.GetCellv(cell_pos);
        if (ind < 0) return 1;
        else if (ind == 0) return 2;        //  树林
        else if (ind == 1) return 3;       //  城镇
        else return -1;
    }


    //  检查某方向是否是六边形的6个法定方向。hex坐标。
    bool IsDirectionValid(Vector2 hex_coord_direction)
    {
        foreach (Vector2 d in game_mnger._directions_.Keys)
        {
            if (d == hex_coord_direction) return true;
        }
        return false;
    }

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

    //  解除敌我所有棋子的 压制
    void UnmarkAllSuppressed()
    {
        foreach (Piece p in game_mnger.pieces_mnger.pieces.GetChildren())
        {
            p.BeS = false;
        }
    }

    //  返回Array<PathPoint>最后一位，只为了写短一点而已
    public PathPoint GetLastPPOf(Array<PathPoint> array)
    {
        if (array == null || array.Count <= 0) return null;

        return array[array.Count - 1];
    }

    #region ——————————————————————————————————————————————————————————————  交互 控制

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
            game_mnger.path.Add(new PathPoint(p_selected.CellPos, p_selected.m_p));
            DrawPathLine(true);
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

    //  进入下一阶段。结束本阶段。须解除所有棋子的压制标记。
    protected override void EnterNextStage()
    {
        UnmarkAllSuppressed();
        superior.ChangeTo<间射阶段>();
        start = false;
    }

    //  清理gui操作残留
    protected override void ClearOperationResidues()
    {
        DeselectPiece();
    }

    #endregion



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