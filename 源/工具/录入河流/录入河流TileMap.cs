using Godot;
using System;

public class 录入河流TileMap : HexTileMap
{
    Vector2 mouse_pos = new Vector2(200, 200);
    Vector2 mouse_cell = new Vector2(0, 0);

    bool draw_line = false;
    Vector2 first_cell;
    Vector2 last_cell;


    public override void _Process(float delta)
    {
        mouse_pos = GetGlobalMousePosition();

        DrawMouseHex();

        Update();
    }
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb)
        {
            if (mb.IsActionPressed("click_left"))
            {
                if (draw_line)
                {
                    draw_line = !draw_line;
                    last_cell = mouse_cell;

                    GD.Print(first_cell.x, ",", first_cell.y, ",", last_cell.x, ",", last_cell.y);
                }
                else
                {
                    draw_line = !draw_line;
                    first_cell = mouse_cell;
                }
            }
            else if (mb.IsActionPressed("click_right"))
            {
                draw_line = false;
            }
        }
    }

    public override void _Draw()
    {
        Control control = new Control();
        Font default_font = control.GetFont("font");
        DrawString(default_font, new Vector2(10, 100), mouse_cell.ToString());

        if (draw_line)
        {
            DrawLine(HexGridCenter(first_cell), mouse_pos, new Color(0, 0, 200));
        }

                // foreach (var itm in game_mnger.river.data)
        // {
        //     Vector2 f = new Vector2(itm[0], itm[1]);
        //     Vector2 l = new Vector2(itm[2], itm[3]);

        //     DrawLine(HexGridCenter(f), HexGridCenter(l), Colors.Black);
        // }
    }


    //  设置鼠标指示格
    void DrawMouseHex()
    {
        SetCellv(mouse_cell, -1);
        mouse_cell = DetermineCellOfHexGrid(mouse_pos);
        SetCellv(mouse_cell, 3);
    }

}
