# 杀戮尖塔2 Mod开发 - AI快速参考手册（Beta版）

> 精简版，便于AI快速检索关键API和代码模式
> **适用版本：Beta版** | 正式版文档请参考：`杀戮尖塔2_Mod开发教程API快速参考.md`

---

## ⚠️ Beta版 vs 正式版 核心差异速览

| 模块 | 正式版 | Beta版 | 注意事项 |
|------|--------|--------|---------|
| 卡牌去向方法 | `GetResultPileTypeForCardPlay()` | `GetResultLocationForCardPlay()` | 返回值从 `PileType` 改为 `CardLocation` |
| 攻击卡FromCard | `FromCard(CardModel)` | `FromCard(CardModel, CardPlay?)` | 新增 `cardPlay` 参数 |
| 跨玩家传牌 | 需反射手写 | `CardPileCmd.GiveToAnotherPlayer()` | Beta版有原生API |
| CardPlay构造 | 可选Player | **必填** `Player` | `CardPlay.Player` 为必填成员 |
| Targeting参数 | 接受 `List<Creature>` | 单个 `Creature` 或 `TargetingAllOpponents()` | 群体攻击需改用新API |

---

## 📂 项目路径配置

### 当前项目路径
```
项目根目录: D:\RedAlert2Project\red-alert-2-mod
游戏解包目录: D:\RedAlert2Project\SlayTheSpire2Export_beta
Godot引擎: Godot_v4.5.1-stable_mono_win64
红警2图标: D:\RedAlert2Project\icons\红警2图标PNG\
```

### 项目目录结构模板
```
red-alert-2-mod/
├── .idea/                      # IDE配置
├── BaseLib/                    # 基础库（可选）
│   ├── BaseLib.dll
│   ├── BaseLib.json
│   └── BaseLib.pck
├── RedAlert2Mod/               # Mod输出目录
│   ├── RedAlert2Mod.dll
│   ├── RedAlert2Mod.json
│   └── RedAlert2Mod.pdb
├── RedAlert2ModCode/           # C#源代码
│   ├── Allies/                 # 盟军阵营
│   │   ├── Cards/              # 卡牌定义（士兵、装甲、建筑等）
│   │   │   └── AlliesCardValues.cs     # 卡牌数值存储
│   │   ├── Powers/             # 能力定义
│   │   │   └── AlliesPowerValues.cs    # 能力数值存储
│   │   ├── Relics/             # 遗物定义
│   │   │   └── AlliesRelicValues.cs    # 遗物数值存储
│   │   ├── AlliedCardRegistry.cs   # 卡牌注册管理器
│   │   ├── AlliesCardPool.cs       # 卡池
│   │   ├── AlliesCharacter.cs      # 角色定义
│   │   ├── AlliesRelicPool.cs      # 遗物池
│   │   ├── AlliesPotionPool.cs     # 药水池
│   │   ├── AlliesRegistration.cs   # 注册入口
│   │   └── InitialDeckExhaustPatch.cs  # 初始卡组补丁
│   ├── Soviet/                 # 苏军阵营
│   │   ├── SovietCardRegistry.cs
│   │   └── SovietCardValues.cs     # 卡牌数值存储
│   ├── Yuri/                   # 尤里阵营
│   │   ├── YuriCardRegistry.cs
│   │   └── YuriCardValues.cs       # 卡牌数值存储
│   ├── Other/                  # 其他阵营（利赛特、古巴等）
│   │   ├── OtherCardRegistry.cs
│   │   └── OtherCardValues.cs      # 卡牌数值存储
│   ├── Extensions/             # 扩展方法
│   │   └── PathExtensions.cs
│   ├── UI/                     # UI组件
│   │   └── CardSelectionScreen.cs
│   ├── Utils/                  # 工具类
│   │   ├── CardUtils.cs
│   │   └── CardValueStore.cs       # 卡牌数值存储基类
│   └── ModInitializer.cs       # Mod入口
├── RedAlert2ModResources/      # Godot资源
│   ├── images/                 # 图片资源
│   │   ├── character/          # 角色立绘
│   │   ├── charui/             # UI角色图
│   │   ├── packed/             # 打包图片
│   │   │   ├── card_portraits/ # 卡牌肖像
│   │   │   │   └── allies/     # 盟军卡牌
│   │   │   └── character_select/  # 角色选择立绘
│   │   ├── powers/             # 能力图标
│   │   │   └── big/            # 大尺寸能力图标
│   │   └── relics/             # 遗物图标
│   └── scenes/                 # Godot场景
│       ├── creature_visuals/   # 角色待机动画
│       └── ui/                 # UI场景
│           └── character_icons/ # 角色头像图标
├── build/                      # 构建输出
│   ├── RedAlert2Mod.json
│   ├── RedAlert2Mod.pck
│   └── RedAlert2Mod.dll
├── localization/zhs/           # 本地化文件（游戏读取路径）
│   ├── cards.json
│   ├── characters.json
│   ├── powers.json
│   ├── relics.json
│   └── ...
├── 0Harmony.dll                # Harmony库
├── sts2.dll                    # 游戏核心DLL
├── RedAlert2Mod.csproj         # C#项目文件
├── RedAlert2Mod.sln            # 解决方案文件
└── project.godot               # Godot项目配置
```

### 游戏解包资源结构（参考）
```
D:\RedAlert2Project\SlayTheSpire2Export\
├── resources/
│   ├── scenes/
│   │   ├── creature_visuals/      # 角色/怪物待机动画
│   │   │   ├── ironclad.tscn
│   │   │   ├── silent.tscn
│   │   │   └── ...
│   │   ├── ui/
│   │   │   └── character_icons/   # 角色头像图标
│   │   │       ├── ironclad_icon.tscn
│   │   │       └── ...
│   │   └── combat/
│   │       └── energy_counters/   # 能量计数器
│   │           ├── ironclad_energy_counter.tscn
│   │           └── ...
│   ├── images/
│   │   ├── packed/
│   │   │   └── character_select/  # 角色选择立绘
│   │   │       ├── char_select_ironclad.png
│   │   │       └── ...
│   │   ├── atlases/               # 裁切纹理
│   │   ├── relics/                # 遗物图标
│   │   ├── potions/               # 药水图标
│   │   └── powers/                # 能力图标
│   └── localization/              # 本地化文件
└── data_sts2_<platform>/
    ├── sts2.dll                   # 游戏核心DLL
    └── 0Harmony.dll               # Harmony库
```

---

## 📦 Mod结构

```
mods/MyMod/
├── MyMod.json      # 必需
├── MyMod.pck       # 可选（资源）
└── MyMod.dll       # 可选（代码）
```

### JSON配置
```json
{
  "id": "MyMod",
  "name": "我的Mod",
  "author": "作者",
  "version": "v1.0.0",
  "has_pck": true,
  "has_dll": true,
  "affects_gameplay": true
}
```

---

## 🔧 Mod入口

```csharp
using MegaCrit.Sts2.Core.Modding;

[ModInitializer(nameof(Initialize))]
public static class MyModInitializer
{
    public static void Initialize()
    {
        // 注册逻辑
        ModHelper.AddModelToPool(typeof(PoolType), typeof(MyClass));
        
        var harmony = new Harmony("Author.ModID");
        harmony.PatchAll();
    }
}
```

---

## 🎴 卡牌（CardModel）

### 卡牌稀有度（CardRarity）

卡牌的第三个参数 `Rarity` 属性决定卡牌的稀有度，影响卡牌的边框样式、出现逻辑和商店售价：

```csharp
public enum CardRarity
{
    None,     // 无
    Basic,    // 基础
    Common,   // 普通
    Uncommon, // 罕见
    Rare,     // 稀有
    Ancient,  // 先古之民
    Event,    // 事件
    Token,    // 代币
    Status,   // 状态
    Curse,    // 诅咒
    Quest     // 任务
}
```

**稀有度分类说明**：

| 类型 | 是否出现在随机卡池 | 说明 | 示例 |
|------|------------------|------|------|
| Basic/Common/Uncommon/Rare | **是** | 可在战斗奖励、商店、事件中随机获取 | 打击、防御、各类攻击牌 |
| Ancient | 否 | 先古之民专属卡牌 | 先古遗物相关卡牌 |
| Event | 否 | 事件专属卡牌 | 特定事件奖励 |
| **Token** | **否** | 衍生卡牌，需通过特定条件获取 | 小刀、巨石、灵魂 |
| Status | 否 | 状态卡牌 | 虚弱、易伤 |
| Curse | 否 | 诅咒卡牌 | 痛苦、悔恨 |
| Quest | 否 | 任务卡牌 | 藏宝图、多尼斯异鸟蛋 |

**Token 类型卡牌的使用场景**：

Token 卡牌类似于游戏内置的"衍生卡"机制，不会出现在奖励卡池中，只能通过特定条件（如建筑生产、卡牌效果）获取。这对于实现"单位卡只能通过兵营/重工生产获得"的设计非常有用。

**示例：将单位卡设置为 Token 类型**：
```csharp
public sealed class AmericanSoldier : CardModel
{
    // 使用 Token 类型，该卡不会出现在随机奖励池中
    public AmericanSoldier() : base(1, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) { }
}
```

### 基本结构
```csharp
public class MyCard : CardModel
{
    public MyCard() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) { }
    
    protected override List<DynamicVar> CanonicalVars => new()
    {
        new DamageVar(6m)
    };
    
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(play.Target)
            .Execute(ctx);
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
```

### 注册

**方法一：使用 ModHelper（推荐）**
```csharp
ModHelper.AddModelToPool(typeof(IroncladCardPool), typeof(MyCard));
```

**方法二：在卡池类中注册（适用于自定义阵营）**

创建自定义卡池类并在 `GenerateAllCards()` 方法中注册所有卡牌：
```csharp
public sealed class AlliesCardPool : CardPoolModel
{
    public override string Title => "allies";
    
    protected override CardModel[] GenerateAllCards()
    {
        return new CardModel[]
        {
            ModelDb.Card<AmericanSoldier>(),
            ModelDb.Card<DogSoldier>(),
            ModelDb.Card<RocketSoldier>()
            // ... 其他卡牌
        };
    }
}
```

> **重要**：新卡牌必须注册到对应阵营的卡池才能被游戏识别和使用。

### 卡牌数值存储规范

**规则1：数值集中存储**
- 任何卡牌的数值信息（费用、伤害、护盾、重复次数等）都必须在数值文件中统一存储
- 推荐使用 `AlliesCardValues.cs` 这样的静态类来管理所有卡牌数值
- 卡牌类中通过引用数值存储类来获取数值，避免硬编码
- **飞鹰/轨道系列**的卡牌数值和能力数值**统一存储在** `CommonCardValues.cs` 和 `CommonPowerValues.cs`，不使用阵营专属文件

**规则2：资金消耗本地化格式**
- 任何需要消耗"资金"的**非单位**卡牌（如建筑卡、技能卡），必须在本地化描述的开头加上"价格：xxx。"
- **单位卡**的价格由生产序列能力在选择时消耗，一般不在本地化描述中展示价格
- 示例：`"ALLIED_WALL_CARD.description": "价格：${DollarNumber}。获得 {Block} 点护盾。将此牌返回你的手牌。"`

**数值存储示例**：
```csharp
// AlliesCardValues.cs - 统一数值存储
public static class AlliesCardValues
{
    public static CardValueStore.CardValues AlliedWall => new()
    {
        Cost = 0,
        Block = 1,
        BlockUpgraded = 2,
        DollarCost = 100  // 资金消耗
    };
}
```

**价格映射注册（训练UI显示价格）**：
```csharp
// 士兵单位 → CreateSoldierValuesMap()
// 地面单位 → CreateVehicleValuesMap()
// 空军单位 → CreateAircraftValuesMap()
// 海军单位 → CreateShipValuesMap()

public static Dictionary<string, CardValueStore.CardValues> CreateSoldierValuesMap()
{
    return new()
    {
        { "CONSCRIPT", Conscript },
        { "CRAZY_IVAN_CARD", CrazyIvan },  // 类名带Card后缀，映射键必须加_CARD
        { "CHRONO_IVAN_CARD", ChronoIvan },
    };
}
```

> **⚠️ 关键注意事项**：映射键必须与卡牌ID完全匹配。卡牌ID由类名自动生成：`ClassName` → `CLASS_NAME`（大写 + 驼峰处加下划线）。类名带 `Card` 后缀时，映射键必须包含 `_CARD`，否则训练UI价格显示为 $0。

### 本地化 ID 命名规则（`_CARD` 后缀）

游戏的本地化 key 由**卡牌类名**自动生成，规则如下：

- 类名 → 全大写 + 驼峰处加下划线
- **类名以 `Card` 结尾** → 本地化 key **保留** `_CARD` 后缀
- **类名不以 `Card` 结尾** → 本地化 key **没有** `_CARD` 后缀

| 类名 | 本地化 key（title/description） | 是否以 Card 结尾 |
|------|--------------------------------|-------------------|
| `SealCommandos` | `SEAL_COMMANDOS.title` | ❌ 否 |
| `ChronoCommandos` | `CHRONO_COMMANDOS.title` | ❌ 否 |
| `YuriCard` | `YURI_CARD.title` | ✅ 是 |
| `ChronoIvanCard` | `CHRONO_IVAN_CARD.title` | ✅ 是 |
| `PsiCommandoCard` | `PSI_COMMANDO_CARD.title` | ✅ 是 |

> **提示**：如果卡牌标题/描述在游戏中显示为原始 key（如 `cards.PSI_COMMANDO.title`），请检查类名是否以 `Card` 结尾，并同步修改所有 4 个语言的 `cards.json`。

### 百科卡框颜色（`Pool` / `VisualCardPool`）

卡牌在百科（图鉴）中的边框颜色由 `VisualCardPool` 属性决定：

| 效果 | 实现方式 |
|------|---------|
| 🟦 盟军蓝色边框 / 🟥 苏军红色边框 | **不 override** `Pool` 和 `VisualCardPool`，使用基类默认值（继承 `Owner.Character.CardPool`） |
| ⬜ 白色无色边框（公共/中立卡） | override 两者，将 `VisualCardPool` 设为 `TokenCardPool` |

**无色公共卡（渗透单位等）标准写法**：
```csharp
public override CardPoolModel Pool => IsMutable && Owner != null
    ? Owner.Character.CardPool      // 战斗中：角色实际卡池（正常打牌）
    : ModelDb.CardPool<TokenCardPool>();

public override CardPoolModel VisualCardPool => ModelDb.CardPool<TokenCardPool>();  // 百科显示：白色无阵营边框
```

> **常见坑**：如果 override `VisualCardPool` 为 `TokenCardPool`，百科中该卡就是白色边框；如果不 override，会继承注册阵营的颜色。根据卡牌定位选择即可。

### 资源路径
```
res://images/atlases/card_atlas.sprites/<pool>/<card_id>.tres
res://images/packed/card_portraits/<pool>/<card_id>.png
```

### 编译与部署

每次代码更新后，需要重新编译生成新的 `RedAlert2Mod.dll` 文件：

```bash
dotnet build RedAlert2Mod.csproj -c Release -o build
```

编译成功后，游戏需要的是 `build/RedAlert2Mod.dll` 文件。确保将以下文件复制到游戏的 `mods/RedAlert2Mod/` 目录：
- `RedAlert2Mod.dll` - 主程序集（必须）
- `RedAlert2Mod.json` - Mod配置文件（必须）
- `RedAlert2Mod.pck` - 资源包（如果有资源）

---

## 👥 多人联机卡牌

### 多人模式限制（CardMultiplayerConstraint）

通过重写 `MultiplayerConstraint` 属性控制卡牌在单人/多人模式下的可见性：

```csharp
public enum CardMultiplayerConstraint
{
    None,              // 无限制（默认）
    MultiplayerOnly,   // 仅多人模式可用
    SingleplayerOnly   // 仅单人模式可用
}
```

**使用方式**：
```csharp
public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
```

### 多人目标类型（TargetType）

| 目标类型 | 说明 |
|---------|------|
| `TargetType.AnyAlly` | 选择任意单个队友 |
| `TargetType.AllAllies` | 选择所有队友 |
| `TargetType.AnyPlayer` | 选择任意玩家 |

### 获取队友列表

```csharp
// 获取所有队友生物（包含自己）
IEnumerable<Creature> allTeammates = CombatState.GetTeammatesOf(Owner.Creature);

// 过滤：只获取存活的、非自己的队友玩家
var teammates = from c in CombatState.GetTeammatesOf(Owner.Creature)
    where c != null && c.IsAlive && c.IsPlayer && c.Player != Owner
    select c;
```

### 将卡牌转移给队友（核心API）

```csharp
// 签名：CardPileCmd.GiveToAnotherPlayer
await CardPileCmd.GiveToAnotherPlayer(
    cardModel,                    // 要转移的卡牌
    targetPlayer,                 // 目标队友（接收方）
    PileType.Hand,                // 放入目标的哪个牌堆
    CardPilePosition.Random       // 牌堆中的位置
);
```

**PileType 可选值**：`Hand`（手牌）、`Draw`（抽牌堆）、`Discard`（弃牌堆）、`Exhaust`（消耗堆）

**CardPilePosition 可选值**：`Top`（顶部）、`Bottom`（底部）、`Random`（随机）

### 给队友添加生成的卡牌

```csharp
// 给队友生成一张随机牌（参考原版"慷慨捐助"Largesse）
CardModel cardModel = CardFactory.GetDistinctForCombat(
    targetPlayer,
    ModelDb.CardPool<ColorlessCardPool>().GetUnlockedCards(...),
    1,
    Owner.RunState.Rng.CombatCardGeneration
).FirstOrDefault();

await CardPileCmd.AddGeneratedCardToCombat(cardModel, PileType.Hand, Owner);
```

### 参考原版实现

| 卡牌 | 功能 | 实现要点 |
|-----|------|---------|
| **Largesse（慷慨捐助）** | 给队友添加一张随机无色牌 | `CardFactory.GetDistinctForCombat()` + `CardPileCmd.AddGeneratedCardToCombat()` |
| **TheBall（魔球，beta版）** | 将本卡交给随机队友 | `CombatState.GetTeammatesOf()` + `CardPileCmd.GiveToAnotherPlayer()` |
| **EnergySurge（能量涌动）** | 给所有队友加能量 | 遍历 `GetTeammatesOf()` + `PlayerCmd.GainEnergy()` |

---

## 🔄 Beta版特有 API 变化详解

> 以下API仅适用于Beta版，正式版使用不同签名。移植代码时需重点关注。

### 1. 卡牌去向：GetResultLocationForCardPlay

**Beta版变更**：方法名和返回值都变了。

```csharp
// 正式版（已废弃）
protected virtual PileType GetResultPileTypeForCardPlay()

// Beta版（新API）
protected virtual CardLocation GetResultLocationForCardPlay()
```

**CardLocation 结构**（Beta版新增）：
```csharp
public record struct CardLocation
{
    public Player player;          // 目标玩家（支持跨玩家传递）
    public PileType pileType;      // 牌堆类型
    public CardPilePosition position; // 牌堆位置
}
```

**迁移示例（围墙卡）**：
```csharp
// 正式版
protected override PileType GetResultPileTypeForCardPlay()
{
    PileType result = base.GetResultPileTypeForCardPlay();
    if (result != PileType.Discard) return result;
    return PileType.Hand;
}

// Beta版
protected override CardLocation GetResultLocationForCardPlay()
{
    CardLocation result = base.GetResultLocationForCardPlay();
    if (result.pileType != PileType.Discard) return result;
    result.pileType = PileType.Hand;
    return result;
}
```

