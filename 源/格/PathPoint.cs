using Godot;

public class PathPoint : Reference
{
	public Vector2 cell_pos;
	public float remaining_m_p;     //  剩余移动点数

	public PathPoint(Vector2 cell_pos, float remaining_m_p)
	{
		this.cell_pos = cell_pos;
		this.remaining_m_p = remaining_m_p;
	}
}
