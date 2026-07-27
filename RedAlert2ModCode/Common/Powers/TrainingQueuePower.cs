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

    public static async Task<TrainingQueuePower?> ApplyTrainingQueue(Creature owner, string cardId, string unitName, string iconPath, int unitPrice = 0, bool isUpgraded = false, CardModel? sourceCard = null, bool exhaustWhenPlayed = true, bool isStopped = false, int amount = 1)
    {
        GD.Print($"[TrainingQueuePower] ApplyTrainingQueue 被调用 - CardId={cardId}, UnitName={unitName}, UnitPrice={unitPrice}, IsUpgraded={isUpgraded}, IsStopped={isStopped}, Amount={amount}");

        GD.Print($"[TrainingQueuePower] 创建新的训练队列能力（独立能力，不叠加）");

        PowerIconManager.SetCurrentIconPath(iconPath);

        var trainingPower = await PowerCmd.Apply<TrainingQueuePower>(new ThrowingPlayerChoiceContext(), owner, amount, owner, sourceCard);

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
            
            int finalPrice = UnitPriceCalculator.CalculateFinalUnitPrice(owner, unitPrice, amount);
            trainingPower.UnitPrice = finalPrice;
            
            GD.Print($"[TrainingQueuePower] 应用大生产和工业工厂效果后价格: 原始={unitPrice}, 最终价格={finalPrice}");

            PowerIconManager.SetIcon(trainingPower, iconPath);

            GD.Print($"[TrainingQueuePower] 属性设置完成 - TrainedCardId={trainingPower.TrainedCardId}, TrainedUnitIconPath={trainingPower.TrainedUnitIconPath}, UnitPrice={trainingPower.UnitPrice}, OriginalUnitPrice={trainingPower.OriginalUnitPrice}, ExhaustWhenPlayed={trainingPower.ExhaustWhenPlayed}, IsStopped={trainingPower.IsStopped}, Amount={trainingPower.Amount}");
        }

        return trainingPower;
    }

    protected override void DeepCloneFields()
    {
        base.DeepCloneFields();
        
        GD.Print($"[TrainingQueuePower] DeepCloneFields 被调用 - InstanceId={_instanceId}, TrainedCardId='{TrainedCardId}', TrainedUnitIconPath='{TrainedUnitIconPath}', ExhaustWhenPlayed={ExhaustWhenPlayed}");
        
        // 注册能力哈希码以保留图标路径
        PowerIconManager.RegisterPowerHashCode(this);
        
        // 尝试从PowerIconManager获取存储的图标路径
        string? storedIconPath = PowerIconManager.GetIconPath(this);
        if (!string.IsNullOrEmpty(storedIconPath))
        {
            TrainedUnitIconPath = storedIconPath;
            GD.Print($"[TrainingQueuePower] 从PowerIconManager恢复图标路径: {storedIconPath}");
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
            
            if (ExhaustWhenPlayed)
            {
                var exhaustText = new LocString("powers", base.Id.Entry + ".exhaust_text").GetFormattedText();
                locString.Add("ExhaustText", exhaustText);
            }
            else
            {
                locString.Add("ExhaustText", "");
            }
            
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

        if (stacks <= 0)
        {
            GD.Print($"[TrainingQueuePower] 层数为0，跳过生产");
            return;
        }

        var dollarPower = Owner.Powers.OfType<DollarPower>().FirstOrDefault();
        if (dollarPower == null)
        {
            GD.Print($"[TrainingQueuePower] 没有刀乐能力，无法生产单位");
            return;
        }

        // 资金不够时跳过生产，且不掉层数
        if (dollarPower.DollarValue < UnitPrice)
        {
            GD.Print($"[TrainingQueuePower] 资金不足，跳过生产 - 当前资金={dollarPower.DollarValue}, 所需资金={UnitPrice}");
            return;
        }

        // 生产成功后层数-1（层数为0游戏自动结束能力）
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

        if (TrainedCardId.Contains("KIROV"))
        {
            PlayKirovDeploySound();
        }

        if (TrainedCardId.Contains("DEMOLITION_TRUCK"))
        {
            PlayDemolitionTruckDeploySound();
        }

        if (TrainedCardId.Contains("CHRONO_COMMANDOS"))
        {
            PlayChronoCommandosDeploySound();
        }

        await CardPileCmd.AddGeneratedCardToCombat(tempCard, PileType.Hand, Owner.Player);

        // 生产成功后层数-1
        await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), this, -1m, Owner, null);
        GD.Print($"[TrainingQueuePower] 生产完成，层数-1，剩余层数={(int)base.Amount}");
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

        GD.Print($"[TrainingQueuePower] GetCardModel 被调用 - cardId={cardId}");

        // 尝试提取卡牌名称部分（移除前缀如 RED_ALERT2_MOD_CARD_）
        string cardName = ExtractCardName(cardId.ToUpper());
        GD.Print($"[TrainingQueuePower] 提取卡牌名称: {cardName}");

        // 如果提取失败，使用原始卡牌ID
        if (string.IsNullOrEmpty(cardName))
        {
            cardName = cardId;
        }

        // 转换为类名格式（驼峰式）
        string[] parts = cardName.Split('_');
        string typeName = string.Concat(parts.Select(p => char.ToUpper(p[0]) + p.Substring(1).ToLower()));
        GD.Print($"[TrainingQueuePower] 生成类型名称: {typeName}");
        
        // 尝试标准命名（PascalCase）
        Type? cardType = FindCardType(typeName);
        
        // 如果没找到，尝试缩写全大写的变体（如 MCV -> Mcv 失败时，尝试 MCV）
        if (cardType == null && parts.Length > 0)
        {
            string upperTypeName = string.Concat(parts.Select(p => p.Length <= 3 ? p.ToUpper() : char.ToUpper(p[0]) + p.Substring(1).ToLower()));
            GD.Print($"[TrainingQueuePower] 尝试全大写缩写类型名称: {upperTypeName}");
            cardType = FindCardType(upperTypeName);
        }
        
        if (cardType != null)
        {
            var method = typeof(ModelDb).GetMethod("Card", System.Type.EmptyTypes)
                ?.MakeGenericMethod(cardType);
            CardModel? result = method?.Invoke(null, null) as CardModel;
            GD.Print($"[TrainingQueuePower] 成功获取卡牌模型: {result?.Id.Entry}");
            return result;
        }
        
        GD.PrintErr($"[TrainingQueuePower] 无法找到卡牌类型: {typeName}");
        return null;
    }

    private Type? FindCardType(string typeName)
    {
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
        
        return cardType;
    }
    
    /// <summary>
    /// 从完整的卡牌ID中提取卡牌名称部分
    /// 例如：RED_ALERT2_MOD_CARD_SOVIET_ATTACK_DOG -> SOVIET_ATTACK_DOG
    /// </summary>
    private string ExtractCardName(string cardKey)
    {
        // 移除前缀 RED_ALERT2_MOD_CARD_
        string prefix = "RED_ALERT2_MOD_CARD_";
        if (cardKey.StartsWith(prefix))
        {
            return cardKey.Substring(prefix.Length);
        }
        
        // 如果没有找到前缀，返回最后一个下划线之后的部分
        int lastUnderscoreIndex = cardKey.LastIndexOf('_');
        if (lastUnderscoreIndex >= 0 && lastUnderscoreIndex < cardKey.Length - 1)
        {
            return cardKey.Substring(lastUnderscoreIndex + 1);
        }
        
        return string.Empty;
    }

    }