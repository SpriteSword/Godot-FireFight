using Godot;
using Godot.Collections;

public class 测试Array : Node
{
	Array<int> array = new Array<int>(0, 1, 2);

	Dictionary dictionary = new Dictionary();

	public override void _Ready()
	{
		//  测试remove返回值
		// array.Add(2);
		// var r = array.Remove(1);
		// GD.Print(r);

		//  测试传入函数的形参
		Func(array);
		GD.Print(array);

		//  试验字典
		dictionary[1]="str";
		dictionary[2]="str2";

		foreach (System.Collections.DictionaryEntry item in dictionary)
		{

			GD.Print(item, item.Key, item.Value);		//  System.Collections.DictionaryEntry？
		}
	}

	void Func(Array<int> a)     //  里外是一样的
	{
		a.Add(1);
	}

}
