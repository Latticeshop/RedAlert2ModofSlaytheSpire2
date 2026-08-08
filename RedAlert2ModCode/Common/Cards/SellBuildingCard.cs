using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.HoverTips;
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Soviet.Powers;
using RedAlert2ModCode.UI;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.Allies.Cards;
using RedAlert2ModCode.Soviet.Cards;
using RedAlert2ModCode.Allies;
using RedAlert2ModCode.Soviet;

namespace RedAlert2ModCode.Common.Cards;

public class SellBuildingCard : CardModel
{
    private static readonly CardValueStore.CardValues Values = CommonCardValues.SellBuilding;

    public SellBuildingCard() : base((int)Values.Cost, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    public override CardPoolModel Pool => IsMutable && Owner != null
        ? Owner.Character.CardPool
        : ModelDb.CardPool<TokenCardPool>();

    public override CardPoolModel VisualCardPool => Pool;

    public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/sellBuilding.png";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ModCardKeywords.Building.CreateHoverTip()
    ];

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new IntVar("MaxSellCount", Values.Repeat)
    };

    protected override void OnUpgrade()
    {
        DynamicVars["MaxSellCount"].BaseValue = Values.Repeat + Values.RepeatUpgraded;
    }

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
		// 注：A2 预选模式下，选择在打出前完成；出售结算由 BuildingResolutionAction 调用 ResolveSoldPowers。
		// 自动打出兜底：若没有手动 A2 的待结算标记，则本地补开预选面板（确认后由结算动作执行效果）
		if (BuildingPrePlayHelper.TryConsumePendingResolution(this))
			return;
		if (MultiplayerSyncHelper.IsLocalPlayer(Owner))
			BuildingPrePlayHelper.OpenAutoPlayPanel(this);
	}

    public static List<SellBuildingItem> GetPrePlayCandidates(Player owner)
    {
        List<SellBuildingItem> result = new();
        var powers = owner.Creature.Powers;
        var sellablePowerTypes = CommonCardValues.GetSellablePowerTypes();

        // 按能力类型分组，合并相同建筑的层数
        var groupedPowers = powers
            .Where(p => sellablePowerTypes.Contains(p.GetType()) && p.Amount > 0)
            .GroupBy(p => p.GetType());

        foreach (var group in groupedPowers)
        {
            var firstPower = group.First();
            int totalAmount = group.Sum(p => p.Amount);
            int dollarValue = CommonCardValues.GetSellablePowerDollarValue(firstPower.GetType());
            int sellValue = dollarValue / 2;
            
            // 获取图标路径
            string iconPath = GetPowerIconPath(firstPower);

            result.Add(new SellBuildingItem
            {
                Power = firstPower,
                Name = firstPower.Id.Entry.Replace("_", " "),
                IconPath = iconPath,
                TotalStacks = totalAmount,
                SellValue = sellValue,
                SelectedCount = 0
            });

            GD.Print($"[SellBuildingCard] 建筑能力: {firstPower.Id.Entry}, 总层数: {totalAmount}, 出售价值: {sellValue}");
        }

        return result;
    }

    /// <summary>
    /// A2 结算：按预选结果出售建筑能力（能力类型名 + 数量），所有端确定性执行。
    /// </summary>
    public static async Task ResolveSoldPowers(Player owner, bool isUpgraded, string[] powerTypeNames, int[] counts)
    {
        if (owner?.Creature == null)
            return;

        int maxTotalSell = isUpgraded ? int.MaxValue : (int)CommonCardValues.SellBuilding.Repeat;
        int totalSold = 0;

        for (int i = 0; i < powerTypeNames.Length && i < counts.Length; i++)
        {
            if (totalSold >= maxTotalSell)
                break;

            string typeName = powerTypeNames[i];
            int count = counts[i];
            if (count <= 0 || string.IsNullOrEmpty(typeName))
                continue;

            var power = owner.Creature.Powers.FirstOrDefault(p => p.GetType().Name == typeName);
            if (power == null)
                continue;

            int remainingQuota = maxTotalSell - totalSold;
            int countToSell = Math.Min(count, remainingQuota);
            if (countToSell <= 0)
                break;

            await ProcessSoldPower(owner, power, countToSell);
            totalSold += countToSell;
        }
    }

    private static string GetPowerIconPath(PowerModel power)
    {
        Type powerType = power.GetType();
        
        if (powerType == typeof(AlliedRefineryPower))
            return "res://RedAlert2ModResources/images/packed/card_portraits/allies/reficon.png";
        if (powerType == typeof(SovietRefineryPower))
            return "res://RedAlert2ModResources/images/packed/card_portraits/soviet/nreficon.png";
        if (powerType == typeof(AlliedWarFactoryPower))
            return "res://RedAlert2ModResources/images/packed/card_portraits/allies/gwepicon.png";
        if (powerType == typeof(SovietWarFactoryPower))
            return "res://RedAlert2ModResources/images/packed/card_portraits/soviet/nwepicon.png";
        if (powerType == typeof(BattleLabPower))
            return "res://RedAlert2ModResources/images/packed/card_portraits/allies/techicon.png";
        if (powerType == typeof(SovietBattleLabPower))
            return "res://RedAlert2ModResources/images/packed/card_portraits/soviet/ntchicon.png";
        if (powerType == typeof(SovietRadarPower))
            return "res://RedAlert2ModResources/images/packed/card_portraits/soviet/nradicon.png";
        if (powerType == typeof(AlliedMCVPower))
            return "res://RedAlert2ModResources/images/packed/card_portraits/allies/mcvicon.png";
        if (powerType == typeof(SovietMCVPower))
            return "res://RedAlert2ModResources/images/packed/card_portraits/soviet/smcvicon.png";
        if (powerType == typeof(AlliedRepairDepotPower))
            return "res://RedAlert2ModResources/images/packed/card_portraits/allies/fixicon.png";
        if (powerType == typeof(SovietRepairDepotPower))
            return "res://RedAlert2ModResources/images/packed/card_portraits/soviet/rfixicon.png";

        string iconPath = power.PackedIconPath;
        if (!string.IsNullOrEmpty(iconPath) && ResourceLoader.Exists(iconPath))
            return iconPath;

        return string.Empty;
    }

    private static async Task ProcessSoldPower(Player owner, PowerModel power, int count)
    {
        int dollarValue = CommonCardValues.GetSellablePowerDollarValue(power.GetType());
        int sellValue = dollarValue / 2 * count;

        // 批量减少层数
        for (int i = 0; i < count; i++)
        {
            await PowerCmd.Decrement(power);
        }

        BuildingSoundHelper.PlayBuildingSellSound();

        var dollarPower = owner.Creature.Powers.OfType<DollarPower>().FirstOrDefault();
        if (dollarPower != null)
        {
            dollarPower.AddDollar(sellValue);
            GD.Print($"[SellBuildingCard] 出售建筑获得资金: {sellValue}");
        }

        await CheckAndRemoveProductionQueues(owner);
        
        await UnitPriceCalculator.RecalculateAllTrainingQueuePrices(owner.Creature);
    }

    private static async Task CheckAndRemoveProductionQueues(Player owner)
    {
        GD.Print("[SellBuildingCard] 检查生产序列是否需要移除");

        // 计算建筑能力的总层数（而非是否存在）
        int alliedBarracksStacks = owner.Creature.Powers.OfType<AlliedBarracksPower>().Sum(p => p.Amount);
        int sovietBarracksStacks = owner.Creature.Powers.OfType<SovietBarracksPower>().Sum(p => p.Amount);
        int alliedWarFactoryStacks = owner.Creature.Powers.OfType<AlliedWarFactoryPower>().Sum(p => p.Amount);
        int sovietWarFactoryStacks = owner.Creature.Powers.OfType<SovietWarFactoryPower>().Sum(p => p.Amount);
        int alliedShipyardStacks = owner.Creature.Powers.OfType<AlliedShipyardPower>().Sum(p => p.Amount);
        int sovietShipyardStacks = owner.Creature.Powers.OfType<SovietShipyardPower>().Sum(p => p.Amount);
        int sovietRadarStacks = owner.Creature.Powers.OfType<SovietRadarPower>().Sum(p => p.Amount);
        int alliedAirForceCommandStacks = owner.Creature.Powers.OfType<AlliedAirForceCommandPower>().Sum(p => p.Amount);

        GD.Print($"[SellBuildingCard] 兵营(盟军): {alliedBarracksStacks}层, 兵营(苏联): {sovietBarracksStacks}层");
        GD.Print($"[SellBuildingCard] 重工(盟军): {alliedWarFactoryStacks}层, 重工(苏联): {sovietWarFactoryStacks}层");
        GD.Print($"[SellBuildingCard] 船厂(盟军): {alliedShipyardStacks}层, 船厂(苏联): {sovietShipyardStacks}层");
        GD.Print($"[SellBuildingCard] 雷达(苏联): {sovietRadarStacks}层, 空指部(盟军): {alliedAirForceCommandStacks}层");

        foreach (var trainingPower in owner.Creature.Powers.OfType<TrainingQueuePower>().ToList())
        {
            // 停产状态的序列也需要检查（如果建筑能力清空，停产序列也应移除）
            bool shouldRemove = false;

            string cardId = trainingPower.TrainedCardId;

            if (IsSoldierCard(cardId))
            {
                // 士兵单位需要任意兵营能力（总层数 > 0）
                int totalBarracksStacks = alliedBarracksStacks + sovietBarracksStacks;
                if (totalBarracksStacks <= 0)
                {
                    shouldRemove = true;
                    GD.Print($"[SellBuildingCard] 兵营能力已清空(总层数: {totalBarracksStacks})，移除士兵生产序列: {trainingPower.UnitName}");
                }
            }
            else if (IsAlliedVehicleCard(cardId))
            {
                if (alliedWarFactoryStacks <= 0)
                {
                    shouldRemove = true;
                    GD.Print($"[SellBuildingCard] 盟军重工能力已清空(总层数: {alliedWarFactoryStacks})，移除盟军车辆生产序列: {trainingPower.UnitName}");
                }
            }
            else if (IsSovietVehicleCard(cardId))
            {
                if (sovietWarFactoryStacks <= 0)
                {
                    shouldRemove = true;
                    GD.Print($"[SellBuildingCard] 苏联重工能力已清空(总层数: {sovietWarFactoryStacks})，移除苏联车辆生产序列: {trainingPower.UnitName}");
                }
            }
            else if (IsAlliedAircraftCard(cardId))
            {
                if (alliedAirForceCommandStacks <= 0)
                {
                    shouldRemove = true;
                    GD.Print($"[SellBuildingCard] 盟军空指部能力已清空(总层数: {alliedAirForceCommandStacks})，移除盟军飞机生产序列: {trainingPower.UnitName}");
                }
            }
            else if (IsSovietAircraftCard(cardId))
            {
                // 苏联飞机需要重工和雷达都存在（总层数 > 0）
                if (sovietWarFactoryStacks <= 0 || sovietRadarStacks <= 0)
                {
                    shouldRemove = true;
                    GD.Print($"[SellBuildingCard] 苏联重工({sovietWarFactoryStacks}层)或雷达({sovietRadarStacks}层)能力已清空，移除苏联飞机生产序列: {trainingPower.UnitName}");
                }
            }
            else if (IsAlliedShipCard(cardId))
            {
                if (alliedShipyardStacks <= 0)
                {
                    shouldRemove = true;
                    GD.Print($"[SellBuildingCard] 盟军船厂能力已清空(总层数: {alliedShipyardStacks})，移除盟军舰船生产序列: {trainingPower.UnitName}");
                }
            }
            else if (IsSovietShipCard(cardId))
            {
                if (sovietShipyardStacks <= 0)
                {
                    shouldRemove = true;
                    GD.Print($"[SellBuildingCard] 苏联船厂能力已清空(总层数: {sovietShipyardStacks})，移除苏联舰船生产序列: {trainingPower.UnitName}");
                }
            }

            if (shouldRemove)
            {
                // 直接移除生产序列能力，而非停产
                owner.Creature.RemovePowerInternal(trainingPower);
                GD.Print($"[SellBuildingCard] 已移除生产序列能力: {trainingPower.UnitName}");
            }
        }
    }

    private static bool IsSoldierCard(string cardId)
    {
        var soldierIds = new[]
        {
            "AMERICAN_SOLDIER", "ALLIES_DOG_SOLDIER", "GUARDIAN_GI", "ROCKET_SOLDIER", "ALLIES_ENGINEER",
            "CONSCRIPT", "SOVIET_ENGINEER", "SOVIET_ATTACK_DOG", "SOVIET_FLAK_TROOPER", "SOVIET_TESLA_TROOPER"
        };
        return soldierIds.Any(id => cardId.Contains(id));
    }

    private static bool IsAlliedVehicleCard(string cardId)
    {
        var vehicleIds = new[]
        {
            "GRIZZLY_TANK", "IFV", "CHRONO_MINER", "MIRAGE_TANK", "PRISM_TANK"
        };
        return vehicleIds.Any(id => cardId.Contains(id));
    }

    private static bool IsSovietVehicleCard(string cardId)
    {
        var vehicleIds = new[]
        {
            "RHINO_TANK", "WAR_MINER", "FLAK_TRACK", "TERROR_DRONE"
        };
        return vehicleIds.Any(id => cardId.Contains(id));
    }

    private static bool IsAlliedAircraftCard(string cardId)
    {
        var aircraftIds = new[]
        {
            "INTRUDER", "NIGHT_HAWK_CHOPPER"
        };
        return aircraftIds.Any(id => cardId.Contains(id));
    }

    private static bool IsSovietAircraftCard(string cardId)
    {
        var aircraftIds = new[]
        {
            "KIROV"
        };
        return aircraftIds.Any(id => cardId.Contains(id));
    }

    private static bool IsAlliedShipCard(string cardId)
    {
        var shipIds = new[]
        {
            "DOLPHIN", "ALLIED_TRANSPORT_SHIP", "DESTROYER", "AGISICON", "AIRCRAFT_CARRIER"
        };
        return shipIds.Any(id => cardId.Contains(id));
    }

    private static bool IsSovietShipCard(string cardId)
    {
        var shipIds = new[]
        {
            "SOVIET_TRANSPORT_SHIP", "FLAK_SUBMARINE", "TYPHOON_SUBMARINE"
        };
        return shipIds.Any(id => cardId.Contains(id));
    }

    private async Task StopTrainingQueue(TrainingQueuePower trainingPower)
    {
        bool wasStopped = trainingPower.IsStopped;
        string cardId = trainingPower.TrainedCardId;
        string unitName = trainingPower.UnitName;
        bool isUpgraded = trainingPower.IsUpgraded;
        string iconPath = trainingPower.TrainedUnitIconPath;
        int unitPrice = trainingPower.UnitPrice;
        bool exhaustWhenPlayed = trainingPower.ExhaustWhenPlayed;
        int amount = trainingPower.Amount;
        Creature owner = trainingPower.Owner;

        owner.RemovePowerInternal(trainingPower);
        GD.Print($"[SellBuildingCard] 移除训练队列能力: {unitName}, 层数: {amount}");

        bool newStopped = !wasStopped;

        await TrainingQueuePower.ApplyTrainingQueue(
            owner: owner,
            cardId: cardId,
            unitName: unitName,
            iconPath: iconPath,
            unitPrice: unitPrice,
            isUpgraded: isUpgraded,
            sourceCard: this,
            exhaustWhenPlayed: exhaustWhenPlayed,
            isStopped: newStopped,
            amount: amount
        );

        GD.Print($"[SellBuildingCard] 训练队列 {unitName} 停产状态反转: {newStopped}");
    }

    private int GetPowerDollarValue(PowerModel power)
    {
        return CommonCardValues.GetSellablePowerDollarValue(power.GetType());
    }
}
