using Godot;


public class 测试GetChildren : Node
{
	public override void _Ready()
	{
		GD.Print(GetChildren().Contains("Sprite"));
		foreach(Node itm in GetChildren())
		{
			if(itm.Name == "Sprite")GD.Print("hh");
		}
	}


}
