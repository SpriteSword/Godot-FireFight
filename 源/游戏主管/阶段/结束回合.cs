using Godot;
using Godot.Collections;

public class 结束回合 : 游戏阶段
{
    public override void Enter()        //  子类必须执行
    {
        base.Enter();
        GD.Print("结束回合");

        game_mnger.Round += 1;
        RecoverAll();
        superior.ChangeTo<直射阶段>();

    }
    public override void UpdatePhysicsProcess(float delta) { }
    public override void UpdateProcess(float delta) { }
    public override void HandleInput(InputEvent _event) { }
    public override void HandleUnhandledInput(InputEvent _event) { }

    public override void Exit()     //  子类必须执行
    {
        base.Exit();
    }


    //  恢复所有棋子状态
    void RecoverAll()
    {
        // bool have_dead = false;
        foreach (Piece p in game_mnger.pieces_mnger.pieces.GetChildren())
        {
            if (p.BeK)
            {
                // have_dead = true;
                p.CleanUp();
                game_mnger.pieces_mnger.RemovePieceFromStack(p);
            }
            else
            {
                p.CanAct = true;
                p.can_be_shot = true;
            }
        }

        // if (have_dead) { game_mnger.sound_mnger.PlaySoundRes(game_mnger.sound_mnger.snd_be_killed); }
    }

}
