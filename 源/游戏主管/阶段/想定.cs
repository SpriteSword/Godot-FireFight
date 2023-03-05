using Godot;
using Godot.Collections;

public class 想定 : 游戏阶段
{
    Piece piece_selected;
    棋子部署系统 deploy_system;

    //  网络
    bool i_finished = false;
    bool oppo_finished = false;
    bool client_ready = false;      //  自己结束部署为finished，同步了对方的叫ready

    //  ---------  玩法
    //  双方兵力总表summary。{id:"型号"}。id是否正确在读取文件时检查
    public Dictionary red_piece_smry = new Dictionary {
            { 1, "T62" }, { 2, "T62" }, { 3, "T62" },{ 4, "T62" }
            };

    public Dictionary blue_piece_smry = new Dictionary {
            { 5, "M60 A1" }, { 6, "M60 A1" }, { 7, "M60 A1" },{ 8, "M60 A1" }, { 9, "M60 A1" }, { 10, "M60 A1" }, { 11, "M60 A1" },{ 12, "M60 A1" },
            { 13, "M60 A1" }, { 14, "M60 A1" }, { 15, "M60 A1" },{ 16, "M60 A1" }, { 17, "M60 A1" }, { 18, "M60 A1" }, { 19, "M60 A1" },{ 20, "M60 A1" },
            { 21, "M60 A1" }
            };


    public override void _Ready()
    {
        deploy_system = GetNode<棋子部署系统>("../../画布层/GUI/棋子部署系统");
        deploy_system.controller = this;
    }


    //——————————————————————————————————————————————————————————————————————
    public override void Enter()
    {
        base.Enter();

        game_mnger.gui.info_box.Hide();
        deploy_system.Hide();      //  由于初始化顺序写这里
        deploy_system.ClearAll();

        CreatePieceList();

        deploy_system.Show();
    }
    public override void HandleUnhandledInput(InputEvent _event)
    {
        mouse_pos = game_mnger.GetGlobalMousePosition();

        if (_event.IsActionPressed("click_left"))
        {
            //  放置棋子
            if (deploy_system.IsHolding())
            {
                Vector2 pos = game_mnger.GetGlobalMousePosition();
                Vector2 cell_pos = game_mnger.mark.DetermineCellOfHexGrid(pos);

                AddPiece(deploy_system.piece_in_bar.piece_id, cell_pos, game_mnger.local_player_side);

                deploy_system.HavePlacedPiece();
            }
            else        //  点取地图上的棋子
            {
                HandleSelectPiece();
            }
        }
    }
    public override void Exit()
    {
        base.Exit();        //  子类必须执行
    }


    //——————————————————————————————————————————————————————————————
    //  处理 编辑地图上棋子
    void HandleSelectPiece()
    {
        var stack = game_mnger.pieces_mnger.GetPieceStackByRectPos(mouse_pos);
        if (stack != null && (Global.联机调试 && stack.side == game_mnger.local_player_side))
        {
            piece_selected = stack.GetTopPiece();
            deploy_system.ShowUIIfClickPieceOnMap(true, piece_selected.id);
            MarkPiecesSelected();
            return;
        }

        piece_selected = null;
        deploy_system.ShowUIIfClickPieceOnMap(false);
    }

    //  创建棋子一览表
    void CreatePieceList()
    {
        var dic = PieceSmryBySide(game_mnger.local_player_side);
        foreach (int itm in dic.Keys)
        {
            deploy_system.AddPieceInBar((uint)itm, game_mnger.local_player_side, (string)dic[itm]);
        }
    }

    //  根据side 决定使用哪个字典。
    Dictionary PieceSmryBySide(GameMnger.Side side)
    {
        if (side == GameMnger.Side.红) return red_piece_smry;
        else return blue_piece_smry;
    }

    //  添加棋子
    void AddPiece(uint id, Vector2 cell_pos, GameMnger.Side side)
    {
        string model_name;
        if (side == GameMnger.Side.红) { model_name = "T62"; }
        else { model_name = "M60 A1"; }

        game_mnger.pieces_mnger.AddPiece(
            id,
            cell_pos,
            side,
            Piece.PieceType.坦克,
            model_name
            );   //+++++++++++++++++

    }



