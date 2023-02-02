using Godot;
using System;

public class 测试二进制 : Node
{
    byte a = 0b1;

    public override void _Ready()
    {
        // uint pos = 1;

        // GD.Print(Cal(pos));

        // uint end = 0b1000000;
        // while ((pos & end) != end)
        // {
        // 	GD.Print(a, " ", pos, " ", pos & a);
        // 	pos = pos << 1;
        // }

        // for (int i = 0; i < 6; i++)
        // {
        //     GD.Print(a, " ", pos, " ", pos & a);
        //     pos = pos << 1;
        // }


        var r = Is2DirInterconnected(0b000101, 0b001010);
		GD.PrintT(r);
    }

    bool Is2DirInterconnected(uint d1, uint d2)     //  这个检查方法是错的！！！！！
    {
        Func<uint, uint, bool> check_3_bit = (d1_, d2_) =>		//  6个bit一次只检查3个，反正过程是重复的
        {
            uint pos = 1;
            for (int i = 0; i < 3; i++, pos = pos << 1)
            {
                if ((pos & d1_) > 0)
                {
                    //  恰好六边形是这样，如果两个互通，则肯定有一个的边的编码1左移3位后，在另一个的相应位置上也为1。
                    //  错！！是有方向的，2个图案调换一下可能就不互通了。
                    uint ch = pos << 3;
                    if ((d2_ & ch) > 0) { return true; }
                }
            }
            return false;
        };

        if (check_3_bit(d1, d2)) { return true; }
        if (check_3_bit(d2, d1)) { return true; }

        return false;
    }

    double Cal(double x_)
    {
        Func<double, double> cube = x => x * x * x;
        return cube(x_);
    }

}
