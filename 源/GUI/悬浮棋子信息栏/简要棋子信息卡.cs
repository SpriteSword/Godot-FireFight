using Godot;

public class 简要棋子信息卡 : PieceCard
{
	[Signal] delegate void SelectMe(简要棋子信息卡 me);      //  传给 信息栏


	Label id_label;
	TextureRect be_s_symbol;
	TextureRect be_kf_symbol;
	TextureRect be_km_symbol;

	//  EmitSignalSelectMe() 用父类的

	//  引用子节点，使自己的变量初始化
	public override void ReferenceChildNode()
	{
		base.ReferenceChildNode();
		id_label = GetNode<Label>("IdLabel");
		be_s_symbol = GetNode<TextureRect>("BeSSymbol");
		be_kf_symbol = GetNode<TextureRect>("BeKFSymbol");
		be_km_symbol = GetNode<TextureRect>("BeKMSymbol");
	}

	//  更新棋子信息
	public void UpdatePieceInfo(Piece p)
	{
		piece = p;
		id_label.Text = "id: " + p.id.ToString();

		if (p.BeS) { be_s_symbol.Modulate = Colors.White; }
		if (p.BeKF) { be_kf_symbol.Modulate = Colors.White; }
		if (p.BeKM) { be_km_symbol.Modulate = Colors.White; }
	}
}
