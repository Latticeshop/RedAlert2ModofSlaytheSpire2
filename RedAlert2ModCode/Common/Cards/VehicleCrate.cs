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

public class VehicleCrate : CardModel
{
    private static readonly CardValueStore.CardValues Values = CommonCardValues.VehicleCrate;

    public VehicleCrate() : base((int)Values.Cost, CardType.Skill, CardRarity.Common, TargetType.Self)
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
        ModCardKeywords.Vehicle.CreateHoverTip(),
    ];

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new IfUpgradedVar(UpgradeDisplay.Normal)
    };

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        AudioHelper.PlayVehicleCrateSound();

        List<CardModel> vehicles = GetAllFactionVehicles();
        if (vehicles.Count == 0)
        {
            GD.PrintErr("[VehicleCrate] 没有可用的装甲单位卡");
            return;
        }

        // 使用联机同步的 RunState.Rng.CombatCardSelection（GD.RandRange 联机不同步且慢）
        Rng rng = Owner.RunState.Rng.CombatCardSelection;

        CardModel card;
        // MCV 概率极低：2% 概率抽到 MCV（苏军/盟军 MCV 各 1%），98% 概率从全部阵营装甲单位池随机
        if (rng.NextInt(100) < 2)
        {
            var mcvs = new List<CardModel>
            {
                Owner.Creature.CombatState.CreateCard(ModelDb.Card<SovietMCV>(), Owner),
                Owner.Creature.CombatState.CreateCard(ModelDb.Card<AlliedMCV>(), Owner),
            };
            card = mcvs[rng.NextInt(mcvs.Count)];
            GD.Print("[VehicleCrate] 极低概率触发：获得 MCV！");
        }
        else
        {
            int index = rng.NextInt(vehicles.Count);
            card = vehicles[index];
        }

        if (IsUpgraded)
        {
            CardCmd.Upgrade(card);
            GD.Print($"[VehicleCrate] 升级装甲单位: {card.Title}");
        }

        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner);
        GD.Print($"[VehicleCrate] 获得装甲单位: {card.Title}");
    }

    /// <summary>
    /// 获取全部阵营（苏军+盟军）的全部装甲单位卡牌实例。
    /// 注意：苏军 GetAllVehicles 已包含基础/雷达/高科技；盟军 GetAllVehicles 仅含基础，需补齐雷达与高科技。
    /// </summary>
    private List<CardModel> GetAllFactionVehicles()
    {
        var list = new List<CardModel>();

        // 苏军全部装甲单位（基础 + 雷达 + 高科技）
        foreach (var v in SovietCardRegistry.GetAllVehicles())
            list.Add(Owner.Creature.CombatState.CreateCard(v, Owner));

        // 盟军全部装甲单位（基础 + 雷达 + 高科技）
        foreach (var v in AlliedCardRegistry.GetAllVehicles())
            list.Add(Owner.Creature.CombatState.CreateCard(v, Owner));
        foreach (var v in AlliedCardRegistry.GetAllRadarVehicles())
            list.Add(Owner.Creature.CombatState.CreateCard(v, Owner));
        foreach (var v in AlliedCardRegistry.GetAllHighTechVehicles())
            list.Add(Owner.Creature.CombatState.CreateCard(v, Owner));

        return list;
    }
}
