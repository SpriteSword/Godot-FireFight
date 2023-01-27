using Godot;

public class 客户端 : 网络组件
{
    [Signal] delegate void Receive(int id, string data);

    string server_address;		//  服务器的IP地址
    WebSocketClient client;

    public 客户端(string _server_address, ushort _port)
    {
        server_address = _server_address;
        port = _port;
    }

    public override void _Ready()
    {
        component = new WebSocketClient();
        client = component as WebSocketClient;

        client.Connect("connection_closed", this, "_connection_closed");
        client.Connect("connection_error", this, "_connection_error");
        client.Connect("connection_established", this, "_connection_established");
        client.Connect("data_received", this, "_data_received");
        client.Connect("server_close_request", this, "_server_close_request");

        ConnectServer();
    }
    public override void _Process(float delta)
    {
        client.Poll();
    }
    public override void _ExitTree()
    {
        client.DisconnectFromHost();
    }



    void ConnectServer()
    {
        var err = client.ConnectToUrl("ws://" + server_address + ":" + port.ToString());
        if (err == Error.Ok) Notice(-1, NetworkMnger.Log2JSON("create_client"));
        else Notice(-1, NetworkMnger.Error2JSON("create_client_fail"));

        SetProcess(err == Error.Ok);
    }

    //  通知程序其它节点
    void Notice(int id, string s)
    {
        EmitSignal("Receive", id, s);
    }



    //---------------------------------------------------------

    //  err
    void _connection_error()        //  为什么信号不被触发？
    {
        GD.Print("con error!");
        Notice(-1, NetworkMnger.Error2JSON("connection_error"));
    }

    void _server_close_request(int code, string reason)
    {
        Notice(-1, NetworkMnger.Log2JSON("server_close_request"));
    }
    void _connection_closed(bool was_clean_close)       //  服务端关闭时
    {
        GD.Print("closed!");
        Notice(-1, NetworkMnger.Log2JSON("connection_closed"));
    }
    void _connection_established(string protocol)
    {
        Notice(-1, NetworkMnger.Log2JSON("connection_established"));

    }
    void _data_received()
    {
        var receive_data = client.GetPeer(1).GetVar();

        Notice(1, receive_data as string);
    }




}

