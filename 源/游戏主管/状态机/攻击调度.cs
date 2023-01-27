using Godot;
using Godot.Collections;

//  攻守方同时进入，同时退出攻击调度
//  判断守方棋子能否被攻击，攻击动画时才判断攻方棋子能否攻击

public class 攻击调度 : 游戏阶段
{
    bool been_attacked = false;      //  守方有无被攻击过





    public override void Enter()
    {
        base.Enter();
        GD.Print("攻击调度");

        //  联机且是守方则啥也不干
        if (Global.联机调试 && !IsLocalPlayerActionable()) return;

        //  如果对方棋子不能被射击
        if (!game_mnger.defender.can_be_shot)
        {
            //  警告
            Warn("警告！目标不能被射击！");
            Back2ShotStage();
            FinishQ();
            return;
        }

        //  执行攻击调度。如果转场了，就return true，就不往下执行。
        if (ConductAttackScheduling()) { return; }


        //  全部攻击完
        FinishQ();      //++++++++++++++++++++++++++++++处理攻击完棋子结果
        Back2ShotStage();
    }
    public override void UpdatePhysicsProcess(float delta) { }
    public override void UpdateProcess(float delta) { }
    public override void HandleInput(InputEvent _event) { }
    public override void HandleUnhandledInput(InputEvent _event) { }
    public override void Exit()
    {
        base.Exit();
    }

    //——————————————————————————————————————————————————————————————————————————
    //  回到攻击阶段
    void Back2ShotStage()
    {
        if (been_attacked)
            game_mnger.defender.can_be_shot = !been_attacked;
        been_attacked = false;

        if (game_mnger.CurrentStageIndex == GameMnger.Stage.直射)
            superior.ChangeTo<直射阶段>();
        else
            superior.ChangeTo<临机射击>();
    }


    #region 玩法

    //  执行攻击调度。返回是否转场到 攻击动画。没执行完攻击都会转场，执行完所有攻击了就返回到Enter()执行退出攻击调度。
    bool ConductAttackScheduling()
    {
        int c = game_mnger.attackers.Count;
        while (c > 0)       //  让可以攻击的棋子攻击。退出循环就是数组空了
        {
            Piece piece = game_mnger.attackers[c - 1];
            //  能否行动
            if (!piece.CanAct)
            {
                Warn("警告！" + piece.id.ToString() + "已执行过任务！");        //  只给攻击方看到
                goto CanNotShoot;
            }

            //  视线遮挡检查+++++++++++++++


            //  计算距离等级
            float d = Math.Hex2RectCoord(game_mnger.defender.HexPos - piece.HexPos).Length() * 50;      //  1格50m
            int d_l = JudgeDistanceLevel(d);
            if (d_l == -1)      //  太远就不用算了。没有消耗行动力。
            {
                Warn("警告！目标距离过远");
                goto CanNotShoot;
            }
            GD.Print("距离：", d, " m", "，距离级别：", d_l);

            //  攻击结果        +++++++++++++++++++++++++++++++++++++++++++++++++=网络如何？
            var result = CalculateAttackResult(piece, d_l);
            GD.Print("攻击结果：", result);

            //  满足条件可以攻击
            game_mnger.attacker = piece;
            game_mnger.attackers.RemoveAt(c - 1);       //  不用管c了，反正return
            been_attacked = true;
            EnterAttackAnimationQ();
            superior.ChangeTo<攻击动画>();
            return true;

        CanNotShoot:        //  正常不会执行到这里
            game_mnger.attackers.RemoveAt(c - 1);
            c = game_mnger.attackers.Count;
        }
        return false;
    }

    //  判断距离等级。太远的（>=3000）返回-1。
    int JudgeDistanceLevel(float distance)
    {
        int last, next;
        for (int i = 0; i < game_mnger._distance_level_.Count; i++)
        {
            if (i == game_mnger._distance_level_.Count - 1)
                return -1;

            last = game_mnger._distance_level_[i];
            next = game_mnger._distance_level_[i + 1];

            if (distance > last && distance <= next) return next;
        }
        return -1;      //  其实到不了这步
    }

