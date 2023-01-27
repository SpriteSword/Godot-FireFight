using Godot;





public class HexTileMap : TileMap
{
    float side_l = 26;       //  六边形边长
    float cell_w = 45;
    float cell_h = 39;     //  砍掉下面2部分后剩下的高。整个是45x52。

    public override void _Ready()
    {
    }

    public override void _PhysicsProcess(float delta)
    {
        if (Input.IsActionJustPressed("click_left"))
        {
            Vector2 mouse_pos = GetGlobalMousePosition();
            Vector2 cell = DetermineHex2CellCoord(ToLocal(mouse_pos));
            GD.Print(cell, Math.Cell2HexCoord(cell));
        }
    }

    //  确定进入的六边形的 cell坐标
    public Vector2 DetermineHex2CellCoord(Vector2 _pos)      //  _pos为相对于tilemap原点的坐标
    {
        //  判断在哪个cell
        Vector2 c0 = WorldToMap(_pos);
        bool is_y_odd = false;      //  是否是奇数

        Vector2 apex_pos;       //  cell 左上角顶点坐标。apex顶点
        if (((int)c0.y) % 2 != 0)       //  单数
        {
            apex_pos = new Vector2((c0.x) * cell_w + cell_w / 2, (c0.y) * cell_h);     //  好像不转int也可
            is_y_odd = true;
        }
        else apex_pos = new Vector2((c0.x) * cell_w, (c0.y) * cell_h);

        //  再判断cell上面的2个三角形
        // Vector2 f_c = c0;
        Vector2 delta_pos = _pos - apex_pos;

        if (delta_pos.x < Math._SqRt_3_ / 2 * side_l)
        {
            if (delta_pos.y < side_l / 2 - 1.0 / Math._SqRt_3_ * delta_pos.x)      //  在左上角
            {
                if (is_y_odd) return new Vector2(c0.x, c0.y - 1);
                else return new Vector2(c0.x - 1, c0.y - 1);
            }
        }
        else
        {
            if (delta_pos.y < -side_l / 2 + 1.0 / Math._SqRt_3_ * delta_pos.x)     //  在右上角
            {
                if (is_y_odd) return new Vector2(c0.x + 1, c0.y - 1);
                else return new Vector2(c0.x, c0.y - 1);
            }
        }

        return c0;
    }
}
