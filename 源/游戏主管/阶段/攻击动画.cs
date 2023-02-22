using Godot;
using Godot.Collections;

//  进入攻击动画的棋子肯定是满足所有条件的，不用在攻击动画里面检查条件
//  攻方要等守方动画完成后才能回到攻击调度，守方动画完成后自己回到攻击调度

public class 攻击动画 : 游戏阶段
{
    Vector2 attack_vector;
    bool i_finished;
    bool dfnder_finshed;        //  Defender守方结束
    GameMnger.AttackResult attack_result;
    int dice_point;     //  骰子点数



    public override void Enter()
    {
        base.Enter();
        GD.Print("攻击动画");

        i_finished = false;
        dfnder_finshed = false;
        game_mnger.tween.Connect("tween_all_completed", this, "_TweenAllCompleted");

        //  若联机且是守方则啥也不干
        if (Global.联机调试 && !IsLocalPlayerActionable()) return;


        ConductAttack();

    }
    public override void Exit()
    {
        base.Exit();        //  子类必须执行

        game_mnger.tween.Disconnect("tween_all_completed", this, "_TweenAllCompleted");
    }


    //————————————————————————————————————————————————————————————————
    void StartAnim()
    {
        attack_vector = (game_mnger.defender.Position - game_mnger.attacker.Position).Normalized();
        game_mnger.tween.InterpolateProperty(game_mnger.attacker, "position",
                                            game_mnger.attacker.Position, game_mnger.attacker.Position + attack_vector * 18,
                                            0.15f, Tween.TransitionType.Quart, Tween.EaseType.Out);
        game_mnger.tween.InterpolateProperty(game_mnger.attacker, "position",
                                            game_mnger.attacker.Position + attack_vector * 18, game_mnger.attacker.Position,
                                            0.2f, Tween.TransitionType.Linear, Tween.EaseType.InOut, 0.15f);
        game_mnger.tween.Start();
        game_mnger.attacker.ZIndex = (int)Piece.ZIndexInStack._act_;
    }

    //  返回攻击调度
    void Back2AttackSchedule()
    {
        game_mnger.attacker.CanAct = false;
        superior.ChangeTo<攻击调度>();
    }

    //--------------------------------------------  信号

    void _TweenAllCompleted()
    {
        i_finished = true;
        game_mnger.pieces_mnger.TopAPiece(game_mnger.attacker);

        if (Global.联机调试)
        {
            //  守方
            if (!IsLocalPlayerActionable())
            {
                AnimationFinishedQ();
                Back2AttackSchedule();
                return;
            }
            //  攻方  。一定要等守方完成动画后才结束！
            if (dfnder_finshed) { Back2AttackSchedule(); }
            return;
        }
        //  单机
        Back2AttackSchedule();
    }



    #region —————————————————————————————————————————————————————————————————— 玩法

    //  执行攻击
    void ConductAttack()
    {
        //  计算距离等级
        float d = Math.Hex2RectCoord(game_mnger.defender.HexPos - game_mnger.attacker.HexPos).Length() * 50;      //  1格50m
        int d_l = JudgeDistanceLevel(d);

        //  攻击结果
        attack_result = CalculateAttackResult(game_mnger.attacker, d_l);
        GD.Print("攻击结果：", attack_result);

        game_mnger.defender.SetBeAttackedResult(attack_result);
        SynAttackQ();

        StartAnim();
    }

    //  判断距离等级。太远的（>=3000m）返回-1。
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
    GameMnger.AttackResult CalculateAttackResult(Piece attacker, int distance_level)
    {
        if (distance_level < 0) { return GameMnger.AttackResult._null_; }       //  距离过远不用算了

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

            var key = MatchModelName(table, attacker.ModelName);
            if (key != null && table[key] is Dictionary dist_dict)
            {
                att_level = (int)(float)(dist_dict[distance_level.ToString()]);
            }
            else GD.PrintErr("棋子类型有误！ : 攻击动画");
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

            var key = MatchModelName(table, attacker.ModelName);       //  +++++++++++++++++++ LAW dragon RPG7 都是是什么啊？
            if (key != null && table[key] is Dictionary table1)
            {
                var key1 = MatchModelName(table1, defender_model_n);
                if (key != null && table1[key1] is Dictionary dist_dict)
                {
                    att_level = (int)(float)(dist_dict[distance_level.ToString()]);
                }
            }
        }
        if (att_level == -2) GD.PrintErr("获取攻击等级出错！ : 攻击动画");
        GD.Print("攻击等级：", att_level);

