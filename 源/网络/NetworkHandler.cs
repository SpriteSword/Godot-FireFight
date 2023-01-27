using Godot;
using Godot.Collections;

//  给其它场景处理网络用
public class NetworkHandler : Node
{
    public NetworkMnger network_mnger;
    protected Dictionary decode_json;

    public override void _Ready()
    {
        network_mnger = GetNode<NetworkMnger>("/root/NetworkMnger");
        network_mnger.Connect("Receive", this, "_Receive");
    }

    public override void _ExitTree()
    {
        GD.Print("duan kai xin hao");
        network_mnger.Disconnect("Receive", this, "_Receive");
    }

    //  处理 log、err、data（就是调用函数）。{type:"data", func:"say_hello", params:[]}。log、err后面就是一个普通str
    protected virtual void _Receive(int id, string data)
    {
        GD.Print(data);

        decode_json = null;
        if (JSON.Parse(data).Result is Dictionary d)
            decode_json = d;

        if (decode_json == null) return;

        switch (decode_json["type"])
        {
            case "log":
                HandleLog(id, decode_json); break;
            case "err":
                HandleError(id, decode_json); break;
            case "data":
                HandleData(id, decode_json); break;
        }
    }

    //------------------------------------------- content 必不为 null
    //  处理错误  //  只要有错误就退出！
    protected virtual void HandleError(int id, Dictionary content) { }

    //  处理普通消息
    protected virtual void HandleLog(int id, Dictionary content) { }

    //  处理数据
    protected virtual void HandleData(int id, Dictionary content) { }

}