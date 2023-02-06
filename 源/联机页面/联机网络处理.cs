using Godot;
using Godot.Collections;

public class 联机网络处理 : NetworkHandler      //  问了没答怎么办？
{
    联机页面 online_page;

    public override void _Ready()
    {
        base._Ready();
        online_page = GetNode<联机页面>("..");
    }

    //——————————————————————————————————————————————————————————————————————————————————————————

    //  处理错误
    protected override void HandleError(int id, Dictionary content)      //  只要有错误就退出！
    {
        network_mnger.DeleteComponent();
        online_page.ShowUIByState(false);
        online_page.host_btn.Visible = true;

        if (!content.Contains("msg")) return;
        online_page.DisplayInfo(content["msg"] as string);
    }

    //  处理普通消息
    protected override void HandleLog(int id, Dictionary content)
    {
        if (!content.Contains("msg")) return;

        string message = content["msg"] as string;
        online_page.DisplayInfo(message);

        switch (message)
        {
            //  服务端
            case "client_connected":
                RegisterQ(id);
                GD.Print(id);
                break;
            case "client_close_request": break;
            case "client_disconnected":
                HandleClientCloseS(id);
                break;

            //  客户端
            case "connection_established":
                RegisterQ(1);
                break;
            case "server_close_request": break;
            case "connection_closed":
                CloseC();
                break;

        }
    }

    //  处理数据
    protected override void HandleData(int id, Dictionary content)
    {
        if (!content.Contains("func") || !content.Contains("params")) return;
        Array _params = content["params"] as Array;

        switch (content["func"])
        {
            case "RegisterA":
                RegisterA(id, _params);       //decode_json["params"] as Array
                break;
            case "Ready2StartC":
                Ready2StartC(id);
                break;
            default:
                GD.PrintErr("联机网络处理：没有找到函数：", content["func"]);
                break;
        }
    }

    //  ————————————————————————————————————————————————————————————————————————————————————————————————

    //  处理客户端退出
    void HandleClientCloseS(int id)
    {
        foreach (PlayerLabel it in online_page.player_label_container.GetChildren())
            if (it.Id == id)
            {
                it.QueueFree();
                break;
            }
    }


    //  客户端关闭，客户端用
    void CloseC()
    {
        network_mnger.DeleteComponent();
        online_page.ShowUIByState(false);
        online_page.host_btn.Visible = true;

        foreach (Node it in online_page.player_label_container.GetChildren())
        {
            it.Disconnect("SelectMe", online_page, "_SelectPlayer");
            it.QueueFree();
        }
    }

    //  让对方注册我的姓名。Q发送
    void RegisterQ(int tar_id)
    {
        Array p = new Array();
        p.Add(online_page.player_name_box.Text);        //  直接初始化时写会变数字？
        network_mnger.Send(tar_id, NetworkMnger.Data2JSON("RegisterA", p));
    }

    //  注册信息。A收到的回应。
    void RegisterA(int id, Array _params)
    {
        if (_params.Count < 1) return;

        var name = _params[0] as string;

        var l = online_page.scn_player_label.Instance<PlayerLabel>();
        l.PlayerName = name;
        l.Id = id;

        online_page.DisplayInfo(l.Text + " 加入");
        online_page.player_label_container.AddChild(l);
        l.Connect("SelectMe", online_page, "_SelectPlayer");
    }

    //  加载游戏准备开始。客户端
    void Ready2StartC(int id)
    {
        if (network_mnger.IsAsServer()) return;     //  不写也可？
        PackedScene game = GD.Load<PackedScene>("res://源/游戏主管/GameMnger.tscn");
        // GameMnger game_mnger = game.Instance<GameMnger>();

        Global.opposite_player_peer_id = 1;
        Global.player_name = online_page.player_name_box.Text;


        GetTree().ChangeSceneTo(game);
    }

}
