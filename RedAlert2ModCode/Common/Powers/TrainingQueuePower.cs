using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Common.Powers;

public sealed class TrainingQueuePower : PowerModel
{
    private static int _instanceCounter = 0;
    private readonly int _instanceId;
    
    public override PowerType Type => PowerType.Buff;
    
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public string TrainedCardId { get; set; } = string.Empty;

    public string UnitName { get; set; } = string.Empty;

    public bool IsUpgraded { get; set; } = false;

    public bool IsStopped { get; set; } = false;

    public string TrainedUnitIconPath { get; set; } = string.Empty;

    public int UnitPrice { get; set; } = 0;

    public int OriginalUnitPrice { get; set; } = 0;

    public bool ExhaustWhenPlayed { get; set; } = true;

    public int InstanceId => _instanceId;

    public TrainingQueuePower()
    {
        _instanceId = ++_instanceCounter;
        GD.Print($"[TrainingQueuePower] 构造函数被调用 - InstanceId={_instanceId}");
    }

    public void SetTrainedUnit(string cardId, string unitName, string iconPath, int unitPrice = 0, bool isUpgraded = false)
    {
        TrainedCardId = cardId;
        UnitName = unitName;
        IsUpgraded = isUpgraded;
        TrainedUnitIconPath = iconPath;
        OriginalUnitPrice = unitPrice;
        UnitPrice = unitPrice;
        
        GD.Print($"[TrainingQueuePower] SetTrainedUnit 设置完成 - TrainedCardId={cardId}, TrainedUnitIconPath={iconPath}, UnitPrice={unitPrice}, OriginalUnitPrice={unitPrice}, InstanceId={_instanceId}");
        
        PowerIconManager.SetIcon(this, iconPath);
    }

    public static async Task<TrainingQueuePower?> ApplyTrainingQueue(Creature owner, string cardId, string unitName, string iconPath, int unitPrice = 0, bool isUpgraded = false, CardModel? sourceCard = null, bool exhaustWhenPlayed = true, bool isStopped = false)
    {
        GD.Print($"[TrainingQueuePower] ApplyTrainingQueue 被调用 - CardId={cardId}, UnitName={unitName}, UnitPrice={unitPrice}, IsUpgraded={isUpgraded}, IsStopped={isStopped}");

        TrainingQueuePower? existingPower = null;
        if (owner?.Powers != null)
        {
            existingPower = owner.Powers
                .OfType<TrainingQueuePower>()
                .FirstOrDefault(p => p.TrainedCardId == cardId && p.IsUpgraded == isUpgraded && p.IsStopped == isStopped);
        }

        if (existingPower != null)
        {
            GD.Print($"[TrainingQueuePower] 发现相同兵种的能力，增加层数 - 当前层数: {existingPower.Amount}");
            await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), existingPower, 1m, owner, sourceCard);
            GD.Print($"[TrainingQueuePower] 增加后层数: {existingPower.Amount}");
            
            int finalPrice = UnitPriceCalculator.CalculateFinalUnitPrice(owner, existingPower.OriginalUnitPrice, (int)existingPower.Amount);
            existingPower.UnitPrice = finalPrice;
            GD.Print($"[TrainingQueuePower] 叠加后重新计算价格: 原始={existingPower.OriginalUnitPrice}, 最终价格={finalPrice}");
            
