using Godot;
using Godot.Collections;

public class NetworkMnger : Node		//+++++++++++++++++++++++++++=  居然是不知道自己的peer_id？
{
	public const ushort _Default_Port_ = 12478;


	//  外面只关心连没连上，不关心是什么原因！
	[Signal] delegate void Receive(int id, string data);


	网络组件 network_component;

	public void CreateServer(ushort _port)
	{
		if (network_component != null) return;

		network_component = new 服务端(_port);
		network_component.Name = "网络组件";
		network_component.Connect("Receive", this, "_Receive");

		AddChild(network_component);
	}

	public void CreateClient(string _server_address, ushort _port)
	{
		if (network_component != null) return;

		network_component = new 客户端(_server_address, _port);
		network_component.Name = "网络组件";
		network_component.Connect("Receive", this, "_Receive");

		AddChild(network_component);
	}

	//  删除网络组件
	public void DeleteComponent()
	{
		var n = GetNode<网络组件>("网络组件");
		if (n == null) return;
		n.QueueFree();
		network_component = null;
	}

	//  是否是作为服务器
	public bool IsAsServer()
	{
		return network_component is 服务端;
	}


	//  发送数据。不用加前缀！
	public void Send(int tar_id, string data)
	{
		if (network_component == null) return;
		network_component.component.GetPeer(tar_id).PutVar(data);
	}

	//  错误信息转成json
	public static string Error2JSON(string msg)
	{
		Dictionary dictionary = new Dictionary();
		dictionary["type"] = "err";
		dictionary["msg"] = msg;
		return JSON.Print(dictionary);
	}

	//  日志信息转成json
	public static string Log2JSON(string msg)
	{
		Dictionary dictionary = new Dictionary();
		dictionary["type"] = "log";
		dictionary["msg"] = msg;
		return JSON.Print(dictionary);
	}

	//  数据（函数调用）转成json
	public static string Data2JSON(string method, Array _params)        //  _params填null也可？
	{
		Dictionary dictionary = new Dictionary();
		dictionary["type"] = "data";
		dictionary["func"] = method;
		dictionary["params"] = _params;

		// GD.Print(JSON.Print(dictionary));
		return JSON.Print(dictionary);
	}


	//--------------------------------------------------------

	void _Receive(int id, string data)
	{
		EmitSignal("Receive", id, data);
	}


}
