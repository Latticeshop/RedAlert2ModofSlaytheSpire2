using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using System.Collections.Generic;
using System.Linq;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.Common.Powers;
using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Common.Cards;

[RegisterCard(typeof(RedAlert2ModCode.Allies.AlliesCardPool))]
[RegisterCard(typeof(RedAlert2ModCode.Soviet.SovietCardPool))]
public class GoldMineColumnCard : CardModel
{
    private static readonly CardValueStore.CardValues Values = CommonCardValues.GoldMineColumn;
    
    public GoldMineColumnCard() : base((int)Values.Cost, CardType.Power, CardRarity.Common, TargetType.Self) { }

    public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/gold_mine_column.png";

    public override CardPoolModel Pool => IsMutable && Owner != null
        ? Owner.Character.CardPool
        : ModelDb.CardPool<TokenCardPool>();

    public override CardPoolModel VisualCardPool => Pool;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.GoldMine.CreateHoverTip(),
		HoverTipFactory.FromPower<GoldMineColumnPower>()
	];

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new IntVar("Reserve", Values.DollarValue),
        new IntVar("PerTurn", Values.Stars)
    };

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        int amount = base.DynamicVars["Reserve"].IntValue;
        GD.Print($"[GoldMineColumnCard] 打出黄金矿柱，获得储备 {amount}");

        var goldMinePower = Owner.Creature.Powers.OfType<GoldMinePower>().FirstOrDefault();
        if (goldMinePower != null)
        {
            goldMinePower.AddReserve(amount);
            GD.Print($"[GoldMineColumnCard] 黄金矿储备增加 {amount}，当前储备: {goldMinePower.CurrentReserve}");
        }
        else
        {
            var newGoldMinePower = await PowerCmd.Apply<GoldMinePower>(ctx, Owner.Creature, 1m, Owner.Creature, null);
            if (newGoldMinePower != null)
            {
                newGoldMinePower.CurrentReserve = amount;
                newGoldMinePower.IsUpgraded = IsUpgraded;
                GD.Print($"[GoldMineColumnCard] 创建新黄金矿能力，初始储备: {newGoldMinePower.CurrentReserve}");
            }
        }

        var goldMineColumnPower = Owner.Creature.Powers.OfType<GoldMineColumnPower>().FirstOrDefault();
        
        if (goldMineColumnPower != null)
        {
            goldMineColumnPower.IncreasePerTurn();
            GD.Print($"[GoldMineColumnCard] 存在黄金矿柱能力，每回合产量已增加，当前每回合产量: {goldMineColumnPower.ReservePerTurn}");
        }
        else
        {
            var newPower = await PowerCmd.Apply<GoldMineColumnPower>(ctx, Owner.Creature, 1m, Owner.Creature, null);
            if (newPower != null)
            {
                GD.Print($"[GoldMineColumnCard] 创建新黄金矿柱能力，每回合产量: {newPower.ReservePerTurn}");
            }
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["Reserve"].BaseValue = Values.DollarValue + Values.DollarValueUpgraded;
        AddKeyword(CardKeyword.Innate);
    }
}