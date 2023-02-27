using Godot;


public class 测试TileMap : HexTileMap
{

	public override void _Ready()
	{
		// GD.Print(-2%2);		//  -1 % 2 == -1 !!!
		// int a = (int)(0.5 + 0.5);
		// GD.Print(a);

		GD.Print(GetUsedCellsById(2));
		GD.Print(TileSet.GetTilesIds());
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



/*测试 图块与方向的索引是否一致
					Vector2 mou_cell_pos = game_mnger.mark.DetermineCellOfHexGrid(mouse_pos);
					// var r_c = game_mnger.road.GetCellv(mou_cell_pos);

					var c = game_mnger.road.GetCellv(mou_cell_pos);
					GD.Print(c, " ",c >= 0 ?
					GetRoadTileIndexByDirection(game_mnger._road_tile_direction_index_[c]) : -1);
*/
