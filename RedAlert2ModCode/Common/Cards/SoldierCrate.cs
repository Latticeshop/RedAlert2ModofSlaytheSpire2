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
/// 士兵箱子 - 0费技能卡，消耗
/// 随机获得一张士兵单位卡牌（升级后获得升级的士兵单位卡牌）
/// 与车辆箱子结构一致，仅卡池不同
/// </summary>
public class SoldierCrate : CardModel
{
    private static readonly CardValueStore.CardValues Values = CommonCardValues.SoldierCrate;

    public SoldierCrate() : base((int)Values.Cost, CardType.Skill, CardRarity.Token, TargetType.Self)
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
        ModCardKeywords.Soldier.CreateHoverTip(),
    ];

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new IfUpgradedVar(UpgradeDisplay.Normal)
    };

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        AudioHelper.PlayVehicleCrateSound();

        List<CardModel> soldiers = GetAllFactionSoldiers();
        if (soldiers.Count == 0)
        {
            GD.PrintErr("[SoldierCrate] 没有可用的士兵单位卡");
            return;
        }

        // 使用联机同步的 RunState.Rng.CombatCardSelection
        Rng rng = Owner.RunState.Rng.CombatCardSelection;

        int index = rng.NextInt(soldiers.Count);
        CardModel card = soldiers[index];

        if (IsUpgraded)
        {
            CardCmd.Upgrade(card);
            GD.Print($"[SoldierCrate] 升级士兵单位: {card.Title}");
        }

        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner);
        GD.Print($"[SoldierCrate] 获得士兵单位: {card.Title}");
    }

    /// <summary>
    /// 获取全部阵营（苏军+盟军）的全部士兵单位卡牌实例。
    /// 苏军 GetAllSoldiers 已包含基础/雷达/遗物解锁；盟军 GetAllSoldiers 已包含基础/雷达/遗物解锁/高科技。
    /// </summary>
    private List<CardModel> GetAllFactionSoldiers()
    {
        var list = new List<CardModel>();

        // 苏军全部士兵单位（基础 + 雷达 + 遗物解锁）
        foreach (var s in SovietCardRegistry.GetAllSoldiers())
            list.Add(Owner.Creature.CombatState.CreateCard(s, Owner));

        // 盟军全部士兵单位（基础 + 雷达 + 遗物解锁 + 高科技）
        foreach (var s in AlliedCardRegistry.GetAllSoldiers())
            list.Add(Owner.Creature.CombatState.CreateCard(s, Owner));

        return list;
    }
}
