using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Random;
using RedAlert2ModCode.Allies;
using RedAlert2ModCode.Allies.Cards;
using RedAlert2ModCode.Soviet;
using RedAlert2ModCode.Soviet.Cards;
using RedAlert2ModCode.Common.Utils;
using System.Collections.Generic;
using System.Linq;

namespace RedAlert2ModCode.Common.Cards;

/// <summary>
/// 海军箱子 - 0费技能卡，消耗
/// 随机获得一张海军单位卡牌（升级后获得升级的海军单位卡牌）
/// 与车辆箱子结构一致，仅卡池不同
/// </summary>
public class NavyCrate : CardModel
{
    private static readonly CardValueStore.CardValues Values = CommonCardValues.NavyCrate;

    public NavyCrate() : base((int)Values.Cost, CardType.Skill, CardRarity.Token, TargetType.Self)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    public override CardPoolModel Pool => IsMutable && Owner != null
        ? Owner.Character.CardPool
        : ModelDb.CardPool<TokenCardPool>();

    public override CardPoolModel VisualCardPool => Pool;

    public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/box.png";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ModCardKeywords.Navy.CreateHoverTip(),
    ];

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new IfUpgradedVar(UpgradeDisplay.Normal)
    };

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        AudioHelper.PlayVehicleCrateSound();

        List<CardModel> ships = GetAllFactionShips();
        if (ships.Count == 0)
        {
            GD.PrintErr("[NavyCrate] 没有可用的海军单位卡");
            return;
        }

        // 使用联机同步的 RunState.Rng.CombatCardSelection
        Rng rng = Owner.RunState.Rng.CombatCardSelection;

        int index = rng.NextInt(ships.Count);
        CardModel card = ships[index];

        if (IsUpgraded)
        {
            CardCmd.Upgrade(card);
            GD.Print($"[NavyCrate] 升级海军单位: {card.Title}");
        }

        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner);
        GD.Print($"[NavyCrate] 获得海军单位: {card.Title}");
    }

    /// <summary>
    /// 获取全部阵营（苏军+盟军）的全部海军单位卡牌实例。
    /// 苏军 GetAllShips 已包含全部舰船；盟军 GetAllShips 含基础舰船，需补齐高科技舰船（航空母舰）。
    /// </summary>
    private List<CardModel> GetAllFactionShips()
    {
        var list = new List<CardModel>();

        // 苏军全部海军单位（运输船 + 防空潜艇 + 台风潜艇 + 无畏舰 + 巨型乌贼）
        foreach (var s in SovietCardRegistry.GetAllShips())
            list.Add(Owner.Creature.CombatState.CreateCard(s, Owner));

        // 盟军全部海军单位（海豚 + 运输船 + 驱逐舰 + 神盾巡洋舰）
        foreach (var s in AlliedCardRegistry.GetAllShips())
            list.Add(Owner.Creature.CombatState.CreateCard(s, Owner));

        // 盟军高科技海军单位（航空母舰）
        foreach (var s in AlliedCardRegistry.GetAllHighTechShips())
            list.Add(Owner.Creature.CombatState.CreateCard(s, Owner));

        return list;
    }
}
