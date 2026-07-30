using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Common.Powers;

public class DollarPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;
    
    public override PowerStackType StackType => PowerStackType.Counter;
    
    public int DollarValue
    {
        get => Amount;
        set => SetAmount(value);
    }

    public DollarPower()
    {
        GD.Print($"[DollarPower] 构造函数被调用 - Amount={Amount}");
    }

    public override LocString Description
    {
        get
        {
            var locString = new LocString("powers", base.Id.Entry + ".description");
            locString.Add("dollar_value", Amount);
            return locString;
        }
    }

    /// <summary>
    /// 能力应用时自动挂载 BuildingDrawPower（建筑抽牌隐藏能力）。
    /// 这样任何途径获得 DollarPower（遗物、转账、矿场、油井等）都会自动获得建筑抽牌能力，
    /// 不依赖特定遗物，所有玩家均可享受此功能。
    /// </summary>
    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        if (Owner == null)
            return;

        var existingBuildingDraw = Owner.Powers.OfType<BuildingDrawPower>().FirstOrDefault();
        if (existingBuildingDraw == null)
        {
            await PowerCmd.Apply<BuildingDrawPower>(new ThrowingPlayerChoiceContext(), Owner, 1m, Owner, null);
            GD.Print("[DollarPower] 已自动挂载建筑抽牌隐藏能力");
        }
    }
    
    public void FlashPower()
    {
        Flash();
        GD.Print("[DollarPower] 刀乐能力图标闪烁");
    }
    
    public void AddDollar(int amount)
    {
        SetAmount(Amount + amount);
        GD.Print($"[DollarPower] 添加资金 {amount}，当前资金 {Amount}");
        
        if (amount > 0)
        {
            DollarVfxHelper.PlayGainVfx(Owner, amount);
        }
        else if (amount < 0)
        {
            DollarVfxHelper.PlaySpendVfx(Owner, -amount);
        }
    }
    
    public void SetDollar(int value)
    {
        SetAmount(value);
        GD.Print($"[DollarPower] 设置资金为 {Amount}");
    }
}