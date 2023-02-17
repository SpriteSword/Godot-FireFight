using Godot;

public class HexTileMap : TileMap
{
    //  六边形格子中心的直角坐标
    public Vector2 HexGridCenter(Vector2 cell_pos)
    {
        return MapToWorld(cell_pos) + GameMnger._hex_center_offset_;
    }

    //  确定坐标（相对于tilemap的直角坐标）是属于哪个六边形格，返回该六边形格所在的cell。
    public Vector2 DetermineCellOfHexGrid(Vector2 rect_pos)      //  _pos相对于tilemap原点的坐标
    {
        Vector2 c0 = WorldToMap(rect_pos);
        bool is_y_odd = false;      //  是否是奇数

        //  相对左上角的坐标
        Vector2 apex_pos;       //  cell 左上角顶点坐标
        if (((int)c0.y) % 2 != 0)       //  单数
        {
            apex_pos = new Vector2((c0.x) * GameMnger._cell_w_ + GameMnger._cell_w_ / 2, (c0.y) * GameMnger._cell_h_);     //  好像不转int也可
            is_y_odd = true;
        }
        else apex_pos = new Vector2((c0.x) * GameMnger._cell_w_, (c0.y) * GameMnger._cell_h_);

        Vector2 f_c = c0;       //  finaly最终确定
        Vector2 delta_pos = rect_pos - apex_pos;

        if (delta_pos.x < Math._SqRt_3_ / 2 * GameMnger._hex_side_len_)
        {
            if (delta_pos.y < GameMnger._hex_side_len_ / 2 - 1.0 / Math._SqRt_3_ * delta_pos.x)      //  在左上角
            {
                if (is_y_odd) { f_c = new Vector2(c0.x, c0.y - 1); }
                else { f_c = new Vector2(c0.x - 1, c0.y - 1); }
            }
        }
        else
        {
            if (delta_pos.y < -GameMnger._hex_side_len_ / 2 + 1.0 / Math._SqRt_3_ * delta_pos.x)     //  在右上角
            {
                if (is_y_odd) { f_c = new Vector2(c0.x + 1, c0.y - 1); }
                else { f_c = new Vector2(c0.x, c0.y - 1); }
            }
        }
        return f_c;
    }
}