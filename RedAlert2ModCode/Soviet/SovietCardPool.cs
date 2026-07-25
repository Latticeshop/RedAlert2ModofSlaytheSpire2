using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Scaffolding.Content;

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
    /// 重写AllCards属性，包含TokenCardPool中的公共卡牌
    /// 这样公共卡牌会出现在苏联角色的卡池查看器和奖励中
    /// </summary>
    public override IEnumerable<CardModel> AllCards
    {
        get
        {
            // 获取基类的卡牌列表
            var baseCards = base.AllCards;
            // 获取TokenCardPool中的公共卡牌
            var commonCards = ModelDb.CardPool<TokenCardPool>().AllCards;
            // 合并两个列表
            return baseCards.Concat(commonCards);
        }
    }
}
