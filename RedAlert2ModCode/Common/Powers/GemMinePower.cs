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
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Common.Powers;

public class GemMinePower : PowerModel
{
    private static readonly CardValueStore.CardValues Values = CommonPowerValues.GemMinePower;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public int CurrentReserve { get; set; } = (int)Values.DollarValue;

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

    public void AddReserve(int amount)
    {
        CurrentReserve += amount;
        GD.Print($"[GemMinePower] 储备增加 {amount}，当前储备: {CurrentReserve}");
    }

    public int SpendReserve(int amount)
    {
        int spent = Mathf.Min(amount, CurrentReserve);
        CurrentReserve -= spent;
        GD.Print($"[GemMinePower] 消耗储备 {spent}，剩余储备: {CurrentReserve}");
        return spent;
    }
}