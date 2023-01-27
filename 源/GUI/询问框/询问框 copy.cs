using Godot;
using System;

public class 询问框0 : ConfirmationDialog
{
	[Signal] delegate void Yes();		//  -> _InquiryBoxYes
	[Signal] delegate void No();		//  -> _InquiryBoxNo

	//  控制者
	Node controller;

	Vector2 center_pos;

	//  子节点
	Button yes_btn;
	Button no_btn;

	public override void _Ready()
	{
		center_pos = RectPosition;
		PopupExclusive = true;
		WindowTitle = "请选择";

		yes_btn = GetOk();
		no_btn = GetCancel();

		GetCloseButton().Visible = false;
		yes_btn.Text = " 是 ";
		yes_btn.Connect("pressed", this, "_PressedYes");
		no_btn.Text = " 否 ";
		no_btn.Connect("pressed", this, "_PressedNo");
	}
	public override void _Process(float delta)
	{
		RectPosition = center_pos;
	}

	//  接受信号
	void _PopUp(string quest, Node who)
	{
		Connect("Yes", who, "_InquiryBoxYes");
		Connect("No", who, "_InquiryBoxNo");

		DialogText = quest;
		controller = who;

		Popup_();
		no_btn.GrabFocus();
	}

	void _PressedYes()
	{
		EmitSignal("Yes");
		Disconnect("Yes", controller, "_InquiryBoxYes");
		Disconnect("No", controller, "_InquiryBoxNo");
	}

	void _PressedNo()
	{
		EmitSignal("No");
		Disconnect("Yes", controller, "_InquiryBoxYes");
		Disconnect("No", controller, "_InquiryBoxNo");
	}

}


