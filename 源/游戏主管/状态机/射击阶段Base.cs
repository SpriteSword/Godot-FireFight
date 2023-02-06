using Godot;
using Godot.Collections;

public class 射击阶段Base : 游戏阶段
{
    //  鼠标位置
    protected Vector2 mouse_old_pos;      //  绘制选框用
    Vector2 mouse_screen_pos;
    //------------------------------------------

    //  在Enter()时确认好双方。
    protected GameMnger.Side own_side;        //  己方，攻方。其实就是game_mnger.ActionableSide，懒得改了
    // protected byte opposite_side;       //  对方，守方


    public override void Enter()
    {
        base.Enter();

        ClearPiecesSelected();
    }
    public override void UpdatePhysicsProcess(float delta) { }
    public override void UpdateProcess(float delta) { }
    public override void HandleInput(InputEvent _event) { }
    public override void HandleUnhandledInput(InputEvent _event)
    {
        if (_event is InputEventMouse)
        {
            mouse_pos = game_mnger.GetGlobalMousePosition();
            mouse_screen_pos = game_mnger.gui.GetGlobalMousePosition();

            if (_event is InputEventMouseButton)        //  鼠标按键
            {
                if (_event.IsActionPressed("click_left"))
                {
                    mouse_old_pos = mouse_pos;
                    DrawSelectBox(true);
                }
                else if (_event.IsActionReleased("click_left"))
                {
                    DrawSelectBox(false);
                    HandleSelectOwnSide();
                }
                else if (_event.IsActionPressed("click_right"))
                {
                    HandleSelectEnemySide();
                }
            }
        }//  InputEventMouse
    }
    public override void Exit()
    {
        base.Exit();

        ClearPiecesSelected();
        DrawSightLine(false);
    }


    //————————————————————————————————————————————————————————————————————
    #region 点选棋子。派生类的主要不同点。
    //  处理选择己方单位。不管是不是本地玩家回合都能选、查看信息
    protected void HandleSelectOwnSide()
    {
        ClearPiecesSelected();
        game_mnger.gui.floating_piece_info_bar.Hide();

        bool box_select = MouseBoxOrClickSelect();

        //  选后
        if (game_mnger.pieces_selected.Count > 0)
        {
            GD.Print("选择：", game_mnger.pieces_selected);
            MarkPiecesSelected();       //   标记选择棋子所在格

            //  GUI显示信息
            if (box_select) game_mnger.gui.piece_info_bar.UpdateList(game_mnger.pieces_selected);
            else game_mnger.gui.piece_info_bar.UpdateListByPieceStack(game_mnger.stack_selected, 1);

            DrawSightLine(true);
            return;
        }

        game_mnger.GrayPieceInfoBar();
        DrawSightLine(false);
    }

    //  处理选择敌人。自input event
    protected virtual void HandleSelectEnemySide()
    {
        if (Global.联机调试)
        {   //  本地玩家非行动方
            if (!IsLocalPlayerActionable()) {GD.Print("非本地玩家回合");return;}
        }

        if (game_mnger.pieces_mnger.pieces_focused.Count > 0 && game_mnger.pieces_selected.Count > 0)       //  pieces_focused 是敌方目标，pieces_selected是己方
        {
            var stack = game_mnger.pieces_mnger.GetPieceStackByRectPos(mouse_pos);

            if (stack != null && PiecesMnger.IsAgainst(stack.side, own_side))
            {
                if (stack.pieces.Count > 1)        //  棋子数量大于1，则显示悬浮棋子信息栏来选择棋子
                {
                    game_mnger.gui.floating_piece_info_bar.Show(mouse_screen_pos, stack);
                }
                else
                {
                    var enemy = stack.GetTopPiece();        //  有堆叠必有棋子
                    if (PiecesMnger.IsAgainst(enemy.side, own_side)) { HandleSelectedEnemySide(enemy); }
                }
            }
        }
    }

    //  选完后该干嘛
    protected void HandleSelectedEnemySide(Piece enemy)
    {
        game_mnger.attackers.Clear();
        game_mnger.attackers = game_mnger.pieces_selected.Duplicate(true);     //  需不需要深拷贝？会多开个内存复制构造一个棋子？
        game_mnger.defender = enemy;

        GD.Print(game_mnger.defender);

        EnterAttackScheduleQ();
        superior.ChangeTo<攻击调度>();
    }

