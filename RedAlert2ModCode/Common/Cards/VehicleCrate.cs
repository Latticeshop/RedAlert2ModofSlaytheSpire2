using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using RedAlert2ModCode.Allies;
using RedAlert2ModCode.Soviet;
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

    protected override List<DynamicVar> CanonicalVars => new();

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        AudioHelper.PlayVehicleCrateSound();

        List<CardModel> vehicles = GetFactionVehicles();
        if (vehicles.Count == 0)
        {
            GD.PrintErr("[VehicleCrate] 没有可用的装甲单位卡");
            return;
        }

        // 使用联机同步的 RunState.Rng.CombatCardSelection（GD.RandRange 联机不同步且慢）
        int index = Owner.RunState.Rng.CombatCardSelection.NextInt(vehicles.Count);
        var card = vehicles[index];
        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner);
        GD.Print($"[VehicleCrate] 获得装甲单位: {card.Title}");
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }

    private List<CardModel> GetFactionVehicles()
    {
        bool isSoviet = Owner.Character?.GetType().Name.Contains("Soviet") ?? false;
        bool isAllies = Owner.Character?.GetType().Name.Contains("Allies") ?? false;

        if (isSoviet)
        {
            var list = SovietCardRegistry.GetAllVehicles();
            return list.Select(v => Owner.Creature.CombatState.CreateCard(v, Owner)).ToList();
        }
        else if (isAllies)
        {
            var list = AlliedCardRegistry.GetAllVehicles();
            return list.Select(v => Owner.Creature.CombatState.CreateCard(v, Owner)).ToList();
        }

        return new List<CardModel>();
    }
}
