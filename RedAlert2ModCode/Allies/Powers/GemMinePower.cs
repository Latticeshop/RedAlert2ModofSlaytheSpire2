using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using RedAlert2ModCode.Utils;

namespace RedAlert2ModCode.Allies.Powers;

/// <summary>
/// 宝石矿能力 - 存储宝石矿储备
/// 当矿车打出时会额外增加储备（比黄金矿多）
/// </summary>
public class GemMinePower : PowerModel
{
    private static readonly CardValueStore.CardValues Values = AlliesPowerValues.GemMinePower;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    /// <summary>
    /// 当前储备金额
    /// </summary>
    public int CurrentReserve { get; set; } = (int)Values.DollarValue;

    /// <summary>
    /// 是否升级
    /// </summary>
    public bool IsUpgraded { get; set; } = false;

    public GemMinePower()
    {
        GD.Print($"[GemMinePower] 构造函数被调用 - Reserve={CurrentReserve}");
    }

    public new string PackedIconPath => "res://RedAlert2ModResources/images/packed/powers/gem_mine_power.png";

    public override LocString Description
    {
        get
        {
            var locString = new LocString("powers", base.Id.Entry + ".description");
            locString.Add("Reserve", CurrentReserve);
            return locString;
        }
    }

    /// <summary>
    /// 添加储备
    /// </summary>
    public void AddReserve(int amount)
    {
        CurrentReserve += amount;
        GD.Print($"[GemMinePower] 储备增加 {amount}，当前储备: {CurrentReserve}");
    }

    /// <summary>
    /// 消耗储备
    /// </summary>
    /// <param name="amount">消耗数量</param>
    /// <returns>实际消耗的数量</returns>
    public int SpendReserve(int amount)
    {
        int spent = Mathf.Min(amount, CurrentReserve);
        CurrentReserve -= spent;
        GD.Print($"[GemMinePower] 消耗储备 {spent}，剩余储备: {CurrentReserve}");
        return spent;
    }
}