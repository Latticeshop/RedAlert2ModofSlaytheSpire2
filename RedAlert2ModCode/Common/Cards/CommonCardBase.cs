using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;

namespace RedAlert2ModCode.Common.Cards;

/// <summary>
/// 公共卡牌基类 - 同时注册给盟军和苏联两个阵营的卡牌
/// 使用 ModHelper.AddModelToPool 注册到 TokenCardPool
/// 运行时根据持有者的角色阵营动态显示卡框颜色
/// </summary>
public abstract class CommonCardBase : CardModel
{
    protected CommonCardBase(int cost, CardType type, CardRarity rarity, TargetType target) 
        : base(cost, type, rarity, target) { }

    /// <summary>
    /// 运行时卡池：当卡牌有所有者时，返回所有者角色的卡池；否则返回TokenCardPool
    /// </summary>
    public override CardPoolModel Pool => IsMutable && Owner != null
        ? Owner.Character.CardPool
        : ModelDb.CardPool<TokenCardPool>();

    /// <summary>
    /// 视觉卡池：用于确定卡牌的边框颜色等视觉表现
    /// 运行时与Pool相同，卡池查看器中通过重写AllCards属性实现显示
    /// </summary>
    public override CardPoolModel VisualCardPool => Pool;
}
