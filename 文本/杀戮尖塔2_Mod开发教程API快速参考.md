# 杀戮尖塔2 Mod开发 - AI快速参考手册

> 精简版，便于AI快速检索关键API和代码模式

---

## 📂 项目路径配置

### 当前项目路径
```
项目根目录: D:\RedAlert2Project\red-alert-2-mod
游戏解包目录: D:\RedAlert2Project\SlayTheSpire2Export
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

*本手册为快速参考，详细教程请查看完整文档。*

---