**跨玩家传递示例（魔球TheBall）**：
```csharp
protected override CardLocation GetResultLocationForCardPlay()
{
    CardLocation result = base.GetResultLocationForCardPlay();
    if (CombatState == null) return result;
    var teammates = (from c in CombatState.GetTeammatesOf(Owner.Creature)
        where c != null && c.IsAlive && c.IsPlayer && c.Player != Owner
        select c).ToList();
    if (teammates.Count == 0) return result;
    // 传递给随机队友的抽牌堆
    result.player = Owner.RunState.Rng.CombatTargets.NextItem(teammates).Player;
    if (result.pileType == PileType.Discard)
    {
        result.pileType = PileType.Draw;
        result.position = CardPilePosition.Random;
    }
    return result;
}
```

---

### 2. 攻击卡：FromCard 新增 cardPlay 参数

**影响范围：所有攻击卡**

```csharp
// 正式版
public AttackCommand FromCard(CardModel card)

// Beta版（新增 cardPlay 参数）
public AttackCommand FromCard(CardModel card, CardPlay? cardPlay)
```

**迁移示例**：
```csharp
// 正式版
await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
    .FromCard(this)
    .Targeting(cardPlay.Target)
    .Execute(choiceContext);

// Beta版
await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
    .FromCard(this, cardPlay)  // 新增第二个参数
    .Targeting(cardPlay.Target)
    .Execute(choiceContext);
```

---

### 3. 群体攻击：Targeting 参数变化

**Beta版变更**：`Targeting` 不再接受 `List<Creature>`，需改用新API。

```csharp
// 正式版
DamageCmd.Attack(amount).FromCard(this).Targeting(List<Creature>)

// Beta版（二选一）
DamageCmd.Attack(amount).FromCard(this, cardPlay).Targeting(Creature)           // 单个目标
DamageCmd.Attack(amount).FromCard(this, cardPlay).TargetingAllOpponents(CombatState) // 所有敌人
```

---

### 4. CardPlay 构造：Player 为必填成员

```csharp
// 正式版
new CardPlay
{
    Card = this,
    Target = target,
    // ...
}

// Beta版（CardPlay.Player 为必填）
new CardPlay
{
    Player = Owner,  // 新增必填项
    Card = this,
    Target = target,
    // ...
}
```

---

### 5. CardModel 新增方法

| 方法 | 说明 |
|-----|------|
| `GiveToAnotherPlayer(Player)` | 将卡牌所有权移交给另一个玩家（直接设置 `_owner`） |
| `CreateCloneForPlayer(Player)` | 为指定玩家创建卡牌克隆 |

---

### 6. CardPileCmd 新增参数

`Add` 方法新增 `isChangingOwners` 参数：
```csharp
// Beta版
public static async Task<IReadOnlyList<CardPileAddResult>> Add(
    IEnumerable<CardModel> cards, 
    CardPile newPile, 
    CardPilePosition position = CardPilePosition.Bottom, 
    AbstractModel? clonedBy = null, 
    bool skipVisuals = false, 
    bool isChangingOwners = false)  // 新增：防止重复触发 AfterCardEnteredCombat
```

---

## 🏷️ 自定义词条（Custom Keywords）

### 设计理念

Mod可以添加自定义词条来增强卡牌的视觉效果和交互体验。词条会在卡牌描述下方显示金色文本，鼠标悬停时显示详细描述。

### 实现方式

#### 1. 创建词条定义类

```csharp
// CustomKeyword.cs - 自定义词条框架
public class CustomKeyword
{
    public string Id { get; }
    public LocString Title { get; }
    public LocString Description { get; }

    public CustomKeyword(string id, LocString title, LocString description)
    {
        Id = id;
        Title = title;
        Description = description;
    }

    /// <summary>
    /// 创建悬停提示
    /// </summary>
    public IHoverTip CreateHoverTip()
    {
        return new HoverTip(Title, Description);
    }
}

/// <summary>
/// 预定义的自定义词条
/// </summary>
public static class ModCardKeywords
{
    /// <summary>
    /// MCV词条 - 拥有建造厂才能打出建筑卡牌
    /// </summary>
    public static readonly CustomKeyword Mcv = new(
        "MCV",
        new LocString("card_keywords", "mcv.title"),
        new LocString("card_keywords", "mcv.description")
    );
}
```

#### 2. 在卡牌中使用 ExtraHoverTips

```csharp
// AlliedMCV.cs - 基地车卡牌
public sealed class AlliedMCV : CardModel
{
    public AlliedMCV() : base(0, CardType.Power, CardRarity.Rare, TargetType.Self) { }

    /// <summary>
    /// 额外的悬停提示（包含自定义MCV词条）
    /// </summary>
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ModCardKeywords.Mcv.CreateHoverTip()
    ];
}
```

#### 3. 本地化文件

**card_keywords.json** - 词条本地化：
```json
{
    "mcv.title": "MCV",
    "mcv.description": "拥有建造厂才能打出建筑卡牌。"
}
```

**cards.json** - 卡牌描述中添加词条文本：
```json
{
    "ALLIED_MC_V.description": "[gold]MCV. [/gold]\n展开：从当前建筑中选择一张加入手牌。"
}
```

### 效果说明

- **卡牌显示**：在描述下方显示金色的"建造厂."文本
- **悬停提示**：鼠标悬停在词条上时显示详细描述

### 进阶：带行为逻辑的自定义词条（超时空词条案例）

当自定义词条不仅需要视觉效果，还需要绑定游戏行为时，需要创建基类来封装词条逻辑。

#### 1. 创建词条定义

```csharp
// CustomKeyword.cs - 添加超时空词条
public static class ModCardKeywords
{
    public static readonly CustomKeyword Chrono = new(
        "CHRONO",
        new LocString("card_keywords", "chrono.title"),
        new LocString("card_keywords", "chrono.description")
    );
}
```

#### 2. 创建词条行为基类

```csharp
// ChronoCardModel.cs - 超时空卡牌基类
public abstract class ChronoCardModel : CardModel
{
    private bool _chronoConsumed;

    protected ChronoCardModel(int cost, CardType cardType, CardRarity cardRarity, TargetType targetType)
        : base(cost, cardType, cardRarity, targetType) { }

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new StringVar("ChronoTitle", "[gold]超时空.[/gold]\n")
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            var tips = GetExtraHoverTips();
            if (!_chronoConsumed)
            {
                tips.Add(ModCardKeywords.Chrono.CreateHoverTip());
            }
            return tips;
        }
    }

    protected abstract List<IHoverTip> GetExtraHoverTips();

    protected override CardLocation GetResultLocationForCardPlay()
    {
        if (_chronoConsumed)
            return base.GetResultLocationForCardPlay();

        bool hasExhaust = Keywords.Contains(CardKeyword.Exhaust);
        if (hasExhaust)
        {
            _chronoConsumed = true;
            DynamicVars["ChronoTitle"].StringValue = string.Empty;
            return new CardLocation(Owner, PileType.Draw, CardPilePosition.Bottom);
        }
        return new CardLocation(Owner, PileType.Draw, CardPilePosition.Bottom);
    }
}
```

#### 3. 卡牌继承基类

```csharp
public sealed class ChronoMiner : ChronoCardModel
{
    public ChronoMiner() : base(0, CardType.Skill, CardRarity.Token, TargetType.Self) { }

    protected override List<IHoverTip> GetExtraHoverTips()
    {
        return new List<IHoverTip>
        {
            ModCardKeywords.TechLevelT1.CreateHoverTip(),
            ModCardKeywords.Vehicle.CreateHoverTip()
        };
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        // 卡牌特有逻辑...
    }
}
```

#### 4. 本地化配置

**card_keywords.json**：
```json
{
    "chrono.title": "超时空",
    "chrono.description": "打出时进入摸牌堆。与消耗词条共存时，首次打出进入摸牌堆并移除超时空，下次打出正常消耗。"
}
```

**cards.json**（在描述开头添加动态变量）：
```json
{
    "CHRONO_MINER.description": "{ChronoTitle}获得 {DollarValue} 资金。"
}
```

#### 核心机制说明

| 机制 | 说明 |
|------|------|
| `GetResultLocationForCardPlay()` | 控制卡牌打出后的去向，返回 `CardLocation(Draw, Bottom)` 使卡牌进入摸牌堆底部 |
| `_chronoConsumed` | 状态标记，控制超时空效果是否已消耗 |
| `StringVar("ChronoTitle")` | 动态变量，控制描述开头的"超时空."文本显示/隐藏 |
| `GetExtraHoverTips()` | 抽象方法，子类返回额外的悬浮提示 |

---

## 🏗️ 科技树系统（Tech Tree）

### 科技线架构

本Mod实现了类似红警2的科技树系统，分为**核心建筑解锁**与**科技等级升级**两套独立机制：

```
核心建筑解锁（MCV选项）：
  T1: 基地车能力 → 发电厂、兵营、矿场（始终可见）
  T2: 矿场能力   → 重工、空指部/雷达、船厂（仅MCV选项可见，不升级科技等级）
  T3: 作战实验室 → 作战实验室（通过空指部/雷达能力升级科技等级后解锁）

科技等级升级（用于过滤牌组建筑）：
  T1: 默认等级 → T1牌组建筑可见
  T2: 空指部/雷达能力触发 → T2牌组建筑可见
  T3: 作战实验室能力触发 → T3牌组建筑可见
```

### 核心机制：BuildingTechTree

`BuildingTechTree` 分离了**核心建筑解锁**与**科技等级升级**两个概念：

| 机制 | 触发条件 | 效果 | 标记方式 |
|------|----------|------|----------|
| **核心生产解锁** | 获得矿场能力 | T2核心建筑出现在MCV选项（重工/空指部/船厂） | `WithProductionUnlock()` |
| **科技等级升级** | 获得空指部/雷达能力 | `CurrentTechLevel` 从T1升至T2，T2牌组建筑可见 | `unlocksNextTech: true` |

#### TechBuildingInfo 配置

```csharp
// 盟军 TechTreeConfig 示例
var refinery = new TechBuildingInfo(typeof(AlliedRefinery), TechLevel.T1, 
    powerType: typeof(AlliedRefineryPower));
refinery.WithProductionUnlock();  // 标记为生产解锁（解锁T2核心建筑，不升级科技等级）

var buildings = new List<TechBuildingInfo>
{
    new(typeof(PowerPlantCard), TechLevel.T1),           // T1：始终可见
    new(typeof(AlliesBarracksCard), TechLevel.T1),       // T1：始终可见
    refinery,                                             // T1：生产解锁标记
    
    new(typeof(AlliedWarFactory), TechLevel.T2, powerType: typeof(AlliedWarFactoryPower)),
    new(typeof(AlliesShipyardCard), TechLevel.T2),
    new(typeof(AirForceCommand), TechLevel.T2, unlocksNextTech: true, powerType: typeof(AlliedAirForceCommandPower)),  // 科技等级升级
    
    new(typeof(AlliedBattleLab), TechLevel.T3, powerType: typeof(BattleLabPower), 
        requiredPowers: new[] { typeof(AlliedAirForceCommandPower) }),
};
```

#### MCV 选项面板流程

```
MCV选项 = 核心建筑（GetUnlockedCoreBuildingTypes） + 牌组建筑（AddDeckBuildings）

核心建筑判定（GetUnlockedCoreBuildingTypes）：
  T1 → 始终解锁
  T2 → 矿场能力触发后解锁（_productionUnlockedBuildingTypes 包含该类型）
  T3 → CurrentTechLevel >= T3（需空指部/雷达能力升级后才能获得）

牌组建筑判定（AddDeckBuildings + BuildingCardUtils._deckBuildingTechLevelMap）：
  需同时满足：在牌组中 + CurrentTechLevel >= 所需等级
```

#### 牌组建筑科技等级映射（BuildingCardUtils）

非核心建筑（防御塔、超武、围墙、维修厂等）通过 `_deckBuildingTechLevelMap` 定义科技等级需求：

```csharp
private static readonly Dictionary<Type, TechLevel> _deckBuildingTechLevelMap = new()
{
    // 盟军
    { typeof(AlliedWallCard), TechLevel.T1 },
    { typeof(AlliesPillboxCard), TechLevel.T1 },
    { typeof(AlliesRepairDepot), TechLevel.T2 },     // 维修厂：T2
    { typeof(PrismTowerCard), TechLevel.T2 },
    { typeof(PatriotMissile), TechLevel.T2 },
    { typeof(GrandCannon), TechLevel.T2 },
    { typeof(OreRefineryCard), TechLevel.T3 },       // 矿石精炼器：T3
    { typeof(WeatherController), TechLevel.T3 },
    { typeof(ChronoSphere), TechLevel.T3 },

    // 苏军
    { typeof(SovietWallCard), TechLevel.T1 },
    { typeof(SovietPillboxCard), TechLevel.T1 },
    { typeof(BattleBunkerCard), TechLevel.T1 },
    { typeof(SovietRepairDepot), TechLevel.T2 },    // 维修厂：T2
    { typeof(SovietTeslaCoilCard), TechLevel.T2 },
    { typeof(SovietFlakCannon), TechLevel.T2 },
    { typeof(NuclearPlantCard), TechLevel.T3 },
    { typeof(IndustrialPlantCard), TechLevel.T3 },
    { typeof(IronCurtainCard), TechLevel.T3 },
    { typeof(NuclearMissileSiloCard), TechLevel.T3 },
};
```

### T1/T2/T3 科技等级规则

| 等级 | 解锁条件 | 解锁内容 | 示例 |
|------|----------|----------|------|
| **T1** | 基地车默认解锁 | 发电厂、兵营、矿场、围墙、机枪碉堡/哨戒炮 | 基础防御塔 |
| **T2** | 矿场解锁核心生产 + 空指部/雷达升级科技等级 | 重工、船厂、空指部/雷达 + 爱国者/磁暴线圈/防空炮/巨炮 + 维修厂 | 进阶防御塔 |
| **T3** | 空指部/雷达升级科技等级 + 作战实验室 | 作战实验室 + 超武（天气控制器/超时空/铁幕/核弹井）+ 矿石精炼器/核电站/工业工厂 | 超级武器 |

### 科技等级关键字

在 `CustomKeyword.cs` 中定义了三个科技等级关键字：

```csharp
public static class ModCardKeywords
{
    public static readonly CustomKeyword TechLevelT1 = new(
        "TECH_LEVEL_T1",
        new LocString("card_keywords", "tech_level_t1.title"),
        new LocString("card_keywords", "tech_level_t1.description")
    );

    public static readonly CustomKeyword TechLevelT2 = new(
        "TECH_LEVEL_T2",
        new LocString("card_keywords", "tech_level_t2.title"),
        new LocString("card_keywords", "tech_level_t2.description")
    );

    public static readonly CustomKeyword TechLevelT3 = new(
        "TECH_LEVEL_T3",
        new LocString("card_keywords", "tech_level_t3.title"),
        new LocString("card_keywords", "tech_level_t3.description")
    );
}
```

### 单位卡牌添加科技等级Tip

所有 **Token类型** 的单位卡牌（除围墙外）必须在 `ExtraHoverTips` 的**第一位**添加对应的科技等级关键字：

```csharp
protected override IEnumerable<IHoverTip> ExtraHoverTips =>
[
    ModCardKeywords.TechLevelT2.CreateHoverTip(),  // 科技等级Tip放在第一位
    ModCardKeywords.Vehicle.CreateHoverTip()       // 其他词条放在后面
];
```

### 本地化配置

在 `card_keywords.json` 中添加科技等级词条的本地化：

```json
{
    "tech_level_t1.title": "T1",
    "tech_level_t1.description": "初始科技。",
    "tech_level_t2.title": "T2",
    "tech_level_t2.description": "建造[gold]空指部/雷达[/gold]解锁。",
    "tech_level_t3.title": "T3",
    "tech_level_t3.description": "建造[gold]作战实验室[/gold]解锁。",
    "building_tech_tree.title": "建筑科技线",
    "building_tech_tree.description": "科技线：T1:基地车能力→矿场(解锁重工/空指部/船厂)→T2:空指部/雷达(解锁T2单位)→T3:作战实验室。其他建筑：在自己卡组里，且解锁对应科技等级时，添加到MCV选项。"
}
```

---

## 🖱️ 卡牌悬浮提示（HoverTip）

### 核心原理

卡牌上展示悬浮的其他卡牌和能力，是通过重写 `CardModel` 类的 **`ExtraHoverTips`** 属性实现的。游戏引擎会自动将这些提示显示在卡牌描述下方。

### HoverTipFactory 工具类

```csharp
using MegaCrit.Sts2.Core.HoverTips;

// 生成卡牌预览
HoverTipFactory.FromCard<Shiv>();

// 生成升级后的卡牌预览
HoverTipFactory.FromCard<Shiv>(upgrade: true);

// 生成卡牌预览 + 卡牌附带的所有悬浮提示
HoverTipFactory.FromCardWithCardHoverTips<SovereignBlade>();

// 生成能力预览
HoverTipFactory.FromPower<PoisonPower>();

// 生成指定层数的能力预览
HoverTipFactory.FromPower<PoisonPower>(3);

// 生成球体预览
HoverTipFactory.FromOrb<LightningOrb>();

// 生成遗物预览
HoverTipFactory.FromRelic<MyRelic>();
```

### 卡牌中使用

```csharp
public sealed class Accuracy : CardModel
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => 
        [HoverTipFactory.FromCard<Shiv>()];
}

public sealed class Abrasive : CardModel
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => 
    [
        HoverTipFactory.FromPower<DexterityPower>(),
        HoverTipFactory.FromPower<ThornsPower>()
    ];
}
```

### 动态悬浮Tip升级机制（HoverTipHelper）

当卡牌的衍生卡效果会随升级而变化时，使用 `HoverTipHelper` 可以根据源卡牌的升级状态动态显示对应版本的衍生卡牌。

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

**核心工具类**：

```csharp
// HoverTipHelper.cs
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

**使用场景**：
- 建筑卡生产的单位卡会随建筑升级而升级（如矿场→矿车）
- 超级武器建筑产生的超级武器卡牌会随建筑升级而增强
- 任何需要根据升级状态显示不同衍生卡牌效果的场景

### 能力中使用

```csharp
public sealed class MyBuff : PowerModel
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<DexterityPower>()
    ];
}
```

---

## 💎 遗物（RelicModel）

### 基本结构
```csharp
public class MyRelic : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Common;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => new[] { new EnergyVar(1) };
    
    public override async Task AfterSideTurnStart(CombatSide side, CombatState state)
    {
        if (side == Owner.Creature.Side && state.RoundNumber == 1)
        {
            Flash();
            await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
        }
    }
}
```

### 注册
```csharp
ModHelper.AddModelToPool(typeof(SharedRelicPool), typeof(MyRelic));
```

### HarmonyPatch修改初始遗物
```csharp
[HarmonyPatch(typeof(Ironclad), nameof(Ironclad.StartingRelics), MethodType.Getter)]
public static class Patch
{
    static void Postfix(ref IReadOnlyList<RelicModel> __result)
    {
        var list = __result.ToList();
        list.Add(ModelDb.Relic<MyRelic>());
        __result = list;
    }
}
```

### 资源路径
```
res://images/relics/my_relic.png
res://images/atlases/relic_atlas.sprites/my_relic.tres
```

### 遗物卡牌转换补丁（NewLeaf / LeafyPoultice）

原版遗物「新叶」（NewLeaf）和「树叶膏药」（LeafyPoultice）会转换牌组中的卡牌。Mod 通过 Harmony Prefix 拦截 `AfterObtained` 方法，为盟军/苏军角色提供自定义的转换卡池（Mod 单位卡）。

#### 转换逻辑对比

| 遗物 | 转换目标 | 选择方式 | 卡池 |
|------|----------|----------|------|
| `LeafyPoultice`（树叶膏药） | 牌组中的 Strike/Defend 对应单位（盟军: AmericanSoldier/GrizzlyTank，苏军: Conscript/RhinoTank） | 自动转换，无选择面板 | Mod 全部单位卡 |
| `NewLeaf`（新叶） | 牌组中玩家选择的1张卡 | 玩家从选择面板选1张 | Mod 单位卡（选中 Mod 单位卡时）或原版随机（选中非 Mod 卡时） |

#### 单位卡注册体系

转换卡池通过 `AlliedCardRegistry` / `SovietCardRegistry` 的注册方法动态获取，**不硬编码单位列表**：

```csharp
// AlliedCardRegistry.cs / SovietCardRegistry.cs

