using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using RedAlert2ModCode.Allies.Cards;

namespace RedAlert2ModCode.Allies;

/// <summary>
/// 盟军卡池 - 继承自原版CardPoolModel，手动注册所有卡牌
/// </summary>
public sealed class AlliesCardPool : CardPoolModel
{
    public override string Title => "allies";
    public override string EnergyColorName => "defect"; // 使用故障机器人的蓝色能量图标
    public override bool IsColorless => false;
    
    // 卡牌框材质路径 - 使用蓝色框
    public override string CardFrameMaterialPath => "card_frame_blue";
    
    // 角色颜色配置
    public static readonly Color Color = new("2060a0");
    public override Color DeckEntryCardColor => Color;
    public override Color EnergyOutlineColor => new("103080");

    /// <summary>
    /// 注册所有盟军卡牌
    /// </summary>
    protected override CardModel[] GenerateAllCards()
    {
        return new CardModel[]
        {
            ModelDb.Card<AmericanSoldier>(),
            ModelDb.Card<GrizzlyTank>(),
            ModelDb.Card<AlliedMCV>()
        };
    }
}