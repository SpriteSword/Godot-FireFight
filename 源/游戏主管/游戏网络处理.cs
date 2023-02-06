//  函数后缀S/C代表：远程函数调用 服务器/客户端 所执行

using Godot;
using Godot.Collections;


//  自己只处理一些通用的、刚从联机页面进入时的，细节的远程调用由各控制器分别写好。
public class 游戏网络处理 : NetworkHandler
{
    [Signal] delegate void Give2External(int id, Dictionary content);		//  交给控制的来调用函数

    GameMnger game_mnger;
    TextureRect loading_screen;
    圆形进度条 progress_bar;

    public override void _Ready()
    {
        base._Ready();

        game_mnger = GetNode<GameMnger>("..");
        loading_screen = GetNode<TextureRect>("../画布层/GUI/加载画面");
        progress_bar = GetNode<圆形进度条>("../画布层/GUI/圆形进度条");

        if (Global.联机调试)
        {
            ShowLoadingScreen(true);
        }
        GD.Print("对面id: ", Global.opposite_player_peer_id);

    }

    //——————————————————————————————————————————————————————————————————————————————————————————

    //  处理错误
    protected override void HandleError(int id, Dictionary content)      //  只要有错误就退出！
    {
        network_mnger.DeleteComponent();


        if (!content.Contains("msg")) return;
        //  保存游戏退出！
        // online_page.DisplayInfo(content["msg"] as string);
    }

    //  处理普通消息
    protected override void HandleLog(int id, Dictionary content)
    {
        if (!content.Contains("msg")) return;

        string message = content["msg"] as string;
        // online_page.DisplayInfo(message);

        switch (message)
        {
            //  服务端
            case "client_connected": break;
            case "client_close_request": break;
            case "client_disconnected":
                break;

            //  客户端
            case "connection_established": break;
            case "server_close_request": break;
            case "connection_closed":
                // CloseC();
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
            case "ClientReady":
                ClientReady();
                break;
            case "StartC":
                StartC();
                break;
            default:
                EmitSignal("Give2External", id, content);
                break;
        }


    }

    //————————————————————————————————————————————————————————————————————————————————————————————————————  网络func

    //  服务器接收到客户端传来的已经准备好的信息
    void ClientReady()
    {
        game_mnger.client_ready = true;
        if (game_mnger.i_ready)
            Try2Start();
    }

    //  服务器正式开始游戏，向客户端发送正式开始
    void StartS()
    {
        network_mnger.Send(Global.opposite_player_peer_id, NetworkMnger.Data2JSON("StartC", null));

        //  自己开始
        ShowLoadingScreen(false);
    }

    //  客户端收到正式开始信息，正式开始
    void StartC()
    {
        ShowLoadingScreen(false);
    }

    //  ————————————————————————————————————————————————————————————————————————————————————————————————

    //  显示加载页面
    void ShowLoadingScreen(bool show, string txt = "游戏加载中...")
    {
        loading_screen.Visible = show;
        progress_bar.Show(show, txt);

  //+++++++++++++++++++++++degug
    }

    //  自己准备好后尝试开始
    public void Try2Start()
    {
        //  服务器
        if (network_mnger.IsAsServer())
        {
            if (game_mnger.client_ready) StartS();
            return;
        }
        //  客户端
        network_mnger.Send(1, NetworkMnger.Data2JSON("ClientReady", new Array()));
    }

}