            return existingPower;
        }

        GD.Print($"[TrainingQueuePower] 创建新的训练队列能力");

        PowerIconManager.SetCurrentIconPath(iconPath);

        var trainingPower = await PowerCmd.Apply<TrainingQueuePower>(new ThrowingPlayerChoiceContext(), owner, 1m, owner, sourceCard);

        if (trainingPower != null)
        {
            GD.Print($"[TrainingQueuePower] 设置属性 - TrainedCardId={cardId}, IconPath={iconPath}, UnitPrice={unitPrice}, ExhaustWhenPlayed={exhaustWhenPlayed}, IsStopped={isStopped}");
            trainingPower.TrainedCardId = cardId;
            trainingPower.UnitName = unitName;
            trainingPower.IsUpgraded = isUpgraded;
            trainingPower.TrainedUnitIconPath = iconPath;
            trainingPower.ExhaustWhenPlayed = exhaustWhenPlayed;
            trainingPower.IsStopped = isStopped;
            trainingPower.OriginalUnitPrice = unitPrice;
            
            int finalPrice = UnitPriceCalculator.CalculateFinalUnitPrice(owner, unitPrice, 1);
            trainingPower.UnitPrice = finalPrice;
            
            GD.Print($"[TrainingQueuePower] 应用大生产和工业工厂效果后价格: 原始={unitPrice}, 最终价格={finalPrice}");

            PowerIconManager.SetIcon(trainingPower, iconPath);

            GD.Print($"[TrainingQueuePower] 属性设置完成 - TrainedCardId={trainingPower.TrainedCardId}, TrainedUnitIconPath={trainingPower.TrainedUnitIconPath}, UnitPrice={trainingPower.UnitPrice}, OriginalUnitPrice={trainingPower.OriginalUnitPrice}, ExhaustWhenPlayed={trainingPower.ExhaustWhenPlayed}, IsStopped={trainingPower.IsStopped}");
        }

        return trainingPower;
    }

    protected override void DeepCloneFields()
    {
        base.DeepCloneFields();
        
        GD.Print($"[TrainingQueuePower] DeepCloneFields 被调用 - InstanceId={_instanceId}, TrainedCardId='{TrainedCardId}', TrainedUnitIconPath='{TrainedUnitIconPath}'");
        
        PowerIconManager.RegisterPowerHashCode(this);
        
        string? storedIconPath = PowerIconManager.GetIconPath(this);
        if (!string.IsNullOrEmpty(storedIconPath))
        {
            GD.Print($"[TrainingQueuePower] DeepCloneFields: 从PowerIconManager恢复图标路径: {storedIconPath}");
            TrainedUnitIconPath = storedIconPath;
            return;
        }
        
        System.Reflection.FieldInfo? originalField = null;
        Type? currentType = GetType();
        while (currentType != null)
        {
            originalField = currentType.GetField("_original", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (originalField != null)
                break;
            currentType = currentType.BaseType;
        }
        
        if (originalField != null)
        {
            var original = originalField.GetValue(this) as TrainingQueuePower;
            if (original != null)
                {
                    GD.Print($"[TrainingQueuePower] 原始对象 - TrainedCardId={original.TrainedCardId}, TrainedUnitIconPath={original.TrainedUnitIconPath}, UnitPrice={original.UnitPrice}, OriginalUnitPrice={original.OriginalUnitPrice}");
                    TrainedCardId = original.TrainedCardId;
                    UnitName = original.UnitName;
                    IsUpgraded = original.IsUpgraded;
                    TrainedUnitIconPath = original.TrainedUnitIconPath;
                    UnitPrice = original.UnitPrice;
                    OriginalUnitPrice = original.OriginalUnitPrice;
                    GD.Print($"[TrainingQueuePower] 克隆后 - TrainedCardId={TrainedCardId}, TrainedUnitIconPath={TrainedUnitIconPath}, UnitPrice={UnitPrice}, OriginalUnitPrice={OriginalUnitPrice}");
                }
            else
            {
                GD.PrintErr($"[TrainingQueuePower] 警告: 无法获取原始对象引用");
            }
        }
        else
        {
            GD.PrintErr($"[TrainingQueuePower] 警告: 无法找到 _original 字段");
        }
    }

    public new string PackedIconPath
    {
        get
        {
            if (!string.IsNullOrEmpty(TrainedUnitIconPath))
            {
                return TrainedUnitIconPath;
            }
            
            if (!string.IsNullOrEmpty(TrainedCardId))
            {
                CardModel? cardModel = GetCardModel(TrainedCardId);
                if (cardModel != null && !string.IsNullOrEmpty(cardModel.PortraitPath))
                {
                    return cardModel.PortraitPath;
                }
            }
            
            string? customPath = PowerIconManager.GetIconPath(this);
            if (!string.IsNullOrEmpty(customPath))
            {
                return customPath;
            }
            
            return "res://RedAlert2ModResources/images/packed/card_portraits/allies/brrkicon.png";
        }
    }

    public override LocString Description
    {
        get
        {
            var locString = new LocString("powers", base.Id.Entry + ".description");
            string displayName = IsUpgraded ? UnitName + "+" : UnitName;
            locString.Add("UnitName", displayName);
            locString.Add("UnitPrice", UnitPrice.ToString());
            
            locString.Add("ExhaustText", ExhaustWhenPlayed ? new LocString("card_keywords", "exhaust_text").GetFormattedText() : "");
            
            if (IsStopped)
            {
                locString.Add("StoppedMarker", "[gold]已停产[/gold]。");
            }
            else
            {
                locString.Add("StoppedMarker", "");
            }
            
            return locString;
        }
    }

    public override async Task AfterSideTurnStart(CombatSide side, System.Collections.Generic.IReadOnlyList<Creature> participants, MegaCrit.Sts2.Core.Combat.ICombatState combatState)
    {
        if (side != CombatSide.Player)
            return;

        if (IsStopped)
        {
            GD.Print($"[TrainingQueuePower] 已停产，跳过生产 - UnitName={UnitName}");
            return;
        }

        if (string.IsNullOrEmpty(TrainedCardId))
            return;

        CardModel? cardModel = GetCardModel(TrainedCardId);
        if (cardModel == null)
            return;

        int stacks = (int)base.Amount;
        GD.Print($"[TrainingQueuePower] 回合开始触发 - 层数={stacks}, TrainedCardId={TrainedCardId}, UnitPrice={UnitPrice}");

        var dollarPower = Owner.Powers.OfType<DollarPower>().FirstOrDefault();
        if (dollarPower == null)
        {
            GD.Print($"[TrainingQueuePower] 没有刀乐能力，无法生产单位");
            return;
        }

        for (int i = 0; i < stacks; i++)
        {
            if (dollarPower.DollarValue < UnitPrice)
            {
                GD.Print($"[TrainingQueuePower] 资金不足，停止生产 - 当前资金={dollarPower.DollarValue}, 所需资金={UnitPrice}");
                break;
            }

            dollarPower.AddDollar(-UnitPrice);
            GD.Print($"[TrainingQueuePower] 扣除资金 {UnitPrice}，剩余资金 {dollarPower.DollarValue}");

            CardModel tempCard = combatState.CreateCard(cardModel, base.Owner.Player);

            if (IsUpgraded)
            {
                CardCmd.Upgrade(tempCard);
            }

            tempCard.EnergyCost.SetCustomBaseCost(0);

            if (ExhaustWhenPlayed)
            {
                tempCard.AddKeyword(CardKeyword.Exhaust);
                GD.Print($"[TrainingQueuePower] 单位消耗: 是 - UnitName={UnitName}");
            }
            else
            {
                GD.Print($"[TrainingQueuePower] 单位消耗: 否 - UnitName={UnitName}");
            }

            GD.Print($"[TrainingQueuePower] 检查语音播放 - TrainedCardId={TrainedCardId}");

            if (TrainedCardId == "KIROV")
            {
                PlayKirovDeploySound();
            }

            if (TrainedCardId == "DEMOLITION_TRUCK_CARD")
            {
                PlayDemolitionTruckDeploySound();
            }

            if (TrainedCardId == "CHRONO_COMMANDOS")
            {
                PlayChronoCommandosDeploySound();
            }

            await CardPileCmd.AddGeneratedCardToCombat(tempCard, PileType.Hand, Owner.Player);
        }
    }

    private static AudioStreamPlayer? _kirovDeployAudioPlayer;
    private static AudioStreamPlayer? _demolitionTruckDeployAudioPlayer;
    private static AudioStreamPlayer? _chronoCommandosDeployAudioPlayer;

    private static void EnsureKirovDeployAudioPlayer()
    {
        if (_kirovDeployAudioPlayer != null && GodotObject.IsInstanceValid(_kirovDeployAudioPlayer))
            return;

        _kirovDeployAudioPlayer = new AudioStreamPlayer();
        _kirovDeployAudioPlayer.Name = "KirovDeployAudioPlayer";
        var root = Engine.GetMainLoop() as SceneTree;
        root?.Root.AddChild(_kirovDeployAudioPlayer);
    }

    private static void EnsureDemolitionTruckDeployAudioPlayer()
    {
        if (_demolitionTruckDeployAudioPlayer != null && GodotObject.IsInstanceValid(_demolitionTruckDeployAudioPlayer))
            return;

        _demolitionTruckDeployAudioPlayer = new AudioStreamPlayer();
        _demolitionTruckDeployAudioPlayer.Name = "DemolitionTruckDeployAudioPlayer";
        var root = Engine.GetMainLoop() as SceneTree;
        root?.Root.AddChild(_demolitionTruckDeployAudioPlayer);
    }

    private void PlayKirovDeploySound()
    {
        try
        {
            EnsureKirovDeployAudioPlayer();
            if (_kirovDeployAudioPlayer == null) return;

            var soundFile = GD.Load<AudioStream>("res://RedAlert2ModResources/audio/SovietUnits/Kirov/kirov_deploy.mp3");
            if (soundFile != null)
            {
                _kirovDeployAudioPlayer.Stream = soundFile;
                _kirovDeployAudioPlayer.VolumeDb = -5;
                _kirovDeployAudioPlayer.Play();
                GD.Print("[TrainingQueuePower] 播放基洛夫出厂音效");
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[TrainingQueuePower] 播放基洛夫出厂音效失败: {ex.Message}");
        }
    }

    private void PlayDemolitionTruckDeploySound()
    {
        try
        {
            EnsureDemolitionTruckDeployAudioPlayer();
            if (_demolitionTruckDeployAudioPlayer == null) return;

            var soundFile = GD.Load<AudioStream>("res://RedAlert2ModResources/audio/SovietUnits/DemolitionTruck/Vdemsea_factory.mp3");
            if (soundFile != null)
            {
                _demolitionTruckDeployAudioPlayer.Stream = soundFile;
                _demolitionTruckDeployAudioPlayer.VolumeDb = -5;
                _demolitionTruckDeployAudioPlayer.Play();
                GD.Print("[TrainingQueuePower] 播放自爆卡车出厂音效");
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[TrainingQueuePower] 播放自爆卡车出厂音效失败: {ex.Message}");
        }
    }

    private static void EnsureChronoCommandosDeployAudioPlayer()
    {
        if (_chronoCommandosDeployAudioPlayer != null && GodotObject.IsInstanceValid(_chronoCommandosDeployAudioPlayer))
            return;

        _chronoCommandosDeployAudioPlayer = new AudioStreamPlayer();
        _chronoCommandosDeployAudioPlayer.Name = "ChronoCommandosDeployAudioPlayer";
        var root = Engine.GetMainLoop() as SceneTree;
        root?.Root.AddChild(_chronoCommandosDeployAudioPlayer);
    }

    private void PlayChronoCommandosDeploySound()
    {
        try
        {
            EnsureChronoCommandosDeployAudioPlayer();
            if (_chronoCommandosDeployAudioPlayer == null) return;

            var soundFile = GD.Load<AudioStream>("res://RedAlert2ModResources/audio/AlliedUnits/SealAndChronoCommandos/Iseasec_chrono.mp3");
            if (soundFile != null)
            {
                _chronoCommandosDeployAudioPlayer.Stream = soundFile;
                _chronoCommandosDeployAudioPlayer.VolumeDb = -5;
                _chronoCommandosDeployAudioPlayer.Play();
                GD.Print("[TrainingQueuePower] 播放超时空突击队出厂音效");
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[TrainingQueuePower] 播放超时空突击队出厂音效失败: {ex.Message}");
        }
    }

    private CardModel? GetCardModel(string cardId)
    {
        if (string.IsNullOrEmpty(cardId))
            return null;

        string[] parts = cardId.Split('_');
        string typeName = string.Concat(parts.Select(p => char.ToUpper(p[0]) + p.Substring(1).ToLower()));
        
        var cardType = Assembly.GetExecutingAssembly()
            .GetType($"RedAlert2ModCode.Allies.Cards.{typeName}");
        
        if (cardType == null)
        {
            cardType = Assembly.GetExecutingAssembly()
                .GetType($"RedAlert2ModCode.Soviet.Cards.{typeName}");
        }
        
        if (cardType == null)
        {
            cardType = Assembly.GetExecutingAssembly()
                .GetType($"RedAlert2ModCode.Common.Cards.{typeName}");
        }
        
        if (cardType == null)
        {
            cardType = typeof(CardModel).Assembly.GetType($"MegaCrit.Sts2.Core.Models.Cards.{typeName}");
        }
        
        if (cardType != null)
        {
            var method = typeof(ModelDb).GetMethod("Card", System.Type.EmptyTypes)
                ?.MakeGenericMethod(cardType);
            return method?.Invoke(null, null) as CardModel;
        }
        
        return null;
    }

    }