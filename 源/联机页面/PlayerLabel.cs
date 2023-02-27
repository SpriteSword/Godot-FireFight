using Godot;


public class PlayerLabel : Label
{
	[Signal] delegate void SelectMe(PlayerLabel me);

	string player_name;
	int id;

	bool mouse_in = false;


	public string PlayerName
	{
		get { return player_name; }
		set
		{
			player_name = value;
			Text = player_name + "(" + id.ToString() + ")";
		}
	}

	public int Id
	{
		get { return id; }
		set
		{
			id = value;
			Text = player_name + "(" + id.ToString() + ")";
		}
	}



	public override void _GuiInput(InputEvent @event)
	{
		if (@event.IsActionPressed("click_left"))
		{
			if (mouse_in)
				EmitSignal("SelectMe", this);
		}
	}


	private void _on_PlayerLabel_mouse_entered()
	{
		mouse_in = true;
	}

	private void _on_PlayerLabel_mouse_exited()
	{
		mouse_in = false;
	}

}

