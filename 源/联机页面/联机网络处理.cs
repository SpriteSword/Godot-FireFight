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
        online_page.main.players_name_id.Remove(id);

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
        online_page.main.players_name_id.Clear();

        online_page.UpdatePlayerLabels();
        online_page.ShowUIByState(false);
        online_page.host_btn.Visible = true;
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

        online_page.main.players_name_id[id] = name;

        online_page.AddPlayerLabel(id, name);
        // online_page.DisplayInfo(l.Text + " 加入");
    }


    //  加载游戏一起开始。服务端使用
    public void Ready2StartS()
    {
        Global.opposite_player_peer_id = online_page.opposite_player_peer_id;       //  .Instance<GameMnger>() 与正式游戏实例的不是同一个？
        Global.player_name = online_page.player_name_box.Text;

        // PackedScene game = GD.Load<PackedScene>("res://源/游戏主管/GameMnger.tscn");
        // GetTree().ChangeSceneTo(game);      //  可以通过viewport来更换！

        network_mnger.Send(online_page.opposite_player_peer_id, NetworkMnger.Data2JSON("Ready2StartC", new Array()));       //  Array 写null不知可否？
        DisconnectSgnlFromNetworkMnger();
        online_page.main.EnterGame();
    }
    //  客户端
    void Ready2StartC(int id)
    {
        if (network_mnger.IsAsServer()) return;     //  不写也可？

        Global.opposite_player_peer_id = 1;
        Global.player_name = online_page.player_name_box.Text;

        // PackedScene game = GD.Load<PackedScene>("res://源/游戏主管/GameMnger.tscn");
        // GetTree().ChangeSceneTo(game);
        DisconnectSgnlFromNetworkMnger();
        online_page.main.EnterGame();
    }
}
