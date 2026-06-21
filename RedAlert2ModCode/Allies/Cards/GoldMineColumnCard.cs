using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
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
using RedAlert2ModCode.Utils;
using RedAlert2ModCode.Allies.Powers;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 黄金矿柱 - 运转卡/能力卡
/// 1费，增加黄金矿储备并获得黄金矿柱能力（每回合增加储备）
/// 逻辑：不取消黄金矿能力，只将储备加到黄金矿上，矿柱能力负责每回合增加储备
/// </summary>
public sealed class GoldMineColumnCard : CardModel
{
    private static readonly CardValueStore.CardValues Values = AlliesCardValues.GoldMineColumn;
    
    public GoldMineColumnCard() : base((int)Values.Cost, CardType.Power, CardRarity.Common, TargetType.Self) { }

    public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/gold_mine_column.png";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ModCardKeywords.GoldMine.CreateHoverTip(),
        ModCardKeywords.GoldMineColumn.CreateHoverTip()
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

        // 将储备加到黄金矿上（如果没有黄金矿能力，则创建一个）
        var goldMinePower = Owner.Creature.Powers.OfType<GoldMinePower>().FirstOrDefault();
        if (goldMinePower != null)
        {
            // 如果存在黄金矿能力，增加储备
            goldMinePower.AddReserve(amount);
            GD.Print($"[GoldMineColumnCard] 黄金矿储备增加 {amount}，当前储备: {goldMinePower.CurrentReserve}");
        }
        else
        {
            // 如果没有黄金矿能力，创建一个新的黄金矿能力
            var newGoldMinePower = await PowerCmd.Apply<GoldMinePower>(ctx, Owner.Creature, 1m, Owner.Creature, null);
            if (newGoldMinePower != null)
            {
                newGoldMinePower.CurrentReserve = amount;
                newGoldMinePower.IsUpgraded = IsUpgraded;
                GD.Print($"[GoldMineColumnCard] 创建新黄金矿能力，初始储备: {newGoldMinePower.CurrentReserve}");
            }
        }

        // 检查是否存在黄金矿柱能力
        var goldMineColumnPower = Owner.Creature.Powers.OfType<GoldMineColumnPower>().FirstOrDefault();
        
        if (goldMineColumnPower != null)
        {
            // 如果存在黄金矿柱能力，增加每回合产量（叠加效果）
            goldMineColumnPower.IncreasePerTurn();
            GD.Print($"[GoldMineColumnCard] 存在黄金矿柱能力，每回合产量已增加，当前每回合产量: {goldMineColumnPower.ReservePerTurn}");
        }
        else
        {
            // 创建新的黄金矿柱能力
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
    }
}