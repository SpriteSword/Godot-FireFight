using Godot;
using System;

public class GameMngerStateMachine : Node       //  懒得继承了，反正也只有一个状态机
{
    protected 游戏阶段 current_state;
    GameMnger game_mnger;

    public override void _Ready()
    {
        foreach (State item in GetChildren())
        {
            item.superior = this;
        }

        game_mnger = GetNode<GameMnger>("..");
    }


    public override void _UnhandledInput(InputEvent @event)
    {
        if (current_state == null) return;
        current_state.HandleUnhandledInput(@event);
    }
    public override void _Input(InputEvent @event)
    {
        if (current_state == null) return;
        current_state.HandleInput(@event);
    }
    public override void _PhysicsProcess(float delta)
    {
        if (current_state == null) return;
        current_state.UpdatePhysicsProcess(delta);
    }
    public override void _Process(float delta)
    {
        if (current_state == null) return;
        current_state.UpdateProcess(delta);
    }

    public void ChangeTo<T>() where T : 游戏阶段       //  如果不是C#7.3不支持继承后的where
    {
        Type t = typeof(T);
        游戏阶段 tmp = GetNode<游戏阶段>(t.Name);
        if (tmp == null) return;

        current_state.Exit();
        current_state = tmp;
        game_mnger.current_stage = current_state;
        current_state.Enter();
    }

    //  GameMnger 初始化完才到各控制器
    public void MngerReady()
    {
        foreach (游戏阶段 itm in GetChildren())
        {
            itm.game_mnger = game_mnger;
        }

        if (Global.联机调试) { current_state = GetNode<想定>("想定"); }
        else { current_state = GetNode<直射阶段>("直射阶段"); }

        current_state.Enter();
        game_mnger.current_stage = current_state;
    }


}