// 特殊单位卡（属于单位卡的特殊卡，不含 Paratrooper 伞兵和 AirborneDivision 空降师团——两者均不属于单位卡）
public static List<Func<CardModel>> SpecialUnits { get; } = new()
{
    // 盟军: 无（PsiCommandoCard 已在 RelicUnlockedSoldiers 中，Paratrooper/AirborneDivision 不属于单位卡）
    // 苏军: YuriCard, YuriPrimeCard
};

// MCV 卡（既是装甲单位也是建筑）
public static List<Func<CardModel>> MobileConstructionVehicles { get; } = new()
{
    // 盟军: AlliedMCV
    // 苏军: SovietMCV
};

// GetAllUnits() 包含所有单位卡（士兵/装甲/飞机/船只/特殊单位卡/MCV）
public static List<CardModel> GetAllUnits() { ... }

// GetAllUnitTypes() 返回 HashSet<Type>（自动去重），供判断"是否为单位卡"使用
public static HashSet<Type> GetAllUnitTypes() { ... }
```

#### 树叶膏药转换实现

```csharp
[HarmonyPrefix]
[HarmonyPatch(typeof(LeafyPoultice), "AfterObtained")]
public static bool LeafyPoulticeAfterObtainedPrefix(LeafyPoultice __instance, ref Task __result)
{
    if (!IsAlliesCharacter(__instance.Owner.Character) && !IsSovietCharacter(__instance.Owner.Character))
        return true; // 非Mod角色走原版逻辑

    __result = LeafyPoulticeTransformAsync(__instance);
    return false;
}

private static async Task LeafyPoulticeTransformAsync(LeafyPoultice __instance)
{
    var deck = PileType.Deck.GetPile(__instance.Owner).Cards;
    var allUnitCards = GetAllModUnitCards(); // Mod 全部单位卡池
    var rng = __instance.Owner.PlayerRng.Transformations;

    // 查找牌组中的 Strike/Defend 对应单位并转换为随机 Mod 单位卡
    // 盟军: AmericanSoldier + GrizzlyTank
    // 苏军: Conscript + RhinoTank
    // 转换后还需扣除 12 点最大生命值（原版逻辑）
}
```

#### 新叶转换实现

```csharp
[HarmonyPrefix]
[HarmonyPatch(typeof(NewLeaf), "AfterObtained")]
public static bool NewLeafAfterObtainedPrefix(NewLeaf __instance, ref Task __result)
{
    if (!IsAlliesCharacter(__instance.Owner.Character) && !IsSovietCharacter(__instance.Owner.Character))
        return true;

    __result = NewLeafTransformAsync(__instance);
    return false;
}

private static async Task NewLeafTransformAsync(NewLeaf __instance)
{
    var prefs = new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 1, 1);

    // 选择面板 filter：排除围墙和诅咒卡（CardType.Curse）
    var selectedCards = (await CardSelectCmd.FromDeckGeneric(
        player: __instance.Owner,
        prefs: prefs,
        filter: card => !IsWallCard(card) && card.Type != CardType.Curse
    )).ToList();

    if (selectedCards.Any())
    {
        var selectedCard = selectedCards.First();

        if (IsModUnitCard(selectedCard))
        {
            // Mod 单位卡 → 从 Mod 单位卡池随机转换（排除自身）
            var allUnitCards = GetAllModUnitCards();
            var rng = __instance.Owner.PlayerRng.Transformations;
            var targets = allUnitCards.Where(t => t.Id.Entry != selectedCard.Id.Entry).ToList();
            if (targets.Any())
            {
                var replacement = __instance.Owner.RunState.CreateCard(rng.NextItem(targets), __instance.Owner);
                await CardCmd.Transform(selectedCard, replacement);
            }
        }
        else
        {
            // 非 Mod 卡 → 走原版随机转换
            await CardCmd.TransformToRandom(selectedCard, __instance.Owner.RunState.Rng.Niche);
        }
    }
}
```

#### 单位卡判断（注册类方法，非硬编码）

```csharp
// 缓存合并后的所有 Mod 单位类型
private static HashSet<Type>? _allModUnitTypes;

private static HashSet<Type> GetAllModUnitTypes()
{
    if (_allModUnitTypes != null) return _allModUnitTypes;
    var types = new HashSet<Type>();
    types.UnionWith(AlliedCardRegistry.GetAllUnitTypes());
    types.UnionWith(SovietCardRegistry.GetAllUnitTypes());
    _allModUnitTypes = types;
    return types;
}

private static bool IsModUnitCard(CardModel card)
{
    return GetAllModUnitTypes().Contains(card.GetType());
}
```

#### 围墙判断

```csharp
private static bool IsWallCard(CardModel card)
{
    return card is AlliedWallCard || card is FortifiedWall ||
           card is SovietWallCard || card is SovietFortifiedWall;
}
```

#### 转换卡池范围

| 卡牌类型 | 是否在卡池中 | 说明 |
|---------|:----------:|------|
| 士兵（AmericanSoldier/Conscript 等） | ✅ | 通过 `Soldiers`/`RadarSoldiers`/`HighTechSoldiers`/`RelicUnlockedSoldiers` 注册 |
| 装甲（GrizzlyTank/RhinoTank 等） | ✅ | 通过 `Vehicles`/`RadarVehicles`/`HighTechVehicles` 注册 |
| 飞机（Intruder/Kirov 等） | ✅ | 通过 `Aircraft` 注册 |
| 船只（Destroyer/Dreadnought 等） | ✅ | 通过 `Ships`/`HighTechShips` 注册 |
| 特殊单位卡（YuriCard/YuriPrimeCard 等） | ✅ | 通过 `SpecialUnits` 注册（苏军: YuriCard, YuriPrimeCard；盟军: 无） |
| MCV（AlliedMCV/SovietMCV） | ✅ | 通过 `MobileConstructionVehicles` 注册 |
| Paratrooper（伞兵） | ❌ | **不属于单位卡**，不注册 |
| AirborneDivision（空降师团） | ❌ | **不属于单位卡**，不注册 |
| 围墙/坚固围墙 | ❌ | 选择面板 filter 排除 |
| 诅咒卡（CardType.Curse） | ❌ | 选择面板 filter 排除 |
| 建筑卡/防御塔/能力卡 | ❌ | 不在单位卡池中 |

---

## 🧪 药水（PotionModel）

### 基本结构
```csharp
public class MyPotion : PotionModel
{
    public override PotionRarity Rarity => PotionRarity.Rare;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.AllEnemies;
    
    protected override List<DynamicVar> CanonicalVars => new() { new DamageVar(30m) };
    
    protected override async Task OnUse(PlayerChoiceContext ctx, Creature? target)
    {
        await CreatureCmd.Damage(ctx, Owner.Creature.CombatState.HittableEnemies,
            DynamicVars.Damage.BaseValue, ValueProp.Unpowered, Owner.Creature, null);
    }
}
```

### 注册
```csharp
ModHelper.AddModelToPool(typeof(SharedPotionPool), typeof(MyPotion));
```

---

## 💰 经济系统（刀乐）

### 设计理念

红警2 Mod引入了经济系统，通过"刀乐"能力来管理资金。建筑和单位的生产需要消耗资金，资源采集会增加资金。

### 刀乐遗物（DollarRelic）

初始遗物，战斗开始时赋予刀乐能力并设置启动资金：

### 刀乐能力（DollarPower）

专门用于存储资金数值的能力：

```csharp
public class DollarPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    
    // 当前资金值
    public int DollarValue { get; set; } = 0;

    public override LocString Description
    {
        get
        {
            var locString = new LocString("powers", base.Id.Entry + ".description");
            locString.Add("dollar_value", DollarValue);
            return locString;
        }
    }

    // 设置资金值
    public void SetDollar(int value)
    {
        DollarValue = value;
    }

    // 增加资金
    public void AddDollar(int amount)
    {
        DollarValue += amount;
    }

    // 减少资金（返回是否成功）
    public bool SpendDollar(int amount)
    {
        if (DollarValue >= amount)
        {
            DollarValue -= amount;
            return true;
        }
        return false;
    }
}
```

---

## 🏗️ 建筑打出系统（BuildingDrawPower & UrbanizationPower）

### 设计理念

红警2 Mod 中，建筑卡牌打出后有两套独立的自动触发逻辑，均通过 `PowerModel.AfterCardPlayed` 钩子集中实现，**建筑卡牌自身无需写任何触发代码**：

| 能力 | 持有者 | 触发条件 | 效果 |
|------|--------|----------|------|
| `BuildingDrawPower`（隐藏） | 所有获得 `DollarPower` 的玩家 | 打出**非围墙且非防御塔**的建筑牌 | 抽1张牌（从抽牌堆顶） |
| `UrbanizationPower`（可见） | 打出 `UrbanizationCard` 的玩家 | 打出**非围墙**的建筑/防御塔牌 | 从牌堆中抽取建筑牌 |

两者在各自的 `AfterCardPlayed` 钩子中独立触发，互不干扰。

### 核心机制

#### 1. 建筑抽牌能力（BuildingDrawPower）

隐藏能力（`IsVisibleInternal = false`），通过 `DollarPower.AfterApplied` 自动挂载——**任何途径获得 `DollarPower`**（遗物、转账、矿场、油井、资金箱等）都会自动获得此能力，不依赖特定遗物。

```csharp
public sealed class BuildingDrawPower : PowerModel
{
    protected override bool IsVisibleInternal => false;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != base.Owner.Player)
            return;

        // 只有非围墙且非防御塔的建筑才触发抽牌（防御塔不抽牌）
        if (!CardUtils.IsNonWallNonDefenseTowerBuilding(cardPlay.Card))
            return;

        // 选择面板类建筑卡取消选择时跳过
        if (CardUtils.WasCardPlayCancelled(cardPlay))
            return;

        await CardPileCmd.Draw(choiceContext, 1, base.Owner.Player);
    }
}
```

#### 2. 城市化能力（UrbanizationPower）

可见能力，由 `UrbanizationCard` 授予。打出非围墙建筑/防御塔牌时，从弃牌堆/抽牌堆中抽取建筑牌。

```csharp
public sealed class UrbanizationPower : PowerModel
{
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != base.Owner.Player)
            return;

        // 非围墙建筑/防御塔才触发（围墙不触发，建筑和防御塔都触发）
        if (!CardUtils.IsNonWallBuildingOrDefenseTower(cardPlay.Card))
            return;

        // 选择面板类建筑卡取消选择时跳过
        if (CardUtils.WasCardPlayCancelled(cardPlay))
            return;

        await TriggerDrawInternal(choiceContext, base.Owner.Player);
    }
}
```

### 卡牌类型判断（CardUtils）

建筑/防御塔类型判断逻辑已集中到 `CardUtils`，提供三套不同范围的判断方法：

| 方法 | 范围 | 用途 |
|------|------|------|
| `IsBuildingOrDefenseTower(card)` | 含围墙 | 从牌堆过滤建筑卡（城市化抽牌筛选） |
| `IsNonWallBuildingOrDefenseTower(card)` | 不含围墙 | 城市化触发判定（建筑+防御塔都触发） |
| `IsNonWallNonDefenseTowerBuilding(card)` | 不含围墙且不含防御塔 | 建筑抽牌触发判定（只有建筑触发） |

```csharp
public static HashSet<Type> GetNonWallNonDefenseTowerBuildingTypes()
{
    var set = new HashSet<Type>(GetNonWallBuildingOrDefenseTowerTypes());
    // 移除所有防御塔类型
    foreach (var towerType in AlliedCardRegistry.GetAllDefenseTowerTypes())
        set.Remove(towerType);
    foreach (var towerType in SovietCardRegistry.GetAllDefenseTowerTypes())
        set.Remove(towerType);
    return set;
}
```

### 选择面板类建筑卡的取消处理

部分建筑卡（重工、兵营、MCV、船厂、维修厂等）实现 `ICancellableCardPlay`，玩家可以在选择面板取消选择。取消时统一走 `CardUtils.HandleCardCancellation`，该方法通过 `ConditionalWeakTable<CardPlay, object>` 标记取消状态：

```csharp
// CardUtils.cs
private static readonly ConditionalWeakTable<CardPlay, object> _cancelledCardPlays = new();

public static void MarkCardPlayCancelled(CardPlay play)
{
    _cancelledCardPlays.Remove(play);
    _cancelledCardPlays.Add(play, new object());
}

public static bool WasCardPlayCancelled(CardPlay play)
{
    return play != null && _cancelledCardPlays.TryGetValue(play, out _);
}
```

`BuildingDrawPower` 和 `UrbanizationPower` 的 `AfterCardPlayed` 钩子均检测 `WasCardPlayCancelled`，取消则跳过触发。

### 自动挂载机制（DollarPower.AfterApplied）

`BuildingDrawPower` 通过 `DollarPower.AfterApplied` 钩子自动挂载，确保所有获得 `DollarPower` 的玩家都能享受建筑抽牌效果：

```csharp
public class DollarPower : PowerModel
{
    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        if (Owner == null)
            return;

        var existingBuildingDraw = Owner.Powers.OfType<BuildingDrawPower>().FirstOrDefault();
        if (existingBuildingDraw == null)
        {
            await PowerCmd.Apply<BuildingDrawPower>(
                new ThrowingPlayerChoiceContext(), Owner, 1m, Owner, null);
        }
    }
}
```

### 触发逻辑速查

| 卡牌类型 | BuildingDrawPower（抽1张） | UrbanizationPower（抽建筑牌） |
|---------|:------------------------:|:---------------------------:|
| 围墙 | ❌ | ❌ |
| 防御塔（光棱塔/机枪碉堡等） | ❌ | ✅ |
| 建筑（发电厂/重工/兵营等） | ✅ | ✅ |
| 选择面板类建筑卡（取消选择） | ❌ | ❌ |
| 选择面板类建筑卡（成功选择） | ✅ | ✅ |

### 新增建筑卡注意事项

1. **无需写任何抽牌/城市化触发代码**：两套系统均通过 `AfterCardPlayed` 钩子自动触发
2. **无需硬编码 `CardPileCmd.Draw(ctx, 1, Owner)`**：建筑抽牌由 `BuildingDrawPower` 统一处理
3. **无需硬编码 `UrbanizationPower.TriggerOnSuccessfulPlay(...)`**：该方法已删除，城市化由 `AfterCardPlayed` 统一处理
4. **选择面板类建筑卡**：实现 `ICancellableCardPlay`，取消时调用 `CardUtils.HandleCardCancellation(play, this, Owner)` 即可，无需额外处理

---

## 💳 转账系统（DollarTransfer）

### 核心组件

| 组件 | 文件路径 | 作用 |
|------|----------|------|
| `DollarTransferManager` | `RedAlert2ModCode/Common/Utils/DollarTransferManager.cs` | 转账逻辑管理器，处理并发控制和网络同步 |
| `DollarTransferScreen` | `RedAlert2ModCode/UI/DollarTransferScreen.cs` | 转账UI面板，供玩家选择目标和金额 |
| `DollarTransferGameAction` | `RedAlert2ModCode/Common/GameActions/DollarTransferGameAction.cs` | 转账游戏动作，处理资金转移 |
| `NetDollarTransferGameAction` | `RedAlert2ModCode/Common/GameActions/NetDollarTransferGameAction.cs` | 转账网络同步动作 |
| `DollarTransferUnlockAction` | `RedAlert2ModCode/Common/GameActions/DollarTransferUnlockAction.cs` | 转账锁解锁动作（网络同步） |
| `NetDollarTransferUnlockAction` | `RedAlert2ModCode/Common/GameActions/NetDollarTransferUnlockAction.cs` | 转账锁解锁网络同步动作 |

### 核心方法

```csharp
// 检查是否可以转账（资金充足）
DollarTransferManager.CanTransfer(sender, amount);

// 获取有效转账目标（存活的队友）
DollarTransferManager.GetValidTargets(sender);

// 执行转账（返回bool表示是否成功发起）
DollarTransferManager.ExecuteTransfer(sender, receiver, amount);

// 获取发送者资金余额
DollarTransferManager.GetSenderBalance(sender);

// 重置转账锁（手动解锁）
DollarTransferManager.ResetTransferLock();

// 显示转账面板（异步）
await DollarTransferScreen.ShowTransferScreen(sender);
```

### 并发控制机制

转账系统采用多重并发保护：

| 机制 | 说明 |
|------|------|
| `_isTransferring` 标志 | 静态布尔值，防止同时发起多次转账 |
| 锁保护 | 使用 `lock(_lock)` 确保线程安全 |
| 网络同步解锁 | 转账完成后发送 `DollarTransferUnlockAction` 同步给其他玩家 |
| 面板打开自动解锁 | 重新打开转账面板时自动调用 `ResetTransferLock()` |

### 工作流程

```
玩家打开转账面板
    ↓
ResetTransferLock() → _isTransferring = false
    ↓
玩家选择目标和金额，点击转账
    ↓
ExecuteTransfer() 检查 _isTransferring
    ↓ 如果为true → 显示错误提示，不关闭面板
    ↓ 如果为false → 设置 _isTransferring = true，发送转账请求
    ↓
转账完成（AfterFinished回调）
    ↓
_isTransferring = false
    ↓
发送 DollarTransferUnlockAction 给所有玩家
    ↓
其他玩家收到解锁信号，执行 ResetTransferLock()
    ↓
