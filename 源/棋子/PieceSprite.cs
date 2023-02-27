using Godot;

public class PieceSprite : TextureRect
{
	public Sprite bg;
	public Sprite body;
	public Label label;
	public Sprite symbol;


	public override void _Ready()
	{
		ReferenceChildNode();
	}


	//  引用子节点，使自己的变量初始化
	public void ReferenceChildNode()
	{
		bg = GetNode<Sprite>("Bg");
		body = GetNode<Sprite>("Body");
		label = GetNode<Label>("Label");
		symbol = GetNode<Sprite>("Symbol");
	}

	//  设置棋子的图像。要在_Ready后才能执行！！
	public void SetTexture(string model_name, Texture txtur_bg, Texture txtur_body)
	{
		label.Text = model_name;
		bg.Texture = txtur_bg;
		body.Texture = txtur_body;
	}

}
