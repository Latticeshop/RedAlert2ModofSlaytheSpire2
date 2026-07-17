# MOD卡牌快速创建流程

> 基于红警2 Mod开发经验整理，包含单位卡、建筑卡、绝地战备三种卡牌类型的完整创建流程

---

## 一、创造单位卡牌流程（以V3火箭为例）

### 1. 注册语音

#### 准备语音文件
```
RedAlert2ModResources/audio/SovietUnits/V3Rocket/
├── Vv3lata.mp3
├── Vv3latd.mp3
└── ...
```

#### 在 UnitVoiceConfig.cs 中注册
```csharp
["V3Rocket"] = new List<string>
{
    "res://RedAlert2ModResources/audio/SovietUnits/V3Rocket/Vv3lata.mp3",
    "res://RedAlert2ModResources/audio/SovietUnits/V3Rocket/Vv3latd.mp3",
    // ... 更多语音文件
},
```

#### 在卡牌 OnPlay() 中调用
```csharp
UnitVoiceHelper.PlayUnitVoice(this.GetType(), "Soviet");
```

---

### 2. 设置图标

#### 卡牌图片路径
```
RedAlert2ModResources/images/packed/card_portraits/soviet/v3icon.png
```

#### 在卡牌类中引用
```csharp
public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/soviet/v3icon.png";
```

---

### 3. 数值存储

#### 在 *CardValues.cs 中定义
```csharp
public static CardValueStore.CardValues V3Rocket => new()
{
    Cost = 2,
    Damage = 15,
    DamageUpgraded = 3,
    DollarValue = 800
};
```

#### 在价格映射表中注册（关键！）
```csharp
// 地面单位 → CreateVehicleValuesMap()
// 空军单位 → CreateAircraftValuesMap()
// 海军单位 → CreateShipValuesMap()

public static Dictionary<string, CardValueStore.CardValues> CreateVehicleValuesMap()
{
    return new()
    {
        { "RHINOTANK", RhinoTank },
        { "FLAKTRACK", FlakTrack },
        // ... 其他单位
        { "V3ROCKET", V3Rocket }  // 必须添加
    };
}
```

---

### 4. 生产序列选项

#### 在 *CardRegistry.cs 中注册
```csharp
public static List<Func<CardModel>> Vehicles { get; } = new()
{
    () => ModelDb.Card<RhinoTank>(),
    () => ModelDb.Card<FlakTrack>(),
    // ... 其他单位
    () => ModelDb.Card<V3Rocket>(),
};
```

---

### 5. 科技等级悬浮Tip（必须）

#### 在卡牌类中添加科技等级Tip（放在第一位）
所有单位卡必须添加T1/T2/T3科技等级悬浮Tip，放在第一位：

```csharp
protected override IEnumerable<IHoverTip> ExtraHoverTips =>
[
    ModCardKeywords.TechLevelT1.CreateHoverTip(),  // 必须放在第一位
    ModCardKeywords.Vehicle.CreateHoverTip()
];
```

#### 科技等级规则
| 等级 | 解锁条件 | 适用单位 |
|------|----------|----------|
| **T1** | 初始科技 | 基础单位（美国大兵、警犬、工程师等） |
| **T2** | 建造[gold]空指部/雷达/心灵探测仪[/gold]解锁 | 中级单位（入侵者战机、光棱塔、磁暴线圈等） |
| **T3** | 建造[gold]作战实验室[/gold]解锁 | 高级单位（超时空军团兵、超级武器等） |

---

### 6. 科技解锁（如需要）

#### 在 *TechTreeConfig.cs 中配置解锁条件
```csharp
new(typeof(SovietRadar), TechLevel.T3)
```

#### 在 *CardRegistry.cs 中添加条件解锁列表
```csharp
public static List<Func<CardModel>> RadarVehicles { get; } = new()
{
    () => ModelDb.Card<V3Rocket>(),
};
```

#### 在 CreateVehicles() 中检查能力
```csharp
public static List<CardModel> CreateVehicles(Player owner)
{
    List<CardModel> vehicles = Vehicles.Select(s => s()).ToList();
    if (HasRadarPower(owner.Creature)) vehicles.AddRange(RadarVehicles);
    return vehicles;
}
```

---

### 7. 动态数值配置

#### 在卡牌类中注册动态变量
```csharp
protected override List<DynamicVar> CanonicalVars => new()
{
    new DamageVar(Values.Damage, ValueProp.Move),       // 伤害（升级后自动变化）
    new BlockVar(Values.Block, ValueProp.Unpowered),    // 格挡
    new IntVar("MagicNumber", Values.MagicNumber),      // 自定义数值
    new DollarVar(Values.DollarValue),                  // 价格
};
```

#### 在 OnUpgrade() 中更新数值
```csharp
protected override void OnUpgrade()
{
    DynamicVars.Damage.UpgradeValueBy(Values.DamageUpgraded);
    DynamicVars["MagicNumber"].UpgradeValueBy(Values.MagicNumberUpgraded);
}
```

> **注意**：`DamageVar`、`BlockVar`、`DollarVar` 等自带升级方法，而 `IntVar` 需要手动处理升级逻辑。

---

### 8. 本地化

#### cards.json（中文）
```json
{
    "V3_ROCKET.title": "V3火箭",
    "V3_ROCKET.description": "选择一名敌人获得[gold]目标锁定[/gold]，获得[gold]V3火箭[/gold]。"
}
```

