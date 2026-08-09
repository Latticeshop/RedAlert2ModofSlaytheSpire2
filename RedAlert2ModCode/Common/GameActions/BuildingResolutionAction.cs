using System;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Runs;
using RedAlert2ModCode.Allies.Cards;
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.Common.Cards;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.Soviet.Cards;
using RedAlert2ModCode.Soviet.Powers;

namespace RedAlert2ModCode.Common.GameActions;

/// <summary>
/// A2 预选模式的结算动作：在玩家于预选面板确认后入队，按打出建筑 + 选择结果执行效果。
/// 与转账一样，选择结果作为动作载荷随入队握手同步，所有端确定性执行。
/// </summary>
public class BuildingResolutionAction : GameAction
{
    public override ulong OwnerId => Owner.NetId;

    public override GameActionType ActionType => GameActionType.CombatPlayPhaseOnly;

    public Player Owner { get; }

    public string BuildingEntry { get; }

    public bool IsUpgraded { get; }

    /// <summary>生产建筑/基地车：单位/建筑卡牌 Entry；出售建筑：能力类型名。</summary>
    public string[] Entries { get; }

    public int[] Counts { get; }

    public BuildingResolutionAction(Player owner, string buildingEntry, bool isUpgraded, string[] entries, int[] counts)
    {
        Owner = owner;
        BuildingEntry = buildingEntry;
        IsUpgraded = isUpgraded;
        Entries = entries ?? Array.Empty<string>();
        Counts = counts ?? Array.Empty<int>();
    }

    protected override async Task ExecuteAction()
    {
        var creature = Owner?.Creature;
        if (creature == null || creature.CombatState == null)
            return;

        try
        {
            if (BuildingEntry == EntryOf<AlliesBarracksCard>())
            {
                await ResolveProduction(creature, "allies_barracks");
                return;
            }
            if (BuildingEntry == EntryOf<SovietBarracksCard>())
            {
                await ResolveProduction(creature, "soviet_barracks");
                return;
            }
            if (BuildingEntry == EntryOf<AlliedWarFactory>())
            {
                await ResolveProduction(creature, "allied_war_factory");
                return;
            }
            if (BuildingEntry == EntryOf<SovietWarFactory>())
            {
                await ResolveProduction(creature, "soviet_war_factory");
                return;
            }
            if (BuildingEntry == EntryOf<AlliesShipyardCard>())
            {
                await ResolveProduction(creature, "allies_shipyard");
                return;
            }
            if (BuildingEntry == EntryOf<SovietShipyardCard>())
            {
                await ResolveProduction(creature, "soviet_shipyard");
                return;
            }
            if (BuildingEntry == EntryOf<AirForceCommand>())
            {
                await ResolveProduction(creature, "air_force_command");
                return;
            }
            if (BuildingEntry == EntryOf<AlliedMCV>())
            {
                await ResolveMcv(creature, isSoviet: false);
                return;
            }
            if (BuildingEntry == EntryOf<SovietMCV>())
            {
                await ResolveMcv(creature, isSoviet: true);
                return;
            }
            if (BuildingEntry == EntryOf<SellBuildingCard>())
            {
                await SellBuildingCard.ResolveSoldPowers(Owner, IsUpgraded, Entries, Counts);
            }
        }
        catch (Exception ex)
        {
            Godot.GD.PrintErr($"[BuildingResolutionAction] 结算失败: {ex}");
        }
    }

    private static string EntryOf<T>() where T : CardModel
    {
        return ModelDb.Card<T>().Id.Entry;
    }

    private async Task ResolveProduction(Creature creature, string buildingKey)
    {
        var dollarPower = creature.Powers.OfType<DollarPower>().FirstOrDefault();
        int cost = GetBuildingCost(buildingKey);
        if (dollarPower == null || dollarPower.DollarValue < cost)
        {
            Godot.GD.Print($"[BuildingResolutionAction] 资金不足，取消结算（{buildingKey}）");
            return;
        }
        dollarPower.AddDollar(-cost);
        Godot.GD.Print($"[BuildingResolutionAction] 扣除建筑资金 {cost}（{buildingKey}）");

        await ApplyBuildingPower(creature, buildingKey);

        for (int i = 0; i < Entries.Length && i < Counts.Length; i++)
        {
            string entry = Entries[i];
            int count = Counts[i];
            if (count <= 0 || string.IsNullOrEmpty(entry))
                continue;

            var model = CardUtils.GetCardModelByEntry(entry);
            if (model == null)
            {
                Godot.GD.PrintErr($"[BuildingResolutionAction] 无法解析单位卡牌: {entry}");
                continue;
            }

            int unitPrice = GetUnitPrice(entry, buildingKey);
            bool exhaust = GetExhaust(buildingKey, entry);

            await TrainingQueuePower.ApplyTrainingQueue(
                owner: creature,
                cardId: entry,
                unitName: model.Title.ToString(),
                iconPath: model.PortraitPath,
                unitPrice: unitPrice,
                isUpgraded: IsUpgraded,
                sourceCard: null,
                exhaustWhenPlayed: exhaust,
                isStopped: false,
                amount: count);
            Godot.GD.Print($"[BuildingResolutionAction] 创建生产序列 - {entry} × {count}");
        }

        // 空指部：美国国旗额外给一张空降部队
        if (buildingKey == "air_force_command" && FlagManager.HasUSA(Owner))
        {
            var airborne = Owner.Creature.CombatState.CreateCard(ModelDb.Card<AirborneDivision>(), Owner);
            if (IsUpgraded && !airborne.IsUpgraded)
                CardCmd.Upgrade(airborne);
            await CardPileCmd.AddGeneratedCardToCombat(airborne, PileType.Hand, Owner);
        }
    }

