using Godot;
using Godot.Collections;

//  先执行已计划的间接射击（动画），再计划未来的间接射击
public class 间射阶段 : 射击阶段Base
{
	public const GameMnger.Stage stage_index = GameMnger.Stage.间射;

	public override void Enter()        //  子类必须执行
	{
		base.Enter();
		GD.Print("间射阶段");
		game_mnger.CurrentStageIndex = stage_index;       //  是游戏规则里的都要写！

	}
	public override void UpdatePhysicsProcess(float delta) { }
	public override void UpdateProcess(float delta) { }
	public override void HandleInput(InputEvent _event) { }
	public override void HandleUnhandledInput(InputEvent _event) { }

	public override void Exit()     //  子类必须执行
	{
		base.Exit();
	}

}