        //  调整攻击等级
        if (attacker.BeS) att_level -= 3;

        //  掷色子
        dice_point = Math.ThrowDice() + Math.ThrowDice();
        GD.Print("色子：", dice_point);

        //  判定结果
        return JudgeAttackResult(is_personnel, att_level, dice_point);
    }

    //  判定攻击结果。查表。表：{色子:{攻击等级:结果}}。++++++++++++++++++++++步兵被多次压制、车辆多次失动 = K。
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
            case "K": return GameMnger.AttackResult.K;
            case "S": return GameMnger.AttackResult.S;
            case "KF": return GameMnger.AttackResult.KF;
            case "KM": return GameMnger.AttackResult.KM;
            case "KMS": return GameMnger.AttackResult.KMS;
            default: return GameMnger.AttackResult._null_;
        }
    }

    //  匹配字典的类号名称。匹配上就返回 key 键名，否则返回 null。正常输入型号像："M60 20" "tow"。字典中的 key 只有像 "M60/APC" "M60 32"这两种形式，没有"M60/APC 31"这种。
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

    //————————————————————————————————————————————————————————————————————  网络
    //  RPC分发
    protected override void _RPC(int id, Dictionary content)
    {
        if (!content.Contains("func") || !content.Contains("params")) return;
        Array _params = content["params"] as Array;

        switch (content["func"])
        {
            case "SynAttackA":
                SynAttackA(_params);
                break;
            case "AnimationFinishedA":
                AnimationFinishedA();
                break;
            case "Back2AttackScheduleA":
                Back2AttackScheduleA();
                break;
            default:
                GD.PrintErr("攻击动画：没有找到函数：", content["func"]);
                break;
        }
    }

    //  同步进行攻击
    void SynAttackQ()
    {
        //  参数：攻击者id，守者id，攻击结果，色子……
        Array _params = new Array();
        _params.Add(game_mnger.attacker.id);
        _params.Add(game_mnger.defender.id);
        _params.Add((int)attack_result);        //  不用写转int也行
        _params.Add(dice_point);

        game_mnger.Send(Global.opposite_player_peer_id, NetworkMnger.Data2JSON("SynAttackA", _params));
    }
    void SynAttackA(Array _params)
    {
        if (_params == null || _params.Count != 4)
        {
            GD.PrintErr("SynAttackA() 参数不合");
            return;
        }

        if (_params[0] is float p_id0)
        {
            Piece p = game_mnger.pieces_mnger.GetPiece((uint)p_id0);
            if (p == null)
            {
                GD.PrintErr("棋子id错误！"); return;
            }
            game_mnger.attacker = p;
        }

        if (_params[1] is float p_id1)
        {
            Piece p = game_mnger.pieces_mnger.GetPiece((uint)p_id1);
            if (p == null)
            {
                GD.PrintErr("棋子id错误！"); return;
            }
            game_mnger.defender = p;
        }

        GD.Print("攻守的棋子id: ", game_mnger.attacker.id, " ", game_mnger.attacker.HexPos, " | ", game_mnger.defender.id, " ", game_mnger.defender.HexPos);

        if (_params[2] is float p_r && _params[3] is float p_d)
        {
            if (p_r <= 5 && p_r >= 0)
            {
                attack_result = (GameMnger.AttackResult)p_r;
                dice_point = (int)p_d;
                game_mnger.defender.SetBeAttackedResult(attack_result);
            }
        }

        GD.Print("攻击结果：", attack_result, " ", "dice: ", dice_point);

        StartAnim();
    }

    //  通知攻方动画完成，守方使用
    void AnimationFinishedQ()
    {
        game_mnger.Send(Global.opposite_player_peer_id, NetworkMnger.Data2JSON("AnimationFinishedA", null));
    }

    //  收到守方动画完成，攻方使用
    void AnimationFinishedA()
    {
        dfnder_finshed = true;
        if (i_finished) Back2AttackSchedule();
    }

    //  通知守方返回攻击调度，攻方使用
    void Back2AttackScheduleQ()
    {
        game_mnger.Send(Global.opposite_player_peer_id, NetworkMnger.Data2JSON("Back2AttackScheduleA", null));
    }

    //  收到攻方通知返回攻击调度
    void Back2AttackScheduleA()
    {
        Back2AttackSchedule();
    }

}