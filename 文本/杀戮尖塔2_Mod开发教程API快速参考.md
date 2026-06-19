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
}
```

**重要提示**：新增能力类型后，必须将其添加到 `_customIconPaths` 字典中，否则图标将无法正常显示。例如添加 `TransportShipPower` 后：
```csharp
{ typeof(TransportShipPower), "res://RedAlert2ModResources/images/packed/card_portraits/allies/landicon.png" },
```

### 施加能力
```csharp
await PowerCmd.Apply<MyBuff>(target, amount, source, sourceCard);
```

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

**ID转换规则**: `MyClassName` → `MY_CLASS_NAME`

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

---

## 📁 标准项目结构

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
- 因此 `AfterSideTurnStart(CombatSide side, CombatState combatState)` 在整个玩家方回合**只会触发一次**
- 不会因为联机人数而重复触发

**阵营判断**：
- `side == CombatSide.Player` —— 玩家方回合开始（所有玩家共用一个回合）
- `side == CombatSide.Enemy` —— 敌方回合开始

**示例**：
```csharp
public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
{
    if (side != CombatSide.Player)
        return; // 跳过敌方回合

    // 在玩家方回合开始时执行一次
    await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
}
```

---

*本手册为快速参考，详细教程请查看完整文档。*
