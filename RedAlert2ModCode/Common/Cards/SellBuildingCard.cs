using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Entities.Creatures;
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

    public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/sellBuilding.png";

    public override CardPoolModel Pool => IsMutable && Owner != null
        ? Owner.Character.CardPool
        : ModelDb.CardPool<TokenCardPool>();

    public override CardPoolModel VisualCardPool => Pool;

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

        List<(PowerModel Power, int Index)> buildingPowerItems = GetAllBuildingPowersWithStacks();

        if (buildingPowerItems.Count == 0)
        {
            GD.Print("[SellBuildingCard] 没有可出售的建筑能力");
            return;
        }

        int maxSelection = IsUpgraded ? buildingPowerItems.Count : (int)Values.Repeat;
        if (maxSelection > buildingPowerItems.Count)
            maxSelection = buildingPowerItems.Count;

        FactionType faction = Owner.Character.Id.Entry?.Contains("SOVIET") ?? false
            ? FactionType.Soviet
            : FactionType.Allied;

        List<int> selectedIndices = await SellBuildingScreen.ShowSelectionWithSync(buildingPowerItems, maxSelection, Owner, faction);

        if (selectedIndices.Count == 0)
        {
            await CardUtils.HandleCardCancellation(play, this, Owner);
            return;
        }

        PlaySellSound();

        foreach (var index in selectedIndices)
        {
            await ProcessSoldPower(buildingPowerItems[index].Power);
        }
    }

    private List<(PowerModel Power, int Index)> GetAllBuildingPowersWithStacks()
    {
        List<(PowerModel Power, int Index)> result = new();
        var powers = Owner.Creature.Powers;
        var sellablePowerTypes = CommonCardValues.GetSellablePowerTypes();

        foreach (var power in powers)
        {
            if (sellablePowerTypes.Contains(power.GetType()) && power.Amount > 0)
            {
                for (int i = 0; i < power.Amount; i++)
                {
                    result.Add((power, result.Count));
                }
            }
        }

        return result;
    }

    private async Task ProcessSoldPower(PowerModel power)
    {
        int dollarValue = GetPowerDollarValue(power);
        int sellValue = dollarValue / 2;

        await PowerCmd.Decrement(power);

        var dollarPower = Owner.Creature.Powers.OfType<DollarPower>().FirstOrDefault();
        if (dollarPower != null)
        {
            dollarPower.AddDollar(sellValue);
            GD.Print($"[SellBuildingCard] 出售建筑获得资金: {sellValue}");
        }

        await CheckAndStopProductionQueues();
    }

    private async Task CheckAndStopProductionQueues()
    {
        GD.Print("[SellBuildingCard] 检查生产序列是否需要停产");

        bool hasAlliedBarracks = Owner.Creature.Powers.Any(p => p is AlliedBarracksPower);
        bool hasSovietBarracks = Owner.Creature.Powers.Any(p => p is SovietBarracksPower);
        bool hasAlliedWarFactory = Owner.Creature.Powers.Any(p => p is AlliedWarFactoryPower);
        bool hasSovietWarFactory = Owner.Creature.Powers.Any(p => p is SovietWarFactoryPower);
        bool hasAlliedShipyard = Owner.Creature.Powers.Any(p => p is AlliedShipyardPower);
        bool hasSovietShipyard = Owner.Creature.Powers.Any(p => p is SovietShipyardPower);
        bool hasSovietRadar = Owner.Creature.Powers.Any(p => p is SovietRadarPower);
        bool hasAlliedAirForceCommand = Owner.Creature.Powers.Any(p => p is AlliedAirForceCommandPower);

        GD.Print($"[SellBuildingCard] 兵营(盟军): {hasAlliedBarracks}, 兵营(苏联): {hasSovietBarracks}");
        GD.Print($"[SellBuildingCard] 重工(盟军): {hasAlliedWarFactory}, 重工(苏联): {hasSovietWarFactory}");
        GD.Print($"[SellBuildingCard] 船厂(盟军): {hasAlliedShipyard}, 船厂(苏联): {hasSovietShipyard}");
        GD.Print($"[SellBuildingCard] 雷达(苏联): {hasSovietRadar}, 空指部(盟军): {hasAlliedAirForceCommand}");

        foreach (var trainingPower in Owner.Creature.Powers.OfType<TrainingQueuePower>().ToList())
        {
            if (trainingPower.IsStopped)
                continue;

            bool shouldStop = false;

            string cardId = trainingPower.TrainedCardId;

            if (IsSoldierCard(cardId))
            {
                bool hasAnyBarracks = hasAlliedBarracks || hasSovietBarracks;
                if (!hasAnyBarracks)
                {
                    shouldStop = true;
                    GD.Print($"[SellBuildingCard] 无兵营能力，停产士兵单位: {trainingPower.UnitName}");
                }
            }
            else if (IsAlliedVehicleCard(cardId))
            {
                if (!hasAlliedWarFactory)
                {
                    shouldStop = true;
                    GD.Print($"[SellBuildingCard] 无盟军重工能力，停产盟军车辆: {trainingPower.UnitName}");
                }
            }
            else if (IsSovietVehicleCard(cardId))
            {
                if (!hasSovietWarFactory)
                {
                    shouldStop = true;
                    GD.Print($"[SellBuildingCard] 无苏联重工能力，停产苏联车辆: {trainingPower.UnitName}");
                }
            }
            else if (IsAlliedAircraftCard(cardId))
            {
                if (!hasAlliedAirForceCommand)
                {
                    shouldStop = true;
                    GD.Print($"[SellBuildingCard] 无盟军空指部能力，停产盟军飞机: {trainingPower.UnitName}");
                }
            }
            else if (IsSovietAircraftCard(cardId))
            {
                if (!hasSovietWarFactory || !hasSovietRadar)
                {
                    shouldStop = true;
                    GD.Print($"[SellBuildingCard] 无苏联重工或雷达能力，停产苏联飞机: {trainingPower.UnitName}");
                }
            }
            else if (IsAlliedShipCard(cardId))
            {
                if (!hasAlliedShipyard)
                {
                    shouldStop = true;
                    GD.Print($"[SellBuildingCard] 无盟军船厂能力，停产盟军舰船: {trainingPower.UnitName}");
                }
            }
            else if (IsSovietShipCard(cardId))
            {
                if (!hasSovietShipyard)
                {
                    shouldStop = true;
                    GD.Print($"[SellBuildingCard] 无苏联船厂能力，停产苏联舰船: {trainingPower.UnitName}");
                }
            }

            if (shouldStop)
            {
                await StopTrainingQueue(trainingPower);
            }
        }
    }

    private bool IsSoldierCard(string cardId)
    {
        var soldierIds = new HashSet<string>
        {
            "AMERICAN_SOLDIER", "ALLIES_DOG_SOLDIER", "GUARDIAN_GI", "ROCKET_SOLDIER", "ALLIES_ENGINEER",
            "CONSCRIPT", "SOVIET_ENGINEER", "SOVIET_ATTACK_DOG", "SOVIET_FLAK_TROOPER", "SOVIET_TESLA_TROOPER"
        };
        return soldierIds.Contains(cardId);
    }

    private bool IsAlliedVehicleCard(string cardId)
    {
        var vehicleIds = new HashSet<string>
        {
            "GRIZZLY_TANK", "IFV", "CHRONO_MINER", "MIRAGE_TANK", "PRISM_TANK"
        };
        return vehicleIds.Contains(cardId);
    }

    private bool IsSovietVehicleCard(string cardId)
    {
        var vehicleIds = new HashSet<string>
        {
            "RHINO_TANK", "WAR_MINER", "FLAK_TRACK", "TERROR_DRONE"
        };
        return vehicleIds.Contains(cardId);
    }

    private bool IsAlliedAircraftCard(string cardId)
    {
        var aircraftIds = new HashSet<string>
        {
            "INTRUDER", "NIGHT_HAWK_CHOPPER"
        };
        return aircraftIds.Contains(cardId);
    }

    private bool IsSovietAircraftCard(string cardId)
    {
        var aircraftIds = new HashSet<string>
        {
            "KIROV"
        };
        return aircraftIds.Contains(cardId);
    }

    private bool IsAlliedShipCard(string cardId)
    {
        var shipIds = new HashSet<string>
        {
            "DOLPHIN", "ALLIED_TRANSPORT_SHIP", "DESTROYER", "AGISICON", "AIRCRAFT_CARRIER"
        };
        return shipIds.Contains(cardId);
    }

    private bool IsSovietShipCard(string cardId)
    {
        var shipIds = new HashSet<string>
        {
            "SOVIET_TRANSPORT_SHIP", "FLAK_SUBMARINE", "TYPHOON_SUBMARINE"
        };
        return shipIds.Contains(cardId);
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

        var newPower = await TrainingQueuePower.ApplyTrainingQueue(
            owner: owner,
            cardId: cardId,
            unitName: unitName,
            iconPath: iconPath,
            unitPrice: unitPrice,
            isUpgraded: isUpgraded,
            sourceCard: this,
            exhaustWhenPlayed: exhaustWhenPlayed,
            isStopped: newStopped
        );

        if (newPower != null && amount > 1)
        {
            await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), newPower, amount - 1, owner, this);
            GD.Print($"[SellBuildingCard] 恢复训练队列层数: {newPower.Amount}");
        }

        GD.Print($"[SellBuildingCard] 训练队列 {unitName} 停产状态反转: {newStopped}");
    }

    private int GetPowerDollarValue(PowerModel power)
    {
        return CommonCardValues.GetSellablePowerDollarValue(power.GetType());
    }

    private void PlaySellSound()
    {
        try
        {
            AudioStreamPlayer audioPlayer = new();
            audioPlayer.Name = "SellBuildingSoundPlayer";
            var root = Engine.GetMainLoop() as SceneTree;
            if (root != null)
            {
                root.Root.AddChild(audioPlayer);
                var soundFile = GD.Load<AudioStream>("res://RedAlert2ModResources/audio/CommonSFX/sell_building.wav");
                if (soundFile != null)
                {
                    audioPlayer.Stream = soundFile;
                    audioPlayer.VolumeDb = -5;
                    audioPlayer.Play();
                    GD.Print("[SellBuildingCard] 播放出售音效");
                }
                else
                {
                    GD.PrintErr("[SellBuildingCard] 无法加载出售音效文件");
                }
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[SellBuildingCard] 播放音效失败: {ex.Message}");
        }
    }
}