所有玩家均可再次发起转账
```

### 本地化配置

```json
{
    "ui.dollar_transfer.title": "转账给队友",
    "ui.dollar_transfer.balance": "当前资金",
    "ui.dollar_transfer.amount": "输入转账金额",
    "ui.dollar_transfer.recipient": "选择接收人",
    "ui.dollar_transfer.no_target": "没有可转账的队友",
    "ui.dollar_transfer.cancel": "取消",
    "ui.dollar_transfer.failed": "转账失败，请稍后重试"
}
```

### 注意事项

1. **网络同步**：`DollarTransferUnlockAction` 通过 `INetAction` 接口自动注册并同步
2. **异常处理**：转账失败时会自动发送解锁同步，防止永久锁定
3. **保险机制**：即使网络同步失败，重新打开面板也会自动解锁
4. **UI反馈**：转账成功关闭面板，失败显示红色错误提示并保留面板

---

## ⚡ 能力/Buff（PowerModel）

### 基本结构
```csharp
public class MyBuff : PowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    
    public override async Task AfterCardPlayed(PlayerChoiceContext ctx, CardPlay play)
    {
        if (play.Card.Owner.Creature == Owner && Amount > 0)
        {
            Flash();
            await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null, fast: true);
        }
    }
    
    public override async Task AfterTurnEnd(PlayerChoiceContext ctx, CombatSide side)
    {
        if (side == Owner.Side)
            await PowerCmd.Decrement(this);
    }
}
```

### 叠层与实例化策略（PowerInstanceType + PowerStackType）

#### PowerInstanceType（枚举：实例化方式）
控制 `PowerCmd.Apply` 时是"叠加Amount到已有实例"还是"新建独立实例"。通过解包 `sts2.dll` 的 `PowerCmd.cs:167-173` 确认行为：

| 值 | PowerCmd.Apply 行为 | 适用场景 | 官方示例 / 红警Mod示例 |
|---|---|---|---|
| `None`（默认） | 用 `target.GetPower(Id)` 查找同ID实例 → 找到就 `ModifyAmount` 叠加Amount，找不到才新建。Creature.AddPower 会校验，非Instanced类型禁止重复添加多个实例。 | 纯数值Buff/Debuff，不需要每个实例独立状态 | Strength、Dexterity、Vulnerable、RaidDollarPower（资金） |
| `Instanced` | **查找existing直接返回null → 每次都新建独立实例**，互不干扰 | 每个实例需要独立的自定义状态/倒计时 | TheBombPower（炸弹倒计时）、GemMinePower（独立储备）、NuclearReactorCorePower（独立血量） |
| `InstancedPerApplier` | 按 `Applier`（施放者）匹配实例 → 同施放者叠加Amount，不同施放者新建实例 | 多人联机场景，需要按玩家区分效果来源 | OblivionPower |

> **⚠️ 常见坑**：配置了 `InstanceType = Instanced` 后，又在 `ApplyPower()` 里手写 `OfType<YourPower>().FirstOrDefault() → ModifyAmount`，会完全绕过 Instanced 语义导致无法多实例！

#### PowerStackType（枚举：层数显示方式）
控制UI中层数的可视化样式，与 InstanceType 正交可自由组合：

| 值 | 说明 | 典型搭配 |
|---|---|---|
| `Counter` | 右下角显示Amount数值（例："力量 2"） | 纯数值叠层 + None，或 Instanced 但每个实例有Amount |
| `Single` | 不显示层数，只显示图标 | 状态类能力：Weak、Frail、Spirit |

#### 场景速选："要叠层/不要叠层"怎么选？

| 你的需求 | InstanceType | StackType | 卡牌侧OnPlay写法 |
|---------|-------------|-----------|----------------|
| 再打一张会增加效果数值（例：两张力量卡=力量+2） | `None` | `Counter` | 直接 `PowerCmd.Apply<T>(amount, ...)`，框架自动叠加 |
| 再打一张是"新的独立效果单元"（例：两张炸弹=两个独立倒计时；两个核电站=两套独立血量） | `Instanced` | `Counter` / `Single` | 直接 `PowerCmd.Apply<T>(amount: 1, ...)`，每次新建实例 |
| 多人联机时同一个效果按玩家区分（例：玩家A放的Oblivion和玩家B放的各算各的） | `InstancedPerApplier` | `Counter` | 直接 `PowerCmd.Apply<T>(amount, ..., applier)` |

> **红警Mod常见选择速查**
> - 建筑类能力（核电站、矿场、雷达、宝石矿、作战实验室等）→ 每个建筑独立状态 → **`Instanced`**
> - 资金/能量/科技点（DollarPower、TechPointPower等）→ 所有来源合并数值 → **`None`**
> - 中毒、易伤、虚弱、力量等Debuff/Buff → 纯数值合并 → **`None`**
> - 战备卡类（飞鹰500kg、闪电风暴等回合触发器）→ 多个战备独立触发 → **`Instanced`**

---

### Power 高级模式（推荐）：自监听「未格挡伤害」+ 动态状态描述注入

> **为什么需要这个模式？** 自爆卡车（利比亚 Libya Relic）的传统实现把"受击触发卡牌打出"写在遗物里是正确的；但**受击后要更新Power自身状态 + 触发Power自身效果**（例：核电站扣血、磁暴线圈蓄能、可破坏护盾破损）如果也配遗物就会严重耦合。核电站（NuclearReactorCorePower）提供了更通用的模式：**不借助任何遗物实例，Harmony 直接挂框架广播点把「未格挡伤害」推送给目标上的 Power，再通过 Description getter 的 LocString 动态注入把自定义状态字段（如 CurrentHealth）实时显示到悬浮 tip 上。** 这是以后最常遇到的「带独立状态 + 受击触发」类 Power 的标准写法。

> **⚠️ 先用二分法判断你属于哪种场景！不要误用！**
>
> | 你监听伤害后要做什么？ | 推荐模式 |
> |---------------------|---------|
> | **Power自己的状态（扣血/充能/计数）+ Power自己的效果（爆炸/蓄能释放/护盾破裂）** | ⭐ Power 自监听模式（本节） |
> | **卡牌级操作（从牌堆搜卡、自动打出、生成Token卡、检索卡入手牌）** | 遗物模式（利比亚 Libya Relic 写法） |
> | **跨多战斗的玩家级永久加成（全局+力量/+血量上限/开局得卡）** | 遗物模式 |
>
> 简单一句话：**改Power自己 → Power自监听；动卡牌 → 遗物；跨战斗 → 遗物。** 利比亚这种"受击→搜牌堆→自动打出"必须用遗物更稳定。

---

#### 一、整体链路（不创建任何遗物）

```
 CreatureCmd.Damage（解包内部结算完格挡）
       ↓
  框架对所有 HookListener 广播 RelicModel.AfterDamageReceived
   （N 个 Relic/Power/Card 监听器各自触发一次）
       ↓
  ★ Harmony Postfix：挂「方法」不挂「遗物实例」→ 偷听广播
    ├─ 过滤：UnblockedDamage > 0 且 target 身上有目标 Power
    ├─ 外层去重：_processedGlobalEvents（引用hash，N 次广播只放行 1 次）
    └─ 遍历：target.Powers.OfType<YourPower>() → 逐个调用你的 Power.ReceiveUnblockedDamage()
       ↓
  ★ Power 内部处理（每个实例独立）
    ├─ 防重入：_isExploding 连锁触发标志
    ├─ 内层去重：_processedDamageEventIds（再保险，确保同一事件不重复扣血）
    ├─ 更新自定义状态：CurrentHealth -= UnblockedDamage
    └─ 触发阈值：CurrentHealth <= 0 → 爆炸/自爆/其他效果
       ↓
  ★ Description getter 动态注入（每次悬浮tip才计算，不用手动刷新）
    new LocString(...).Add("CurrentHealth", CurrentHealth).Add(...)
```

---

#### 二、为什么挂 `RelicModel.AfterDamageReceived` 而不是 `PowerModel.AfterTakeDamage`？

| Hook 点 | 参数里是否有 `DamageResult`（含 UnblockedDamage） | 需要遗物实例存在 | 推荐 |
|---------|---------------------------------------------------|----------------|------|
| `RelicModel.AfterDamageReceived(..., DamageResult result, ...)` | ✅ 有（`result.BlockedDamage` + `result.UnblockedDamage`，**格挡已完全算完**） | ❌ **不需要**（我们 Patch 的是方法签名，挂在广播链出口偷听完就走；不需要玩家/怪物身上有任何遗物） | ⭐⭐⭐ **唯一推荐** |
| `PowerModel.AfterTakeDamage(DamageResult result)` | ⚠️ 部分版本有，但 PowerModel 事件链在 CreatureCmd 中是单独广播，**顺序与 Relic 不同步**，且部分版本没有 DamageResult 重载 | ✅ Power 实例本身需要存在（当然有，因为是 Power 自监听），但**事件广播不稳定**（解包源码中 Power 事件可能被战斗结束守卫跳过） | ❌ 不推荐 |
| 直接 Patch `CreatureCmd.Damage` Postfix | ❌ 方法内部的 local 变量拿不到，且 `DamageResult` 还在栈上没构造完 | - | ❌ 极易 DLL 初始化失败（async 状态机 + 迭代链） |
| 传统模式：写一个配套 Relic 类实现 AfterDamageReceived | ✅ 有 | ✅ **必须创建遗物实例 + 注册遗物池 + 玩家获得遗物** | ❌ 耦合重（例：自爆卡车旧实现） |

---

#### 三、完整可复制模板

##### 模板 Part 1：Harmony 广播补丁（放在 `{阵营}/Patches/` 目录）

```csharp
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using YourModCode.YourFaction.Powers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace YourModCode.YourFaction.Patches;

[HarmonyPatch]
public static class YourPowerDamagePatch
{
    private static MethodBase TargetMethod()
    {
        // ★ 精准定位带 DamageResult 的重载，避免匹配到其他重载
        return typeof(RelicModel).GetMethod("AfterDamageReceived",
            BindingFlags.Public | BindingFlags.Instance,
            null,
            new[]
            {
                typeof(PlayerChoiceContext),
                typeof(Creature),       // target：受伤者
                typeof(DamageResult),   // result：★含 Blocked/Unblocked
                typeof(ValueProp),
                typeof(Creature),       // dealer：攻击者（可空）
                typeof(CardModel)       // cardSource：来源卡（可空）
            },
            null);
    }

    // ★ 外层去重：同一次伤害被 N 个 Relic 回调 N 次 -> 只放行一次
    private static readonly HashSet<int> _processedGlobalEvents = new();

    private static async void Postfix(
        PlayerChoiceContext choiceContext,
        Creature target, DamageResult result, ValueProp props,
        Creature? dealer, CardModel? cardSource)
    {
        // 快速失败 4 层过滤（95% 调用在这里 return，性能极低开销）
        if (target == null || !target.IsAlive || result == null || result.UnblockedDamage <= 0)
            return;

        var powers = target.Powers?.OfType<YourStatefulPower>().ToList();
        if (powers == null || powers.Count == 0)
            return;

        // 引用地址哈希（值类型 DamageResult 取 boxed 引用地址稳定）
        int eventHashCode = target.GetHashCode()
                            ^ RuntimeHelpers.GetHashCode(result)
                            ^ (dealer != null ? RuntimeHelpers.GetHashCode(dealer) : 0)
                            ^ (cardSource != null ? RuntimeHelpers.GetHashCode(cardSource) : 0);

        if (!_processedGlobalEvents.Add(eventHashCode))
            return;

        // 防止超长战斗内存膨胀，超过阈值自动清空
        const int maxEvents = 4096;
        if (_processedGlobalEvents.Count > maxEvents)
            _processedGlobalEvents.Clear();

        // InstanceType=Instanced 时同一 Creature 上可能有多个独立实例 -> 逐个推送
        foreach (var power in powers)
        {
            power.ReceiveUnblockedDamage((int)result.UnblockedDamage, eventHashCode);
        }
    }
}
```

##### 模板 Part 2：Power 自接收 + 动态描述注入（放在 `{阵营}/Powers/` 目录）

```csharp
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using System.Collections.Generic;

namespace YourModCode.YourFaction.Powers;

public class YourStatefulPower : PowerModel
{
    private static readonly PowerValueStore.PowerValues Values = YourFactionPowerValues.YourStatefulPower;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced; // ★独立状态用 Instanced

    // ★ 自定义独立状态字段（决定了必须用 Instanced）
    public int CurrentHealth { get; set; } = (int)Values.Damage;
    public int CurrentEnergy { get; set; } = (int)Values.MagicNumber;
    public bool IsUpgraded { get; set; } = false;

    // 防重入 + 内层去重
    private bool _isExploding = false;
    private readonly HashSet<int> _processedDamageEventIds = new();

    public new string PackedIconPath => "res://YourModResources/images/packed/powers/your_power.png";

    // ★★★ 动态描述：每次悬浮 tip 时才 new，注入实时状态（不需要手动刷新）
    public override LocString Description
    {
        get
        {
            var locString = new LocString("powers", base.Id.Entry + ".description");
            // 常量类注入（升级态/基础态覆盖）
            locString.Add("Energy", IsUpgraded ? (int)(Values.MagicNumber + Values.MagicNumberUpgraded) : CurrentEnergy);
            locString.Add("Health", IsUpgraded ? (int)(Values.Damage + Values.DamageUpgraded) : (int)Values.Damage);
            locString.Add("Poison", (int)Values.Repeat);
            // ★ 实时变化的自定义字段直接注入（CurrentHealth 扣了就立刻显示新值）
            locString.Add("CurrentHealth", CurrentHealth);
            return locString;
        }
    }

    // ★ 静态 Apply 方法（卡牌 OnPlay 调用这个，不要手写 OfType→ModifyAmount，会破坏 Instanced 语义）
    public static async Task<YourStatefulPower?> ApplyToCreature(Creature owner, bool isUpgraded = false)
    {
        var power = await PowerCmd.Apply<YourStatefulPower>(
            new ThrowingPlayerChoiceContext(), owner, 1m, owner, null);
        if (power != null)
        {
            power.CurrentEnergy = isUpgraded ? (int)(Values.MagicNumber + Values.MagicNumberUpgraded) : (int)Values.MagicNumber;
            power.CurrentHealth = isUpgraded ? (int)(Values.Damage + Values.DamageUpgraded) : (int)Values.Damage;
            power.IsUpgraded = isUpgraded;
        }
        return power;
    }

    // ★ 接收补丁推送来的未格挡伤害（由 Part 1 的 Postfix 逐个调用）
    public void ReceiveUnblockedDamage(int unblockedDamage, int eventHashCode)
    {
        if (Owner == null || !Owner.IsAlive || unblockedDamage <= 0) return;

        // 1. 防重入：爆炸/效果过程中产生的新伤害（例如 Poison 扣血）直接忽略，防止连锁
        if (_isExploding)
        {
            GD.Print($"[YourStatefulPower] 效果进行中，忽略连锁伤害 {unblockedDamage}");
            return;
        }

        // 2. 内层去重：再保险（如果外层 hash 因极端情况冲突，这里兜底）
        if (!_processedDamageEventIds.Add(eventHashCode))
        {
            GD.Print($"[YourStatefulPower] 同事件已处理，跳过 {eventHashCode:X8}");
            return;
        }

        // 3. 更新独立状态字段
        CurrentHealth -= unblockedDamage;
        GD.Print($"[YourStatefulPower] 受 {unblockedDamage} 点未格挡伤害，剩余 {CurrentHealth}");

        // 4. 阈值触发（示例：血量<=0 爆炸，可替换为其他阈值）
        if (CurrentHealth <= 0)
        {
            _ = TriggerEffectAsync();
        }
    }

    private async Task TriggerEffectAsync()
    {
        _isExploding = true;
        try
        {
            // ... 你的效果：播放音效、造成伤害、施加 Poison、清场等
            // 注意：使用 PowerCmd.Apply 施加的 Poison/其他 可能再次触发伤害，
            //       但 _isExploding=true 已在 ReceiveUnblockedDamage 开头拦截，不会再连锁

            // 最后处理：Instanced 每个实例独立，一般直接 Remove 整个实例
            // 若你用 Amount 叠层（InstanceType=None），则改为 ModifyAmount -1 重置状态
            await PowerCmd.Remove(this);
        }
        finally
        {
            // 防御性代码：如果 Remove 成功（Owner不再包含this），永久锁死
            if (Owner == null || !Owner.Powers.Contains(this))
                _isExploding = true;
        }
    }
}
```

---

#### 四、动态描述注入原理（为什么不需要手动刷新 UI？）

| 传统方式（不推荐） | 动态 getter 方式（核电站用的，推荐） |
|-------------------|------------------------------------|
| 把 Description 当缓存字段，在事件触发后手动修改或赋值 `this.description = ...` | **Description 是一个 `override LocString get` 属性，每次访问（鼠标悬浮）才 new 一个新 LocString 并注入当前字段值** |
| 问题：修改完容易漏掉某些入口，或框架内部缓存不刷新导致玩家看到旧数值 | **零手动刷新**：玩家鼠标悬浮的那个瞬间，拿到的永远是 CurrentHealth/CurrentEnergy 最新值 |

---

#### 五、常见坑速查

| 现象 | 根因 | 修复 |
|-----|------|------|
| 一刀伤害扣了 N 次血（N = 怪物身上 Relic 数 + 玩家 Relic 数） | 只写了内层去重没写外层 `_processedGlobalEvents` | Part 1 补丁加全局 `HashSet<int>` 按引用 hash 去重 |
| 爆炸触发后，Poison 扣血又触发第二次爆炸 → 连锁死亡 | 没加 `_isExploding` 防重入标志 | `ReceiveUnblockedDamage` 开头 `if (_isExploding) return;`，TriggerEffectAsync 第一行 `_isExploding = true;` |
| CurrentHealth 描述显示和战斗日志对不上 | `Description.get` 里用了 `Values.Damage` 常量而不是注入字段值 | 必须 `locString.Add("CurrentHealth", CurrentHealth)` 注入**字段**，不是注入常量 |
| InstanceType=Instanced 但打了两张卡只看到一个图标 Amount=2 | `ApplyToCreature` 里写了 `OfType().FirstOrDefault() → ModifyAmount` 手动叠层 | 删掉手动叠层，直接 `PowerCmd.Apply<T>(amount: 1, ...)` 让框架按 Instanced 语义每次新建 |
| CurrentHealth 在 `ReceiveUnblockedDamage` 修改后没显示变化 → 显示的总是初始值 | 用了字段缓存（`private LocString _desc;` 或在构造函数里 new LocString 保存） | LocString 必须写在 Description getter 里，每次 get 重新 new |

### 能力图标配置

由于 `PowerModel.Icon` 属性不是 `virtual` 的，无法通过重写来设置自定义图标。需要使用 `PowerIconPatch` 来拦截图标获取：

```csharp
// PowerIconPatch.cs - Harmony补丁配置
[HarmonyPatch]
public static class PowerIconPatch
{
    private static readonly Dictionary<Type, string> _customIconPaths = new()
    {
        { typeof(MyBuff), "res://path/to/icon.png" },
        { typeof(TransportShipPower), "res://RedAlert2ModResources/images/packed/card_portraits/allies/landicon.png" },
        { typeof(Eagle500kgPower), "res://RedAlert2ModResources/images/packed/powers/Eagle500kgPower.png" },
        // 添加更多能力类型和图标路径
    };

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PowerModel), nameof(PowerModel.Icon), MethodType.Getter)]
    public static bool IconPrefix(PowerModel __instance, ref Texture2D __result)
    {
        Type type = __instance.GetType();
        if (_customIconPaths.TryGetValue(type, out string iconPath))
        {
            if (ResourceLoader.Exists(iconPath))
            {
                __result = ResourceLoader.Load<Texture2D>(iconPath);
                return false; // 跳过原方法
            }
        }
        return true; // 执行原方法
    }
    
    // 同样需要Patch PackedIconPath 和 BigIcon 属性
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PowerModel), nameof(PowerModel.PackedIconPath), MethodType.Getter)]
    public static bool PackedIconPathPrefix(PowerModel __instance, ref string __result)
    {
        Type type = __instance.GetType();
        if (_customIconPaths.TryGetValue(type, out string iconPath))
        {
            __result = iconPath;
            return false;
        }
        return true;
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PowerModel), nameof(PowerModel.BigIcon), MethodType.Getter)]
    public static bool BigIconPrefix(PowerModel __instance, ref Texture2D __result)
    {
        Type type = __instance.GetType();
        if (_customIconPaths.TryGetValue(type, out string iconPath))
        {
            if (ResourceLoader.Exists(iconPath))
            {
                __result = ResourceLoader.Load<Texture2D>(iconPath);
                return false;
            }
        }
        return true;
    }
}
```

**重要提示**：
1. **新增能力类型后，必须将其添加到 `_customIconPaths` 字典中**，否则图标将无法正常显示。
2. **图标文件必须存在于指定路径**，建议放在 `RedAlert2ModResources/images/packed/powers/` 目录下。
3. **图标路径格式**：`res://RedAlert2ModResources/images/packed/powers/<能力名称>Power.png`

**示例**：添加 `Eagle500kgPower` 能力图标：
```csharp
{ typeof(Eagle500kgPower), "res://RedAlert2ModResources/images/packed/powers/Eagle500kgPower.png" },
```

**常见问题排查**：
- 如果图标不显示，检查：
  1. `_customIconPaths` 字典中是否注册了该能力类型
  2. 图标文件路径是否正确
  3. 图标文件是否存在于指定位置
  4. 文件名大小写是否匹配（区分大小写）

