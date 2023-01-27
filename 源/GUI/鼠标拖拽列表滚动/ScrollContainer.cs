using Godot;
using System;

public class MyScrollContainer : Godot.ScrollContainer
{
	bool dragging = false;
	float start_pos = 0;
	int offset;

	Tween tween;

	public override void _Ready()
	{
		tween = GetNode<Tween>("Tween");
	}

	private void _on_ScrollContainer_gui_input(InputEventMouseButton @event)
	{
		if (@event.Pressed && @event.IsActionPressed("click_left"))     //  只有点的那一下会触发！！
		{
			dragging = true;
			start_pos = @event.Position.y;      //  相对父节点
		}
		else if (@event.IsActionReleased("click_left"))
		{
			dragging = false;
			start_pos = 0;

			//  惯性。要用回 set_v_scroll ！！！
			tween.InterpolateMethod(this, "set_v_scroll", this.ScrollVertical, this.ScrollVertical + 15 * ((offset > 0) ? -1 : 1),
									0.2f, Tween.TransitionType.Linear, Tween.EaseType.Out);
			tween.Start();
		}
		if (dragging)
		{
			offset = (int)(@event.Position.y - start_pos);
			ScrollVertical += (-offset);
			start_pos = @event.Position.y;
		}
	}


}


