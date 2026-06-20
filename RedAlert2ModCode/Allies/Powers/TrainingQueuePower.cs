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

namespace RedAlert2ModCode.Allies.Powers;

public sealed class TrainingQueuePower : PowerModel
{
    // 用于追踪实例创建顺序
    private static int _instanceCounter = 0;
    private readonly int _instanceId;
    
    /// <summary>
    /// 根据停产状态动态返回能力类型
    /// 生产中 -> Buff（绿色数字）
    /// 停产 -> Debuff（红色数字）
    /// </summary>
    public override PowerType Type => IsStopped ? PowerType.Debuff : PowerType.Buff;
    
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>
    /// 设置为Instanced确保每个能力都是独立实例
    /// 相同兵种的叠加逻辑在 ApplyTrainingQueue 中手动处理
    /// 这样可以确保不同兵种的能力不会被游戏引擎自动合并
    /// </summary>
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public string TrainedCardId { get; set; } = string.Empty;

    public string UnitName { get; set; } = string.Empty;

    public bool IsUpgraded { get; set; } = false;

    /// <summary>
    /// 是否停产
    /// </summary>
    public bool IsStopped { get; set; } = false;

    /// <summary>
    /// 训练单位的图标路径（直接存储，避免依赖PowerIconManager的对象引用）
    /// </summary>
    public string TrainedUnitIconPath { get; set; } = string.Empty;

    /// <summary>
    /// 训练单位的价格（用于生产时的资金检查）
    /// </summary>
    public int UnitPrice { get; set; } = 0;

    /// <summary>
    /// 生产的单位打出时是否消耗（默认为true，矿车等不消耗单位设为false）
    /// </summary>
    public bool ExhaustWhenPlayed { get; set; } = true;

    /// <summary>
    /// 追踪实例ID
    /// </summary>
    public int InstanceId => _instanceId;

    public TrainingQueuePower()
    {
        _instanceId = ++_instanceCounter;
        GD.Print($"[TrainingQueuePower] 构造函数被调用 - InstanceId={_instanceId}");
    }

    /// <summary>
    /// 设置训练单位的属性（从卡牌信息）
    /// </summary>
    public void SetTrainedUnit(string cardId, string unitName, string iconPath, int unitPrice = 0, bool isUpgraded = false)
    {
        TrainedCardId = cardId;
        UnitName = unitName;
        IsUpgraded = isUpgraded;
        TrainedUnitIconPath = iconPath;
        UnitPrice = unitPrice;
        
        GD.Print($"[TrainingQueuePower] SetTrainedUnit 设置完成 - TrainedCardId={cardId}, TrainedUnitIconPath={iconPath}, UnitPrice={unitPrice}, InstanceId={_instanceId}");
        
        // 同时保存到 PowerIconManager
        PowerIconManager.SetIcon(this, iconPath);
    }