### 施加能力
```csharp
await PowerCmd.Apply<MyBuff>(target, amount, source, sourceCard);
```

### 数值可变能力的叠加逻辑

对于数值会变化的能力（如油井、黄蜂舰载机），需要实现特殊的叠加逻辑：**检查数值相同的能力是否存在，存在便叠加上去，否则创建独立能力**。

**示例场景**：
- 打出两张基础油井（每回合$500）→ 叠加为1个能力，显示"油井 2"
- 打出一张基础油井（$500）+ 一张升级油井（$800）→ 创建2个独立能力

**实现模式**（参考黄蜂舰载机）：
```csharp
public static async Task ApplyOilDerricks(Creature owner, int count, bool isUpgraded = false)
{
    // 计算目标数值
    int targetDollarPerTurn = (int)Values.DollarValue + (isUpgraded ? (int)Values.DollarValueUpgraded : 0);
    
    // 查找相同数值的能力
    var existingPower = owner.Powers
        .OfType<OilDerrickPower>()
        .FirstOrDefault(p => p.CurrentDollarPerTurn == targetDollarPerTurn);
    
    if (existingPower != null)
    {
        // 叠加层数
        await PowerCmd.ModifyAmount(ctx, existingPower, count, owner, null);
    }
    else
    {
        // 创建新能力
        var newPower = await PowerCmd.Apply<OilDerrickPower>(ctx, owner, count, owner, null);
        newPower.CurrentDollarPerTurn = targetDollarPerTurn;
    }
}
```

### 动态切换能力类型（Buff/Debuff）

能力类型可以根据状态动态切换，实现视觉上的状态区分。例如生产序列能力在"生产中"时显示为Buff（绿色数字），"停产"时显示为Debuff（红色数字）。

**实现方式**：
```csharp
public class TrainingQueuePower : PowerModel
{
    public bool IsStopped { get; set; } = false;
    
    /// <summary>
    /// 根据停产状态动态返回能力类型
    /// 生产中 -> Buff（绿色数字）
    /// 停产 -> Debuff（红色数字）
    /// </summary>
    public override PowerType Type => IsStopped ? PowerType.Debuff : PowerType.Buff;
    
    public override PowerStackType StackType => PowerStackType.Counter;
}
```

**效果说明**：
- 当 `IsStopped = false`（生产中）：能力图标显示为绿色边框，数字为绿色
- 当 `IsStopped = true`（停产）：能力图标显示为红色边框，数字为红色

这种方式可以让玩家直观地通过颜色区分能力的当前状态。

---

## 👤 角色（CharacterModel）

### 基本结构
```csharp
public sealed class MyCharacter : CharacterModel
{
    public override int StartingHp => 80;
    public override int StartingGold => 99;
    public override CardPoolModel CardPool => ModelDb.CardPool<MyCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<MyRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<MyPotionPool>();
    public override CharacterModel? UnlocksAfterRunAs => null;
}
```

### 卡池
```csharp
public class MyCardPool : CardPoolModel
{
    public override string Title => "MyCharacter";
    public override List<CardModel> GenerateAllCards() => new() { /* cards */ };
}
```

### 注册角色（HarmonyPatch）
```csharp
[HarmonyPatch(typeof(ModelDb), nameof(ModelDb.AllCharacters), MethodType.Getter)]
public static class CharactersPatch
{
    static void Postfix(ref IEnumerable<CharacterModel> __result)
    {
        __result = __result.Append(new MyCharacter()).Distinct();
    }
}

// 同样需要Patch AllCardPools, AllRelicPools, AllPotionPools
```

### 必需资源（从解包目录复制）
```
res://scenes/creature_visuals/<char_id>.tscn
res://images/packed/character_select/char_select_<char_id>.png
res://scenes/ui/character_icons/<char_id>_icon.tscn
res://scenes/combat/energy_counters/<char_id>_energy_counter.tscn
```

### 修复脚本检索
```csharp
// 在Initialize中调用
Godot.Bridge.ScriptManagerBridge.LookupScriptsInAssembly(Assembly.GetExecutingAssembly());
```

---

## 👾 敌怪（MonsterModel + EncounterModel）

### 怪物
```csharp
public sealed class MyMonster : MonsterModel
{
    public override int MinInitialHp => 30;
    public override int MaxInitialHp => 34;
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState attack = new MoveState("ATTACK", AttackMove, new SingleAttackIntent(8));
        attack.FollowUpState = attack;
        return new MonsterMoveStateMachine(new List<MonsterState> { attack }, attack);
    }
    
    private async Task AttackMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(8).FromMonster(this).Execute(null);
    }
}
```

### 遭遇
```csharp
public sealed class MyEncounter : EncounterModel
{
    public override RoomType RoomType => RoomType.Monster;
    public override bool IsWeak => true;
    
    public override List<MonsterModel> AllPossibleMonsters => new()
    {
        ModelDb.Monster<MyMonster>()
    };
    
    protected override List<(MonsterModel, string?)> GenerateMonsters()
    {
        return new() { (ModelDb.Monster<MyMonster>().ToMutable(), null) };
    }
}
```

### 注册遭遇
```csharp
[HarmonyPatch(typeof(Overgrowth), nameof(Overgrowth.GenerateAllEncounters))]
public static class EncountersPatch
{
    static void Postfix(ref IEnumerable<EncounterModel> __result)
    {
        __result = __result.Concat(new[] { ModelDb.Encounter<MyEncounter>() }).Distinct();
    }
}
```

---

## 🎭 事件（EventModel）

### 基本结构
```csharp
public class MyEvent : EventModel
{
    public override bool IsAllowed(RunState runState) => true;
    
    protected override List<EventOption> GenerateInitialOptions()
    {
        return new List<EventOption>
        {
            new EventOption(this, ActReward, InitialOptionKey("REWARD"))
        };
    }
    
    private async Task ActReward()
    {
        await PlayerCmd.GainGold(50, Owner);
        SetEventFinished(L10NLookup("MY_EVENT.pages.REWARD.description"));
    }
}
```

### 注册事件
```csharp
[HarmonyPatch(typeof(Overgrowth), nameof(Overgrowth.AllEvents), MethodType.Getter)]
public static class EventsPatch
{
    static void Postfix(ref IEnumerable<EventModel> __result)
    {
        __result = __result.Concat(new[] { ModelDb.Event<MyEvent>() }).Distinct();
    }
}
```

---

## ✨ 附魔（EnchantmentModel）

### 基本结构
```csharp
public sealed class MyEnchant : EnchantmentModel
{
    public override bool ShowAmount => true;
    public override bool HasExtraCardText => true;
    
    public override bool CanEnchantCardType(CardType type) => type == CardType.Attack;
    
    protected override List<DynamicVar> CanonicalVars => new() { new DamageVar(0m) };
    
    public override void RecalculateValues()
    {
        DynamicVars.Damage.BaseValue = Amount;
    }
    
    public override decimal EnchantDamageAdditive(decimal original, ValueProp props)
    {
        return Status == EnchantmentStatus.Disabled ? 0m : DynamicVars.Damage.BaseValue;
    }
}
```

### 添加附魔
```csharp
CardCmd.Enchant<MyEnchant>(card, 1m);
```

---

## 🌍 本地化键名规则

| 类型 | 键格式 | 文件 |
|------|--------|------|
| 卡牌 | `CARD_ID.title/description` | cards.json |
| 遗物 | `RELIC_ID.title/description/flavor` | relics.json |
| 药水 | `POTION_ID.title/description` | potions.json |
| 能力 | `POWER_ID.title/smartDescription` | powers.json |
| 事件 | `EVENT_ID.pages.INITIAL.options.OPT.title` | events.json |
| 角色 | `CHAR_ID.title/description` | characters.json |
| 怪物 | `MONSTER_ID.name/moves.STATE.title` | monsters.json |
| 遭遇 | `ENCOUNTER_ID.title/loss` | encounters.json |
| 附魔 | `ENCHANT_ID.title/description` | enchantments.json |
| 自定义词条/UI文本 | `keyword.title/description` 或 `ui.xxx` | card_keywords.json |

**ID转换规则**: `MyClassName` → `MY_CLASS_NAME`

---

## 🎨 UI选择页面本地化配置

### 核心原理

游戏仅加载特定名称的JSON本地化文件，自定义的 `ui_strings.json`、`engineer_choices.json` 等文件**不会被游戏自动识别**。自定义UI文本必须整合到游戏原生支持的本地化文件中，推荐使用 `card_keywords.json`。

### 游戏支持的本地化文件

| 文件 | 用途 |
|------|------|
| `cards.json` | 卡牌标题和描述 |
| `card_keywords.json` | 卡牌词条、自定义UI文本 |
| `powers.json` | 能力标题和描述 |
| `relics.json` | 遗物标题和描述 |
| `characters.json` | 角色标题和描述 |
| `monsters.json` | 怪物名称和动作 |
| `events.json` | 事件标题和选项 |
| `ancients.json` | 先古之民内容 |
| `modifiers.json` | 修饰词 |

### 实现步骤

#### 1. 在 card_keywords.json 中添加本地化键

```json
{
    "ui.card_select.title_multi": "请选择 1-{count} 张牌",
    "ui.card_select.title_single": "请选择单位",
    "ui.card_select.cost_label": "费用",
    "ui.card_select.price_label": "价格",
    "ui.production_queue.title": "请选择要启动或停止的生产序列",
    "ui.production_queue.cancel": "X 取消",
    "ui.production_queue.confirm": "确认选择",
    "ui.deploy_choice.title": "选择行动"
}
```

#### 2. 在UI类中添加 GetLocStringText 方法

```csharp
private string GetLocStringText(object? locStringObj)
{
    if (locStringObj == null) return string.Empty;
    if (locStringObj is string str) return str;

    System.Reflection.MethodInfo? rawMethod = locStringObj.GetType().GetMethod("GetRawText");
    if (rawMethod != null)
    {
        object? result = rawMethod.Invoke(locStringObj, null);
        if (result is string rawText && !string.IsNullOrEmpty(rawText))
        {
            return rawText;
        }
    }

    System.Reflection.MethodInfo? formatMethod = locStringObj.GetType().GetMethod("GetFormattedText");
    if (formatMethod != null)
    {
        try
        {
            object? result = formatMethod.Invoke(locStringObj, null);
            if (result is string formattedText && !string.IsNullOrEmpty(formattedText))
            {
                return formattedText;
            }
        }
        catch { }
    }

    string toString = locStringObj.ToString() ?? string.Empty;
    if (!toString.StartsWith("MegaCrit.Sts2.Core.Localization") && !toString.Contains("LocString"))
    {
        return toString;
    }

    return string.Empty;
}
```

#### 3. 在代码中使用 LocString

```csharp
// 简单文本
Text = GetLocStringText(new LocString("card_keywords", "ui.card_select.title_single"));

// 带动态变量的文本
var titleLocString = new LocString("card_keywords", "ui.card_select.title_multi");
titleLocString.Add("count", _maxSelection);
Text = GetLocStringText(titleLocString);

// 在 ChoiceOption 类中使用
public class ChoiceOption
{
    public object Title { get; set; } = string.Empty;
    public object Description { get; set; } = string.Empty;
}

// 创建选项时使用 LocString
new DeployChoiceScreen.ChoiceOption
{
    Title = new LocString("card_keywords", "ui.flak_track.deploy_title"),
    Description = new LocString("card_keywords", "ui.flak_track.deploy_desc")
}
```

### 命名空间约定

为了避免键名冲突，建议使用以下命名空间前缀：

| 前缀 | 用途 |
|------|------|
| `ui.card_select.xxx` | 卡牌选择界面 |
| `ui.production_queue.xxx` | 生产序列界面 |
| `ui.deploy_choice.xxx` | 部署选择界面 |
| `ui.chrono_warp.xxx` | 超时空传送界面 |
| `ui.flak_track.xxx` | 防空履带车选项 |
| `ui.tesla_trooper.xxx` | 磁暴步兵选项 |
| `engineer_choice.xxx` | 工程师选项 |

### 动态变量替换

LocString 支持动态变量，使用 `{变量名}` 格式：

```json
{
    "ui.card_select.title_multi": "请选择 1-{count} 张牌",
    "ui.guardian_gi.deploy_desc": "造成 {Damage} 点伤害，赋予 1 层易伤"
}
```

在代码中添加变量：

```csharp
var locString = new LocString("card_keywords", "ui.guardian_gi.deploy_desc");
locString.Add("Damage", DynamicVars.Damage.BaseValue);
Text = GetLocStringText(locString);
```

### 支持的 Add 方法重载

```csharp
locString.Add("name", decimal value);    // 数值
locString.Add("name", bool value);       // 布尔值
locString.Add("name", string value);     // 字符串
locString.Add("name", IList<string> value); // 字符串列表
locString.Add("name", LocString value);  // 嵌套本地化字符串
```

### 注意事项

1. **不要创建自定义JSON文件**：游戏不会自动加载非标准名称的本地化文件
2. **使用 object 类型**：ChoiceOption 的 Title 和 Description 应定义为 `object` 类型，同时支持 `string` 和 `LocString`
3. **统一使用 GetLocStringText**：所有显示文本的地方都应通过此方法处理
4. **中英文文件同步**：修改中文 `zhs/card_keywords.json` 后，必须同步修改英文 `eng/card_keywords.json`

---

## 🎮 控制台命令

```bash
card CARD_ID                    # 获得卡牌
addcard CARD_ID                 # 添加到卡组
relic RELIC_ID                  # 获得遗物
potion POTION_ID                # 获得药水
power POWER_ID AMOUNT TARGET    # 施加能力（0=玩家）
enchant ENCHANT_ID AMOUNT INDEX # 为手牌附魔
event EVENT_ID                  # 触发事件
ancient ANCIENT_ID              # 触发先古之民
fight ENCOUNTER_ID              # 进入遭遇战
```

---

## 🔑 关键API速查

### 伤害命令
```csharp
await DamageCmd.Attack(damage)
    .FromCard(card) / .FromMonster(monster) / .FromOsty(osty)
    .Targeting(creature) / .TargetingAllOpponents(state)
    .WithHitFx("vfx/path")
    .Execute(context);
```

### 能力命令
```csharp
await PowerCmd.Apply<PowerType>(target, amount, source, sourceCard);
await PowerCmd.Remove(powerInstance);
await PowerCmd.Decrement(powerInstance);
```

### 玩家命令
```csharp
await PlayerCmd.GainEnergy(amount, owner);
await PlayerCmd.GainGold(amount, owner);
await PlayerCmd.GainBlock(amount, owner);
```

### 生物命令
```csharp
await CreatureCmd.Damage(context, target, amount, props, source, card);
await CreatureCmd.Heal(target, amount);
await CreatureCmd.GainBlock(target, amount, props, card, fast);
```

### 卡牌命令
```csharp
await CardPileCmd.Add(card, PileType.Deck);
CardCmd.Enchant<EnchantType>(card, amount);
CardCmd.RemoveKeyword(card, CardKeyword.Exhaust);
```

### 遗物命令
```csharp
await RelicCmd.Obtain(relic.ToMutable(), owner);
```

---

## 🔗 联机同步（Multiplayer Sync）

### 设计理念

自定义UI面板（如卡牌选择、建筑出售、工程师选择等）在联机模式下必须确保同步，否则会导致客户端状态不一致（StateDivergence）。核心原则是：**仅本地玩家显示和操作面板，其他玩家等待结果同步**。

### MultiplayerSyncHelper 核心方法

```csharp
public static bool IsMultiplayerGame()
public static bool IsLocalPlayer(Player player)
public static Task<int?> ExecuteSyncChoice(Player player, Func<Task<int?>> localChoiceFunc)
public static Task<List<int>> ExecuteSyncMultiChoice(Player player, Func<Task<List<int>?>> localChoiceFunc)
```

### UI面板设计模式

#### 1. 基础显示方法（ShowSelection）

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

#### 2. 同步显示方法（ShowSelectionWithSync）

```csharp
public static async Task<int?> ShowSelectionWithSync(Player player, object title, List<ChoiceOption> options, FactionType faction = FactionType.Allied)
{
    return await MultiplayerSyncHelper.ExecuteSyncChoice(player, async () =>
    {
        return await ShowSelection(title, options, player, faction);
    });
}
```

### 单选同步（ExecuteSyncChoice）

适用于只需选择一个选项的场景（如工程师选择、超时空传送选择）：

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

### 多选同步（ExecuteSyncMultiChoice）

适用于可选择多个选项的场景（如出售建筑、生产序列管理）：

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

### 调用示例

在卡牌逻辑中使用同步方法：

```csharp
protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
{
    // 获取建筑能力列表
    var buildingPowerItems = GetBuildingPowerItems(Owner.Creature);
    
    // 使用同步方法显示面板
    List<int> selectedIndices = await SellBuildingScreen.ShowSelectionWithSync(
        buildingPowerItems, maxSelection, Owner, faction);
    
    // 处理选择结果
    foreach (int index in selectedIndices)
    {
        var item = buildingPowerItems[index];
        // ... 执行出售逻辑
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
| **单例面板** | 同一类型的面板在同步时应保证只有一个实例 |

### 已实现同步的面板

| 面板类 | 同步方法 | 同步类型 |
|--------|---------|---------|
| `CardSelectionScreen` | `ShowSelectionWithSync` | 单选 |
| `CardSelectionSyncHelper` | `ShowMultiSelectionWithSync` | 多选 |
| `SellBuildingScreen` | `ShowSelectionWithSync` | 多选 |
| `ProductionQueueSelectionScreen` | `ShowSelectionWithSync` | 多选 |
| `EngineerChoiceScreen` | `ShowSelectionWithSync` | 单选 |
| `DeployChoiceScreen` | `ShowSelectionWithSync` | 单选 |
| `ChronoWarpScreen` | `ShowPileSelectionWithSync` | 单选 |

---

## 🚀 卡牌存储与消耗机制（IFV / 步兵车系列）

### IFV 存储普通士兵的消耗逻辑

| IFV 状态 | 操作 | 存储卡牌去向 | IFV 去向 |
|----------|------|-------------|----------|
| 无消耗词条 | 攻击（抽牌弃牌+格挡） | 存储 | 弃牌堆 |
| 无消耗词条 | 部署（释放存储） | 士兵返回手牌 | 弃牌堆 |
| 有消耗词条 | 攻击（抽牌弃牌+格挡） | 无 | 消耗堆 |
| 有消耗词条 | 部署（释放存储） | 士兵返回手牌 | 消耗堆 |

> **关键实现**：通过 `GetPlayTargetPile()` 方法根据 `CardKeyword.Exhaust` 词条动态决定卡牌去向。
> 存储士兵时 `CanonicalKeywords` 动态添加 `CardKeyword.Exhaust`，使卡牌显示消耗视觉指示器。

### IFV 存储特殊士兵 → 特殊步兵车的转化机制

#### 一、转化总览

IFV 在部署时识别手牌中的**特殊士兵卡牌**，转化为对应的**特殊步兵车卡牌**。步兵车继承 IFV 的消耗词条，拥有两条行动路径。

| 特殊士兵 | 转化结果 | 特殊效果 |
|---------|---------|---------|
| 工程师 (Engineer) | 维修车 (RepairVehicle) | 维修：赋予手牌卡牌Replay |
| (待扩展) 磁暴步兵 → 磁暴步兵车 |
| (待扩展) 海豹突击队 → 海豹步兵车 |
| (待扩展) 尤里新兵 → 尤里步兵车 |
| ... | ... |

#### 二、转化流程

```
IFV部署 → 选择士兵
    ↓
判断是否为特殊士兵类型
    ├── 普通士兵 → 存储（IFV获得Exhaust词条，可释放）
    └── 特殊士兵 → 转化流程：
        1. IFV自身携带的Exhaust状态 → 传递给新步兵车
        2. 从战斗中移除 IFV + 特殊士兵
        3. 创建新的步兵车卡牌（携带存储的IFV和士兵）
        4. 步兵车加入手牌，继承消耗词条
