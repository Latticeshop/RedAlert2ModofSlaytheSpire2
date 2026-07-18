#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using RedAlert2ModCode.Soviet.Powers;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Soviet.Cards;

public sealed class NuclearMissileSiloCard : CardModel
{
    private static readonly CardValueStore.CardValues Values = SovietCardValues.NuclearMissileSiloCard;

    public NuclearMissileSiloCard() : base((int)Values.Cost, CardType.Power, CardRarity.Rare, TargetType.Self) { }

    public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/soviet/msslicon.png";

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new IntVar("DollarNumber", Values.DollarValue),
        new IntVar("TurnsRemaining", Values.Repeat),
        new StringVar("NuclearAttackName", ModelDb.Card<NuclearAttack>().Title.ToString())
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.TechLevelT3.CreateHoverTip(),
		ModCardKeywords.Building.CreateHoverTip(),
		ModCardKeywords.SovietSuperWeapon.CreateHoverTip(),
		HoverTipHelper.FromCardWithUpgrade<NuclearAttack>(() => IsUpgraded)
	];

    protected override void OnUpgrade()
    {
        DynamicVars["TurnsRemaining"].BaseValue = Values.RepeatUpgraded;
        ((StringVar)DynamicVars["NuclearAttackName"]).StringValue = $"{ModelDb.Card<NuclearAttack>().Title.ToString()}+";
    }

    protected override bool IsPlayable
    {
        get
        {
            if (!base.IsPlayable)
                return false;

            if (!CardUtils.HasMcvPower(Owner.Creature))
                return false;

            if (!Owner.Creature.Powers.Any(p => p is SovietBattleLabPower))
                return false;

            var dollarPower = Owner.Creature.Powers.OfType<DollarPower>().FirstOrDefault();
            if (dollarPower == null || dollarPower.DollarValue < Values.DollarValue)
                return false;

            return true;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        GD.Print("[NuclearMissileSiloCard] OnPlay 被调用");
        BuildingSoundHelper.PlayBuildingPlaceSound();

        var dollarPower = Owner.Creature.Powers.OfType<DollarPower>().FirstOrDefault();
        if (dollarPower != null)
        {
            dollarPower.AddDollar(-(int)Values.DollarValue);
            GD.Print($"[NuclearMissileSiloCard] 扣除资金 {Values.DollarValue}");
        }

        var nuclearMissileSiloPower = await PowerCmd.Apply<NuclearMissileSiloPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, 1m, Owner.Creature, this);

        if (nuclearMissileSiloPower != null)
        {
            nuclearMissileSiloPower.IsUpgraded = IsUpgraded;
            GD.Print($"[NuclearMissileSiloCard] 已获得核弹井能力，升级状态: {nuclearMissileSiloPower.IsUpgraded}");
        }

        var nuclearAttackTemplate = ModelDb.Card<NuclearAttack>();
        var nuclearAttackCard = Owner.Creature.CombatState.CreateCard(nuclearAttackTemplate, Owner);
        nuclearAttackCard.AddKeyword(CardKeyword.Exhaust);
        await CardPileCmd.AddGeneratedCardToCombat(nuclearAttackCard, PileType.Hand, Owner);
        GD.Print("[NuclearMissileSiloCard] 已添加核弹攻击卡牌到手牌");

        await CardPileCmd.Draw(ctx, 1, Owner);
    }
}