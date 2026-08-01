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
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.Soviet;
using RedAlert2ModCode.Soviet.Cards;
using RedAlert2ModCode.Common.Utils;
using System.Collections.Generic;
using System.Linq;

namespace RedAlert2ModCode.Common.Cards;

/// <summary>
/// 空军箱子 - 0费技能卡，消耗
/// 随机获得一张空军单位卡牌（升级后获得升级的空军单位卡牌）
/// 有概率获得一层黄蜂攻击机能力（这也算飞机）
/// 与车辆箱子结构一致，仅卡池不同
/// </summary>
public class AirForceCrate : CardModel
{
    private static readonly CardValueStore.CardValues Values = CommonCardValues.AirForceCrate;

    /// <summary>黄蜂攻击机能力触发概率（百分比）</summary>
    private const int HornetPowerChance = 10;

    public AirForceCrate() : base((int)Values.Cost, CardType.Skill, CardRarity.Token, TargetType.Self)
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
        ModCardKeywords.Aircraft.CreateHoverTip(),
    ];

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new IfUpgradedVar(UpgradeDisplay.Normal)
    };

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        AudioHelper.PlayVehicleCrateSound();

        // 使用联机同步的 RunState.Rng.CombatCardSelection
        Rng rng = Owner.RunState.Rng.CombatCardSelection;

        // 黄蜂攻击机能力选项：有概率获得一层黄蜂攻击机能力（这也算飞机）
        if (rng.NextInt(100) < HornetPowerChance)
        {
            await HornetPower.ApplyHornets(Owner.Creature, 1, IsUpgraded);
            GD.Print($"[AirForceCrate] 触发黄蜂攻击机能力选项！获得1层黄蜂攻击机能力（升级={IsUpgraded}）");
            return;
        }

        List<CardModel> aircraft = GetAllFactionAircraft();
        if (aircraft.Count == 0)
        {
            GD.PrintErr("[AirForceCrate] 没有可用的空军单位卡");
            return;
        }

        int index = rng.NextInt(aircraft.Count);
        CardModel card = aircraft[index];

        if (IsUpgraded)
        {
            CardCmd.Upgrade(card);
            GD.Print($"[AirForceCrate] 升级空军单位: {card.Title}");
        }

        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner);
        GD.Print($"[AirForceCrate] 获得空军单位: {card.Title}");
    }

    /// <summary>
    /// 获取全部阵营（苏军+盟军）的全部空军单位卡牌实例。
    /// 苏军空军包含侦察机和基洛夫（基洛夫虽在重工生产序列，但属于空军而非车辆）。
    /// 盟军空军包含入侵者、黑鹰战机、夜鹰直升机。
    /// </summary>
    private List<CardModel> GetAllFactionAircraft()
    {
        var list = new List<CardModel>();

        // 苏军全部空军单位（侦察机 + 基洛夫）
        foreach (var a in SovietCardRegistry.GetAllAircraft())
            list.Add(Owner.Creature.CombatState.CreateCard(a, Owner));

        // 盟军全部空军单位（入侵者 + 黑鹰战机 + 夜鹰直升机）
        foreach (var a in AlliedCardRegistry.GetAllAircraft())
            list.Add(Owner.Creature.CombatState.CreateCard(a, Owner));

        return list;
    }
}