```

#### 三、步兵车通用消耗规则

| IFV 状态 | 步兵车获得 | 路径A（特殊效果） | 路径B（释放） |
|----------|-----------|-----------------|-------------|
| 无消耗词条 | 步兵车(无消耗) | 执行效果 → 步兵车进弃牌堆 | 释放士兵到手牌+IFV进弃牌堆+步兵车消耗 |
| 有消耗词条 | 步兵车(有消耗) | 执行效果 → IFV+士兵+步兵车全消耗 | 释放士兵到手牌+IFV消耗+步兵车消耗 |

#### 四、核心机制详解

##### 1. 消耗词条继承（关键）

**问题**：`CardModel.LocalKeywords` 会缓存 `CanonicalKeywords` 的结果。首次访问后，即使 `CanonicalKeywords` 返回值变化，UI 读取的仍是旧缓存。

**正确做法**：使用 `AddKeyword(CardKeyword.Exhaust)` 动态修改关键词缓存：

```csharp
// ❌ 错误：CanonicalKeywords 动态返回值不会刷新缓存
public override IEnumerable<CardKeyword> CanonicalKeywords
{
    get
    {
        var keywords = new List<CardKeyword>();
        if (_inheritedExhaust)
            keywords.Add(CardKeyword.Exhaust);
        return keywords;
    }
}

// ✅ 正确：用 AddKeyword 直接修改缓存的 _keywords 集合
public void SetStoredCards(CardModel ifvCard, CardModel soldierCard, bool inheritedExhaust = false)
{
    _storedCards.Clear();
    _storedCards.Add(ifvCard);
    _storedCards.Add(soldierCard);
    _hasStored = true;
    _inheritedExhaust = inheritedExhaust;

    if (inheritedExhaust)
    {
        AddKeyword(CardKeyword.Exhaust);  // 直接修改缓存+触发UI刷新
    }
}
```

##### 2. 克隆状态保留

`DeepCloneFields` 必须复制/重置所有动态状态字段：

```csharp
protected override void DeepCloneFields()
{
    base.DeepCloneFields();
    _storedCards = new List<CardModel>(_storedCards);
    _hasStored = false;        // 重置：克隆体不应继承存储状态
    _inheritedExhaust = false; // 重置：由 SetStoredCards 重新设置
}
```

> 注意：`_keywords` 的克隆由 `base.DeepCloneFields()` 负责（基类已正确实现）。

##### 3. 双路径消耗逻辑

```csharp
// 路径A：执行特殊效果
if (_inheritedExhaust && _hasStored)
{
    // 消耗 IFV + 士兵 + 步兵车（全进消耗堆）
    await CardPileCmd.Add(engineerCard, PileType.Exhaust, ...);
    await CardPileCmd.Add(ifvCard, PileType.Exhaust, ...);
    await CardPileCmd.Add(this, PileType.Exhaust, ...);
}
else
{
    // 无消耗：步兵车进弃牌堆
    await CardPileCmd.Add(this, PileType.Discard, ...);
}

// 路径B：释放存储
// 士兵 → 始终返回手牌
await CardPileCmd.Add(soldierCard, PileType.Hand, ...);
// IFV → 消耗堆(有消耗) 或 弃牌堆(无消耗)
var ifvTargetPile = _inheritedExhaust ? PileType.Exhaust : PileType.Discard;
await CardPileCmd.Add(ifvCard, ifvTargetPile, ...);
// 步兵车 → 始终消耗
await CardPileCmd.Add(this, PileType.Exhaust, ...);
```

#### 五、新增步兵车卡牌模板（基类方案）

由于所有步兵车共享相同的存储/消耗继承/双路径释放逻辑，推荐使用**基类模式**消除重复代码。

##### Step 0：创建 IfvVehicleBase 基类

```csharp
// Allies/Cards/IfvVehicleBase.cs
public abstract class IfvVehicleBase : CardModel
{
    protected List<CardModel> _storedCards = new();
    protected bool _hasStored;
    protected bool _inheritedExhaust;

    protected IfvVehicleBase(int cost, CardType type, CardRarity rarity, TargetType target)
        : base(cost, type, rarity, target) { }

    protected override IEnumerable<CardKeyword> CanonicalKeywords => Array.Empty<CardKeyword>();

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new IntVar("ReplayCount", 1),
        new StringVar("StoredCards"),
        new IntVar("StoreCount", 1)
    };

    protected override void DeepCloneFields()
    {
        base.DeepCloneFields();
        _storedCards = new List<CardModel>(_storedCards);
        _hasStored = false;
        _inheritedExhaust = false;
    }

    public void SetStoredCards(CardModel ifvCard, CardModel soldierCard, bool inheritedExhaust = false)
    {
        _storedCards.Clear();
        _storedCards.Add(ifvCard);
        _storedCards.Add(soldierCard);
        _hasStored = true;
        _inheritedExhaust = inheritedExhaust;

        if (inheritedExhaust)
        {
            AddKeyword(CardKeyword.Exhaust);
        }

        var storedText = new LocString("cards", $"{CardId}.stored_info");
        storedText.Add("0", soldierCard.Title);
        ((StringVar)DynamicVars["StoredCards"]).StringValue = GetLocStringText(storedText);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var options = new List<DeployChoiceScreen.ChoiceOption>
        {
            new()
            {
                Id = "attack",
                Title = new LocString("card_keywords", $"{CardId}.attack_title"),
                Description = new LocString("card_keywords", $"{CardId}.attack_desc"),
                IconPath = "res://RedAlert2ModResources/images/ui/attack.png"
            },
            new()
            {
                Id = "deploy",
                Title = new LocString("card_keywords", $"{CardId}.deploy_title"),
                Description = new LocString("card_keywords", $"{CardId}.stored_deploy_desc"),
                IconPath = "res://RedAlert2ModResources/images/ui/deploy.png"
            }
        };

        var selectedIndex = await DeployChoiceScreen.ShowSelectionWithSync(
            Owner, new LocString("card_keywords", $"{CardId}.title"), options, FactionType.Allied);

        if (selectedIndex.HasValue)
        {
            if (options[selectedIndex.Value].Id == "attack")
                await ExecuteEffect(ctx, play);
            else
                await ExecuteDeployRelease(ctx, play);
        }
    }

    // 子类实现：特殊效果（如维修、磁暴等）
    protected abstract Task ExecuteEffect(PlayerChoiceContext ctx, CardPlay play);

    // 通用释放逻辑（所有步兵车相同）
    protected async Task ExecuteDeployRelease(PlayerChoiceContext ctx, CardPlay play)
    {
        if (!_hasStored || _storedCards.Count == 0)
        {
            await CardPileCmd.Add(this, PileType.Exhaust, CardPilePosition.Bottom, this);
            return;
        }

        await ReleaseStoredCards();
        await CardPileCmd.Add(this, PileType.Exhaust, CardPilePosition.Bottom, this);
    }

    protected async Task ReleaseStoredCards()
    {
        var ifvCard = _storedCards[0];
        var soldierCard = _storedCards[1];

        soldierCard.HasBeenRemovedFromState = false;
        await CardPileCmd.Add(soldierCard, PileType.Hand, CardPilePosition.Bottom, this);

        ifvCard.HasBeenRemovedFromState = false;
        var ifvTargetPile = _inheritedExhaust ? PileType.Exhaust : PileType.Discard;
        await CardPileCmd.Add(ifvCard, ifvTargetPile, CardPilePosition.Bottom, this);

        _storedCards.Clear();
        _hasStored = false;
        ((StringVar)DynamicVars["StoredCards"]).StringValue = string.Empty;
    }

    protected string CardId => GetType().Name.ToUpperInvariant();

    protected static string GetLocStringText(object locStringObj)
    {
        if (locStringObj == null) return string.Empty;
        if (locStringObj is string str) return str;
        var method = locStringObj.GetType().GetMethod("GetFormattedText", Type.EmptyTypes);
        if (method != null)
        {
            try
            {
                var result = method.Invoke(locStringObj, null);
                if (result is string text && !string.IsNullOrEmpty(text)) return text;
            }
            catch { }
        }
        return string.Empty;
    }
}
```

##### Step 1：IFV 中添加特殊士兵识别

在 `Ifv.ExecuteDeploy` 中添加新的类型判断分支：

```csharp
if (selectedCard is AlliesEngineer or SovietEngineer)
{
    await VehicleDeployHelper.DeploySpecialVehicle<RepairVehicle>(ctx, this, selectedCard, Owner);
    return;
}
// 新增：磁暴步兵 → 磁暴步兵车
if (selectedCard is TeslaTrooper or SovietTeslaTrooper)
{
    await VehicleDeployHelper.DeploySpecialVehicle<TeslaVehicle>(ctx, this, selectedCard, Owner);
    return;
}
```

**定时炸弹关键词检测**：除了类型判断，还支持通过 `TimedBombManager.HasTimedBombEffect()` 检测被伊文部署功能添加了"定时炸弹"词条的任意卡牌，转化为自爆步兵车（DemoVehicle）：

```csharp
// 定时炸弹检测：关键词（任意被伊文部署的卡）或类型（炸弹单位本身）
if (TimedBombManager.HasTimedBombEffect(selectedCard)
    || selectedCard is TerrorMan or CrazyIvanCard or ChronoIvanCard)
{
    await VehicleDeployHelper.DeploySpecialVehicle<DemoVehicle>(ctx, this, selectedCard, Owner);
    return;
}
```

> **防空履带车**也支持同样的定时炸弹转化逻辑，在 `ExecuteDeploy` 的卡牌选择后检测：
> ```csharp
> var timedBombCard = selectedCards.FirstOrDefault(c =>
>     TimedBombManager.HasTimedBombEffect(c)
>     || c is TerrorMan or CrazyIvanCard or ChronoIvanCard);
> if (timedBombCard != null)
> {
>     await VehicleDeployHelper.DeploySpecialVehicle<DemoVehicle>(ctx, this, timedBombCard, Owner);
>     return;
> }
> ```

##### Step 2：使用公共转化工具 VehicleDeployHelper

转化逻辑已提取到 `Common/Utils/VehicleDeployHelper.cs`，IFV 和防空履带车共用：

```csharp
// 调用方式（TVehicle 必须继承 IfvVehicleBase）
await VehicleDeployHelper.DeploySpecialVehicle<TVehicle>(
    ctx,        // 玩家选择上下文
    sourceCard, // 源卡牌（IFV 或 防空履带车）
    soldierCard,// 被存储的士兵卡牌
    Owner);     // 拥有者
```

工具类内部处理：
1. 创建转化后的载具卡（`CreateCard`）
2. 源卡牌升级则载具也升级（`CardCmd.Upgrade`）
3. 继承消耗词条（源卡牌或士兵卡牌任意一方有消耗则继承）
4. 移除士兵卡和源卡，将载具卡加入手牌

##### Step 3：创建新步兵车（继承基类，只需 ~30 行）

```csharp
[RegisterCard(typeof(AlliesCardPool))]
public sealed class TeslaVehicle : IfvVehicleBase
{
    public TeslaVehicle() : base(1, CardType.Skill, CardRarity.Token, TargetType.Self) { }

