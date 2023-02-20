using Godot;
using Godot.Collections;
using System;



public static class Math
{
    public const double _SqRt_3_ = 1.732_050_807_569;
    public const double _Equal_Area_Circle_R_ = 0.525_037_56789;		//  等面积圆的半径，在hex坐标中，2个六边形之间的距离为1时。


    //--------------------------------------------------------  坐标

    //  tilemap的 cell坐标转为六边形Hex用坐标（必须为整数！）
    // public static Vector2 Cell2HexCoord(Vector2 cell_pos)
    // {
    //     if (((int)cell_pos.y) % 2 != 0)
    //         cell_pos.x += 0.5F;      //  值传递没事
    //     cell_pos.y *= _SqRt_3_ / 2;
    //     //  转坐标
    //     cell_pos.x = Mathf.Round(cell_pos.x + 1 / _SqRt_3_ * cell_pos.y);
    //     cell_pos.y = Mathf.Round(cell_pos.y * 2 / _SqRt_3_);

    //     return cell_pos;
    // }


    //  直角坐标转为六边形Hex坐标（不取整）
    public static Vector2 Rect2HexCoord(Vector2 pos)
    {
        pos.x = (float)(pos.x + 1 / _SqRt_3_ * pos.y);
        pos.y *= (float)(2 / _SqRt_3_);
        return pos;
    }
    //  六边形Hex坐标转直角坐标（不取整）
    public static Vector2 Hex2RectCoord(Vector2 pos)
    {
        pos.x -= pos.y / 2;
        pos.y *= (float)(_SqRt_3_ / 2);
        return pos;
    }

    //  tilemap的 cell坐标转为六边形Hex用坐标（必须为整数！）
    public static Vector2 Cell2HexCoord(Vector2 cell_pos)
    {
        if (((int)cell_pos.y) % 2 != 0)
            cell_pos.x += 0.5F;
        cell_pos.x += cell_pos.y / 2;		//  中间小数也就0.5，不用取整吧？
        return cell_pos;
    }

    //  六边形Hex用坐标（必须为整数！）转为 cell坐标
    public static Vector2 Hex2CellCoord(Vector2 hex_pos)
    {
        if (((int)hex_pos.y) % 2 != 0)
            hex_pos.x -= 0.5f;
        hex_pos.x -= hex_pos.y / 2;
        return hex_pos;
    }

    //  返回与直线相交的六边形，用与六边形等面积的圆来简化相交的判断。
    //  输入头尾两个六边形的 hex 坐标，返回 六边形们的坐标（hex 坐标）。
    public static Array<Vector2> GetHexsOnLine(Vector2 hex_from, Vector2 hex_to)        //++++++++++++++++++++++有bug！！！坐标为负数就不对！
    {
        if (hex_from == hex_to) return new Array<Vector2>(hex_from);

        Array<Vector2> array = new Array<Vector2>();

        //  3 个方向可以直接看出
        if (hex_to.y == hex_from.y || hex_to.x == hex_from.x || (hex_to - hex_from).x == (hex_to - hex_from).y)
        {
            Vector2 d = new Vector2(Mathf.Sign(hex_to.x - hex_from.x), Mathf.Sign(hex_to.y - hex_from.y));
            Vector2 i = hex_from;
            while (true)
            {
                array.Add(i);
                if (i == hex_to) break;
                i += d;
            }
            return array;
        }
        //  不是3个方向中的，用与六边形等面积的圆来简化相交的判断。
        //  直角坐标
        Vector2 f_rect = Hex2RectCoord(hex_from);
        Vector2 t_rect = Hex2RectCoord(hex_to);
        float k = 0, b;

        //  直线是否垂直
        bool is_vertical_in_rect = false;
        if (IsEqual(t_rect.x - f_rect.x, 0)) { is_vertical_in_rect = true; }
        else { k = (t_rect.y - f_rect.y) / (t_rect.x - f_rect.x); }     //  直线斜率

        b = f_rect.y - k * f_rect.x;        //  直线截距

        //  遍历
        int dx = Mathf.Sign(hex_to.x - hex_from.x);     //  是hex坐标的增量。整数
        int dy = Mathf.Sign(hex_to.y - hex_from.y);

        for (int x = (int)hex_from.x; ; x += dx)
        {
            for (int y = (int)hex_from.y; ; y += dy)
            {
                Vector2 hex = new Vector2(x, y);
                Vector2 o_rt_pos = Hex2RectCoord(hex);     //  圆心的直角坐标
                if (is_vertical_in_rect)
                {
                    if (Mathf.Abs(o_rt_pos.x - f_rect.x) < _Equal_Area_Circle_R_) { array.Add(hex); }
                }
                else        //  二次方程的 Δ。Δ>=0，有交点
                {
                    if (hex == hex_from || hex == hex_to) { array.Add(hex); }
                    else if (Mathf.Pow((-2 * o_rt_pos.x + 2 * k * (b - o_rt_pos.y)), 2) - 4 * (1 + k * k) * (Mathf.Pow(o_rt_pos.x, 2) + Mathf.Pow((b - o_rt_pos.y), 2) - _Equal_Area_Circle_R_ * _Equal_Area_Circle_R_) >= 0)
                        array.Add(hex);
                }

                if (y == hex_to.y) break;       //  整数直接写==没事
            }

            if (x == hex_to.x) break;
        }
        return array;
    }

    //  两个数是否相等
    public static bool IsEqual(float l, float r, float ep = 0.000_001f)     //  为什么1e-50的精度都可以？
    {
        return Mathf.Abs(r - l) < ep;
    }

    //  判断点是否在矩形内，point,  rect 都要同坐标系！
    public static bool IsPointInRect(Vector2 point, Rect2 rect)
    {
        Vector2 distance = point - rect.Position;
        if (distance.Sign() != rect.Size.Sign()) return false;        //  不同号就是不在里面！在边上也不行。rect.Size有正负！
        if (Mathf.Abs(distance.x) > Mathf.Abs(rect.Size.x) || Mathf.Abs(distance.y) > Mathf.Abs(rect.Size.y)) return false;
        return true;
    }


    // --------------------------------------------------------- 概率
    //  掷色子
    public static int ThrowDice()
    {
        RandomNumberGenerator random = new RandomNumberGenerator();
        random.Randomize();
        return random.RandiRange(1, 6);
    }
}






