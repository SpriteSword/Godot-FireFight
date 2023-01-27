// using Godot;
// using Godot.Collections;
// using System;


// public class StateMachine : Node
// {
//     protected State current_state;
//     // [Export(PropertyHint.None, "填子节点的相对路径")] string initial_state_path;

//     public override void _Ready()
//     {
//         foreach (State item in GetChildren())
//         {
//             item.superior = this;
//         }
//     }

//     public override void _UnhandledInput(InputEvent @event)
//     {
//         current_state.HandleUnhandledInput(@event);
//     }
//     public override void _Input(InputEvent @event)
//     {
//         current_state.HandleInput(@event);
//     }
//     public override void _PhysicsProcess(float delta)
//     {
//         current_state.UpdatePhysicsProcess(delta);
//     }
//     public override void _Process(float delta)
//     {
//         current_state.UpdateProcess(delta);
//     }

//     public virtual void ChangeTo<T>() where T : State
//     {
//         Type t = typeof(T);
//         State tmp = GetNode<State>(t.Name);
//         if (tmp == null) return;

//         current_state.Exit();
//         current_state = tmp;
//         current_state.Enter();
//     }


//     // public override string _GetConfigurationWarning()
//     // {
//     //     return (initial_state_path == null) ? "initial_state_path不能为空！" : "";
//     // }

// }