    public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/allies/tesla_vehicle.png";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ModCardKeywords.TechLevelT2.CreateHoverTip(),
        ModCardKeywords.Vehicle.CreateHoverTip(),
        ModCardKeywords.Deploy.CreateHoverTip(),
        ModCardKeywords.Unit.CreateHoverTip(),
        HoverTipFactory.Static(StaticHoverTip.ReplayStatic)
    ];

    protected override async Task ExecuteEffect(PlayerChoiceContext ctx, CardPlay play)
    {
        // 特殊效果：赋予Replay给选中的手牌单位卡
        // ... 具体效果逻辑 ...

        // 消耗判断（与RepairVehicle一致）
        if (_inheritedExhaust && _hasStored)
        {
            var ifvCard = _storedCards[0];
            var soldierCard = _storedCards[1];
            soldierCard.HasBeenRemovedFromState = false;
            await CardPileCmd.Add(soldierCard, PileType.Exhaust, CardPilePosition.Bottom, this);
            ifvCard.HasBeenRemovedFromState = false;
            await CardPileCmd.Add(ifvCard, PileType.Exhaust, CardPilePosition.Bottom, this);
            _storedCards.Clear();
            _hasStored = false;
            ((StringVar)DynamicVars["StoredCards"]).StringValue = string.Empty;
            await CardPileCmd.Add(this, PileType.Exhaust, CardPilePosition.Bottom, this);
        }
        else
        {
            await CardPileCmd.Add(this, PileType.Discard, CardPilePosition.Bottom, this);
        }
    }
}
```

> **注意**：`ExecuteEffect` 中的消耗逻辑虽然在基类中已有释放版本，但因每个步兵车的"效果路径"结束后的消耗行为可能不同（有的全消耗、有的仅步兵车进弃牌堆），所以仍由子类实现。可进一步抽取 `ExecuteEffectConsumption()` 辅助方法。

##### Step 4：注册到卡池

在 `AlliesCardPool.GenerateAllCards()` 或 `AlliedCardRegistry` 中注册新卡牌。

##### Step 5：本地化

在 4 语言的 `cards.json` 和 `card_keywords.json` 中添加对应条目。

```json
// card_keywords.json 示例
{
    "ui.tesla_vehicle.title": "选择磁暴步兵车的行动",
    "ui.tesla_vehicle.attack_title": "磁暴",
    "ui.tesla_vehicle.attack_desc": "赋予手牌中单位卡Replay",
    "ui.tesla_vehicle.deploy_title": "部署",
    "ui.tesla_vehicle.stored_deploy_desc": "释放驻扎的士兵"
}
```

#### 六、关键代码位置

| 文件 | 功能 |
|------|------|
| `Allies/Cards/Ifv.cs` | IFV 卡牌逻辑，特殊士兵识别与转化 |
| `Allies/Cards/IfvVehicleBase.cs` | 步兵车基类，封装存储/消耗/释放/双路径逻辑 |
| `Allies/Cards/RepairVehicle.cs` | 维修车（参考模板），继承基类实现特殊效果 |
| `Allies/Cards/AlliesCardValues.cs` | 数值配置 |
| `localization/*/card_keywords.json` | UI 选项本地化 |

#### 七、新增特殊士兵检查清单

在添加新步兵车前，确保：

- [ ] 特殊士兵类型已注册到对应阵营 `CardRegistry` 的士兵列表（`Soldiers`/`RadarSoldiers`/`HighTechSoldiers`）
- [ ] IFV 的 `GetSoldierCardsFromHand()` 能识别该士兵类型
- [ ] 已创建 `IfvVehicleBase` 子类并注册到卡池
- [ ] 已完成 4 语言本地化（`cards.json` + `card_keywords.json`）
- [ ] 卡牌肖像图片已放置到正确路径

---

### 无士兵时的降级行为

当 IFV 选择部署但手牌无士兵卡牌时，卡牌**正常打出**（进弃牌堆/消耗堆）而非返回手牌：

```csharp
if (soldierCards.Count == 0)
{
    await CardPileCmd.Add(this, GetPlayTargetPile(), CardPilePosition.Bottom, this);
    return;
}
```

---

## ✨ 攻击特效（VFX）

### 特效类型速查

| 特效名称 | 路径 | 说明 |
|---------|------|------|
| 斩击 | `vfx/vfx_attack_slash` | 普通斩击特效 |
| 钝击 | `vfx/vfx_attack_blunt` | 钝器攻击特效 |
| 突刺 | `vfx/vfx_attack_stab` | 刺击特效 |
| 闪电 | `vfx/vfx_attack_lightning` | 闪电攻击特效 |
| 火焰 | `vfx/vfx_attack_fire` | 火焰攻击特效 |
| 冰霜 | `vfx/vfx_attack_frost` | 冰霜攻击特效 |
| 毒素 | `vfx/vfx_attack_poison` | 毒素攻击特效 |
| 烟雾 | `vfx/vfx_smoke_puff` | 烟雾特效 |

### 在伤害命令中使用特效

```csharp
await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
    .FromCard(this)
    .Targeting(cardPlay.Target)
    .WithHitFx("vfx/vfx_attack_slash")  // 添加攻击特效
    .Execute(choiceContext);
```

### 常用VFX节点类

```csharp
// 刺击特效
var stabVfx = NStabVfx.Create(target, goingRight: true);
NCombatRoom.Instance?.CombatVfxContainer.AddChild(stabVfx);

// 斩击特效
var slashVfx = NSlashVfx.Create(target, goingRight: true);
NCombatRoom.Instance?.CombatVfxContainer.AddChild(slashVfx);

// 火焰燃烧特效
var fireVfx = NFireBurningVfx.Create(target, duration: 1.5f, goingRight: true);
NCombatRoom.Instance?.CombatVfxContainer.AddChild(fireVfx);

// 毒药冲击特效
var poisonVfx = NPoisonImpactVfx.Create(target, goingRight: true);
NCombatRoom.Instance?.CombatVfxContainer.AddChild(poisonVfx);
```

### 创建自定义特效场景

```gdscript
# vfx_my_custom_attack.tscn
[gd_scene load_steps=2 format=3]

[ext_resource type="Texture2D" path="res://images/vfx/my_custom_attack.png" id="1"]

[node name="MyCustomVfx" type="Node2D"]
script = ExtResource("2")

[node name="Sprite" type="Sprite2D" parent="."]
texture = ExtResource("1")
centered = false
```

### 特效资源路径

```
res://scenes/vfx/vfx_<特效名称>.tscn
res://images/vfx/<特效名称>_00-03.png
res://images/atlases/vfx_atlas.sprites/<特效名称>.tres
```

---

## ⚠️ 常见问题

### 📋 游戏日志路径
遇到问题时，第一个要查看的就是游戏日志：

**日志位置**：
```
C:\Users\<你的用户名>\AppData\Roaming\SlayTheSpire2\logs\
```

**关键文件**：`godot.log` - 包含所有启动和运行时错误信息

---

### Mod文件无法加载 - 致命错误
**症状**：游戏启动时报错找不到DLL或PCK文件

**原因**：JSON中的 `"id"` 字段必须与文件名完全一致！

```json
// RedAlert2Mod.json
{
  "id": "RedAlert2Mod",  // ← 这个ID
  ...
}
```

**必须确保三个文件同名**：
- `RedAlert2Mod.json` （ID必须匹配）
- `RedAlert2Mod.dll`
- `RedAlert2Mod.pck`

如果ID是 `"Ra2Mod"`，文件名必须是 `Ra2Mod.json/dll/pck`，否则游戏找不到文件！

**解决步骤**：
1. 打开 `godot.log` 查看具体报错
2. 检查JSON中的 `"id"` 字段
3. 确保三个文件名完全一致
4. 重新复制到游戏mods文件夹

###  资源不显示（场景文件缺少Texture）
**症状**：角色场景文件已创建，但游戏中图标/立绘/背景显示为空白

**原因**：Godot场景文件中的 `Sprite2D` 节点没有设置 `texture` 属性

**解决方案**：
1. 在Godot中打开 `.tscn` 场景文件
2. 选中 `Sprite2D` 节点（如 `Visuals`、`Bg`、`CharacterIconCharName`）
3. 在右侧检查器找到 **Texture** 属性
4. 点击下拉箭头 → **快速加载** → 选择对应的图片资源
5. 保存场景并重新导出PCK

**示例场景文件**：
```gdscript
# allies.tscn - 角色立绘场景
[node name="Visuals" type="Sprite2D" parent="."]
texture = ExtResource("1_allies")  # ← 必须有这行！

[ext_resource type="Texture2D" path="res://images/character/allies_character.png" id="1_allies"]
```

### PlatformNotSupportedException
修改 `GodotPlugins.runtimeconfig.json` 中的 version 为 `9.0.0`

### 脚本无法找到
```csharp
Godot.Bridge.ScriptManagerBridge.LookupScriptsInAssembly(Assembly.GetExecutingAssembly());
```

### 资源不显示（其他原因）
- 检查路径大小写
- 确认已导出PCK
- 验证裁切纹理存在

### Mod未加载
- 检查三个文件同名且同目录
- 验证JSON中has_pck/has_dll与实际相符

---

## 🏛️ 阵营架构设计

### 设计理念
为了管理红警2中大量的单位卡牌，采用阵营分类架构：盟军、苏军、尤里、其他四大阵营，每个阵营包含：
- **单位卡**：士兵、装甲、飞机、船只
- **建筑卡**：兵营、重工、防御建筑等
- **技能卡**：用于卡组构造的特殊卡牌

### 卡牌注册管理器（CardRegistry）

每个阵营都有对应的 `CardRegistry` 类，用于统一管理和批量获取卡牌：

```csharp
public static class AlliedCardRegistry
{
    // 单位卡分类
    public static List<Func<CardModel>> Soldiers { get; } = new()
    {
        () => ModelDb.Card<AmericanSoldier>(),
        () => ModelDb.Card<DogSoldier>(),
        () => ModelDb.Card<RocketSoldier>(),
        () => ModelDb.Card<Engineer>()
    };

    public static List<Func<CardModel>> Vehicles { get; } = new()
    {
        () => ModelDb.Card<GrizzlyTank>(),
        () => ModelDb.Card<Ifv>()
    };

    // 获取所有士兵卡
    public static List<CardModel> GetAllSoldiers()
    {
        return Soldiers.Select(s => s()).ToList();
    }

    // 根据玩家创建卡牌实例
    public static List<CardModel> CreateSoldiers(Player owner)
    {
        return Soldiers.Select(s => owner.Creature.CombatState.CreateCard(s(), owner)).ToList();
    }
}
```

### 架构优势

**1. 统一导入管理**
```csharp
// 在兵营卡牌中直接获取所有士兵单位
List<CardModel> availableCards = AlliedCardRegistry.CreateSoldiers(Owner);
```

**2. 模块化扩展**
- 新增单位只需在对应阵营的 `CardRegistry` 中注册
- 建筑卡可以轻松获取特定类型的单位卡

**3. 类型安全**
- 使用 `Func<CardModel>` 延迟初始化，避免过早创建实例
- 编译时类型检查，减少运行时错误

### 阵营目录结构

```
RedAlert2ModCode/
├── Allies/           # 盟军阵营
│   ├── Cards/        # 卡牌定义
│   ├── Powers/       # 能力定义
│   ├── UI/           # UI组件
│   ├── AlliedCardRegistry.cs   # 卡牌注册管理器
│   └── AlliesCardPool.cs       # 卡池
├── Soviet/           # 苏军阵营
│   └── SovietCardRegistry.cs
├── Yuri/             # 尤里阵营
│   └── YuriCardRegistry.cs
└── Other/            # 其他阵营
    └── OtherCardRegistry.cs
```

### 卡牌分类层级结构

> **注意**：以下"建筑卡""单位卡""技能卡"等分类**仅适用于红警2 Mod**，与杀戮尖塔2原生的"攻击卡(Attack)""技能卡(Skill)""能力卡(Power)"没有直接关联。红警Mod的卡牌类型是在游戏原生机制之上的自定义分类。

> **设计说明**：由于没有机场的阵营，空军单位由重工来生产。

```
四大阵营
├── 盟军 (Allies)
│   ├── 建筑卡 (BuildingCards)
│   │   ├── 建筑：兵营、盟军重工、盟军高科、盟军基地车
│   │   ├── 防御：围墙、爱国者飞弹、光棱塔
│   │   └── 其他：油井、科技前哨站
│   ├── 单位卡 (UnitCards)
│   │   ├── 士兵：美国大兵、警犬、火箭兵、工程师
│   │   ├── 装甲：灰熊坦克、IFV步兵战车
│   │   ├── 飞机：黑鹰战机、入侵者战机
│   │   └── 船只：航空母舰、驱逐舰、海豚
│   └── 技能卡 (PowerCards)
│       └── 独创用于卡组构造的卡牌
├── 苏军 (Soviet)
│   ├── 建筑卡 (BuildingCards)
│   │   ├── 建筑：兵营、苏军重工、苏军高科、苏军基地车
│   │   ├── 防御：围墙、哨戒炮、磁暴线圈
│   │   └── 其他：油井、核弹发射井
│   ├── 单位卡 (UnitCards)
│   │   ├── 士兵：动员兵、军犬、磁暴步兵、工程师
│   │   ├── 装甲：犀牛坦克、防空履带车
│   │   ├── 飞机：米格战机
│   │   └── 船只：无畏级战舰、台风潜艇
│   └── 技能卡 (PowerCards)
│       └── 独创用于卡组构造的卡牌
├── 尤里 (Yuri)
│   ├── 建筑卡 (BuildingCards)
│   │   ├── 建筑：兵营、尤里重工、尤里高科、尤里基地车
│   │   ├── 防御：围墙、心灵控制塔、尤里雕像
│   │   └── 其他：油井、基因突变器、心灵控制器
│   ├── 单位卡 (UnitCards)
│   │   ├── 士兵：尤里新兵、狂兽人、心灵突击队、工程师
│   │   ├── 装甲：狂风坦克、盖特机炮坦克
│   │   ├── 飞机：镭射幽浮
│   │   └── 船只：雷鸣潜艇
│   └── 技能卡 (PowerCards)
│       └── 独创用于卡组构造的卡牌
└── 其他 (Other)
    ├── 建筑卡 (BuildingCards)
    │   ├── 建筑：特殊建筑
    │   ├── 防御：特殊防御
    │   └── 其他：中立建筑、特殊设施
    ├── 单位卡 (UnitCards)
    │   ├── 士兵：特殊步兵（如古巴恐怖分子）
    │   ├── 装甲：特殊载具
    │   ├── 飞机：特殊战机
    │   └── 船只：特殊舰艇
    └── 技能卡 (PowerCards)
        └── 独创用于卡组构造的卡牌
```

### CardRegistry 分类字段对应表

| 分类层级 | CardRegistry 字段 | 说明 |
|---------|------------------|------|
| 单位卡-士兵 | `Soldiers` | 步兵单位 |
| 单位卡-装甲 | `Vehicles` | 坦克、战车 |
| 单位卡-飞机 | `Aircraft` | 空军单位 |
| 单位卡-船只 | `Ships` | 海军单位 |
| 建筑卡 | `BuildingCards` | 建筑、防御、其他建筑 |
| 技能卡 | `PowerCards` | 卡组构造卡牌 |
| 特殊卡 | `SpecialCards` | 其他特殊卡牌 |

> **重要说明**：由于苏联没有机场（空指部），苏军的空军单位（如基洛夫飞艇）由重工来生产。因此在 `SovietCardRegistry` 中，基洛夫注册在 `Vehicles` 列表中，而非 `Aircraft` 列表。

---

## � 公共卡牌架构

### 设计背景

红警2中有一些公共建筑和技能（如油井、黄金矿、伞兵等），这些卡牌在盟军和苏军中都存在，逻辑完全相同，但游戏UI需要区分阵营（卡框颜色等）。由于游戏的本地化系统使用类名自动生成 key（`MyClassName` → `MY_CLASS_NAME`），且卡牌实例是单例模式（`ModelDb.Card<T>()` 返回同一实例），直接共用一份卡牌会导致：

1. **卡框颜色问题**：第一个注册的阵营会决定卡框颜色，另一个阵营的卡框会继承错误颜色
2. **本地化问题**：两个阵营使用同一本地化key，无法独立控制
3. **实例冲突**：单例模式导致两个阵营共享同一实例状态

### 方案一：继承分离模式（传统方案）

采用"公共基类 + 阵营子类"的架构，实现逻辑共用但实例分离：

```
Common/Cards/
├── GoldMineCard.cs          # 公共基类 - 包含完整逻辑
├── OilDerrickCard.cs        # 公共基类 - 包含完整逻辑
├── SellMCV.cs               # 公共基类 - 包含完整逻辑
├── Ra2Rally.cs              # 公共基类 - 包含完整逻辑
├── Paratrooper.cs           # 公共基类 - 包含完整逻辑
├── StopProductionCard.cs    # 公共基类 - 包含完整逻辑
├── GemMineCard.cs           # 公共基类 - 包含完整逻辑
└── GoldMineColumnCard.cs    # 公共基类 - 包含完整逻辑

Allies/Cards/
├── AlliesGoldMineCard.cs    # 盟军子类 - 仅继承，无逻辑
├── AlliesOilDerrickCard.cs  # 盟军子类 - 仅继承，无逻辑
└── ...                      # 其他盟军公共卡牌

Soviet/Cards/
├── SovietGoldMineCard.cs    # 苏军子类 - 仅继承，无逻辑
├── SovietOilDerrickCard.cs  # 苏军子类 - 仅继承，无逻辑
└── ...                      # 其他苏军公共卡牌
```

**公共基类**（[Common/Cards/GoldMineCard.cs](file:///d:/RedAlert2Project/red-alert-2-mod/RedAlert2ModCode/Common/Cards/GoldMineCard.cs)）：

```csharp
public class GoldMineCard : CardModel
{
    private static readonly CardValueStore.CardValues Values = CommonCardValues.GoldMine;
    
    public GoldMineCard() : base((int)Values.Cost, CardType.Power, CardRarity.Common, TargetType.Self) { }

    public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/gold_mine.png";

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new IntVar("Reserve", Values.DollarValue)
    };

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        // 完整的卡牌逻辑
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

**阵营子类**（[Allies/Cards/AlliesGoldMineCard.cs](file:///d:/RedAlert2Project/red-alert-2-mod/RedAlert2ModCode/Allies/Cards/AlliesGoldMineCard.cs)）：

```csharp
using RedAlert2ModCode.Common.Cards;

namespace RedAlert2ModCode.Allies.Cards;

public sealed class AlliesGoldMineCard : GoldMineCard
{
}
```

**阵营子类**（[Soviet/Cards/SovietGoldMineCard.cs](file:///d:/RedAlert2Project/red-alert-2-mod/RedAlert2ModCode/Soviet/Cards/SovietGoldMineCard.cs)）：

```csharp
using RedAlert2ModCode.Common.Cards;

namespace RedAlert2ModCode.Soviet.Cards;

public sealed class SovietGoldMineCard : GoldMineCard
{
}
```

**本地化规则**（两份本地化条目）：

| 公共卡牌 | 盟军本地化key | 苏军本地化key |
|---------|-------------|-------------|
| 黄金矿 | `ALLIES_GOLD_MINE_CARD.title` | `SOVIET_GOLD_MINE_CARD.title` |
| 宝石矿 | `ALLIES_GEM_MINE_CARD.title` | `SOVIET_GEM_MINE_CARD.title` |
| 黄金矿柱 | `ALLIES_GOLD_MINE_COLUMN_CARD.title` | `SOVIET_GOLD_MINE_COLUMN_CARD.title` |
| 油井 | `ALLIES_OIL_DERRICK_CARD.title` | `SOVIET_OIL_DERRICK_CARD.title` |
| 卖本 | `ALLIES_SELL_MC_V.title` | `SOVIET_SELL_MC_V.title` |
| 集结 | `ALLIES_RA2_RALLY.title` | `SOVIET_RA2_RALLY.title` |
| 伞兵 | `ALLIES_PARATROOPER.title` | `SOVIET_PARATROOPER.title` |
| 停产 | `ALLIES_STOP_PRODUCTION_CARD.title` | `SOVIET_STOP_PRODUCTION_CARD.title` |

**注册方式**：

```csharp
// AlliedCardRegistry.cs
cards.Add(() => ModelDb.Card<AlliesGoldMineCard>());

// SovietCardRegistry.cs
cards.Add(() => ModelDb.Card<SovietGoldMineCard>());
```

### 方案二：Pool动态切换模式（推荐方案）

参考"海克斯符文"mod的"白洞"卡牌实现，通过重写 `Pool` 和 `VisualCardPool` 属性，让同一公共卡牌实例根据当前持有者动态切换阵营颜色。此方案更简洁，无需创建阵营子类。

**核心原理**：

1. **颜色切换**：重写 `Pool` 属性，当卡牌有Owner时返回Owner的阵营卡池，否则返回 `TokenCardPool`（白色/无色）
2. **本地化简化**：只需要一份不带阵营前缀的本地化键，百科和游戏中都使用相同的key

**公共卡牌实现**（[Common/Cards/OilDerrickCard.cs](file:///d:/RedAlert2Project/red-alert-2-mod/RedAlert2ModCode/Common/Cards/OilDerrickCard.cs)）：

```csharp
using MegaCrit.Sts2.Core.Models.CardPools;

public class OilDerrickCard : CardModel
{
    private static readonly CardValueStore.CardValues Values = CommonCardValues.OilDerrick;

    public OilDerrickCard() : base((int)Values.Cost, CardType.Power, CardRarity.Common, TargetType.Self) { }

    public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/oil_derrick.png";

    public override CardPoolModel Pool => IsMutable && Owner != null
        ? Owner.Character.CardPool
        : ModelDb.CardPool<TokenCardPool>();

    public override CardPoolModel VisualCardPool => Pool;

    // ... 卡牌逻辑 ...
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

**本地化规则**（仅需一份）：

| 公共卡牌 | 本地化key |
|---------|-----------|
| 黄金矿 | `GOLD_MINE_CARD.title` |
| 宝石矿 | `GEM_MINE_CARD.title` |
| 黄金矿柱 | `GOLD_MINE_COLUMN_CARD.title` |
| 油井 | `OIL_DERRICK_CARD.title` |
| 卖本 | `SELL_MC_V.title` |
| 集结 | `RA2_RALLY.title` |
| 伞兵 | `PARATROOPER.title` |
| 停产 | `STOP_PRODUCTION_CARD.title` |

**注册方式**（直接使用公共基类）：

```csharp
// AlliedCardRegistry.cs 和 SovietCardRegistry.cs 都注册同一个类
cards.Add(() => ModelDb.Card<OilDerrickCard>());
```

**方案对比**：

| 对比项 | 方案一：继承分离 | 方案二：Pool动态切换 |
|--------|----------------|-------------------|
| 代码量 | 多（每个卡牌需要3个文件） | 少（每个卡牌只需要1个文件） |
| 本地化 | 需要两份（带阵营前缀） | 需要一份（无阵营前缀） |
| 百科显示 | 显示阵营颜色 | 显示白色/无色（符合预期） |
| 游戏内颜色 | 根据阵营子类自动切换 | 根据Owner动态切换 |
| 实例隔离 | 完全隔离（不同单例） | 共享实例（但通过Pool动态区分） |
| 适用场景 | 需要独立定制描述 | 逻辑完全相同的公共卡牌 |

### 资源与能力共享

虽然卡牌实例分离，但以下资源和逻辑仍然共用：

| 共享类型 | 说明 |
|---------|------|
| **能力(Power)** | 公共卡牌使用的能力（如 `GoldMinePower`、`OilDerrickPower`）存放在 `Common/Powers/` 目录，两个阵营共用同一份 |
| **资源文件** | 卡牌图片、能力图标等资源文件存放在 `RedAlert2ModResources/`，两个阵营共用同一份 |
| **数值配置** | 卡牌数值存放在 `Common/Cards/CommonCardValues.cs`，两个阵营共用同一份 |
| **逻辑代码** | 所有 `OnPlay`、`OnUpgrade` 等方法在公共基类中实现，子类自动继承 |

### 架构优势

**1. 逻辑复用**
- 修改公共卡牌逻辑时，只需修改 `Common/Cards/` 目录下的基类文件
- 两个阵营的卡牌会自动获得更新

**2. 实例隔离**
- 通过不同类名创建不同单例，避免卡框颜色和状态冲突
- 游戏根据类名自动分配正确的阵营卡框颜色

**3. 独立本地化**
- 虽然两份本地化内容相同，但可以独立修改
- 如果未来需要为不同阵营定制不同描述，可以轻松实现

**4. 资源共享**
- 图片、图标等资源只需要一份，减少资源包体积

### 公共卡牌注册

在阵营的 `CardRegistry` 中注册各自的公共卡牌子类：

```csharp
// AlliedCardRegistry.cs
private static List<Func<CardModel>> CreatePowerCards()
{
    var cards = new List<Func<CardModel>>();
    cards.Add(() => ModelDb.Card<AlliesGoldMineCard>());
    cards.Add(() => ModelDb.Card<AlliesOilDerrickCard>());
    cards.Add(() => ModelDb.Card<AlliesSellMCV>());
    // ... 其他盟军公共卡牌
    return cards;
}

// SovietCardRegistry.cs
private static List<Func<CardModel>> CreatePowerCards()
{
    var cards = new List<Func<CardModel>>();
    cards.Add(() => ModelDb.Card<SovietGoldMineCard>());
    cards.Add(() => ModelDb.Card<SovietOilDerrickCard>());
    cards.Add(() => ModelDb.Card<SovietSellMCV>());
    // ... 其他苏军公共卡牌
    return cards;
}
```

### 新增公共卡牌流程（方案二推荐）

1. **创建公共基类**：在 `Common/Cards/` 目录下创建卡牌类，重写 `Pool` 和 `VisualCardPool` 属性
2. **注册卡牌**：在 `AlliedCardRegistry` 和 `SovietCardRegistry` 中注册同一个公共基类
3. **添加本地化**：在 `cards.json` 中添加一份不带阵营前缀的本地化条目
4. **验证编译**：运行 `dotnet build` 确保没有错误

### UI刷新注意事项

**问题**：当卡牌打出后需要向手牌添加新卡牌时（如基地车选择建筑后），可能会出现卡牌卡在画面中央的情况，需要手动刷新游戏UI才能恢复正常。

**原因**：游戏的卡牌堆刷新机制需要通过特定操作触发，单纯调用 `CardPileCmd.AddGeneratedCardToCombat()` 添加卡牌可能不会自动触发UI刷新。

**解决方案**：在添加卡牌到手牌后，调用 `CardPileCmd.Draw(ctx, 0, Owner)` 触发UI刷新。虽然抽0张牌，但会强制更新手牌区域的UI显示。

```csharp
// 在 OnPlay 方法中
protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
{
    // ... 选择建筑逻辑 ...
    
    // 将选择的卡牌加入手牌
    await CardPileCmd.AddGeneratedCardToCombat(selectedCard, PileType.Hand, Owner);
    
    // 触发UI刷新：抽0张牌（仅触发刷新机制）
    await CardPileCmd.Draw(ctx, 0, Owner);
}
```

**适用场景**：
- 基地车卡牌（选择建筑后加入手牌）
- 集结卡牌（添加单位卡到手牌）
- 伞兵卡牌（添加士兵卡到手牌）
- 所有需要在打出后向手牌添加卡牌的场景

---

## � 标准项目结构

```
ProjectRoot/
├── build/                  # 输出目录
│   ├── MyMod.json
│   ├── MyMod.pck
│   └── MyMod.dll
├── libs/                   # 依赖库
│   ├── sts2.dll
│   └── 0Harmony.dll
├── src/                    # 源代码
│   ├── Cards/
│   ├── Relics/
│   ├── Potions/
│   ├── Powers/
│   ├── Characters/
│   ├── Monsters/
│   └── Events/
├── images/                 # 图片资源
│   ├── packed/
│   ├── atlases/
│   ├── relics/
│   ├── potions/
│   └── powers/
├── scenes/                 # 场景资源
│   ├── creature_visuals/
│   ├── encounters/
│   └── ui/
├── localization/           # 本地化
│   └── zhs/
│       ├── cards.json
│       ├── relics.json
│       └── ...
├── RedAlert2Mod.csproj
├── project.godot
└── ModInitializer.cs
```

---

## 🎮 联机模式注意事项

### 回合机制

**重要知识点**：回合切换是以"阵营"为单位进行的。

根据 `CombatManager.cs` 的源码逻辑：
- 所有玩家轮流出牌的过程都发生在同一个"玩家方回合"内
- 只有当所有玩家都结束回合后，才会切换到 `CombatSide.Enemy`

---

## 🎮 联机模式同步机制

### 多人同步随机数生成

**问题**：使用 `GD.RandRange()` 或 `new Random()` 会导致联机模式下不同客户端随机结果不一致，引发 `StateDivergence` 错误。

**正确用法**：
```csharp
var rng = Owner?.Player?.RunState?.Rng?.CombatCardSelection;
var randomIndex = rng?.NextInt(enemies.Count) ?? GD.RandRange(0, enemies.Count - 1);
```

**常用方法**：
| 方法 | 说明 |
|------|------|
| `NextInt(int max)` | [0, max) |
| `NextInt(int min, int max)` | [min, max) |
| `NextDouble()` | [0.0, 1.0) |
| `NextBool()` | 随机布尔值 |

**使用场景**：防御塔随机目标、工程师选项排序等需要多人同步的随机操作。

**错误示例**：
```csharp
// ❌ 会导致联机不同步
var idx = GD.RandRange(0, enemies.Count - 1);
var idx = new Random().Next(enemies.Count);
```

### DamageVar攻击类型与增伤机制

#### ValueProp枚举

| 值 | 说明 | 是否受增伤buff |
|----|------|--------------|
| `ValueProp.Move` | 攻击卡伤害 | ✅ |
| `ValueProp.Unpowered` | 能力/遗物/药水伤害 | ❌ |

#### 使用示例

**攻击卡（受增伤buff影响）**：
```csharp
new DamageVar(6m, ValueProp.Move)
```

**能力卡（不受增伤buff影响）**：
```csharp
new DamageVar(8m, ValueProp.Unpowered)
```

#### 红警Mod增伤规则

| 卡牌类型 | 是否受增伤buff | ValueProp | 示例 |
|---------|--------------|-----------|------|
| 攻击卡（单位卡、武器卡） | ✅ | `ValueProp.Move` | 动员兵、灰熊坦克、核弹 |
| 技能卡（非能力类） | ✅ | `ValueProp.Move` | 飞鹰空袭、闪电风暴 |
| 能力卡（防御塔等Power卡） | ❌ | `ValueProp.Unpowered` | 哨戒炮、磁暴线圈、光棱塔 |
| 遗物/药水伤害 | ❌ | `ValueProp.Unpowered` | 遗物效果、药水效果 |

**关键原则**：
- 打出卡牌直接造成的伤害用 `ValueProp.Move`
- 能力(Power)回合触发的伤害用 `ValueProp.Unpowered`
- 防御塔其伤害通过能力触发，使用 `ValueProp.Unpowered`

---

## 🔊 音效播放系统

### 建筑音效播放

`BuildingSoundHelper` 用于播放建筑卡牌打出时的音效：

```csharp
// 在建筑卡牌的 OnPlay 方法中调用
BuildingSoundHelper.PlayBuildingPlaceSound();
```

**使用示例**（建筑卡牌）：
```csharp
protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
{
    // 播放建筑释放音效
    BuildingSoundHelper.PlayBuildingPlaceSound();
    
    // 其他卡牌逻辑...
}
```

### 单位语音播放

`UnitVoiceHelper` 提供集中处理单位语音播放的接口，使用预定义的语音文件列表进行随机播放，完全绕过 DirAccess 目录枚举，确保在 PCK 打包环境下也能正常工作。

#### 配置方式

单位语音配置集中在 `UnitVoiceConfig.cs` 文件中，按阵营管理：

```csharp
// UnitVoiceConfig.cs - 语音配置类
public static class UnitVoiceConfig
{
    // 盟军单位语音配置
    public static readonly Dictionary<string, List<string>> AlliedUnits = new()
    {
        ["AmericanSoldier"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/AlliedUnits/AmericanSoldier/Igiata.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/AmericanSoldier/Igiatc.mp3",
            // ... 更多语音文件
        },
        ["GrizzlyTank"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/AlliedUnits/GrizzlyTank/Igtata.mp3",
            // ... 更多语音文件
        },
    };

    // 苏军单位语音配置（预留）
    public static readonly Dictionary<string, List<string>> SovietUnits = new();

    // 尤里单位语音配置（预留）
    public static readonly Dictionary<string, List<string>> YuriUnits = new();

    // 根据阵营和单位名称获取语音列表
    public static List<string> GetUnitVoices(string unitName, string faction = "Allied")
    {
        return faction switch
        {
            "Soviet" => SovietUnits.TryGetValue(unitName, out var voices) ? voices : new List<string>(),
            "Yuri" => YuriUnits.TryGetValue(unitName, out var voices) ? voices : new List<string>(),
            _ => AlliedUnits.TryGetValue(unitName, out var voices) ? voices : new List<string>(),
        };
    }
}
```

#### 基础用法

```csharp
// 通过类型播放（推荐）- 自动去除Card后缀
UnitVoiceHelper.PlayUnitVoice(typeof(AmericanSoldier));

// 通过名称播放
UnitVoiceHelper.PlayUnitVoice("AmericanSoldier");

// 指定阵营播放（默认 Allied）
UnitVoiceHelper.PlayUnitVoice("Conscript", "Soviet");  // 苏军动员兵
UnitVoiceHelper.PlayUnitVoice("YuriTrooper", "Yuri");   // 尤里新兵
```

#### 添加新单位语音

1. **准备语音文件**，放入对应阵营目录：
   ```
   RedAlert2ModResources/audio/AlliedUnits/AmericanSoldier/
   ├── Igiata.mp3
   ├── Igiatc.mp3
   └── ...
   ```

2. **在 UnitVoiceConfig.cs 中注册**：
   ```csharp
   ["NewUnit"] = new List<string>
   {
       "res://RedAlert2ModResources/audio/AlliedUnits/NewUnit/voice1.mp3",
       "res://RedAlert2ModResources/audio/AlliedUnits/NewUnit/voice2.mp3",
   },
   ```

#### 在单位卡牌中使用

```csharp
public sealed class AmericanSoldier : CardModel
{
    public AmericanSoldier() : base(1, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) { }
    
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        // 播放单位语音（自动使用类名 AmericanSoldier）
        UnitVoiceHelper.PlayUnitVoice(this.GetType());
        
        // 执行攻击逻辑
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(play.Target)
            .Execute(ctx);
    }
}
```

#### 阵营扩展

添加苏军/尤里阵营时，只需在 `UnitVoiceConfig.cs` 中添加对应配置：

```csharp
// 添加苏军单位
public static readonly Dictionary<string, List<string>> SovietUnits = new()
{
    ["Conscript"] = new List<string>
    {
        "res://RedAlert2ModResources/audio/SovietUnits/Conscript/conscript1.mp3",
        "res://RedAlert2ModResources/audio/SovietUnits/Conscript/conscript2.mp3",
    },
    ["RhinoTank"] = new List<string>
    {
        "res://RedAlert2ModResources/audio/SovietUnits/RhinoTank/rhino1.mp3",
    },
};
```

#### 检查语音配置

```csharp
// 检查单位是否有语音配置
bool hasVoice = UnitVoiceHelper.HasVoice("AmericanSoldier");
bool hasSovietVoice = UnitVoiceHelper.HasVoice("Conscript", "Soviet");
```

#### 注意事项

1. **语音文件格式**：支持 `.mp3`、`.wav`、`.ogg` 格式
2. **PCK 兼容**：硬编码路径，不受打包影响
3. **静默失败**：如果找不到语音配置或文件，不会抛出异常，仅输出日志
4. **概率控制**：如需控制播放概率，可多次添加同一文件增加权重

#### 语音目录结构

```
RedAlert2ModResources/audio/
├── AlliedUnits/          # 盟军单位语音
│   ├── AmericanSoldier/  # 美国大兵
│   ├── GrizzlyTank/      # 灰熊坦克
│   ├── Engineer/         # 工程师
│   └── ...
├── SovietUnits/          # 苏军单位语音（预留）
│   └── ...
├── YuriUnits/            # 尤里单位语音（预留）
│   └── ...
└── building_place.wav    # 建筑音效
```

#### 现有语音配置列表（盟军）

| 单位名称 | 配置Key | 语音数量 |
|---------|---------|---------|
| AmericanSoldier | AmericanSoldier | 11 |
| GrizzlyTank | GrizzlyTank | 11 |
| Engineer | Engineer | 5 |
| Intruder | Intruder | 9 |
| MirageTank | MirageTank | 11 |
| NightHawk | NightHawk | 7 |
| PrismTank | PrismTank | 8 |
| RocketSoldier | RocketSoldier | 6 |
| Spy | Spy | 9 |
| TransportShip | TransportShip | 7 |
| ChronoMiner | ChronoMiner | 8 |
| DogSoldier | DogSoldier | 5 |
| AircraftCarrier | AircraftCarrier | 9 |
| Destroyer | Destroyer | 11 |
| Dolphin | Dolphin | 4 |
| IFV | IFV | 9 |

---

### 建筑音效播放

`BuildingSoundHelper` 提供建筑放置音效的集中播放接口。

#### 基础用法

```csharp
// 在建筑卡牌的 OnPlay 方法中调用
BuildingSoundHelper.PlayBuildingPlaceSound();
```

#### 在建筑卡牌中使用

```csharp
protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
{
    // 播放建筑放置音效
    BuildingSoundHelper.PlayBuildingPlaceSound();
    
    // 执行建筑逻辑
    // ...
}
```

#### 现有的播放建筑音效的建筑列表

| 建筑名称 | 卡牌文件 |
|---------|---------|
| 基地车 | AlliedMCV.cs |
| 兵营 | BarracksCard.cs |
| 发电站 | PowerPlantCard.cs |
| 矿场 | AlliedRefinery.cs |
| 重工 | AlliedWarFactory.cs |
| 船厂 | ShipyardCard.cs |
| 空指部 | AirForceCommand.cs |
| 作战实验室 | BattleLab.cs |
| 超时空传送仪 | ChronoSphere.cs |
| 天气控制器 | WeatherController.cs |
| 维修厂 | RepairDepot.cs |
| 碉堡 | PillboxCard.cs |
| 光棱塔 | PrismTowerCard.cs |
| 油井 | OilDerrickCard.cs |

#### 不需要播放建筑音效的卡牌

以下卡牌虽然带有 `Building` 关键词，但不属于建筑物，不播放建筑音效：

| 卡牌名称 | 卡牌文件 | 说明 |
|---------|---------|------|
| 黄金矿 | GoldMineCard.cs | 资源类卡牌 |
| 黄金矿柱 | GoldMineColumnCard.cs | 资源类卡牌 |
| 宝石矿 | GemMineCard.cs | 资源类卡牌 |
| 围墙 | AlliedWallCard.cs | 防御工事，无声效 |
| 强化围墙 | FortifiedWall.cs | 防御工事，无声效 |

#### 新建筑卡牌配置规范

**新增建筑卡牌时，必须在 `OnPlay` 方法开头添加建筑音效调用**：

```csharp
protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
{
    // 必须：播放建筑放置音效
    BuildingSoundHelper.PlayBuildingPlaceSound();
    
    // 建筑逻辑...
}
```

**例外情况**：如果新卡牌属于以下类别，则不需要播放建筑音效：
1. 资源类卡牌（如金矿、宝石矿等）
2. 围墙卡牌（如围墙，坚固围墙）
3. 非建筑类型的卡牌

---

---

## 🛠️ 高级战备体系实现模式（飞鹰 & 轨道）

### 一、飞鹰战备体系（盟军）

#### 1. 架构设计：双基类封装

飞鹰战备体系通过两个基类实现了高度的代码复用和统一的行为逻辑：

-   **`DesperateMeasureCardBase<TPower>` (卡牌基类)**：统一管理卡牌的打出流程、关键词显示（Pool/VisualCardPool 切换）、升级逻辑等。
-   **`DesperateMeasurePowerBase` (能力基类)**：统一管理回合触发流程、目标锁定判定、溅射伤害处理、独立叠层逻辑等。

**核心代码模式**：

```csharp
// 卡牌基类示例
public abstract class DesperateMeasureCardBase<TPower> : CardModel
    where TPower : DesperateMeasurePowerBase
{
    // 自动处理 Pool/VisualCardPool 切换
    public override CardPoolModel Pool => IsMutable && Owner != null
        ? Owner.Character.CardPool
        : ModelDb.CardPool<TokenCardPool>();
    public override CardPoolModel VisualCardPool => Pool;

    // 自动添加关键词
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ModCardKeywords.DesperateMeasure.CreateHoverTip(),
        NeedsTargetLock ? ModCardKeywords.TargetLocked.CreateHoverTip() : null,
        // ... 其他Tip
    ];

    // 子类只需重写核心逻辑
    protected abstract Task<TPower?> ApplyPower(Creature owner, bool isUpgraded);
}

// 能力基类示例
public abstract class DesperateMeasurePowerBase : PowerModel
{
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;
    public override PowerStackType StackType => PowerStackType.Counter;

    // 统一的回合触发逻辑
    public override async Task AfterSideTurnStart(...)
    {
        // 1. 校验 Owner/Target 有效性
        // 2. 若 NeedsTargetLock=true 但未锁定，自动从存活敌人中随机选一个并打上锁定
        // 3. 调用子类的 ExecuteAttackEffect 实现
        // 4. 消耗一层 Amount
        await ConsumeOrRemove();
    }

    // 子类实现：具体的攻击效果
    protected abstract Task ExecuteAttackEffect(Creature target, PlayerChoiceContext ctx);
}
```

#### 2. 独立叠层规范（强制要求）

所有战备能力必须实现「**相同数值叠加层数，不同数值独立实例**」，以支持在同一张卡可以升级前后独立存在的需求。

**标准模式**：按实际最终数值判断，而非 `IsUpgraded` 布尔值。

```csharp
// 在卡牌 ApplyPower 方法中
var existingPower = owner.Powers.OfType<TPower>()
    .FirstOrDefault(p => p.CurrentDamage == finalDamage 
                       && p.CurrentRepeat == repeatCount); // 如果有多个自定义字段

if (existingPower != null)
{
    // 数值完全相同 → 叠加层数
    await PowerCmd.ModifyAmount(ctx, existingPower, 1m, owner, null);
}
else
{
    // 数值不同 → 创建新实例
    var power = await PowerCmd.Apply<TPower>(ctx, owner, 1m, owner, null);
    if (power != null)
    {
        power.CurrentDamage = finalDamage;
        power.IsUpgraded = isUpgraded;
        // ... 设置其他自定义字段
    }
}
```

> **⚠️ 禁止**使用 `IsUpgraded` 布尔值作为叠层判断的唯一依据，这会导致升级前后数值不同的情况无法正确区分。

#### 3. 添加新飞鹰战备卡步骤速查

| 步骤 | 文件路径 | 操作 |
|-----|---------|------|
| 1 | `CommonCardValues.cs` / `CommonPowerValues.cs` | 添加数值条目 |
| 2 | `Allies/Cards/` | 创建卡牌类，继承 `DesperateMeasureCardBase<TPower>` |
| 3 | `Allies/Powers/` | 创建能力类，继承 `DesperateMeasurePowerBase` |
| 4 | `Allies/Powers/PowerIconPatch.cs` | 注册能力图标 |
| 5 | `AlliedCardRegistry.cs` | 注册卡牌到卡池 |
| 6 | `localization/*/cards.json` & `powers.json` | 添加本地化文本 |

---

### 二、轨道战备体系（苏军）

#### 1. 与飞鹰战备的关键差异

-   **前置依赖**：轨道战备卡**必须**在拥有「雷达（Radar）」能力时才能打出或出现在卡池中。
-   **实现方式**：无统一的卡牌/能力基类，三张轨道卡（120mm、380mm、毒气）独立实现，但遵循相同的叠层和移除规范。

#### 2. 刚需雷达能力（前置约束实现）

通过在 `SovietCardRegistry` 中将轨道卡放入一个独立的列表，并在构建卡池时进行条件判断来实现。

```csharp
// SovietCardRegistry.cs
public static List<Func<CardModel>> RadarPowerCards { get; } = new()
{
    () => ModelDb.Card<Orbital120mm>(),
    () => ModelDb.Card<Orbital380mm>(),
    () => ModelDb.Card<OrbitalGasStrike>(),
    // ... 其他依赖雷达的卡牌
};