    private async Task ResolveMcv(Creature creature, bool isSoviet)
    {
        string selectedEntry = Entries.FirstOrDefault();
        if (string.IsNullOrEmpty(selectedEntry))
        {
            Godot.GD.Print("[BuildingResolutionAction] MCV 未选择建筑");
            return;
        }

        var model = CardUtils.GetCardModelByEntry(selectedEntry);
        if (model == null)
        {
            Godot.GD.PrintErr($"[BuildingResolutionAction] MCV 无法解析建筑卡牌: {selectedEntry}");
            return;
        }

        await CreatureCmd.TriggerAnim(creature, "Cast", Owner.Character.CastAnimDelay);
        if (isSoviet)
            await PowerCmd.Apply<SovietMCVPower>(new ThrowingPlayerChoiceContext(), creature, 1m, creature, null);
        else
            await PowerCmd.Apply<AlliedMCVPower>(new ThrowingPlayerChoiceContext(), creature, 1m, creature, null);

        // 先获得建筑牌到手牌，再抽牌（建筑抽牌由 BuildingDrawPower 跳过 MCV、改在此处执行），
        // 避免手牌满时先抽牌导致建筑卡无法入手。
        var card = creature.CombatState.CreateCard(model, Owner);
        if (IsUpgraded && !card.IsUpgraded)
            CardCmd.Upgrade(card);
        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner);
        await CardPileCmd.Draw(new ThrowingPlayerChoiceContext(), 1, Owner);
        Godot.GD.Print($"[BuildingResolutionAction] MCV 已先获得建筑 {selectedEntry} 再抽牌");
    }

    private async Task ApplyBuildingPower(Creature creature, string buildingKey)
    {
        var ctx = new ThrowingPlayerChoiceContext();
        switch (buildingKey)
        {
            case "allies_barracks":
                await PowerCmd.Apply<AlliedBarracksPower>(ctx, creature, 1m, creature, null);
                break;
            case "soviet_barracks":
                await PowerCmd.Apply<SovietBarracksPower>(ctx, creature, 1m, creature, null);
                break;
            case "allied_war_factory":
                await PowerCmd.Apply<AlliedWarFactoryPower>(ctx, creature, 1m, creature, null);
                break;
            case "soviet_war_factory":
                await PowerCmd.Apply<SovietWarFactoryPower>(ctx, creature, 1m, creature, null);
                break;
            case "allies_shipyard":
                await PowerCmd.Apply<AlliedShipyardPower>(ctx, creature, 1m, creature, null);
                break;
            case "soviet_shipyard":
                await PowerCmd.Apply<SovietShipyardPower>(ctx, creature, 1m, creature, null);
                break;
            case "air_force_command":
                await PowerCmd.Apply<AlliedAirForceCommandPower>(ctx, creature, 1m, creature, null);
                break;
        }
    }

    private static int GetBuildingCost(string buildingKey)
    {
        return buildingKey switch
        {
            "allies_barracks" => (int)AlliesCardValues.Barracks.DollarValue,
            "soviet_barracks" => (int)SovietCardValues.Barracks.DollarValue,
            "allied_war_factory" => (int)AlliesCardValues.AlliedWarFactory.DollarValue,
            "soviet_war_factory" => (int)SovietCardValues.SovietWarFactory.DollarValue,
            "allies_shipyard" => (int)AlliesCardValues.Shipyard.DollarValue,
            "soviet_shipyard" => (int)SovietCardValues.Shipyard.DollarValue,
            "air_force_command" => (int)AlliesCardValues.AirForceCommand.DollarValue,
            _ => 0
        };
    }

    private static int GetUnitPrice(string entry, string buildingKey)
    {
        bool soviet = buildingKey.StartsWith("soviet");
        return soviet
            ? SovietCardValues.GetDollarValue(entry)
            : AlliesCardValues.GetDollarValue(entry);
    }

    private static bool GetExhaust(string buildingKey, string entry)
    {
        if (buildingKey == "allied_war_factory")
            return entry != EntryOf<ChronoMiner>() && entry != EntryOf<AlliedMCV>();
        if (buildingKey == "soviet_war_factory")
            return entry != EntryOf<WarMiner>() && entry != EntryOf<SovietMCV>();
        return true;
    }

    public override INetAction ToNetAction()
    {
        return new NetBuildingResolutionAction
        {
            buildingEntry = BuildingEntry,
            isUpgraded = IsUpgraded,
            entries = Entries,
            counts = Counts
        };
    }

    public override string ToString()
    {
        return $"BuildingResolutionAction building={BuildingEntry} owner={Owner.NetId}";
    }
}
