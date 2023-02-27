
using Godot;
// using System.Collections.Generic;
// using System.Linq;
using System.Runtime.InteropServices;
// using System.Text;


public class CppDLL : Godot.Node        //  写 CppDll好像重名？
{

    [DllImport("fun.dll", EntryPoint = "Add", CallingConvention = CallingConvention.Cdecl)]
    public static extern int Add(int a, int b);

    // [DllImport("Dll1.dll", EntryPoint = "Pow", CallingConvention = CallingConvention.Cdecl)]
    // public static extern void Pow(ref int x, double y);

}

//+++++++++++++++dll只能放在程序同一目录?