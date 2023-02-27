using Godot;
using Godot.Collections;


public class 测试解析型号 : Node
{
    string m = "";		//  != null, ==Empty()
    Dictionary dictionary = new Dictionary();

    public override void _Ready()
    {
        dictionary["坦克/APC"] = 1;
        dictionary["M60 37"] = 2;

        // dictionary[""] = 3;
        // GD.Print(dictionary[""]);		//  居然也行！！！！！！

        // dictionary[null] = 4;
        // GD.Print(dictionary[null]);     //  也行！！输出 4。但是没有对象实例的，调用其它就会错！！！


        var rslt = MatchModelName(dictionary, "坦克 37/21");
		GD.Print(rslt);

    }

    //  匹配类型名称。匹配上就返回 key 键名，否则返回 null。正常输入型号像："M60 20" "tow"。字典中的 key 只有像 "M60/APC" "M60 32"这两种形式，没有"M60/APC 31"这种。
    static string MatchModelName(Dictionary dictionary, string model_name)
    {
        if (model_name == null || model_name.Empty()) return null;

        foreach (string key in dictionary.Keys)
        {
            //  分解出大小型号
            var ms = key.Split(" ", true);
            var judged_ms = model_name.Split(" ");

            var big_ms = ms[0].Split("/");		//  字典的大型号可能带"/"
            foreach (var it in big_ms)
            {
                if (judged_ms[0] == it)
                {
                    //  字典的仅有一个大型号，说明包括了所有小型号
                    if (ms.Length == 1) return key;

                    //  目标有小型号并匹配
                    if (judged_ms.Length > 1 && judged_ms[1] == ms[1]) return key;
                }
            }
        }
        return null;
    }

}
