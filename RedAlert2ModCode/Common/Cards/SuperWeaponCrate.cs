using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using RedAlert2ModCode.Allies.Cards;
using RedAlert2ModCode.Soviet.Cards;
using RedAlert2ModCode.Common.Utils;
using System.Collections.Generic;
using System.Linq;

namespace RedAlert2ModCode.Common.Cards;

public class SuperWeaponCrate : CardModel
{
    private static readonly CardValueStore.CardValues Values = CommonCardValues.SuperWeaponCrate;

    public SuperWeaponCrate() : base((int)Values.Cost, CardType.Attack, CardRarity.Token, TargetType.Self) {}

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    public override CardPoolModel Pool => IsMutable && Owner != null
        ? Owner.Character.CardPool
        : ModelDb.CardPool<TokenCardPool>();

    public override CardPoolModel VisualCardPool => Pool;

    public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/box.png";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ModCardKeywords.AlliedSuperWeapon.CreateHoverTip(),
        ModCardKeywords.SovietSuperWeapon.CreateHoverTip()
    ];

    protected override List<DynamicVar> CanonicalVars => new();

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var superWeapons = GetSuperWeaponCards();
        if (superWeapons.Count == 0)
        {
            GD.PrintErr("[SuperWeaponCrate] 没有可用的超武卡");
            return;
        }

        int index = (int)GD.RandRange(0, superWeapons.Count - 1);
        var card = superWeapons[index];
        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner);
        GD.Print($"[SuperWeaponCrate] 获得超武: {card.Title}");
    }

    private List<CardModel> GetSuperWeaponCards()
    {
        bool isSoviet = Owner.Character?.GetType().Name.Contains("Soviet") ?? false;
        bool isAllies = Owner.Character?.GetType().Name.Contains("Allies") ?? false;

        var superWeaponTypes = new List<System.Type>();

        superWeaponTypes.Add(typeof(IronCurtain));
        superWeaponTypes.Add(typeof(NuclearAttack));
        superWeaponTypes.Add(typeof(LightningStorm));
        superWeaponTypes.Add(typeof(ChronoWarp));

        var result = new List<CardModel>();
        foreach (var type in superWeaponTypes)
        {
            var method = typeof(ModelDb).GetMethod("Card", System.Type.EmptyTypes)?.MakeGenericMethod(type);
            var template = (CardModel)method?.Invoke(null, null);
            var card = Owner.Creature.CombatState.CreateCard(template, Owner);
            if (card != null)
            {
                card.EnergyCost.SetCustomBaseCost(0);
                card.AddKeyword(CardKeyword.Exhaust);
            }
            result.Add(card);
        }

        return result;
    }
}
