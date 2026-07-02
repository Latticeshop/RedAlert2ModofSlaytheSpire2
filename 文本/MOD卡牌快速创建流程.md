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

### 5. 科技解锁（如需要）

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

### 6. 动态数值配置

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

### 7. 本地化

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

### 7. 播放建筑音效

#### 在 OnPlay() 中调用
```csharp
protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
{
    BuildingSoundHelper.PlayBuildingPlaceSound();
    // ... 建筑逻辑
}
```

> **例外情况**：资源类卡牌（金矿、宝石矿）和围墙卡牌不需要播放建筑音效

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

### 1. 创建公共基类

#### 在 Common/Cards/ 目录下创建

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

### 2. 创建盟军子类

#### 在 Allies/Cards/ 目录下创建

```csharp
using RedAlert2ModCode.Common.Cards;

namespace RedAlert2ModCode.Allies.Cards;

public sealed class AlliesGoldMineCard : GoldMineCard
{
}
```

> **注意**：盟军子类带 `Allies` 前缀，使用 `sealed`（不允许进一步继承）。

---

### 3. 创建苏军子类

#### 在 Soviet/Cards/ 目录下创建

```csharp
using RedAlert2ModCode.Common.Cards;

namespace RedAlert2ModCode.Soviet.Cards;

public sealed class SovietGoldMineCard : GoldMineCard
{
}
```

> **注意**：苏军子类带 `Soviet` 前缀，使用 `sealed`（不允许进一步继承）。

---

### 4. 数值存储

#### 在 CommonCardValues.cs 中定义

```csharp
public static CardValueStore.CardValues GoldMine => new()
{
    Cost = 1,
    DollarValue = 1000,
    DollarValueUpgraded = 500
};
```

---

### 5. 注册到阵营卡池

#### 在 AlliedCardRegistry.cs 中注册

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

#### 在 SovietCardRegistry.cs 中注册

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

### 6. 创建对应能力（如需要）

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

### 7. 本地化（两份）

由于游戏使用类名自动生成本地化key，公共卡牌需要创建两份本地化条目：

#### cards.json（中文）

```json
{
    "ALLIES_GOLD_MINE_CARD.title": "黄金矿",
    "ALLIES_GOLD_MINE_CARD.description": "获得 {Reserve} [gold]黄金矿[/gold]储备。",
    "SOVIET_GOLD_MINE_CARD.title": "黄金矿",
    "SOVIET_GOLD_MINE_CARD.description": "获得 {Reserve} [gold]黄金矿[/gold]储备。"
}
```

#### cards.json（英文）

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

### 8. 能力本地化（共用）

能力本地化只需一份，存放在 `powers.json` 中：

```json
{
    "GOLD_MINE_POWER.title": "黄金矿",
    "GOLD_MINE_POWER.smartDescription": "黄金矿储备：{CurrentReserve}"
}
```

---

### 公共卡牌创建流程总结

| 步骤 | 操作 | 文件路径 |
|------|------|---------|
| 1 | 创建公共基类（完整逻辑） | `Common/Cards/GoldMineCard.cs` |
| 2 | 创建盟军子类（仅继承） | `Allies/Cards/AlliesGoldMineCard.cs` |
| 3 | 创建苏军子类（仅继承） | `Soviet/Cards/SovietGoldMineCard.cs` |
| 4 | 定义数值 | `Common/Cards/CommonCardValues.cs` |
| 5 | 注册到盟军卡池 | `Allies/AlliedCardRegistry.cs` |
| 6 | 注册到苏军卡池 | `Soviet/SovietCardRegistry.cs` |
| 7 | 创建共用能力（如需要） | `Common/Powers/GoldMinePower.cs` |
| 8 | 注册能力图标 | `Allies/Powers/PowerIconPatch.cs` |
| 9 | 添加盟军本地化 | `localization/zhs/cards.json` |
| 10 | 添加苏军本地化 | `localization/zhs/cards.json` |
| 11 | 添加能力本地化（共用） | `localization/zhs/powers.json` |

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
| 音效播放 | ✅ | ✅ | ❌ | ❌（资源卡） |

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

*本文档基于实际开发经验整理，与API快速参考手册配合使用效果更佳。*