// 构建卡池时
public static List<CardModel> CreatePowerCards(Player owner)
{
    var cards = CommonCardRegistry.GetAllPowerCardsForSoviet().Select(s => s()).ToList();
    // ... 添加其他苏军专属卡

    // ★ 关键：只有当玩家拥有雷达能力时，才将轨道卡加入卡池
    if (HasRadarPower(owner.Creature))
        cards.AddRange(RadarPowerCards.Select(s => s()));

    return cards;
}

// 雷达能力检查
private static bool HasRadarPower(Creature creature)
{
    return creature.Powers.Any(p => p is SovietRadarPower);
}
```

#### 3. 独立叠层规范（与飞鹰一致）

轨道卡能力（120mm/380mm/GasStrike）同样需要按实际数值进行独立叠层。

```csharp
// 轨道120mm/380mm：Damage × Repeat 两个维度一起判定
var existing = owner.Powers.OfType<Orbital120mmPower>()
    .FirstOrDefault(p => p.CurrentDamage == damage 
                       && p.CurrentRepeat == repeat);

// 轨道毒气：只有 Poison 一个维度
var existing = owner.Powers.OfType<OrbitalGasStrikePower>()
    .FirstOrDefault(p => p.CurrentPoison == poison);
```

#### 4. 添加新轨道战备卡步骤速查

| 步骤 | 文件路径 | 操作 |
|-----|---------|------|
| 1 | `CommonCardValues.cs` / `CommonPowerValues.cs` | 添加数值条目 |
| 2 | `Common/Cards/` | 创建卡牌类，继承 `CardModel` |
| 3 | `Soviet/Powers/` | 创建能力类，继承 `PowerModel` |
| 4 | `Allies/Powers/PowerIconPatch.cs` | 注册能力图标 |
| 5 | `SovietCardRegistry.cs` | **必须**将卡牌加入 `RadarPowerCards` 列表 |
| 6 | `localization/*/cards.json` & `powers.json` | 添加本地化文本 |

---

### 三、战备体系常见问题排查

| 现象 | 可能原因 | 解决方案 |
|-----|---------|---------|
| 叠层后伤害数值显示异常 | 使用了 `IsUpgraded` 而非实际数值作为叠层判断依据 | 改为按所有自定义数值字段（如 `CurrentDamage`, `CurrentWeak`）进行判断 |
| 战备能力触发后未移除 | `AfterSideTurnStart` 方法末尾漏写移除逻辑 | 检查并添加 `await PowerCmd.Remove(this);` 或调用基类的 `ConsumeOrRemove` |
| 苏军玩家无雷达却在奖励中拿到轨道卡 | 卡池过滤仅在 `CreatePowerCards` 中生效，未覆盖所有卡牌来源 | 在轨道卡的 `OnPlay` 方法开头增加雷达能力检查作为防御性代码 |
| 多张战备卡只触发了一次 | `Amount` 递减和 `Remove` 的逻辑位置不对，导致提前移除实例 | 确保在触发逻辑执行完毕后再消耗 `Amount` 或移除实例 |

---

*本手册为快速参考，详细教程请查看完整文档。*

---