#### cards.json（英文）
```json
{
    "V3_ROCKET.title": "V3 Rocket",
    "V3_ROCKET.description": "Select an enemy to gain [gold]Target Locked[/gold]. Gain [gold]V3 Rocket[/gold]."
}
```

---

## 二、创造建筑卡牌流程（以雷达为例）

### 1. 设置图标

#### 卡牌图片路径
```
RedAlert2ModResources/images/packed/card_portraits/soviet/nradicon.png
```

#### 在卡牌类中引用
```csharp
public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/soviet/nradicon.png";
```

---

### 2. 数值存储

#### 在 *CardValues.cs 中定义
```csharp
public static CardValueStore.CardValues SovietRadar => new()
{
    Cost = 0,
    DollarValue = 1000
};
```

---

### 3. 动态数值配置

#### 在卡牌类中注册动态变量
```csharp
protected override List<DynamicVar> CanonicalVars => new()
{
    new DollarVar(Values.DollarValue),                  // 价格
    new IntVar("Damage", Values.Damage),                // 伤害（如磁暴线圈）
};
```

#### 在 OnUpgrade() 中更新数值
```csharp
protected override void OnUpgrade()
{
    DynamicVars["Damage"].BaseValue = Values.Stars;     // 升级后伤害变为升级值
}
```

> **示例**：磁暴线圈使用 `IntVar` 存储伤害，升级后通过 `OnUpgrade()` 将伤害值替换为 `Values.Stars`。

---

### 4. 创建对应能力

#### 创建 *Power.cs
```csharp
public sealed class SovietRadarPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    
    // 能力逻辑...
}
```

#### 注册能力图标（通过 PowerIconPatch.cs）
```csharp
{ typeof(SovietRadarPower), "res://RedAlert2ModResources/images/packed/card_portraits/soviet/nradicon.png" },
```

---

### 5. 注册到建筑列表（关键！）

#### 在 *CardRegistry.cs 的 BuildingCards 中注册
```csharp
public static List<Func<CardModel>> BuildingCards { get; } = new()
{
    () => ModelDb.Card<SovietBarracksCard>(),
    () => ModelDb.Card<SovietWarFactory>(),
    // ... 其他建筑
    () => ModelDb.Card<SovietRadar>(),  // 必须添加
};
```

---

### 6. 科技线配置

#### 在 *TechTreeConfig.cs 中配置解锁条件
```csharp
new(typeof(SovietRadar), TechLevel.T3),
```

---

### 7. 播放建筑音效（强制规则：全部建筑一律使用通用建筑部署语音）

> **⚠️ 强制规定**：所有建筑卡牌（苏军/盟军/公共）**必须统一调用** `BuildingSoundHelper.PlayBuildingPlaceSound()` 播放通用建筑部署音效（资源：`CommonSFX/building_place.wav`），**禁止为单个建筑定义或尝试加载专属部署音效**。
>
> **理由（核电站踩坑教训）**：
> - 如果尝试加载不存在的专属音效（如 `nuclear_plant_deploy.mp3`），Godot 会在控制台打印 `Error loading resource: ... Condition "err != OK" is true. Returning: ret` 以及一长串 C# backtrace，污染日志且可能影响其他资源加载稳定性。
> - 建筑部署音效在红警系列中本身就是统一听觉标识，通用音效符合玩家预期。
> - 减少音频资源目录碎片化，降低 Godot `.import` 文件数量和 PCK 包体积。

#### 在 OnPlay() 中调用（唯一正确写法）

```csharp
protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
{
    // ↓ 第一行就调用，必须写在任何 await 之前
    BuildingSoundHelper.PlayBuildingPlaceSound();

    // ... 扣除资金、申请对应 Power 等建筑逻辑
}
```

#### 错误写法（不要这样写）

```csharp
// ❌ 错误1：尝试 GD.Load 不存在的专属音效
var sound = GD.Load<AudioStream>("res://.../nuclear_plant_deploy.mp3"); // Godot会报错！
if (sound != null) { ... } else { BuildingSoundHelper.PlayBuildingPlaceSound(); }

// ❌ 错误2：每次 OnPlay new AudioStreamPlayer，不使用单例导致对象泄漏
var audioPlayer = new AudioStreamPlayer();
root.Root.AddChild(audioPlayer);
audioPlayer.Stream = sound; audioPlayer.Play();
```

> **例外情况（不播放音效）**：
> - 资源类卡牌（黄金矿 GoldMineCard、宝石矿 GemMineCard、黄金矿柱、油井 OilDerrickCard）——已有资金/储备相关音效或反馈
> - 围墙类卡牌（Wall / Fence / Barricade 等轻量级防御）
> - 出售建筑类卡牌（SellBuildingCard 等）→ 使用 `BuildingSoundHelper.PlayBuildingSellSound()`

---

### 8. 本地化

#### cards.json（建筑卡需包含价格）
```json
{
    "SOVIET_RADAR.title": "雷达",
    "SOVIET_RADAR.description": "价格：${DollarNumber}。获得雷达能力，解锁V3火箭生产。"
}
```

#### powers.json
```json
{
    "SOVIET_RADAR_POWER.title": "雷达",
    "SOVIET_RADAR_POWER.smartDescription": "已解锁V3火箭生产。"
}
```

