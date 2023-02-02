using Godot;
using Godot.Collections;
using System;

public class GUI : Control		//  人类的所有输入都经这里分发！并负责 抬头显示(HUD) 的绘制
{
    //  节点
    public GameMnger game_mnger;
    public 回合显示栏 round_display_bar;
    public 棋子信息栏 piece_info_bar;
    public 悬浮棋子信息栏 floating_piece_info_bar;
    public 棋子部署系统  piece_deployment_system;

    Font default_font;
    Array<HUDString> hud_str_buffer = new Array<HUDString>();       //  绘制HUD文字的缓冲区



    public override void _Ready()
    {
        game_mnger = GetNode("..").GetNode<GameMnger>("..");
        round_display_bar = GetNode<回合显示栏>("回合显示栏");
        piece_info_bar = GetNode<棋子信息栏>("棋子信息栏");
        floating_piece_info_bar = GetNode<悬浮棋子信息栏>("悬浮棋子信息栏");
        piece_deployment_system  =GetNode<棋子部署系统>("棋子部署系统");

        default_font = GetFont("font");

    }

    public override void _Process(float delta)
    {
        Update();
    }

    //  只处理最开始输入！
    public override void _Input(InputEvent @event)
    {
    }

    public override void _Draw()
    {
        //  绘制 HUD字符串
        if (hud_str_buffer.Count > 0)
        {
            foreach (var it in hud_str_buffer)
            {
                DrawString(default_font, it.pos, it.str, it.color);
            }
            hud_str_buffer.Clear();
        }

    }


    //  绘制 HUD字符串，只是将字符串加入队列。使用 以 game_mnger 所在坐标系为全局坐标系
    public void DrawStringByGlobalPos(string str, Vector2 global_pos, Color color)
    {
        //  game_mnger 是全局坐标系，camera中心 是局部坐标系
        var camera_glbl_pos = game_mnger.camera.GetCameraScreenCenter();		//  相机相对于原点的坐标。相机不使用平滑是没有差别的：GetCameraPosition、Position、GetCameraScreenCenter
        var local_pos = (global_pos - camera_glbl_pos) / game_mnger.camera.Zoom;      //  因为zoom <1 是放大，自然除以zoom，令其远离中心点
        var scrn_pos = local_pos + OS.WindowSize / 2;       //  只是转到以屏幕左上角为原点

        HUDString s = new HUDString(str, scrn_pos, color);
        hud_str_buffer.Add(s);
    }


    //  抬头显示(HUD)上的文字
    class HUDString : Reference
    {
        public Vector2 pos;
        public string str;
        public Color color;

        public HUDString(string str, Vector2 pos, Color color)
        {
            this.pos = pos;
            this.str = str;
            this.color = color;
        }
    }

}
