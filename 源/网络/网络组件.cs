using Godot;

public class 网络组件 : Node
{
    public WebSocketMultiplayerPeer component;
    public ushort port = NetworkMnger._Default_Port_;
}