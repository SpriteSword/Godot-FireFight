using Godot;
using System;

public class PieceSprite : Sprite
{
	public Sprite bg;
	public Sprite body;
	public Label label;
	public Sprite symbol;


	public override void _Ready()
	{
		bg = GetNode<Sprite>("Bg");
		body = GetNode<Sprite>("Body");
		label = GetNode<Label>("Label");
		symbol = GetNode<Sprite>("Symbol");
	}
}