---

## 三、创造绝地战备流程（以飞鹰500kg为例）

### 1. 创建能力类

#### 创建 *Power.cs
```csharp
public sealed class Eagle500kgPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == CombatSide.Player && Owner != null)
        {
            // 播放攻击动画
            PlaySmashEffect(target);
            // 造成伤害
            await CreatureCmd.Damage(...);
            // 去除自身一层
            await PowerCmd.Remove(this);
        }
    }
}
```

---

### 2. 注册能力图标

#### 准备图标文件
```
RedAlert2ModResources/images/packed/powers/Eagle500kgPower.png
```

#### 在 PowerIconPatch.cs 中注册
```csharp
{ typeof(Eagle500kgPower), "res://RedAlert2ModResources/images/packed/powers/Eagle500kgPower.png" },
```

---

### 3. 创建战备卡牌

#### 创建卡牌类
```csharp
public sealed class Eagle500kg : CardModel
{
    public Eagle500kg() : base(1, CardType.Power, CardRarity.Rare, TargetType.Self) { }
    
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await PowerCmd.Apply<Eagle500kgPower>(ctx, Owner.Creature, 1, Owner.Creature, this);
    }
}
```

---

### 4. 数值存储

#### 在 *PowerValues.cs 中定义
```csharp
public static PowerValueStore.PowerValues Eagle500kg => new()
{
    Damage = 15,
    DamageUpgraded = 3
};
```

---

### 5. 动态数值配置

#### 在卡牌类中注册动态变量
```csharp
protected override List<DynamicVar> CanonicalVars => new()
{
    new IntVar("Damage", Values.Damage),
};
```

#### 在 OnUpgrade() 中更新数值
```csharp
protected override void OnUpgrade()
{
    DynamicVars["Damage"].BaseValue = Values.Damage + Values.DamageUpgraded;
}
```

---

### 6. 配置攻击动画

#### 使用游戏内置特效
```csharp
private void PlaySmashEffect(Creature target)
{
    VfxCmd.Play("vfx_heavy_blunt", target);
    VfxCmd.Play("vfx_bloody_impact", target);
}
```

---

### 7. 本地化

#### cards.json
```json
{
    "EAGLE_500KG.title": "飞鹰500kg",
    "EAGLE_500KG.description": "获得[gold]飞鹰500kg[/gold]能力。"
}
```

#### powers.json
```json
{
    "EAGLE_500KG_POWER.title": "飞鹰500kg",
    "EAGLE_500KG_POWER.smartDescription": "回合开始对随机敌人造成{Damage}点伤害。"
}
```

---

### 8. 注册到卡池

#### 在 *CardRegistry.cs 的技能卡列表中注册
```csharp
public static List<Func<CardModel>> PowerCards { get; } = CreatePowerCards();

private static List<Func<CardModel>> CreatePowerCards()
{
    var cards = CommonCardRegistry.GetAllPowerCardsForSoviet();
    cards.Add(() => ModelDb.Card<Eagle500kg>());
    return cards;
}
```

---

## 四、创造公共卡牌流程（以黄金矿为例）

### 适用场景

公共卡牌是指在两个或多个阵营中都存在的卡牌，逻辑完全相同，但需要独立的卡牌实例和本地化（用于卡框颜色和独立文本）。

**公共卡牌列表**：
| 卡牌名称 | 类型 | 说明 |
|---------|------|------|
| 黄金矿 | Power卡 | 资源类卡牌，获得黄金矿储备 |
| 宝石矿 | Power卡 | 资源类卡牌，获得宝石矿储备 |
| 黄金矿柱 | Power卡 | 资源类卡牌，获得黄金矿储备和矿柱能力 |
| 油井 | Power卡 | 建筑类卡牌，获得资金 |
| 卖本 | Attack卡 | 技能卡，售卖基地车获得资金 |
| 集结 | Skill卡 | 技能卡，召集单位卡到手牌 |
| 伞兵 | Attack卡 | 技能卡，添加虚无士兵到手牌 |
| 停产 | Skill卡 | 技能卡，控制生产序列启停 |

---

### 方案一：继承分离模式（传统方案）

#### 1. 创建公共基类

在 `Common/Cards/` 目录下创建：

```csharp
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.HoverTips;
using System.Collections.Generic;
using System.Linq;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Common.Cards;

public class GoldMineCard : CardModel
{
    private static readonly CardValueStore.CardValues Values = CommonCardValues.GoldMine;
    
    public GoldMineCard() : base((int)Values.Cost, CardType.Power, CardRarity.Common, TargetType.Self) { }

    public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/gold_mine.png";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ModCardKeywords.GoldMine.CreateHoverTip()
    ];

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new IntVar("Reserve", Values.DollarValue)
    };

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        int amount = base.DynamicVars["Reserve"].IntValue;

        var goldMinePower = Owner.Creature.Powers.OfType<GoldMinePower>().FirstOrDefault();
        if (goldMinePower != null)
        {
            goldMinePower.AddReserve(amount);
        }
        else
        {
            var newPower = await PowerCmd.Apply<GoldMinePower>(ctx, Owner.Creature, 1m, Owner.Creature, null);
            if (newPower != null)
            {
                newPower.CurrentReserve = amount;
                newPower.IsUpgraded = IsUpgraded;
            }
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["Reserve"].BaseValue = Values.DollarValue + Values.DollarValueUpgraded;
    }
}
```