    //——————————————————————————————————————————————————————————————  棋子部署系统调用
    //  重新部署。移动已经摆放好的棋子
    public void Redeploy(uint id)
    {
        Piece piece = game_mnger.pieces_mnger.GetPiece(id);
        if (piece == null) return;
        piece_selected = null;

        game_mnger.pieces_mnger.RemovePieceFromStack(piece);
        piece.QueueFree();
    }

    //  询问是否结束想定
    public void InquireFinish()
    {
        Inquire("是否结束部署并开始游戏？", IC._i_结束想定);
    }

    //——————————————————————————————————————————————————————————————  GUI
    //  询问框返回
    public override void InquiryBoxYes(IC command)
    {
        switch (command)
        {
            case IC._i_结束想定:
                Finish();
                break;
        }
    }
    public override void InquiryBoxNo(IC command)
    {
        switch (command)
        {
            case IC._i_结束想定:
                break;
        }
    }

    //——————————————————————————————————————————————————————————

    void Finish()
    {
        if (!Global.联机调试) return;

        game_mnger.gui.info_box.Show();
        deploy_system.Hide();
        i_finished = true;
        SynDeploymentQ();
    }

    //——————————————————————————————————————————————————————————————————————————————————————————————————————  网络
    //  RPC分发
    protected override void _RPC(int id, Dictionary content)
    {
        if (!content.Contains("func") || !content.Contains("params")) return;
        Array _params = content["params"] as Array;

        switch (content["func"])
        {
            case "SynDeploymentA":
                SynDeploymentA(_params);        //  同步部署的数据
                break;
            case "ClientReadySA":        //  客户端同步完成
                ClientReadySA();
                break;
            case "StartCA":
                StartCA();
                break;
            default:
                GD.PrintErr("想定：没有找到函数：", content["func"]);
                break;
        }
    }

    //  同步部署的数据 Q
    void SynDeploymentQ()
    {
        Array _params = new Array();
        Dictionary d = new Dictionary();

        foreach (Piece p in game_mnger.pieces_mnger.pieces.GetChildren())
        {
            if (p.side == game_mnger.local_player_side) { d[p.id] = p.CellPos; }
        }

        _params.Add(d);       //  字典{棋子id : cell 坐标}
        game_mnger.Send(Global.opposite_player_peer_id, NetworkMnger.Data2JSON("SynDeploymentA", _params));
    }

    //  收到对方的部署数据 A
    void SynDeploymentA(Array _params)
    {
        if (_params[0] is Dictionary dictionary)
        {
            foreach (string itm in dictionary.Keys)        //  itm 是string
            {
                var id = (itm).ToInt();

                Dictionary oppo_smry = PieceSmryBySide(SwapRedBlue(game_mnger.local_player_side));
                if (!oppo_smry.Contains(id)) continue;

                string str_pos = (string)dictionary[itm];
                Vector2 cell_pos;
                try
                {
                    cell_pos = MyString.Decode2Vector(str_pos);

                    AddPiece((uint)id, cell_pos, SwapRedBlue(game_mnger.local_player_side));
                }
                catch (MyException.DecodeException de)
                {
                    GD.PrintErr(de);
                }
            }
        }
        else { GD.PrintErr("想定：SynDeploymentA：参数错误，不是Dictionary"); }

        oppo_finished = true;


        if (game_mnger.IsAsServer())
        {
            if (i_finished && oppo_finished && client_ready) { StartSQ(); }
        }
        else
        {
            ClientReadyCQ();     //  自己是客户端，发送已准备好，等待主机开始命令
        }
    }
    /*
    {"type":"data","func":"SynDeploymentA","params":[{"1":"(2, 1)"}]}
    [{1:(2, 1)}]Godot.Collections.Dictionary
    */

    //  部署完成，客户端发送
    void ClientReadyCQ()
    {
        Array _params = new Array();
        game_mnger.Send(Global.opposite_player_peer_id, NetworkMnger.Data2JSON("ClientReadySA", _params));
    }
    void ClientReadySA()
    {
        client_ready = true;
        if (i_finished && oppo_finished && client_ready) { StartSQ(); }
    }

    //  服务器调用通知客户端开始。
    void StartSQ()
    {
        Array _params = new Array();
        game_mnger.Send(Global.opposite_player_peer_id, NetworkMnger.Data2JSON("StartCA", _params));

        superior.ChangeTo<直射阶段>();
    }
    //  客户端开始
    void StartCA()
    {
        superior.ChangeTo<直射阶段>();
    }

}
