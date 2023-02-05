using Godot;
using MyException;

public static class MyString
{
    //  将字符串解码为向量。如果有多个向量，只解码第一个。必须形如："(1,2)"。解码失败会抛出 DecodeException。
    public static Vector2 Decode2Vector(string str)
    {
        if (str.Length < 5) { throw new DecodeException("字符串长度不足"); }
        if (str[0] != '(') { throw new DecodeException("缺少‘(’"); }
        if (str[str.Length - 1] != ')') { throw new DecodeException("缺少‘)’"); }

        string tmp = Intercept(str, 1, ')');

        int pos = tmp.IndexOf(',');
        if (pos < 0) { throw new DecodeException("缺少‘,’"); }

        float x = Intercept(tmp, 0, ',').ToFloat();
        float y = Intercept(tmp, pos + 1, '\0').ToFloat();

        return new Vector2(x, y);
    }

    //  返回新的截取的字符串，从指定位置开始，以指定结束符为止，不包含结束符。pos > length-1，则是""
    public static string Intercept(string str, int pos, char end)
    {
        if (str == null || pos < 0) return null;

        string tmp = "";
        for (int i = pos; i < str.Length; i++)
        {
            if (str[i] == end) { break; }
            else { tmp += str[i]; }
        }
        return tmp;
    }

}