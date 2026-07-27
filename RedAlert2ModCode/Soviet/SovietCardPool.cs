using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Scaffolding.Content;
using RedAlert2ModCode.Common.Cards;

namespace RedAlert2ModCode.Soviet;

/// <summary>
/// 苏军卡池 - 使用RitsuLib的TypeListCardPoolModel
/// 卡牌通过RegisterOwnedCardPoolAttribute属性自动注册到卡池
/// </summary>
public sealed class SovietCardPool : TypeListCardPoolModel
{
    public override string Title => "soviet";
    public override string EnergyColorName => "ironclad";
    public override bool IsColorless => false;
    
    public override string CardFrameMaterialPath => "card_frame_red";
    
    public static readonly Color Color = new("a02020");
    public override Color DeckEntryCardColor => Color;
    public override Color EnergyOutlineColor => new("801010");
    
    /// <summary>
    /// 本mod的公共卡牌类型列表（不包含原版TokenCardPool中的卡牌）
    /// </summary>
    private static readonly List<System.Type> CommonCardTypes = new()
    {
        typeof(ChronoIvanCard),
        typeof(F2A),
        typeof(ForceField),
        typeof(GemMineCard),
        typeof(GoldMineCard),
        typeof(GoldMineColumnCard),
        typeof(KitingCard),
        typeof(MassProductionCard),
        typeof(MineRaid),
        typeof(OilDerrickCard),
        typeof(Paratrooper),
        typeof(Ra2Rally),
        typeof(SellBuildingCard),
        typeof(SellMCV),
        typeof(StopProductionCard),
        typeof(SupportCard),
        typeof(UrbanizationCard),
        typeof(YuriCard),
        typeof(YuriPrimeCard),
    };
    
    /// <summary>
    /// 重写AllCards属性，包含本mod的公共卡牌（不影响原版TokenCardPool中的卡牌）
    /// 这样公共卡牌会出现在苏联角色的卡池查看器和奖励中
    /// </summary>
    public override IEnumerable<CardModel> AllCards
    {
        get
        {
            // 获取基类的卡牌列表
            var baseCards = base.AllCards;
            
            // 获取本mod的公共卡牌（通过ModelDb获取，而不是从TokenCardPool获取）
            var commonCards = CommonCardTypes.Select(type =>
            {
                var method = typeof(ModelDb).GetMethod("Card", System.Type.EmptyTypes)
                    ?.MakeGenericMethod(type);
                return method?.Invoke(null, null) as CardModel;
            }).Where(c => c != null);
            
            // 合并两个列表
            return baseCards.Concat(commonCards);
        }
    }
}
