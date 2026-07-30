using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using RedAlert2ModCode.Common.Utils;
using System.Collections.Generic;
using System.Linq;

namespace RedAlert2ModCode.Common.Cards;

public class HealCrate : CardModel
{
    private static readonly CardValueStore.CardValues Values = CommonCardValues.HealCrate;

    public HealCrate() : base((int)Values.Cost, CardType.Skill, CardRarity.Uncommon, TargetType.Self) {}

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    public override CardPoolModel Pool => IsMutable && Owner != null
        ? Owner.Character.CardPool
        : ModelDb.CardPool<TokenCardPool>();

    public override CardPoolModel VisualCardPool => Pool;

    public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/box.png";

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[0];

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new IntVar("HealAmount", (int)Values.Damage)
    };

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        AudioHelper.PlayHealAllSound();

        int healAmount = IsUpgraded ? (int)(Values.Damage + Values.DamageUpgraded) : (int)Values.Damage;
        GD.Print($"[HealCrate] 治疗队友，每人 {healAmount} 血量");

        var combatState = Owner.Creature.CombatState;
        var teammates = combatState.GetTeammatesOf(Owner.Creature).Where(c => c.IsAlive && c != Owner.Creature).ToList();

        foreach (var teammate in teammates)
        {
            await CreatureCmd.Heal(teammate, healAmount);
            GD.Print($"[HealCrate] 治疗队友 {healAmount} 血量");
        }

        await CreatureCmd.Heal(Owner.Creature, healAmount);
        GD.Print($"[HealCrate] 治疗自己 {healAmount} 血量");
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
