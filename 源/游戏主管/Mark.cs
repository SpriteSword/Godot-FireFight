
using Godot;
using Godot.Collections;
using System;

public class Mark : HexTileMap
{
    Vector2 mouse_pos = new Vector2(200, 200);
    Vector2 mouse_cell = new Vector2(0, 0);

    //  绘制选框
    public bool draw_select_box = false;
    public Vector2 select_box_origin_pos;       //   选框的原点

    //  绘制视线
    public bool draw_sight_line = false;
    public Array<Vector2> sight_line_start_points = new Array<Vector2>();       //  hex坐标
                                                                                // public Vector2 sight_line_start;       //  视线的起点，用cell 坐标

    //  画路径线
    public bool draw_path_line = false;
    public Array<PathPoint> path_line;        //  要不要深拷贝？其实是 game_mnger.path

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

        DrawMouseHex();

        Update();
    }

    public override void _Draw()
    {
        DrawSightLine();
        DrawPathLine();
        DrawSelectBox();
    }

    //-------------------------------------------------------------------

    //  设置鼠标指示格
    void DrawMouseHex()
    {
        SetCellv(mouse_cell, -1);
        mouse_cell = DetermineCellOfHexGrid(mouse_pos);
        SetCellv(mouse_cell, 3);
    }

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
                var t_cell_pos = DetermineCellOfHexGrid(mouse_pos);     //  to
                DrwaLineByCellCoord(f_cell_pos, t_cell_pos, Colors.Cyan);       //  青色。好像还没用数字直观

                //  绘制距离数字
                var t_hex_pos = Math.Cell2HexCoord(t_cell_pos);
                float dist = Math.Hex2RectCoord(t_hex_pos - hex_pos).Length() * 50;
                var pos = (HexGridCenter(f_cell_pos) + HexGridCenter(t_cell_pos)) / 2;      //  图上直线的中点
                game_mnger.gui.DrawStringByGlobalPos(dist.ToString("0") + "m", pos, Colors.Cyan);
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
                    var r_m_p = path_line[i + 1].remaining_m_p;
                    if (r_m_p >= 0)
                    {
                        DrwaLineByCellCoord(path_line[i].cell_pos, path_line[i + 1].cell_pos, Color.Color8(255, 255, 255), r_m_p.ToString(), 1);
                    }
                    else
                    {
                        DrwaLineByCellCoord(path_line[i].cell_pos, path_line[i + 1].cell_pos, Color.Color8(255, 200, 200), r_m_p.ToString(), 1);
                    }

                }
            }
        }
    }

    //  画连接2个六边形的线段，用 TileMap的 cell 坐标！text 位置，例：0起点，1终点，0.5中点。文字与线同色。
    public void DrwaLineByCellCoord(Vector2 from, Vector2 to, Color color, string text = null, float text_pos = 0.5f)
    {
        Vector2 f = HexGridCenter(from);
        Vector2 t = HexGridCenter(to);
        DrawLine(f, t, color, 1);

        if (text == null) return;

        game_mnger.gui.DrawStringByGlobalPos(text, (t - f) * text_pos + f, color);
    }
}