    //  计算攻击结果。输入( Piece 攻击者, int 距离等级)
    GameMnger.AttackResult CalculateAttackResult(Piece attacker, int dist_level)
    {
        bool is_personnel = (game_mnger.defender.type == Piece.PieceType.人);

        //  获取攻击强度等级
        int att_level = -2;     //  没有 0 等级，-1 是无效，-2是错误！
        if (is_personnel)      //  敌方是人员。{攻击单位:{距离:等级}}
        {
            Dictionary table;
            if (attacker.side == GameMnger.Side.红)      //  攻方是红军
            {
                table = game_mnger._red_attack_blue_personnel_level_;
            }
            else        //  攻方是蓝军
            {
                table = game_mnger._blue_attack_red_personnel_level_;
            }

            var key = MatchModelName(table, attacker.model_name);
            if (key != null && table[key] is Dictionary dist_dict)
            {
                att_level = (int)(float)(dist_dict[dist_level.ToString()]);
            }
            else GD.PrintErr("棋子类型有误！ : 攻击调度");
        }
        else        //  敌方是车辆。{开火单位:{ 目标单位:{距离:等级} }}
        {
            string defender_model_n;
            if (game_mnger.defender.type == Piece.PieceType.坦克) defender_model_n = "坦克";
            else defender_model_n = "APC";

            Dictionary table;
            if (attacker.side == GameMnger.Side.红)      //  攻方是红军
            {
                table = game_mnger._red_attack_blue_vehicles_level_;
            }
            else
            {
                table = game_mnger._blue_attack_red_vehicles_level_;
            }

            var key = MatchModelName(table, attacker.model_name);       //  +++++++++++++++++++ LAW dragon RPG7 都是是什么啊？
            if (key != null && table[key] is Dictionary table1)
            {
                var key1 = MatchModelName(table1, defender_model_n);
                if (key != null && table1[key1] is Dictionary dist_dict)
                {
                    att_level = (int)(float)(dist_dict[dist_level.ToString()]);
                }
            }
        }
        if (att_level == -2) GD.PrintErr("获取攻击等级出错！ : 攻击调度");
        GD.Print("攻击等级：", att_level);

        //  掷色子
        int point = Math.ThrowDice() + Math.ThrowDice();
        GD.Print("色子：", point);

        //  判定结果
        return JudgeAttackResult(is_personnel, att_level, point);
    }

    //  判定攻击结果。查表。表：{色子:{攻击等级:结果}}
    GameMnger.AttackResult JudgeAttackResult(bool is_personnel, int attack_level, int dice_result)      //  没有C++那样const，反正也只是修饰而已，无所谓
    {
        if (attack_level <= 0) return GameMnger.AttackResult._null_;

        string result = ".";
        if (is_personnel)
        {
            if (game_mnger._anti_personnel_result_.Contains(attack_level.ToString()) &&
                game_mnger._anti_personnel_result_[attack_level.ToString()] is Dictionary dict_p)
            {
                result = dict_p[dice_result.ToString()] as string;
            }
        }
        else
        {
            if (game_mnger._anti_vehicle_result_.Contains(attack_level.ToString()) &&
                game_mnger._anti_vehicle_result_[attack_level.ToString()] is Dictionary dict_v)
            {
                result = dict_v[dice_result.ToString()] as string;
            }
        }
        switch (result)
        {
            case "K": return GameMnger.AttackResult._k_;
            case "S": return GameMnger.AttackResult._s_;
            case "KF": return GameMnger.AttackResult._kf_;
            case "KM": return GameMnger.AttackResult._km_;
            case "KMS": return GameMnger.AttackResult._kms_;
            default: return GameMnger.AttackResult._null_;
        }
    }

    //  匹配类型名称。匹配上就返回 key 键名，否则返回 null。正常输入型号像："M60 20" "tow"。字典中的 key 只有像 "M60/APC" "M60 32"这两种形式，没有"M60/APC 31"这种。
    static string MatchModelName(Dictionary dictionary, string model_name)
    {
        if (model_name == null || model_name.Empty()) return null;

        foreach (string key in dictionary.Keys)
        {
            //  分解出大小型号
            var ms = key.Split(" ", true);
            var judged_ms = model_name.Split(" ");

            var big_ms = ms[0].Split("/");		//  字典的大型号可能带"/"
            foreach (var it in big_ms)
            {
                if (judged_ms[0] == it)
                {
                    //  字典的仅有一个大型号，说明包括了所有小型号
                    if (ms.Length == 1) return key;

                    //  目标有小型号并匹配
                    if (judged_ms.Length > 1 && judged_ms[1] == ms[1]) return key;
                }
            }
        }
        return null;
    }

    #endregion

    //——————————————————————————————————————————————————————————————————————————  网络

    //  RPC分发
    protected override void _RPC(int id, Dictionary content)
    {
        if (!content.Contains("func") || !content.Contains("params")) return;
        Array _params = content["params"] as Array;

        switch (content["func"])
        {
            case "EnterAttackAnimationA":
                EnterAttackAnimationA();
                break;
            case "FinishA":
                FinishA();
                break;
            default:
                GD.PrintErr("攻击调度：没有找到函数：", content["func"]);
                break;
        }
    }

    //  通知进入攻击动画
    void EnterAttackAnimationQ()
    {
        game_mnger.Send(Global.opposite_player_peer_id, NetworkMnger.Data2JSON("EnterAttackAnimationA", null));
    }

    //  收到通知进入攻击动画
    void EnterAttackAnimationA()
    {
        been_attacked = true;
        superior.ChangeTo<攻击动画>();
    }

    //  通知结束
    void FinishQ()
    {
        game_mnger.Send(Global.opposite_player_peer_id, NetworkMnger.Data2JSON("FinishA", null));
    }

    //  收到通知结束
    void FinishA()
    {
        GD.Print("攻击调度回射击阶段");
        Back2ShotStage();
    }

}
