
using Godot;
using Godot.Collections;
using System;

public class Mark : MyTileMap
{
    Vector2 mouse_pos = new Vector2(200, 200);

    //  绘制选框
    public bool draw_select_box = false;
    public Vector2 select_box_origin_pos;       //   选框的原点

    //  绘制视线
    public bool draw_sight_line = false;
    public Array<Vector2> sight_line_start_points = new Array<Vector2>();       //  hex坐标
    // public Vector2 sight_line_start;       //  视线的起点，用cell 坐标

    //  画路径线
    public bool draw_path_line = false;
    public Array<Vector2> path_line;        //  要不要深拷贝？其实是 game_mnger.path

    //--------------------------------------------- 节点
    GameMnger game_mnger;

    public override void _Ready()
    {
        game_mnger = GetNode<GameMnger>("..");
        path_line = game_mnger.path;

    }

    public override void _Process(float delta)
    {
        mouse_pos = GetGlobalMousePosition();

        Update();
    }

    public override void _Draw()
    {
        DrawSightLine();
        DrawPathLine();
        DrawSelectBox();
    }

    //-------------------------------------------------------------------

    //  绘制选框
    void DrawSelectBox()
    {
        if (draw_select_box)
            DrawRect(new Rect2(select_box_origin_pos, mouse_pos - select_box_origin_pos), Color.Color8(255, 255, 255), false);
    }

    //  绘制视线
    void DrawSightLine()
    {
        if (draw_sight_line)
        {
            foreach (var hex_pos in sight_line_start_points)
            {
                //  绘制直线
                var f_cell_pos = Math.Hex2CellCoord(hex_pos);       //   from
                var t_cell_pos = WorldToMap(mouse_pos);     //  to
                DrwaLineByCellCoord(f_cell_pos, t_cell_pos, Colors.Cyan);       //  青色。好像还没用数字直观

                //  绘制距离数字
                var t_hex_pos = Math.Cell2HexCoord(t_cell_pos);
                float dist = Math.Hex2RectCoord(t_hex_pos - hex_pos).Length() * 50;
                var pos = (HexGridCenter(f_cell_pos) + HexGridCenter(t_cell_pos)) / 2;      //  图上直线的中点
                game_mnger.gui.DrawStringByGlobalPos(dist.ToString("0")+"m", pos, Colors.Cyan);
            }
        }
    }

    //  绘制路径线
    void DrawPathLine()
    {
        if (draw_path_line)
        {
            for (int i = 0; i < path_line.Count; i++)
            {
                if (i + 1 < path_line.Count)
                {
                    DrwaLineByCellCoord(path_line[i], path_line[i + 1], Color.Color8(255, 255, 255));
                }
            }
        }
    }

    //  画连接2个六边形的线段，用 TileMap的 cell 坐标！
    public void DrwaLineByCellCoord(Vector2 from, Vector2 to, Color color)      //  用 TileMap的 cell 坐标！
    {
        Vector2 f = HexGridCenter(from);
        Vector2 t = HexGridCenter(to);
        DrawLine(f, t, color, 1);
    }
}
