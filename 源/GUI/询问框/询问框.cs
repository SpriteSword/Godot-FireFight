using Godot;
using System;

public class 询问框 : ConfirmationDialog
{
    [Signal] delegate void Yes(IC command);       //  -> _InquiryBoxYes
    [Signal] delegate void No(IC command);        //  -> _InquiryBoxNo


    // public Node controller;		//  控制者
	IC command;		//  命令

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

	//  接受信号，控制者发信号叫弹出
    void _PopUp(string question, IC command)
    {
        DialogText = question;
		this.command = command;

        Popup_();
        no_btn.GrabFocus();
    }

	//  按钮传回
    void _PressedYes()
    {
        EmitSignal("Yes", command);
    }

    void _PressedNo()
    {
        EmitSignal("No", command);
    }

}


