# 杀戮尖塔2 正式版与Beta版 API 差异对比

> 对比路径：
> - 正式版：`D:\RedAlert2Project\SlayTheSpire2Export\`
> - Beta版：`D:\RedAlert2Project\SlayTheSpire2Export_beta\`
> - 生成时间：2026-07-19

---

## 一、核心差异概览

| 模块 | 正式版 | Beta版 | 影响 |
|------|--------|--------|------|
| 多人卡牌传递 | 无原生API | `CardPileCmd.GiveToAnotherPlayer()` | 支援卡等需要传递卡牌的Mod功能 |
| 卡牌Owner切换 | 需反射修改 `_owner` | `CardModel.GiveToAnotherPlayer(player)` | 跨玩家卡牌转移 |
| Add方法参数 | 5个参数 | 新增 `isChangingOwners` 参数 | 防止重复触发 `AfterCardEnteredCombat` |
| 打牌结果返回 | `PileType` | `(PileType, CardPilePosition)` 元组 | 可指定卡牌打牌后的位置 |

---

## 二、CardPileCmd.cs 差异

### 2.1 新增方法：GiveToAnotherPlayer

**Beta版新增**，正式版无此方法。

```csharp
// Beta版新增
public static async Task GiveToAnotherPlayer(
    CardModel card, 
    Player player, 
    PileType pileType, 
    CardPilePosition position = CardPilePosition.Bottom, 
    AbstractModel? clonedBy = null)
{
    NCard cardNode = NCard.FindOnTable(card);
    card.RemoveFromCurrentPile(silent: true);
    card.GiveToAnotherPlayer(player);  // 调用CardModel的新方法
    bool islocalPlayerTheReceivingPlayer = LocalContext.IsMine(card);
    await Add(
        new ReadOnlySingleElementList<CardModel>(card), 
        pileType.GetPile(player), 
        position, 
        clonedBy, 
        skipVisuals: true, 
        isChangingOwners: true  // 新增参数
    );
    // ... VFX视觉效果处理
}
```

**关键步骤**：
1. 找到卡牌UI节点（用于VFX）
2. 从当前牌堆移除卡牌
3. 切换卡牌Owner
4. 添加到目标玩家牌堆（标记 `isChangingOwners: true`）
5. 播放卡牌飞行动画

### 2.2 Add 方法新增 isChangingOwners 参数

```csharp
// 正式版
public static async Task<IReadOnlyList<CardPileAddResult>> Add(
    IEnumerable<CardModel> cards, 
    CardPile newPile, 
    CardPilePosition position = CardPilePosition.Bottom, 
    AbstractModel? clonedBy = null, 
    bool skipVisuals = false)

// Beta版（新增参数）
public static async Task<IReadOnlyList<CardPileAddResult>> Add(
    IEnumerable<CardModel> cards, 
    CardPile newPile, 
    CardPilePosition position = CardPilePosition.Bottom, 
    AbstractModel? clonedBy = null, 
    bool skipVisuals = false, 
    bool isChangingOwners = false)  // 新增
```

**作用**：在Add方法内部，当 `isChangingOwners = true` 时，不会触发 `AfterCardEnteredCombat` 钩子：

```csharp
// Beta版 Add 方法内部
if (oldPile == null && targetPile.IsCombatPile && !isChangingOwners)
{
    await Hook.AfterCardEnteredCombat(card.CombatState, card);
}
```

**原因**：卡牌已经在战斗中（从一个玩家传递给另一个玩家），不应该被当作新进入战斗的卡牌处理。

### 2.3 其他UI改进

- 移除卡牌时新增 `NCardRemoveVfx` 视觉效果
- 大量空值安全检查（`?.` 运算符），防止 `NCombatRoom.Instance` 为 null 时崩溃
- 移除了一些 `modulate` 灰色动画
- 播放 `card_exhaust.mp3` 音效

---

## 三、CardModel.cs 差异

### 3.1 新增方法：GiveToAnotherPlayer

**Beta版新增**，直接设置 `_owner` 字段（绕过Owner setter的检查）。

```csharp
// Beta版新增
public void GiveToAnotherPlayer(Player player)
{
    _owner = player;
}
```

**正式版替代方案**：需要通过反射清除 `_owner` 后再设置Owner：

```csharp
// 正式版兼容写法
var ownerField = typeof(CardModel).GetField("_owner",
    BindingFlags.NonPublic | BindingFlags.Instance);
