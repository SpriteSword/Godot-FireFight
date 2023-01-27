using Godot;
using System;

public class 测试TileMap : HexTileMap
{

	public override void _Ready()
	{
		// GD.Print(-2%2);		//  -1 % 2 == -1 !!!
		// int a = (int)(0.5 + 0.5);
		// GD.Print(a);
	}

	public override void _PhysicsProcess(float delta)
	{
		#region 测试cell hex coord
		if (Input.IsActionJustPressed("click_left"))
		{
			Vector2 mouse_pos = GetGlobalMousePosition();


			Vector2 cell = WorldToMap(ToLocal(mouse_pos));
			Vector2 hex = Math.Cell2HexCoord(cell);

			GD.Print(cell, hex, Math.Hex2CellCoord(hex));
		}
		#endregion

		base._PhysicsProcess(delta);
	}
}
