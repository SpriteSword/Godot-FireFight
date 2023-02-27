using Godot;


public class MyCamera : Camera2D
{
	float view_zoom = 1f;        //  zoom 变焦
	public bool camera_drag = false;       //  相机是否拖拽
	Vector2 camera_old_pos;

	//  鼠标
	Vector2 mouse_pos;
	Vector2 mouse_screen_pos;
	Vector2 mouse_screen_old_pos;

	[Export]
	public float ViewZoom
	{
		get { return view_zoom; }
		set
		{
			view_zoom = value;
			Zoom = new Vector2(view_zoom, view_zoom);
		}
	}


	public override void _Process(float delta)
	{
		if (camera_drag)
		{
			Position = camera_old_pos - (mouse_screen_pos - mouse_screen_old_pos) * view_zoom;
		}
	}
	public override void _UnhandledInput(InputEvent @event)     //  通用的写这里
	{
		if (@event is InputEventMouse m)
		{
			mouse_pos = GetGlobalMousePosition();        //  用get_global_mouse_position()会以其画布层为原点！
			mouse_screen_pos = m.Position;

			if (m is InputEventMouseButton mb)        //  鼠标按键
			{
				switch (mb.ButtonIndex)
				{
					case (int)ButtonList.WheelDown:     //  滚轮向下(后)缩小
						if (mb.Pressed && view_zoom < 2.5)
						{
							ViewZoom += 0.1f;
							Aim(mouse_pos);
						}
						break;

					case (int)ButtonList.WheelUp:       //  向上(前)放大
						if (mb.Pressed && view_zoom > 0.6)
						{
							ViewZoom -= 0.1f;
							Aim(mouse_pos);
						}
						break;

					case (int)ButtonList.Middle:        //  滚轮按下拖拽视角。坐标移动在_Process
						if (mb.Pressed)
						{
							camera_drag = true;
							mouse_screen_old_pos = mouse_screen_pos;
							camera_old_pos = Position;
						}
						else { camera_drag = false; }
						break;
				}
			}
		}//  InputEventMouse
	}

	//  对准
	void Aim(Vector2 mou_pos)
	{
		Position += (mou_pos - GetGlobalMousePosition());
		ForceUpdateScroll();
	}
}