if (ownerField != null)
{
    ownerField.SetValue(card, null);
}
card.Owner = targetPlayer;
```

### 3.2 新增方法：CreateCloneForPlayer

**Beta版新增**，为指定玩家创建卡牌克隆。

```csharp
// Beta版新增
public CardModel CreateCloneForPlayer(Player player)
{
    CardModel cardModel = CreateClone();
    cardModel._owner = player;
    return cardModel;
}
```

### 3.3 GetResultPileTypeForCardPlay → GetResultPileTypeAndPositionForCardPlay

方法签名改变，返回值从单一 `PileType` 改为 `(PileType, CardPilePosition)` 元组。

```csharp
// 正式版
protected virtual PileType GetResultPileTypeForCardPlay()
{
    if (IsDupe || Type == CardType.Power)
        return PileType.None;
    if (ExhaustOnNextPlay || Keywords.Contains(CardKeyword.Exhaust))
    {
        ExhaustOnNextPlay = false;
        return PileType.Exhaust;
    }
    return PileType.Discard;
}

// Beta版
protected virtual (PileType, CardPilePosition) GetResultPileTypeAndPositionForCardPlay()
{
    if (IsDupe || Type == CardType.Power)
        return (PileType.None, CardPilePosition.Bottom);
    if (ExhaustOnNextPlay || Keywords.Contains(CardKeyword.Exhaust))
    {
        ExhaustOnNextPlay = false;
        return (PileType.Exhaust, CardPilePosition.Bottom);
    }
    return (PileType.Discard, CardPilePosition.Bottom);
}
```

**影响**：Mod中如果重写了此方法，需要在Beta版中改为新的签名。

### 3.4 其他改动

- `PortraitPngPath` 从 `private` 改为 `protected virtual`（允许子类重写）
- `PlayCard` 方法中增加战斗结束检查（`CombatManager.Instance.IsOverOrEnding`）

---

## 四、Beta版新增卡牌：TheBall（魔球）

### 4.1 基本信息
- 类型：攻击卡 (Attack)
- 费用：1费
- 稀有度：Uncommon
- 目标：任意敌人 (AnyEnemy)
- 限制：仅多人模式 (`MultiplayerOnly`)

### 4.2 核心效果

```csharp
protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
{
    // 造成伤害
    await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
        .FromCard(this, cardPlay)
        .Targeting(cardPlay.Target)
        .WithHitFx("vfx/vfx_attack_slash")
        .Execute(choiceContext);
    
    // 伤害递增
    DynamicVars.Damage.BaseValue += DynamicVars["Increase"].BaseValue;
    ExtraDamageFromPlays += DynamicVars["Increase"].BaseValue;
    
    // 多人模式：传递给队友
    if (CombatState != null && cardPlay.IsLastInSeries)
    {
        IEnumerable<Creature> teammates = 
            from c in CombatState.GetTeammatesOf(Owner.Creature)
            where c != null && c.IsAlive && c.IsPlayer && c.Player != Owner
            select c;
        
        if (teammates.Count() != 0)
        {
            // 将自己传递给随机队友的抽牌堆
            await CardPileCmd.GiveToAnotherPlayer(
                this, 
                Owner.RunState.Rng.CombatTargets.NextItem(teammates).Player, 
                PileType.Draw, 
                CardPilePosition.Random
            );
        }
    }
}
```

### 4.3 特殊：打牌后进入抽牌堆

```csharp
protected override (PileType, CardPilePosition) GetResultPileTypeAndPositionForCardPlay()
{
    var (pileType, item) = base.GetResultPileTypeAndPositionForCardPlay();
    if (pileType == PileType.Discard)
    {
        return (PileType.Draw, CardPilePosition.Random);
    }
    return (pileType, item);
}
```

---

## 五、CombatState 卡牌管理

### 5.1 正式版 CombatState 中的卡牌操作

```csharp
// 添加卡牌到战斗状态
public void AddCard(CardModel card, Player owner)
{
    card.Owner = owner;
    AddCard(card);
}