    /// <summary>
    /// 应用训练队列能力（统一处理叠加逻辑）
    /// 相同兵种叠加层数，不同兵种创建新能力
    /// </summary>
    /// <param name="owner">拥有者</param>
    /// <param name="cardId">训练的卡牌ID</param>
    /// <param name="unitName">单位名称</param>
    /// <param name="iconPath">图标路径</param>
    /// <param name="unitPrice">单位价格</param>
    /// <param name="isUpgraded">是否升级</param>
    /// <param name="sourceCard">来源卡牌（用于能力关联）</param>
    /// <param name="exhaustWhenPlayed">生产出的单位打出时是否消耗（默认为true）</param>
    /// <param name="isStopped">初始停产状态（默认为false）</param>
    /// <returns>创建或叠加的能力实例</returns>
    public static async Task<TrainingQueuePower?> ApplyTrainingQueue(Creature owner, string cardId, string unitName, string iconPath, int unitPrice = 0, bool isUpgraded = false, CardModel? sourceCard = null, bool exhaustWhenPlayed = true, bool isStopped = false)
    {
        GD.Print($"[TrainingQueuePower] ApplyTrainingQueue 被调用 - CardId={cardId}, UnitName={unitName}, UnitPrice={unitPrice}, IsUpgraded={isUpgraded}, IsStopped={isStopped}");

        // 检查是否已有相同兵种且升级状态相同的训练队列能力
        TrainingQueuePower? existingPower = null;
        if (owner?.Powers != null)
        {
            existingPower = owner.Powers
                .OfType<TrainingQueuePower>()
                .FirstOrDefault(p => p.TrainedCardId == cardId && p.IsUpgraded == isUpgraded);
        }

        if (existingPower != null)
        {
            // 已有相同兵种的能力，增加层数
            GD.Print($"[TrainingQueuePower] 发现相同兵种的能力，增加层数 - 当前层数: {existingPower.Amount}");
            await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), existingPower, 1m, owner, sourceCard);
            GD.Print($"[TrainingQueuePower] 增加后层数: {existingPower.Amount}");
            return existingPower;
        }

        // 没有相同兵种的能力，创建新能力
        GD.Print($"[TrainingQueuePower] 创建新的训练队列能力");

        // 设置当前活跃的图标路径（确保克隆对象也能获取）
        PowerIconManager.SetCurrentIconPath(iconPath);

        var trainingPower = await PowerCmd.Apply<TrainingQueuePower>(new ThrowingPlayerChoiceContext(), owner, 1m, owner, sourceCard);

        if (trainingPower != null)
        {
            GD.Print($"[TrainingQueuePower] 设置属性 - TrainedCardId={cardId}, IconPath={iconPath}, UnitPrice={unitPrice}, ExhaustWhenPlayed={exhaustWhenPlayed}, IsStopped={isStopped}");
            trainingPower.TrainedCardId = cardId;
            trainingPower.UnitName = unitName;
            trainingPower.IsUpgraded = isUpgraded;
            trainingPower.TrainedUnitIconPath = iconPath;
            trainingPower.UnitPrice = unitPrice;
            trainingPower.ExhaustWhenPlayed = exhaustWhenPlayed;
            trainingPower.IsStopped = isStopped;

            // 使用图标管理器设置能力图标
            PowerIconManager.SetIcon(trainingPower, iconPath);

            GD.Print($"[TrainingQueuePower] 属性设置完成 - TrainedCardId={trainingPower.TrainedCardId}, TrainedUnitIconPath={trainingPower.TrainedUnitIconPath}, UnitPrice={trainingPower.UnitPrice}, ExhaustWhenPlayed={trainingPower.ExhaustWhenPlayed}, IsStopped={trainingPower.IsStopped}");
        }

        return trainingPower;
    }

    protected override void DeepCloneFields()
    {
        base.DeepCloneFields();
        
        GD.Print($"[TrainingQueuePower] DeepCloneFields 被调用 - InstanceId={_instanceId}, TrainedCardId='{TrainedCardId}', TrainedUnitIconPath='{TrainedUnitIconPath}'");
        
        // 注册当前实例的哈希码
        PowerIconManager.RegisterPowerHashCode(this);
        
        // 首先尝试从 PowerIconManager 获取已存储的图标路径
        string? storedIconPath = PowerIconManager.GetIconPath(this);
        if (!string.IsNullOrEmpty(storedIconPath))
        {
            GD.Print($"[TrainingQueuePower] DeepCloneFields: 从PowerIconManager恢复图标路径: {storedIconPath}");
            // 由于我们不知道完整的卡牌信息，只设置图标路径
            TrainedUnitIconPath = storedIconPath;
            return;
        }
        
        // 获取原始对象引用（通过反射获取私有字段，遍历所有基类）
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
                    GD.Print($"[TrainingQueuePower] 原始对象 - TrainedCardId={original.TrainedCardId}, TrainedUnitIconPath={original.TrainedUnitIconPath}, UnitPrice={original.UnitPrice}");
                    // 手动复制所有自定义字段
                    TrainedCardId = original.TrainedCardId;
                    UnitName = original.UnitName;
                    IsUpgraded = original.IsUpgraded;
                    TrainedUnitIconPath = original.TrainedUnitIconPath;
                    UnitPrice = original.UnitPrice;
                    GD.Print($"[TrainingQueuePower] 克隆后 - TrainedCardId={TrainedCardId}, TrainedUnitIconPath={TrainedUnitIconPath}, UnitPrice={UnitPrice}");
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

    /// <summary>
    /// 动态获取图标路径
    /// 优先显示训练单位的图标，默认使用兵营卡牌的图标
    /// </summary>
    public new string PackedIconPath
    {
        get
        {
            // 1. 优先使用 TrainedUnitIconPath（直接存储，克隆后仍然有效，最可靠）
            if (!string.IsNullOrEmpty(TrainedUnitIconPath))
            {
                return TrainedUnitIconPath;
            }
            
            // 2. 通过 TrainedCardId 动态获取图标
            if (!string.IsNullOrEmpty(TrainedCardId))
            {
                CardModel? cardModel = GetCardModel(TrainedCardId);
                if (cardModel != null && !string.IsNullOrEmpty(cardModel.PortraitPath))
                {
                    return cardModel.PortraitPath;
                }
            }
            
            // 3. 检查 PowerIconManager（原始对象可用时）
            string? customPath = PowerIconManager.GetIconPath(this);
            if (!string.IsNullOrEmpty(customPath))
            {
                return customPath;
            }
            
            // 4. 默认回退到兵营图标
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
            
            // 根据是否消耗决定是否显示"且消耗"
            locString.Add("ExhaustText", ExhaustWhenPlayed ? "且消耗" : "");
            
            // 如果停产，添加已停产标记
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

        // 如果已停产，不执行生产
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

        // 获取当前层数，按层数循环触发
        int stacks = (int)base.Amount;
        GD.Print($"[TrainingQueuePower] 回合开始触发 - 层数={stacks}, TrainedCardId={TrainedCardId}, UnitPrice={UnitPrice}");

        // 获取刀乐能力
        var dollarPower = Owner.Powers.OfType<DollarPower>().FirstOrDefault();
        if (dollarPower == null)
        {
            GD.Print($"[TrainingQueuePower] 没有刀乐能力，无法生产单位");
            return;
        }

        // 按层数循环扣钱生产，没钱则不扣也不生产
        for (int i = 0; i < stacks; i++)
        {
            // 检查资金是否足够
            if (dollarPower.DollarValue < UnitPrice)
            {
                GD.Print($"[TrainingQueuePower] 资金不足，停止生产 - 当前资金={dollarPower.DollarValue}, 所需资金={UnitPrice}");
                break;
            }

            // 扣除资金
            dollarPower.AddDollar(-UnitPrice);
            GD.Print($"[TrainingQueuePower] 扣除资金 {UnitPrice}，剩余资金 {dollarPower.DollarValue}");

            // 生产单位卡牌
            CardModel tempCard = combatState.CreateCard(cardModel, base.Owner.Player);

            if (IsUpgraded)
            {
                CardCmd.Upgrade(tempCard);
            }

            tempCard.EnergyCost.SetCustomBaseCost(0);

            // 根据 ExhaustWhenPlayed 属性决定是否添加消耗词条
            if (ExhaustWhenPlayed)
            {
                tempCard.AddKeyword(CardKeyword.Exhaust);
                GD.Print($"[TrainingQueuePower] 单位消耗: 是 - UnitName={UnitName}");
            }
            else
            {
                GD.Print($"[TrainingQueuePower] 单位消耗: 否 - UnitName={UnitName}");
            }

            await CardPileCmd.AddGeneratedCardToCombat(tempCard, PileType.Discard, Owner.Player, CardPilePosition.Top);
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