> **注意**：公共基类不带阵营前缀，且**不使用 sealed**（允许继承）。

---

#### 2. 创建盟军子类

在 `Allies/Cards/` 目录下创建：

```csharp
using RedAlert2ModCode.Common.Cards;

namespace RedAlert2ModCode.Allies.Cards;

public sealed class AlliesGoldMineCard : GoldMineCard
{
}
```

> **注意**：盟军子类带 `Allies` 前缀，使用 `sealed`（不允许进一步继承）。

---

#### 3. 创建苏军子类

在 `Soviet/Cards/` 目录下创建：

```csharp
using RedAlert2ModCode.Common.Cards;

namespace RedAlert2ModCode.Soviet.Cards;

public sealed class SovietGoldMineCard : GoldMineCard
{
}
```

> **注意**：苏军子类带 `Soviet` 前缀，使用 `sealed`（不允许进一步继承）。

---

#### 4. 数值存储

在 `CommonCardValues.cs` 中定义：

```csharp
public static CardValueStore.CardValues GoldMine => new()
{
    Cost = 1,
    DollarValue = 1000,
    DollarValueUpgraded = 500
};
```

---

#### 5. 注册到阵营卡池

在 `AlliedCardRegistry.cs` 中注册：

```csharp
private static List<Func<CardModel>> CreatePowerCards()
{
    var cards = new List<Func<CardModel>>();
    cards.Add(() => ModelDb.Card<AlliesGoldMineCard>());
    cards.Add(() => ModelDb.Card<AlliesGemMineCard>());
    cards.Add(() => ModelDb.Card<AlliesGoldMineColumnCard>());
    cards.Add(() => ModelDb.Card<AlliesOilDerrickCard>());
    cards.Add(() => ModelDb.Card<AlliesSellMCV>());
    cards.Add(() => ModelDb.Card<AlliesRa2Rally>());
    cards.Add(() => ModelDb.Card<AlliesParatrooper>());
    cards.Add(() => ModelDb.Card<AlliesStopProductionCard>());
    // ... 其他盟军专属卡牌
    return cards;
}
```

在 `SovietCardRegistry.cs` 中注册：

```csharp
private static List<Func<CardModel>> CreatePowerCards()
{
    var cards = new List<Func<CardModel>>();
    cards.Add(() => ModelDb.Card<SovietGoldMineCard>());
    cards.Add(() => ModelDb.Card<SovietGemMineCard>());
    cards.Add(() => ModelDb.Card<SovietGoldMineColumnCard>());
    cards.Add(() => ModelDb.Card<SovietOilDerrickCard>());
    cards.Add(() => ModelDb.Card<SovietSellMCV>());
    cards.Add(() => ModelDb.Card<SovietRa2Rally>());
    cards.Add(() => ModelDb.Card<SovietParatrooper>());
    cards.Add(() => ModelDb.Card<SovietStopProductionCard>());
    // ... 其他苏军专属卡牌
    return cards;
}
```

---

#### 6. 创建对应能力（如需要）

公共卡牌使用的能力存放在 `Common/Powers/` 目录，两个阵营共用同一份：

```csharp
// Common/Powers/GoldMinePower.cs
public sealed class GoldMinePower : PowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    
    public int CurrentReserve { get; set; } = 0;
    
    public void AddReserve(int amount)
    {
        CurrentReserve += amount;
    }
}
```

注册能力图标到 `PowerIconPatch.cs`：

```csharp
{ typeof(GoldMinePower), "res://RedAlert2ModResources/images/packed/powers/GoldMinePower.png" },
```

---

#### 7. 本地化（两份）

由于游戏使用类名自动生成本地化key，公共卡牌需要创建两份本地化条目：

**cards.json（中文）**：

```json
{
    "ALLIES_GOLD_MINE_CARD.title": "黄金矿",
    "ALLIES_GOLD_MINE_CARD.description": "获得 {Reserve} [gold]黄金矿[/gold]储备。",
    "SOVIET_GOLD_MINE_CARD.title": "黄金矿",
    "SOVIET_GOLD_MINE_CARD.description": "获得 {Reserve} [gold]黄金矿[/gold]储备。"
}
```

**cards.json（英文）**：

```json
{
    "ALLIES_GOLD_MINE_CARD.title": "Gold Mine",
    "ALLIES_GOLD_MINE_CARD.description": "Gains {Reserve} [gold]Gold Mine[/gold] reserve.",
    "SOVIET_GOLD_MINE_CARD.title": "Gold Mine",
    "SOVIET_GOLD_MINE_CARD.description": "Gains {Reserve} [gold]Gold Mine[/gold] reserve."
}
```

> **重要**：两份本地化内容相同，但必须分别创建，因为游戏使用类名自动生成key。

---

#### 8. 能力本地化（共用）

能力本地化只需一份，存放在 `powers.json` 中：

```json
{
    "GOLD_MINE_POWER.title": "黄金矿",
    "GOLD_MINE_POWER.smartDescription": "黄金矿储备：{CurrentReserve}"
}
```

---

### 方案二：Pool动态切换模式（推荐方案）

参考"海克斯符文"mod的"白洞"卡牌实现，通过重写 `Pool` 和 `VisualCardPool` 属性，让同一公共卡牌实例根据当前持有者动态切换阵营颜色。

