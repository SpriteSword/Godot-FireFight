// using Godot;
//

// public class 临机询问 : 游戏阶段
// {
//     [Signal] delegate void Inquire(string q, 游戏阶段 me);      // -> 询问框._PopUp


//     public override void Enter()
//     {
//         base.Enter();
//         GD.Print("临机询问");
//         EmitSignal("Inquire", "是否进行临机射击？", this);

//     }
//     public override void UpdatePhysicsProcess(float delta) { }
//     public override void UpdateProcess(float delta) { }
//     public override void HandleInput(InputEvent _event) { }
//     public override void HandleUnhandledInput(InputEvent _event) { }
//     public override void Exit()
//     {
//         base.Exit();
//     }


//     //  信号。询问框返回的
//     protected override void _InquiryBoxYes()
//     {
//         superior.ChangeTo<临机射击>();
//     }

//     protected override void _InquiryBoxNo()
//     {
//         superior.ChangeTo<移动动画>();
//     }

// }