// 从战斗状态移除卡牌
public void RemoveCard(CardModel card)
{
    _allCards.Remove(card);
    card.Owner = null;  // 注意：这会将Owner设为null
}

// 检查卡牌是否在战斗中
public bool ContainsCard(CardModel card)
{
    return _allCards.Contains(card);
}
```

### 5.2 跨玩家传递卡牌的关键注意事项

在正式版中实现卡牌跨玩家传递时，必须同时处理：
1. **牌堆层面**：从原玩家牌堆移除 → 添加到目标玩家牌堆
2. **战斗状态层面**：从原玩家CombatState移除 → 添加到目标玩家CombatState
3. **Owner层面**：CombatState.RemoveCard会将Owner设为null，然后CombatState.AddCard会设置新Owner

**正式版完整实现参考**：

```csharp
private static async Task GiveCardToAnotherPlayer(
    CardModel card, 
    Player targetPlayer, 
    PileType pileType, 
    CardPilePosition position)
{
    Player originalOwner = card.Owner;
    var originalCombatState = originalOwner.Creature.CombatState;

    // 1. 从当前牌堆移除
    card.RemoveFromCurrentPile(silent: true);

    // 2. 从原玩家CombatState移除（会自动将Owner设为null）
    if (originalCombatState != null)
    {
        originalCombatState.RemoveCard(card);
    }

    // 3. 添加到目标玩家CombatState（会自动设置新Owner）
    var targetCombatState = targetPlayer.Creature.CombatState;
    if (targetCombatState != null)
    {
        targetCombatState.AddCard(card, targetPlayer);
    }
    else
    {
        // 非战斗状态下的兜底处理
        var ownerField = typeof(CardModel).GetField("_owner",
            BindingFlags.NonPublic | BindingFlags.Instance);
        if (ownerField != null)
            ownerField.SetValue(card, null);
        card.Owner = targetPlayer;
    }

    // 4. 添加到目标玩家牌堆
    await CardPileCmd.Add(card, pileType, position, skipVisuals: true);
}
```

---

## 六、版本切换建议

### 如果Mod需要同时兼容正式版和Beta版：

```csharp
// 使用条件编译或运行时检测
public static bool IsBetaVersion()
{
    return typeof(CardModel).GetMethod("GiveToAnotherPlayer") != null;
}

// 使用示例
if (IsBetaVersion())
{
    // Beta版：直接调用原生API
    await CardPileCmd.GiveToAnotherPlayer(card, targetPlayer, PileType.Hand);
}
else
{
    // 正式版：使用兼容实现
    await GiveCardToAnotherPlayerCompat(card, targetPlayer, PileType.Hand);
}
```

### 如果只针对Beta版开发：

可以直接使用所有新API，不需要兼容代码。

### 如果只针对正式版开发：

使用本文档中的兼容实现，注意CombatState层面的卡牌转移。

---

## 七、文件差异列表（部分）

| 文件 | 状态 | 说明 |
|------|------|------|
| `CardPileCmd.cs` | 新增方法+参数 | GiveToAnotherPlayer、isChangingOwners |
| `CardModel.cs` | 新增方法+签名变更 | GiveToAnotherPlayer、CreateCloneForPlayer、GetResultPileTypeAndPositionForCardPlay |
| `TheBall.cs` | 新增文件 | Beta版新卡牌（魔球），多人模式 |
| `CardSelectorPrefs.cs` | 无变化 | - |
| `CombatState.cs` | 无变化 | - |

---

*文档生成完成*