#### 1. 创建公共基类（带Pool切换）

在 `Common/Cards/` 目录下创建：

```csharp
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.HoverTips;
using System.Collections.Generic;
using System.Linq;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Common.Cards;

public class GoldMineCard : CardModel
{
    private static readonly CardValueStore.CardValues Values = CommonCardValues.GoldMine;
    
    public GoldMineCard() : base((int)Values.Cost, CardType.Power, CardRarity.Common, TargetType.Self) { }

    public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/gold_mine.png";

    public override CardPoolModel Pool => IsMutable && Owner != null
        ? Owner.Character.CardPool
        : ModelDb.CardPool<TokenCardPool>();

    public override CardPoolModel VisualCardPool => Pool;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ModCardKeywords.GoldMine.CreateHoverTip()
    ];

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new IntVar("Reserve", Values.DollarValue)
    };

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        int amount = base.DynamicVars["Reserve"].IntValue;

        var goldMinePower = Owner.Creature.Powers.OfType<GoldMinePower>().FirstOrDefault();
        if (goldMinePower != null)
        {
            goldMinePower.AddReserve(amount);
        }
        else
        {
            var newPower = await PowerCmd.Apply<GoldMinePower>(ctx, Owner.Creature, 1m, Owner.Creature, null);
            if (newPower != null)
            {
                newPower.CurrentReserve = amount;
                newPower.IsUpgraded = IsUpgraded;
            }
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["Reserve"].BaseValue = Values.DollarValue + Values.DollarValueUpgraded;
    }
}
```

**关键代码说明**：

| 属性 | 说明 |
|------|------|
| `Pool` | 卡牌所属的卡池。游戏根据卡池决定卡框颜色和其他视觉属性 |
| `VisualCardPool` | 卡牌在UI上显示时使用的卡池。通常与Pool相同 |
| `IsMutable` | 判断卡牌是否为战斗实例（而非原型）。战斗实例才有Owner |
| `Owner.Character.CardPool` | 获取当前持有者的阵营卡池（如 `AlliesCardPool`、`SovietCardPool`） |
| `TokenCardPool` | 无主卡牌（如百科中）使用的卡池，显示为白色/无色 |

**颜色显示逻辑**：

| 场景 | 条件 | 显示颜色 |
|------|------|---------|
| 百科中 | `IsMutable == false` 或 `Owner == null` | 白色/无色（TokenCardPool） |
| 游戏中-盟军 | `Owner.Character.CardPool` 返回 `AlliesCardPool` | 蓝色（盟军） |
| 游戏中-苏军 | `Owner.Character.CardPool` 返回 `SovietCardPool` | 红色（苏军） |

---

#### 2. 数值存储

与方案一相同，在 `CommonCardValues.cs` 中定义。

---

#### 3. 注册到阵营卡池（直接使用公共基类）

在 `AlliedCardRegistry.cs` 中注册：

```csharp
private static List<Func<CardModel>> CreatePowerCards()
{
    var cards = new List<Func<CardModel>>();
    cards.Add(() => ModelDb.Card<GoldMineCard>());
    cards.Add(() => ModelDb.Card<GemMineCard>());
    cards.Add(() => ModelDb.Card<GoldMineColumnCard>());
    cards.Add(() => ModelDb.Card<OilDerrickCard>());
    cards.Add(() => ModelDb.Card<SellMCV>());
    cards.Add(() => ModelDb.Card<Ra2Rally>());
    cards.Add(() => ModelDb.Card<Paratrooper>());
    cards.Add(() => ModelDb.Card<StopProductionCard>());
    // ... 其他盟军专属卡牌
    return cards;
}
```

在 `SovietCardRegistry.cs` 中注册（使用同一个类）：

```csharp
private static List<Func<CardModel>> CreatePowerCards()
{
    var cards = new List<Func<CardModel>>();
    cards.Add(() => ModelDb.Card<GoldMineCard>());
    cards.Add(() => ModelDb.Card<GemMineCard>());
    cards.Add(() => ModelDb.Card<GoldMineColumnCard>());
    cards.Add(() => ModelDb.Card<OilDerrickCard>());
    cards.Add(() => ModelDb.Card<SellMCV>());
    cards.Add(() => ModelDb.Card<Ra2Rally>());
    cards.Add(() => ModelDb.Card<Paratrooper>());
    cards.Add(() => ModelDb.Card<StopProductionCard>());
    // ... 其他苏军专属卡牌
    return cards;
}
```

---

#### 4. 创建对应能力（如需要）

与方案一相同。

---

#### 5. 本地化（仅需一份）

**cards.json（中文）**：

```json
{
    "GOLD_MINE_CARD.title": "黄金矿",
    "GOLD_MINE_CARD.description": "获得 {Reserve} [gold]黄金矿[/gold]储备。"
}
```

**cards.json（英文）**：

```json
{
    "GOLD_MINE_CARD.title": "Gold Mine",
    "GOLD_MINE_CARD.description": "Gains {Reserve} [gold]Gold Mine[/gold] reserve."
}
```

---

#### 6. 能力本地化（共用）

与方案一相同。

---

### 方案对比

