using Godot;
using Godot.Collections;

public class 想定 : 游戏阶段
{
    [Signal] delegate void ShowDeploySystem(bool show);
    [Signal] delegate void Deployed();
    [Signal] delegate void WantEditPiece(bool want, uint id);

    Piece piece_selected;

    棋子部署系统 deploy_system;

    public override void _Ready()
    {
        deploy_system = GetNode<棋子部署系统>("/root/GameMnger/画布层/GUI/棋子部署系统");

        Connect("ShowDeploySystem", deploy_system, "_ShowUI");
        Connect("Deployed", deploy_system, "_Deployed");
        Connect("WantEditPiece", deploy_system, "_WantEditPiece");
        deploy_system.Connect("MovePiece", this, "_Redeploy");
        deploy_system.Connect("Finish", this, "_InquireFinish");

        EmitSignal("ShowDeploySystem", false);
    }


    //——————————————————————————————————————————————————————————————————————
    public override void Enter()
    {
        base.Enter();

        EmitSignal("ShowDeploySystem", true);
        //  根据游戏规则创建所有棋子一览表

    }
    public override void UpdatePhysicsProcess(float delta) { }
    public override void UpdateProcess(float delta) { }
    public override void HandleInput(InputEvent _event) { }
    public override void HandleUnhandledInput(InputEvent _event)
    {
        if (_event.IsActionPressed("click_left"))
        {
            //  放置棋子
            if (deploy_system.HasPicked())
            {
                Vector2 pos = game_mnger.GetGlobalMousePosition();
                Vector2 cell_pos = game_mnger.mark.WorldToMap(pos);
                game_mnger.pieces_mnger.AddBluePiece(Math.Cell2HexCoord(cell_pos), deploy_system.selected_texture.piece_id);     //+++++

                EmitSignal("Deployed");
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


    //-----------------------------------------------------------------
    //  处理地图上棋子
    void HandleSelectPiece()
    {
        if (game_mnger.pieces_mnger.pieces_focused.Count > 0)
        {
            piece_selected = game_mnger.pieces_mnger.pieces_focused[game_mnger.pieces_mnger.pieces_focused.Count - 1];

            //  GUI显示信息+++++++++++

            EmitSignal("WantEditPiece", true, piece_selected.id);

            return;
        }
        piece_selected = null;
        EmitSignal("WantEditPiece", false, 0);      //  参数竟然不能省
    }


    //---------------------------------------------------------

    //  重新部署
    void _Redeploy(uint id)
    {
        Piece piece = game_mnger.pieces_mnger.GetPiece(id);
        if (piece == null) return;
        piece_selected = null;
        piece.QueueFree();
    }

    void _InquireFinish()
    {
        Inquire("是否结束部署并开始游戏？", IC._i_结束想定);
    }

    //----------------------------------------------------------------
    //  询问框返回
    public override void InquiryBoxYes(IC command)
    {
        switch (command)
        {
            case IC._i_结束想定:
                // superior.ChangeTo<临机射击>();       //+++++++++++++++++++game_mnger同步双方部署，一起开始！

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

}
