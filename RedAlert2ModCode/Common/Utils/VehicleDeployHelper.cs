using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using RedAlert2ModCode.Allies.Cards;

namespace RedAlert2ModCode.Common.Utils;

/// <summary>
/// 载具转化工具 - 统一处理 IFV/防空履带车存储士兵后的载具转化逻辑
/// 供 IFV 和 FlakTrack 共用，避免重复代码
/// </summary>
public static class VehicleDeployHelper
{
    /// <summary>
    /// 将源卡牌(IFV/FlakTrack)和士兵卡牌转化为对应的特殊载具卡牌
    /// </summary>
    /// <typeparam name="TVehicle">目标载具类型（必须继承 IfvVehicleBase）</typeparam>
    /// <param name="ctx">玩家选择上下文</param>
    /// <param name="sourceCard">源卡牌（IFV 或 防空履带车）</param>
    /// <param name="soldierCard">被存储的士兵卡牌</param>
    /// <param name="owner">拥有者</param>
    public static async Task DeploySpecialVehicle<TVehicle>(
        PlayerChoiceContext ctx,
        CardModel sourceCard,
        CardModel soldierCard,
        Player owner)
        where TVehicle : IfvVehicleBase
    {
        // 1. 创建转化后的载具卡
        var vehicleTemplate = ModelDb.Card<TVehicle>();
        var vehicleCard = owner.Creature.CombatState.CreateCard(vehicleTemplate, owner);

        // 2. 源卡牌升级则载具也升级
        if (sourceCard.IsUpgraded)
        {
            CardCmd.Upgrade(vehicleCard);
        }

        // 3. 继承消耗词条（源卡牌或士兵卡牌任意一方有消耗则继承）
        if (vehicleCard is IfvVehicleBase rv)
        {
            bool hasExhaust = sourceCard.Keywords.Contains(CardKeyword.Exhaust)
                           || soldierCard.Keywords.Contains(CardKeyword.Exhaust);
            rv.SetStoredCards(sourceCard, soldierCard, hasExhaust);
        }

        // 4. 移除士兵卡和源卡，将载具卡加入手牌
        await CardPileCmd.RemoveFromCombat(soldierCard);
        await CardPileCmd.RemoveFromCombat(sourceCard);
        await CardPileCmd.AddGeneratedCardToCombat(vehicleCard, PileType.Hand, owner);

        Godot.GD.Print($"[VehicleDeployHelper] {sourceCard.GetType().Name} + {soldierCard.Title} → {typeof(TVehicle).Name}");
    }
}