| 对比项 | 方案一：继承分离 | 方案二：Pool动态切换 |
|--------|----------------|-------------------|
| 代码量 | 多（每个卡牌需要3个文件） | 少（每个卡牌只需要1个文件） |
| 本地化 | 需要两份（带阵营前缀） | 需要一份（无阵营前缀） |
| 百科显示 | 显示阵营颜色 | 显示白色/无色（符合预期） |
| 游戏内颜色 | 根据阵营子类自动切换 | 根据Owner动态切换 |
| 实例隔离 | 完全隔离（不同单例） | 共享实例（但通过Pool动态区分） |
| 适用场景 | 需要独立定制描述 | 逻辑完全相同的公共卡牌 |

---

### 公共卡牌创建流程总结（方案二推荐）

| 步骤 | 操作 | 文件路径 |
|------|------|---------|
| 1 | 创建公共基类（完整逻辑+Pool切换） | `Common/Cards/GoldMineCard.cs` |
| 2 | 定义数值 | `Common/Cards/CommonCardValues.cs` |
| 3 | 注册到盟军卡池（直接使用基类） | `Allies/AlliedCardRegistry.cs` |
| 4 | 注册到苏军卡池（直接使用基类） | `Soviet/SovietCardRegistry.cs` |
| 5 | 创建共用能力（如需要） | `Common/Powers/GoldMinePower.cs` |
| 6 | 注册能力图标 | `Allies/Powers/PowerIconPatch.cs` |
| 7 | 添加本地化（仅需一份） | `localization/zhs/cards.json` |
| 8 | 添加能力本地化（共用） | `localization/zhs/powers.json` |

---

## 五、常见遗漏检查清单

| 检查项 | 单位卡 | 建筑卡 | 战备卡 | 公共卡 |
|--------|:------:|:------:|:------:|:------:|
| 语音注册 | ✅ | ❌ | ❌ | ❌ |
| 卡牌图标 | ✅ | ✅ | ✅ | ✅ |
| 数值存储 | ✅ | ✅ | ✅ | ✅（Common） |
| 价格映射 | ✅ | ✅ | ❌ | ❌ |
| CardRegistry注册 | ✅ | ✅ | ✅ | ✅（双阵营） |
| 能力图标补丁 | ❌ | ✅ | ✅ | ✅（共用） |
| 科技解锁配置 | ✅ | ✅ | ❌ | ❌ |
| 本地化 | ✅ | ✅ | ✅ | ✅（双份） |
| 音效播放 | ✅ UnitVoiceHelper | ✅ **强制统一：BuildingSoundHelper.PlayBuildingPlaceSound()，禁止专属音效** | ❌ | ❌（资源卡免播放） |

---

## 五、动态悬浮Tip升级机制（HoverTipHelper）

当卡牌的衍生卡效果会随升级而变化时，应使用 `HoverTipHelper` 根据源卡牌的升级状态动态显示对应版本的衍生卡牌。

### 使用示例

```csharp
using RedAlert2ModCode.Common.Utils;

public sealed class AlliedRefinery : CardModel
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ModCardKeywords.Building.CreateHoverTip(),
        HoverTipHelper.FromCardWithUpgrade<ChronoMiner>(() => IsUpgraded)
    ];
}
```

### 适用场景

| 场景 | 说明 |
|------|------|
| 建筑卡生产单位卡 | 矿场升级后，生产的矿车也会升级 |
| 超级武器建筑 | 升级后冷却回合减少，产生的超级武器卡牌效果增强 |
| 能力卡衍生效果 | 能力卡升级后，衍生卡牌的数值或效果发生变化 |

### 工具类定义

```csharp
// RedAlert2ModCode/Common/Utils/HoverTipHelper.cs
public static class HoverTipHelper
{
    public static IHoverTip FromCardWithUpgrade<T>(Func<bool> isUpgradedFunc) where T : CardModel
    {
        var model = ModelDb.Card<T>();
        var mutable = model.ToMutable();
        
        if (isUpgradedFunc())
        {
            mutable.UpgradeInternal();
        }
        
        return HoverTipFactory.FromCard(mutable);
    }
}
```

---

## 六、路径规范总结

### 语音文件路径
```
res://RedAlert2ModResources/audio/{阵营}Units/{单位名称}/{文件名}.mp3
```

### 卡牌图片路径
```
res://RedAlert2ModResources/images/packed/card_portraits/{阵营}/{图标名}.png
```

### 能力图标路径
```
res://RedAlert2ModResources/images/packed/powers/{能力名称}Power.png
```

### 本地化文件路径
```
RedAlert2Mod/localization/zhs/cards.json    # 中文卡牌
RedAlert2Mod/localization/eng/cards.json    # 英文卡牌
RedAlert2Mod/localization/zhs/powers.json   # 中文能力
RedAlert2Mod/localization/eng/powers.json   # 英文能力
```

---

## 七、ID转换规则

```
MyClassName → MY_CLASS_NAME

示例：
V3Rocket → V3_ROCKET
SovietRadar → SOVIET_RADAR
Eagle500kg → EAGLE_500KG
AlliesGoldMineCard → ALLIES_GOLD_MINE_CARD
SovietGoldMineCard → SOVIET_GOLD_MINE_CARD
```

---

## 八、编译与部署

### 编译命令
```bash
dotnet build RedAlert2Mod.csproj -c Release -o build
```

