using Godot;
using Godot.Collections;

public class 服务端 : 网络组件
{
    [Signal] delegate void Receive(int id, string data);

    WebSocketServer server;

    Array<int> connect_id = new Array<int>();

    public 服务端(ushort _port)
    {
        port = _port;
    }


    public override void _Ready()
    {
        component = new WebSocketServer();
        server = component as WebSocketServer;
        server.Connect("client_close_request", this, "_client_close_request");
        server.Connect("client_connected", this, "_client_connected");
        server.Connect("client_disconnected", this, "_client_disconnected");
        server.Connect("data_received", this, "_data_received");

        var err = server.Listen(port);
        if (err == Error.Ok) Notice(-1, NetworkMnger.Log2JSON("create_server"));
        else Notice(-1, NetworkMnger.Error2JSON("create_server_fail"));

        SetProcess(err == Error.Ok);

    }

    public override void _Process(float delta)
    {
        server.Poll();
    }

    public override void _ExitTree()
    {
        server.Stop();
    }


    void RemoveConnectId(int id)
    {
        // if (connect_id.Contains(id))        //  直接remove也不会有什么问题的！
        connect_id.Remove(id);
    }



    //  通知程序其它节点
    void Notice(int id, string s)
    {
        EmitSignal("Receive", id, s);
    }


    //------------------------------------------------------------


    void _client_connected(int id, string protocol)
    {
        connect_id.Add(id);     //  不用看有没有？
        Notice(id, NetworkMnger.Log2JSON("client_connected"));
    }

    void _client_close_request(int id, int code, string reason)
    {
        RemoveConnectId(id);
        Notice(id, NetworkMnger.Log2JSON("client_close_request"));
    }

    void _client_disconnected(int id, bool was_clean_close)     //  如果因网络问题而断开了会如何？
    {
        RemoveConnectId(id);
        Notice(id, NetworkMnger.Log2JSON("client_disconnected"));
    }

    void _data_received(int id)
    {
        var recevie_data = server.GetPeer(id).GetVar() as string;      //  怎么转类型？
        Notice(id, recevie_data);     //  自动加data
    }


}