    //  鼠标框选或点选。返回是否是框选。
    bool MouseBoxOrClickSelect()
    {
        PieceStack stack = null;

        //  直接点选或选框太小，则只选择鼠标所在点的
        if (Mathf.Abs(mouse_pos.x - mouse_old_pos.x) < 10 || Mathf.Abs(mouse_pos.y - mouse_old_pos.y) < 10)
        {
            stack = game_mnger.pieces_mnger.GetPieceStackByRectPos(mouse_pos);
            if (stack != null)
            {
                Piece piece = stack.GetTopPiece(); GD.Print(piece.be_attacked_result);

                if ((!Global.联机调试 && own_side == piece.side) || (Global.联机调试 && piece.side == game_mnger.local_player_side))
                {
                    game_mnger.pieces_selected.Add(piece);
                    game_mnger.stack_selected = stack;
                }
            }
            return false;
        }
        else        //  选框框选
        {
            foreach (Piece p in game_mnger.pieces_mnger.pieces.GetChildren())
            {
                if ((!Global.联机调试 && own_side == p.side) || (Global.联机调试 && p.side == game_mnger.local_player_side))
                {
                    if (Math.IsPointInRect(p.Position, new Rect2(mouse_old_pos, new Vector2(mouse_pos - mouse_old_pos))))
                        game_mnger.pieces_selected.Add(p);
                }
            }
            return true;
        }
    }

    #endregion 点选棋子

    //————————————————————————————————————————————————————————————————————————————————  GUI
    #region 面板

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

    //--------------------------------------------------  棋子信息栏
    //  棋子信息卡被关闭时调用，由game_mnger调用。多选的棋子可以通过关闭选到自己想选的。
    public override void PieceInfoCardClosed(Piece p_selected)
    {
        if (game_mnger.pieces_selected.Count == 0)
        {
            DrawSightLine(false);
            return;
        }
        DrawSightLine(true);
    }

    //  棋子信息卡被选。由game_mnger调用
    public override void PieceInfoCardSelected(Piece p_selected)
    {
        if (game_mnger.ActionableSide != own_side) return;

        ClearPiecesSelected();
        game_mnger.pieces_selected.Add(p_selected);
        game_mnger.pieces_mnger.TopAPiece(p_selected);
        MarkPiecesSelected();
        DrawSightLine(true);
    }

    //---------------------------------------------------  悬浮棋子信息栏
    //  处理悬浮棋子信息栏中的信息卡被选，关联到棋子。由game_mnger调用
    public void PieceInfoCardSelectedInFBar(Piece p_selected)
    {
        HandleSelectedEnemySide(p_selected);
    }

    //----------------------------------------------------
    //  清理gui操作残留
    protected override void ClearOperationResidues()
    {
        ClearPiecesSelected();
        DrawSightLine(false);
    }

    #endregion

    //--------------------------------------------------
    #region Mark绘制
    //  叫 mark 绘制选框
    protected void DrawSelectBox(bool draw)
    {
        if (draw)
        {
            game_mnger.mark.draw_select_box = true;
            game_mnger.mark.select_box_origin_pos = mouse_old_pos;
            return;
        }
        game_mnger.mark.draw_select_box = false;
    }

    //  绘制视线
    protected void DrawSightLine(bool draw)
    {
        if (draw == false)
        {
            game_mnger.mark.draw_sight_line = false;
            return;
        }

        game_mnger.mark.draw_sight_line = true;
        game_mnger.mark.sight_line_start_points.Clear();
        foreach (Piece p in game_mnger.pieces_selected)
        {
            game_mnger.mark.sight_line_start_points.Add(p.HexPos);
        }
    }
    #endregion

    //————————————————————————————————————————————————————————————————————  网络

    //  RPC分发
    protected override void _RPC(int id, Dictionary content)
    {
        if (!content.Contains("func") || !content.Contains("params")) return;
        Array _params = content["params"] as Array;

        switch (content["func"])
        {
            case "EnterAttackScheduleA":
                EnterAttackScheduleA(id, _params);      //  进入攻击调度
                break;
            case "EndActionA":
                EndActionA();       //  结束行动，临机射击才用，继承
                break;

            case "SynActiveSideA":
                SynActiveSideA(_params);        //  同步活动方，继承自父类
                break;
            case "EnterNextStageA":
                EnterNextStageA();      //  进入下阶段，继承
                break;
            case "SynEndStageSideA":
                SynEndStageSideA(_params);      // 同步结束阶段的标记， 继承
                break;
            default:
                GD.PrintErr("射击阶段Base：没有找到函数：", content["func"]);
                break;
        }
    }

    //  通知进入攻击调度
    protected virtual void EnterAttackScheduleQ()
    {
        game_mnger.Send(Global.opposite_player_peer_id, NetworkMnger.Data2JSON("EnterAttackScheduleA", null));
    }

    //  收到通知进入攻击调度
    protected virtual void EnterAttackScheduleA(int id, Array _params)
    {
        superior.ChangeTo<攻击调度>();
    }


}
