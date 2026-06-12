using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace RedAlert2ModCode.Allies.Powers;

/// <summary>
/// 发电厂能力 - 每抽一定数量的牌获得能量
/// 参考游戏原版 AutomationPower 的实现
/// </summary>
public sealed class PowerPlantPower : PowerModel
{
    private class Data
    {
        public int cardsLeft = 10;
    }

    private const int _baseCardsLeft = 10;
    private const int _upgradedCardsLeft = 7;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>
    /// 显示剩余抽牌数
    /// </summary>
    public override int DisplayAmount => GetInternalData<Data>().cardsLeft;

    public override bool IsInstanced => true;

    /// <summary>
    /// 当前阈值（未升级10，升级7）
    /// </summary>
    public int CurrentThreshold { get; set; } = _baseCardsLeft;

    /// <summary>
    /// 使用mod资源路径
    /// </summary>
    public new string IconPath => "res://RedAlert2ModResources/images/packed/powers/power_plant_power.png";

    public new Texture2D Icon => ResourceLoader.Load<Texture2D>(IconPath, null, ResourceLoader.CacheMode.Reuse);

    protected override object InitInternalData()
    {
        var data = new Data();
        data.cardsLeft = CurrentThreshold;
        return data;
    }

    /// <summary>
    /// 设置阈值并重置计数
    /// </summary>
    public void SetThreshold(int threshold)
    {
        CurrentThreshold = threshold;
        var data = GetInternalData<Data>();
        data.cardsLeft = threshold;
        InvokeDisplayAmountChanged();
    }

    /// <summary>
    /// 抽牌后触发
    /// </summary>
    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card.Owner == base.Owner.Player && Amount > 0)
        {
            Data data = GetInternalData<Data>();
            data.cardsLeft--;
            InvokeDisplayAmountChanged();
            
            if (data.cardsLeft <= 0)
            {
                Flash();
                await PlayerCmd.GainEnergy(1, base.Owner.Player);
                data.cardsLeft = CurrentThreshold;
                InvokeDisplayAmountChanged();
            }
        }
    }
}