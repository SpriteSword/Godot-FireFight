using Godot;
using System;

public class 测试快捷键 : Node
{

	public override void _Ready()
	{
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey i_e_k)
		{
			//  PhysicalScancode 与 Scancode 似乎是一样的
			// string s, s1;
			// s = OS.GetScancodeString(i_e_k.PhysicalScancode);
			// s1 = OS.GetScancodeString(i_e_k.Scancode);
			// GD.Print(i_e_k.PhysicalScancode, " ", s, " ", i_e_k.Scancode, " ", s1);

			string s;
			s = OS.GetScancodeString(i_e_k.PhysicalScancode);
			GD.Print(s," ", i_e_k.Echo, " ", i_e_k.Pressed," ", i_e_k.Control);
		}
	}


}