### 部署文件（复制到游戏 mods/RedAlert2Mod/ 目录）
- `RedAlert2Mod.dll` - 主程序集（必须）
- `RedAlert2Mod.json` - Mod配置文件（必须）
- `RedAlert2Mod.pck` - 资源包（如有资源）

---

## 九、重要注意事项

### 1. 项目文件优先重命名英文再使用

> **必须遵循**：所有资源文件（语音、图片、图标等）在添加到项目前，必须将文件名和目录名改为英文。

**原因**：
- Godot引擎对中文路径支持不完善，可能导致资源无法加载
- Harmony补丁和反射机制在处理中文路径时可能出现编码问题
- 避免跨平台兼容性问题（Linux/macOS对中文支持更差）

**示例**：
```
错误：
RedAlert2ModResources/audio/SovietUnits/基洛夫/Vkirmoc.mp3
RedAlert2ModResources/audio/SovietUnits/台风级潜艇/Vsubata.mp3

正确：
RedAlert2ModResources/audio/SovietUnits/Kirov/Vkirmoc.mp3
RedAlert2ModResources/audio/SovietUnits/TyphoonSubmarine/Vsubata.mp3
```

**操作步骤**：
1. 将中文目录重命名为英文（如 `基洛夫` → `Kirov`）
2. 将中文文件名重命名为英文（如 `V3发射.mp3` → `v3_launch.mp3`）
3. 更新代码中的路径引用
4. 删除重命名后残留的 `.import` 文件（Godot会自动重新生成）

---

---

## 十、联机同步实现指南

### 适用场景

当你的卡牌需要弹出自定义UI面板让玩家进行选择时（如基地车展开、工程师选择、出售建筑等），必须实现联机同步，否则会导致联机模式下状态不一致（StateDivergence）。

### 核心原则

**仅本地玩家显示和操作面板，其他玩家等待结果同步**。

### 实现步骤

#### 步骤1：创建基础显示方法（ShowSelection）

```csharp
public static async Task<int?> ShowSelection(object title, List<ChoiceOption> options, Player player, FactionType faction = FactionType.Allied)
{
    var screen = new DeployChoiceScreen(faction);
    screen._title = title;
    screen._options = options;
    screen.BuildUi();
    screen.UpdateUiText();
    NOverlayStack.Instance?.Push(screen);
    
    if (!MultiplayerSyncHelper.IsLocalPlayer(player))
    {
        screen.Close();
        return null;
    }
    
    return await screen._completionSource.Task;
}
```

**关键要点**：
- 使用 `NOverlayStack.Instance?.Push(screen)` 将面板推入UI栈
- 在 `ShowSelection` 开头检查 `IsLocalPlayer`，非本地玩家立即关闭面板
- 使用 `TaskCompletionSource` 等待用户选择

#### 步骤2：创建同步显示方法（ShowSelectionWithSync）

```csharp
public static async Task<int?> ShowSelectionWithSync(Player player, object title, List<ChoiceOption> options, FactionType faction = FactionType.Allied)
{
    return await MultiplayerSyncHelper.ExecuteSyncChoice(player, async () =>
    {
        return await ShowSelection(title, options, player, faction);
    });
}
```

#### 步骤3：实现 Close() 方法

所有自定义UI面板必须实现 `Close()` 方法，用于清理面板资源：

```csharp
public void Close()
{
    if (_choiceLocked) return;
    _choiceLocked = true;
    _completionSource.TrySetResult(null);
    NOverlayStack.Instance?.Remove(this);
    QueueFree();
}
```

### 单选同步示例

适用于只需选择一个选项的场景（如工程师选择）：

```csharp
public static async Task<EngineerChoice?> ShowSelectionWithSync(
    List<EngineerChoice> choices, 
    string? engineerPortraitPath, 
    Player player, 
    FactionType faction = FactionType.Allied)
{
    List<EngineerChoice> choicesCopy = new(choices);
    
    int? selectedIndex = await MultiplayerSyncHelper.ExecuteSyncChoice(player, async () =>
    {
        EngineerChoice? choice = await ShowSelection(choicesCopy, engineerPortraitPath, player, faction);
        return choice != null ? choicesCopy.FindIndex(c => c.Type == choice.Type) : null;
    });
    
    if (selectedIndex.HasValue && selectedIndex.Value >= 0 && selectedIndex.Value < choicesCopy.Count)
    {
        return choicesCopy[selectedIndex.Value];
    }
    
    return null;
}
```

### 多选同步示例

适用于可选择多个选项的场景（如出售建筑）：

```csharp
public static async Task<List<int>> ShowSelectionWithSync(
    List<(PowerModel Power, int Index)> buildingPowerItems, 
    int maxSelect, 
    Player player, 
    FactionType faction)
{
    List<(PowerModel Power, int Index)> itemsCopy = new(buildingPowerItems);

    return await MultiplayerSyncHelper.ExecuteSyncMultiChoice(player, async () =>
    {
        List<int>? selected = await ShowSelection(itemsCopy, maxSelect, player, faction);
        return selected;
    });
}
```

### 在卡牌中调用同步方法

```csharp
protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
{
    var buildingPowerItems = GetBuildingPowerItems(Owner.Creature);
    
    List<int> selectedIndices = await SellBuildingScreen.ShowSelectionWithSync(
        buildingPowerItems, maxSelection, Owner, faction);
    
    foreach (int index in selectedIndices)
    {
        var item = buildingPowerItems[index];
        // ... 执行逻辑
    }
}
```

