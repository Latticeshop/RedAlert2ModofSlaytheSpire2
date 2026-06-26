using Godot;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Common.Powers;

public class DollarPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;
    
    public override PowerStackType StackType => PowerStackType.Counter;
    
    public int DollarValue { get; set; } = 0;

    public DollarPower()
    {
        GD.Print($"[DollarPower] 构造函数被调用 - DollarValue={DollarValue}");
    }

    public override LocString Description
    {
        get
        {
            var locString = new LocString("powers", base.Id.Entry + ".description");
            locString.Add("dollar_value", DollarValue);
            return locString;
        }
    }
    
    public void FlashPower()
    {
        Flash();
        GD.Print("[DollarPower] 刀乐能力图标闪烁");
    }
    
    public void AddDollar(int amount)
    {
        DollarValue += amount;
        GD.Print($"[DollarPower] 添加资金 {amount}，当前资金 {DollarValue}");
        
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
        DollarValue = value;
        GD.Print($"[DollarPower] 设置资金为 {DollarValue}");
    }
}