using Godot;


public class 测试打开文件 : Node
{

	public override void _Ready()
	{
		GD.Print(OS.GetUserDataDir());      //   C:/Users/xxx/AppData/Roaming/Godot/app_userdata/火力战
		GD.Print(OS.GetExecutablePath());       //  D:/Program Files/Godot_v3.5.1-stable_mono_win64/Godot_v3.5.1-stable_mono_win64.exe

		// OS.Execute(OS.GetExecutablePath()+"../1.txt", null, false);

		// OS.Execute("user://1.txt", null, false);
		// OS.Execute("CMD.exe", new string[] { "/C", "cd %TEMP% && dir" }, true);
		// OS.Execute("CMD.exe", new string[] { "/C", "D:\\1\\火力战规则/火力战中文快速规则.pdf" }, true);

		string s = OS.GetExecutablePath() + "/../1.txt";
		GD.Print(s);
		var r = OS.Execute("CMD.exe", new string[] { "/C", s }, true);
		// var r = OS.Execute("CMD.exe", new string[] { "/C", "D:\\1\\火力战规则/火力战中文快速规则.pdf" }, true);

		GD.PrintErr(r);
		// Error.CantFork
	}

}