### 关键注意事项

| 注意事项 | 说明 |
|---------|------|
| **数据复制** | 在同步方法中必须创建数据副本，避免并发修改导致的状态不一致 |
| **索引传递** | 通过索引而非对象引用传递选择结果，确保不同客户端间的一致性 |
| **Close方法** | 所有自定义UI面板必须实现 `public void Close()` 方法，用于清理非本地玩家的面板 |
| **IsLocalPlayer检查** | 在 `ShowSelection` 方法开头检查，非本地玩家立即关闭面板 |
| **取消处理** | 当用户取消选择时，应返回 `null` 或空列表，调用方需处理此情况 |

### 常见遗漏检查清单（联机同步）

| 检查项 | 是否需要 | 说明 |
|--------|:--------:|------|
| 创建 ShowSelection 方法 | ✅ | 基础显示方法，包含 IsLocalPlayer 检查 |
| 创建 ShowSelectionWithSync 方法 | ✅ | 同步显示方法，使用 MultiplayerSyncHelper |
| 实现 Close() 方法 | ✅ | 用于清理面板资源 |
| 数据副本创建 | ✅ | 在同步方法中创建数据副本 |
| 索引传递 | ✅ | 通过索引而非对象引用传递选择结果 |
| 使用 ShowSelectionWithSync 调用 | ✅ | 在卡牌逻辑中使用同步方法 |

---

## 十一、能力创建：叠层 vs 独立实例 快速决策

创建建筑/战备/资源类 Power 时，第一时间决定 `InstanceType`，否则后续返工成本极高。

### 三步速选法

| 步骤 | 问题 | 选 Yes | 选 No |
|-----|------|--------|-------|
| ① | 这个能力是否含有**自定义实例字段**？<br>（例：CurrentHealth、CurrentReserve、CustomCounter、per-building timer 等） | → 进入步骤② | → **InstanceType = None**（叠 Amount） |
| ② | 再打一张同类卡牌，新字段的值是否可以**和现有实例的字段值不同**？<br>（例：升级卡 vs 非升级卡血量不同；不同宝石矿的独立储备不同） | → **InstanceType = Instanced**（独立实例，不叠 Amount） | → **InstanceType = None**（叠 Amount，共享一套字段） |
| ③ | 多人联机下，效果是否要按施放者(Player)分别计算？ | → **InstanceType = InstancedPerApplier** | → 回到步骤②即可 |

> **什么是"自定义实例字段"？** 除了 `PowerModel.Amount` 框架自带层数外，你自己在 Power 类里 `public int Xxx { get; set; }` 加的任何字段都是。只要有，99% 场景应该选 `Instanced`。

### 一句话口诀

- **有自定义字段 → Instanced（每次都新建）**
- **全靠 Amount 过日子 → None（让框架自动叠加）**

### 快速对照：红警Mod常见场景正确答案

| 卡牌 / 能力 | 正确 InstanceType | 为什么 |
|------------|-------------------|-------|
| 核电站 → NuclearReactorCorePower | `Instanced` | 每个核电站有独立 `CurrentHealth`，爆炸/受伤互不干扰 |
| 宝石矿 → GemMinePower | `Instanced` | 代码里已写 `public int CurrentReserve`，每座矿独立储备 |
| 黄金矿 → GoldMinePower | `Instanced` | 同上，有独立 `CurrentReserve` |
| 雷达 → SovietRadarPower / AlliedRadarPower | `Instanced` / `None` 均可 | 如果只做解锁标志，建议选 `None` + Amount 叠层；如果后续要加每雷达独立扫描计数，选 `Instanced` |
| 作战实验室 → SovietBattleLabPower | `Instanced` | 出售时需要逐个确认，且可能加独立科技点字段 |
| 碉堡 / 磁暴线圈 / 光棱塔 | `Instanced` | 每座建筑独立战斗状态、独立伤害计数 |
| 资金 → DollarPower / RaidDollarPower | `None` | 只有 Amount 一个数值，所有来源合并 |
| 飞鹰500kg / 闪电风暴 战备 | `Instanced` | 多战备独立触发、独立倒计时 |
| 力量 / 敏捷 / 中毒 / 虚弱 / 易伤（原版） | `None` | 纯数值 Amount |

### 常见坑速查

| 现象 | 根因 | 修复 |
|-----|------|------|
| 配置了 `InstanceType = Instanced`，但打了 N 张卡只显示 1 个图标，右下角数字是 N | OnPlay 里手写了 `OfType<Xxx>().FirstOrDefault() → ModifyAmount`，绕过了 Instanced 机制 | 删掉这段手动叠层，**直接 `PowerCmd.Apply<T>(amount: 1, …)`** |
| `InstanceType = None`，打第二张卡抛异常 "Trying to add multiple instances of a non-instanced power" | Creature.AddPower 做了校验。说明你确实需要多实例，但参数写错了 | 改为 `InstanceType = Instanced` |
| 宝石矿/核电站的 `CurrentXxx` 字段值，在打第二张卡时会被第一张卡的逻辑串改 | 第一张卡实例被 Find 到然后手动 ModifyAmount，但自定义字段没同步。根源还是"不该手动叠层却叠了" | 同上，让 Instanced 自己工作 |

---

*本文档基于实际开发经验整理，与API快速参考手册配合使用效果更佳。*