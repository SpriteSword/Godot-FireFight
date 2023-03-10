using Godot;
using Godot.Collections;

public class 联机页面 : Control		//+++++++++++++++++进行完后要断开连接
{
    [Signal] delegate void Inquire(string qusetion, IC command);      // -> 询问框._PopUp

    [Signal] delegate void CreateServer(ushort port);
    [Signal] delegate void Connect2Server(string address, ushort port);
    //------------------------------------------------------
    public PackedScene scn_player_label;

    //------------------------------------------------
    public Main main;
    联机网络处理 network_handler;

    public Button host_btn;
    AcceptDialog host_dialog;
    LineEdit host_port_box;

    Button join_btn;
    AcceptDialog join_dialog;
    LineEdit join_port_box;
    LineEdit join_address_box;

    TextEdit info_box;

    public LineEdit player_name_box;
    public VBoxContainer player_label_container;

    询问框 inquiry_box;


    //---------------------------------------------------
    // Dictionary player_list = new Dictionary();
    public int opposite_player_peer_id = -1;

    //-------------------------------------------------------------
    public override void _EnterTree()
    {
        scn_player_label = GD.Load<PackedScene>("res://源/联机页面/PlayerLabel.tscn");
    }

    public override void _Ready()
    {
        main = GetNode<Main>("..");
        network_handler = GetNode<联机网络处理>("联机网络处理");

        host_btn = GetNode<Button>("HostBtn");
        join_btn = GetNode<Button>("JoinBtn");

        host_dialog = GetNode<AcceptDialog>("HostDialog");
        host_port_box = GetNode<LineEdit>("HostDialog/VBoxContainer/PortHBox/LineEdit");

        join_dialog = GetNode<AcceptDialog>("JoinDialog");
        join_port_box = GetNode<LineEdit>("JoinDialog/VBoxContainer/PortHBox/LineEdit");
        join_address_box = GetNode<LineEdit>("JoinDialog/VBoxContainer/AddressHBox/LineEdit");

        info_box = GetNode<TextEdit>("Info");
        player_name_box = GetNode<LineEdit>("NameHBox/LineEdit");
        player_label_container = GetNode<VBoxContainer>("PlayerList/VBoxContainer");

        inquiry_box = GetNode<询问框>("询问框");

        host_port_box.Text = NetworkMnger._Default_Port_.ToString();
        join_port_box.Text = NetworkMnger._Default_Port_.ToString();

        InitSignal();

    }

    //  信号初始化
    void InitSignal()
    {
        Connect("Inquire", inquiry_box, "_PopUp");
        inquiry_box.Connect("Yes", this, "_InquiryBoxYes");
        inquiry_box.Connect("No", this, "_InquiryBoxNo");
    }

    //  显示UI
    public void ShowUIByState(bool is_connecting)
    {
        if (is_connecting)
        {
            host_btn.Visible = false;
            join_btn.Visible = false;
            // info_box.Visible = true;
            player_name_box.Editable = false;

            return;
        }
        host_btn.Visible = true;
        join_btn.Visible = true;
        // info_box.Visible = true;
        player_name_box.Editable = true;
    }

    //  更新信息栏
    public void DisplayInfo(string s)
    {
        info_box.Text += s + "\n";
    }

    //  更新玩家栏
    public void UpdatePlayerLabels()
    {
        foreach (Node it in player_label_container.GetChildren()) { it.QueueFree(); }

        foreach (System.Collections.DictionaryEntry itm in main.players_name_id)
        {
            AddPlayerLabel((int)itm.Key, (string)itm.Value);GD.Print((int)itm.Key, " ",(string)itm.Value);
        }
    }

    //  实例化PlayerLabel
    public void AddPlayerLabel(int id, string name)
    {
        var l = scn_player_label.Instance<PlayerLabel>();
        l.PlayerName = name;
        l.Id = id;
        l.Connect("SelectMe", this, "_SelectPlayer");
        player_label_container.AddChild(l);
    }

    //  从游戏退回主菜单这里
    public void BackHere()
    {
        Visible = true;
        network_handler.ConnectSgnlFromNetworkMnger();

		UpdatePlayerLabels();
    }

    //------------------------------------------------------------- 信号
    private void _on_HostBtn_pressed()
    {
        host_dialog.Popup_();
    }

    private void _on_JoinBtn_pressed()
    {
        join_dialog.Popup_();
    }

    //  确认创建主机
    private void _on_HostDialog_confirmed()
    {
        ShowUIByState(true);

        string port_str = host_port_box.Text;
        ushort port = port_str.Empty() ? NetworkMnger._Default_Port_ : (ushort)port_str.ToInt();
        network_handler.network_mnger.CreateServer(port);       //  会立马有信号！注意顺序！
    }

    //  确认连接主机
    private void _on_JoinDialog_confirmed()
    {
        ShowUIByState(true);

        string add = join_dialog.GetNode<LineEdit>("VBoxContainer/AddressHBox/LineEdit").Text;
        string port_str = join_dialog.GetNode<LineEdit>("VBoxContainer/PortHBox/LineEdit").Text;
        ushort port = port_str.Empty() ? NetworkMnger._Default_Port_ : (ushort)port_str.ToInt();

        network_handler.network_mnger.CreateClient(add, port);

    }

    //  选择对战的玩家
    void _SelectPlayer(PlayerLabel player_label)
    {
        opposite_player_peer_id = player_label.Id;

        if (network_handler.network_mnger.IsAsServer())
            EmitSignal("Inquire", "是否与该玩家对战？", IC._i_选择对手);

    }


    //--------------------------------------------------------  询问框返回
    protected void _InquiryBoxYes(IC command)
    {
        switch (command)
        {
            case IC._i_选择对手:
                network_handler.Ready2StartS();
                break;
        }
    }
    protected void _InquiryBoxNo(IC command)
    {
        switch (command)
        {
            case IC._i_选择对手:
                opposite_player_peer_id = -1;
                break;
        }
    }
}



