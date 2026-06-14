using Godot;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace RedAlert2ModCode.Allies.Powers;

/// <summary>
/// 刀乐能力
/// 用于存储资金数值，无实际效果
/// </summary>
public class DollarPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;
    
    public override PowerStackType StackType => PowerStackType.Counter;
    
    // 当前资金值
    public int DollarValue { get; set; } = 0;

    public DollarPower()
    {
        GD.Print($"[DollarPower] 构造函数被调用 - DollarValue={DollarValue}");
    }

    /// <summary>
    /// 本地化描述
    /// </summary>
    public override LocString Description
    {
        get
        {
            var locString = new LocString("powers", base.Id.Entry + ".description");
            locString.Add("dollar_value", DollarValue);
            return locString;
        }
    }
    
    /// <summary>
    /// 添加资金
    /// </summary>
    public void AddDollar(int amount)
    {
        DollarValue += amount;
        GD.Print($"[DollarPower] 添加资金 {amount}，当前资金 {DollarValue}");
    }
    
    /// <summary>
    /// 设置资金
    /// </summary>
    public void SetDollar(int value)
    {
        DollarValue = value;
        GD.Print($"[DollarPower] 设置资金为 {DollarValue}");
    }
}