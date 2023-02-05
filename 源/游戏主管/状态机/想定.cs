using Godot;
using Godot.Collections;

public class 想定 : 游戏阶段
{
    Piece piece_selected;
    棋子部署系统 deploy_system;

    //  网络
    bool i_ready = false;
    bool opps_ready = false;


    public override void _Ready()
    {
        deploy_system = GetNode<棋子部署系统>("/root/GameMnger/画布层/GUI/棋子部署系统");
        deploy_system.controller = this;
    }


    //——————————————————————————————————————————————————————————————————————
    public override void Enter()
    {
        base.Enter();

        deploy_system.Hide();      //  由于初始化顺序写这里
        deploy_system.ClearAll();

        //  ++++++++++++++++++++++++++++++根据游戏规则创建所有棋子一览表
        deploy_system.AddTexture(1);
        deploy_system.AddTexture(2);

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
                game_mnger.pieces_mnger.AddBluePiece(Math.Cell2HexCoord(cell_pos), deploy_system.piece_in_bar.piece_id);     //+++++

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
        base.Exit();
    }


    //——————————————————————————————————————————————————————————————
    //  处理 编辑地图上棋子
    void HandleSelectPiece()
    {
        var stack = game_mnger.pieces_mnger.GetPieceStackByRectPos(mouse_pos);
        if (stack != null)
        {
            piece_selected = stack.GetTopPiece();
            deploy_system.ShowUIIfClickPieceOnMap(true, piece_selected.id);
            MarkPiecesSelected();
            return;
        }

        piece_selected = null;
        deploy_system.ShowUIIfClickPieceOnMap(false);
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


    void Finish()
    {
        if (!Global.联机调试) return;

        deploy_system.Hide();
        i_ready = true;
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
        d[1] = new Vector2(2, 1);
        _params.Add(d);       //  字典{棋子id : cell 坐标}

        game_mnger.Send(Global.opposite_player_peer_id, NetworkMnger.Data2JSON("SynDeploymentA", _params));
    }

    //  收到对方的部署数据 A
    void SynDeploymentA(Array _params)
    {
        if (_params[0] is Dictionary dictionary)
        {
            foreach (var itm in dictionary.Keys)        //  itm 是string
            {

            }
        }
        else { GD.PrintErr("想定：SynDeploymentA：参数错误，不是Dictionary"); }

    }
    /*
    {"type":"data","func":"SynDeploymentA","params":[{"1":"(2, 1)"}]}
    [{1:(2, 1)}]Godot.Collections.Dictionary
    */

}
