using Godot;
using System;

public class 测试过直线Hex : HexTileMap
{
	Vector2 target = new Vector2(9, -8);

	public override void _Ready()
	{
		GD.Print(Math.GetHexsOnLine(Vector2.Zero, target));

	}
	public override void _Draw()
	{
		for (int i = -100; i < 100; i++)
		{
			for (int j = -100; j < 100; j++)
			{
				var cell_pos = new Vector2(i, j);
				var hex_pos = Math.Cell2HexCoord(cell_pos);
				var rect_pos = HexGridCenter(cell_pos);

				Control control = new Control();
				Font default_font = control.GetFont("font");

				DrawString(default_font, rect_pos, hex_pos.ToString());

			}
		}

		DrawLine(HexGridCenter(Vector2.Zero), HexGridCenter(Math.Hex2CellCoord(target)), Colors.Beige);

	}
}
