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

using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Soviet.Cards;

[RegisterCard(typeof(SovietCardPool))]
public sealed class IronCurtainCard : CardModel
{
    private static readonly CardValueStore.CardValues Values = SovietCardValues.IronCurtainCard;

    public IronCurtainCard() : base((int)Values.Cost, CardType.Power, CardRarity.Rare, TargetType.Self) { }

    public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/soviet/ironicon.png";

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new IntVar("DollarNumber", Values.DollarValue),
        new IntVar("TurnsRemaining", Values.Repeat),
        new StringVar("IronCurtainName", ModelDb.Card<IronCurtain>().Title.ToString())
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.TechLevelT3.CreateHoverTip(),
		ModCardKeywords.Building.CreateHoverTip(),
		ModCardKeywords.SovietSuperWeapon.CreateHoverTip(),
		HoverTipHelper.FromCardWithUpgrade<IronCurtain>(() => IsUpgraded)
	];

    protected override void OnUpgrade()
    {
        DynamicVars["TurnsRemaining"].BaseValue = Values.RepeatUpgraded;
        ((StringVar)DynamicVars["IronCurtainName"]).StringValue = $"{ModelDb.Card<IronCurtain>().Title.ToString()}+";
    }

    protected override bool IsPlayable
    {
        get
        {
            if (!base.IsPlayable)
                return false;

            if (!CardUtils.HasMcvPower(Owner.Creature))
                return false;

            if (!Owner.Creature.Powers.Any(p => p.GetType().Name == typeof(SovietBattleLabPower).Name))
                return false;

            var dollarPower = Owner.Creature.Powers.OfType<DollarPower>().FirstOrDefault();
            if (dollarPower == null || dollarPower.DollarValue < Values.DollarValue)
                return false;

            return true;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        GD.Print("[IronCurtainCard] OnPlay 被调用");
        BuildingSoundHelper.PlayBuildingPlaceSound();

        var dollarPower = Owner.Creature.Powers.OfType<DollarPower>().FirstOrDefault();
        if (dollarPower != null)
        {
            dollarPower.AddDollar(-(int)Values.DollarValue);
            GD.Print($"[IronCurtainCard] 扣除资金 {Values.DollarValue}");
        }

        var ironCurtainPower = await PowerCmd.Apply<IronCurtainPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, 1m, Owner.Creature, this);

        if (ironCurtainPower != null)
        {
            ironCurtainPower.IsUpgraded = IsUpgraded;
            GD.Print($"[IronCurtainCard] 已获得铁幕装置能力，升级状态: {ironCurtainPower.IsUpgraded}");
        }

        var ironCurtainTemplate = ModelDb.Card<IronCurtain>();
        var ironCurtainCard = Owner.Creature.CombatState.CreateCard(ironCurtainTemplate, Owner);
        ironCurtainCard.AddKeyword(CardKeyword.Exhaust);
        await CardPileCmd.AddGeneratedCardToCombat(ironCurtainCard, PileType.Hand, Owner);
        GD.Print("[IronCurtainCard] 已添加铁幕卡牌到手牌");
    }
}