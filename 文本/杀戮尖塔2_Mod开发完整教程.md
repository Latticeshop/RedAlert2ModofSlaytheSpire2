# 杀戮尖塔2 Mod开发完整教程

> 本教程基于《杀戮尖塔2》官方Mod开发指南整理，适用于使用Godot引擎和C#语言开发Mod。

---

## 📚 目录

1. [环境搭建](#1-环境搭建)
2. [自定义遗物](#2-自定义遗物)
3. [自定义卡牌](#3-自定义卡牌)
4. [自定义药水](#4-自定义药水)
5. [卡牌附魔](#5-卡牌附魔)
6. [自定义事件](#6-自定义事件)
7. [自定义能力](#7-自定义能力)
8. [自定义角色](#8-自定义角色)
9. [自定义敌怪](#9-自定义敌怪)

---

## 1. 环境搭建

### 1.1 Mod的基本构成

一个完整的《杀戮尖塔2》Mod由三个同名文件组成，必须放在游戏 `mods` 目录下的同一级文件夹中：

| 文件类型 | 扩展名 | 必需性 | 说明 |
|---------|--------|--------|------|
| 模组清单文件 | `.json` | ✅ 必需 | Mod的"身份证" |
| 资源包文件 | `.pck` | ⚠️ 可选 | 包含图片、场景等资源 |
| 代码程序集 | `.dll` | ⚠️ 可选 | 包含C#逻辑代码 |

**示例结构：**
```
mods/MyCustomMod/
├── MyCustomMod.json
├── MyCustomMod.pck
└── MyCustomMod.dll
```

### 1.2 模组清单文件（.json）

```json
{
  "id": "MyCustomMod",
  "name": "我的自定义模组",
  "author": "作者名",
  "description": "模组描述",
  "version": "v1.0.0",
  "has_pck": true,
  "has_dll": true,
  "dependencies": [],
  "affects_gameplay": true
}
```

**关键字段说明：**
- `has_pck` / `has_dll`: 如实声明是否包含资源包或代码
- `affects_gameplay`: 若影响游戏玩法（添加卡牌、角色等），必须设为 `true`，否则联机时可能不同步
- `dependencies`: 依赖的其他Mod ID列表

### 1.3 环境与工具要求

| 项目 | 要求 |
|------|------|
| .NET版本 | **.NET 9.0** (必需) |
| C#语言版本 | C# 13 |
| Godot编辑器 | **Megadot** 分支 (https://megadot.megacrit.com) |
| IDE推荐 | Visual Studio 或 JetBrains Rider |

⚠️ **重要**: 必须使用Megadot而非标准Godot版本，以确保兼容性。

### 1.4 创建Mod项目步骤

1. **新建Godot项目**
   - 使用Megadot编辑器创建新项目
   - 项目名建议与Mod ID一致（英文）

2. **推荐目录结构模板**
   ```
   red-alert-2-mod/
   ├── .idea/                      # IDE配置（自动生成）
   ├── BaseLib/                    # 基础库（可选）
   │   ├── BaseLib.dll
   │   ├── BaseLib.json
   │   └── BaseLib.pck
   ├── RedAlert2Mod/               # Mod输出目录
   │   ├── RedAlert2Mod.dll
   │   ├── RedAlert2Mod.json
   │   └── RedAlert2Mod.pdb
   ├── RedAlert2ModCode/           # C#源代码
   │   ├── Allies/                 # 角色相关代码
   │   │   ├── AlliesCharacter.cs
   │   │   ├── AlliesCardPool.cs
   │   │   ├── AlliesRelicPool.cs
   │   │   ├── AlliesPotionPool.cs
   │   │   └── AlliesRegistration.cs
   │   ├── Extensions/             # 扩展方法
   │   └── ModInitializer.cs       # Mod入口
   ├── RedAlert2ModResources/      # Godot资源
   │   ├── images/                 # 图片资源
   │   │   ├── character/          # 角色立绘
   │   │   ├── charui/             # UI角色图
   │   │   ├── packed/character_select/  # 角色选择立绘
   │   │   └── powers/             # 能力图标
   │   ├── localization/zhs/       # 中文本地化
   │   │   ├── cards.json
   │   │   ├── characters.json
   │   │   ├── relics.json
   │   │   └── ...
   │   └── scenes/                 # Godot场景
   │       ├── creature_visuals/   # 角色待机动画
   │       └── ui/character_icons/ # 角色头像图标
   ├── build/                      # 构建输出
   │   ├── RedAlert2Mod.json
   │   ├── RedAlert2Mod.pck
   │   └── RedAlert2Mod.dll
   ├── localization/zhs/           # 本地化文件（游戏读取路径）
   ├── 0Harmony.dll                # Harmony库
   ├── sts2.dll                    # 游戏核心DLL
   ├── RedAlert2Mod.csproj         # C#项目文件
   ├── RedAlert2Mod.sln            # 解决方案文件
   └── project.godot               # Godot项目配置
   ```

3. **目录职责说明**

| 目录 | 职责 |
|------|------|
| `RedAlert2ModCode/` | C#源代码，包含角色、卡牌、遗物等逻辑 |
| `RedAlert2ModResources/` | Godot资源（图片、场景、本地化） |
| `localization/zhs/` | 游戏读取的本地化文件路径 |
| `build/` | 构建输出目录 |
| `BaseLib/` | 可选的基础库模块 |

4. **添加依赖库**
   - 从游戏安装目录的 `data_sts2_<platform>` 文件夹复制：
     - `sts2.dll`
     - `0Harmony.dll`
   - 放入项目的 `libs/` 文件夹
   - 在IDE中引用这两个DLL

5. **配置.csproj文件**
   ```xml
   <Project Sdk="Godot.NET.Sdk/4.5.1">
     <PropertyGroup>
       <TargetFramework>net9.0</TargetFramework>
       <EnableDynamicLoading>true</EnableDynamicLoading>
     </PropertyGroup>
     <ItemGroup>
       <Reference Include="0Harmony">
         <HintPath>libs/0Harmony.dll</HintPath>
       </Reference>
       <Reference Include="sts2">
         <HintPath>libs/sts2.dll</HintPath>
       </Reference>
     </ItemGroup>
   </Project>
   ```

6. **创建Mod入口类**
   ```csharp
   using MegaCrit.Sts2.Core.Modding;
   
   [ModInitializer(nameof(Initialize))]
   public static class MyModInitializer
   {
       public static void Initialize()
       {
           // 初始化代码
           Log.Info("Mod加载成功");
       }
   }
   ```

### 1.5 资源包（.pck）制作

1. **放置资源**
   - 按与游戏本体相同的相对路径放置资源
   - 例如: `res://images/ui/...`

2. **导出PCK**
   - 菜单: `项目 > 导出`
   - 添加Windows导出方案
   - 点击 **导出PCK/ZIP**
   - 命名为 `<Mod ID>.pck`
   - ❌ 取消"使用调试导出"
   - ❌ 取消"导出为补丁"

### 1.6 游戏日志路径

遇到问题时，第一个要查看的就是游戏日志文件：

**日志位置**：
```
C:\Users\<你的用户名>\AppData\Roaming\SlayTheSpire2\logs\
```

**重要日志文件**：
- `godot.log` - 游戏启动和运行时的详细日志，包含所有错误信息
- 当游戏无法启动或Mod加载失败时，这里会告诉你具体哪里出了问题

---

### 1.7 常见问题

**问题1: Mod完全无法加载 - 致命错误**

**症状**：游戏启动时报错，Mod的DLL或PCK文件找不到

**原因**：这是最常见的致命错误！JSON配置文件中的 `"id"` 字段必须与文件名完全一致！

**示例**：
```json
// RedAlert2Mod.json
{
  "id": "RedAlert2Mod",  // ← 这个ID
  "name": "红警2 Mod",
  ...
}
```

**必须确保三个文件同名**：
- `RedAlert2Mod.json` （ID必须匹配）
- `RedAlert2Mod.dll`
- `RedAlert2Mod.pck`

如果你把ID改成 `"Ra2Mod"`，那么文件名也必须是 `Ra2Mod.json`、`Ra2Mod.dll`、`Ra2Mod.pck`，否则游戏根本无法找到你的Mod文件！

**解决步骤**：
1. 打开 `godot.log` 查看具体报错信息
2. 检查JSON中的 `"id"` 字段
3. 确保三个文件名完全一致（包括大小写）
4. 重新复制到游戏mods文件夹

---

**问题2: 角色资源显示为空白**

**症状**：角色已经能在选择页面看到，但图标、立绘、背景图显示为空白

**原因**：Godot场景文件中的 `Sprite2D` 节点没有设置 `texture` 属性

**场景**：你可能已经创建了场景文件（.tscn），也导出了PCK，但打开游戏后角色是空白的

**解决步骤**：
1. 在Godot编辑器中打开你的场景文件（如 `allies.tscn`、`allies_icon.tscn`、`allies_bg.tscn`）
2. 在场景树中选中 `Sprite2D` 节点（通常叫 `Visuals`、`Bg`、`CharacterIconCharName` 等）
3. 在右侧的"检查器"面板找到 **Texture** 属性
4. 点击Texture旁边的下拉箭头，选择 **快速加载**（Quick Load）
5. 在弹出的文件浏览器中选择对应的PNG图片
6. 保存场景（Ctrl+S），然后重新导出PCK

**正确的场景文件应该长这样**：
```gdscript
[node name="Visuals" type="Sprite2D" parent="."]
position = Vector2(0, -150)
texture = ExtResource("1_allies")  # ← 必须有这一行！

[ext_resource type="Texture2D" path="res://images/character/allies_character.png" id="1_allies"]
```

如果缺少 `texture = ExtResource(...)` 这行，场景就无法显示图片！

---

**问题3: PlatformNotSupportedException**
- **原因**: .NET版本不匹配
- **解决**: 修改 Megadot 编辑器目录下的 `GodotPlugins.runtimeconfig.json`，将 `version` 强制改为 `9.0.0`

**问题4: Mod未被加载**
- 检查三个文件是否同名且在同一目录
- 检查JSON中的 `has_pck` / `has_dll` 是否与实际文件相符

**问题5: 资源替换不生效**
- 确认PCK内的资源路径与游戏原版完全一致（包括大小写）

---

## 2. 自定义遗物

### 2.1 遗物基类

所有遗物继承自 `RelicModel` 抽象类。

```csharp
public class MyCustomRelic : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Starter;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => new[] { new EnergyVar(2) };
    
    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    {
        if (side == Owner.Creature.Side && combatState.RoundNumber == 1)
        {
            Flash();  // 遗物图标闪烁
            await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
        }
    }
}
```

### 2.2 遗物稀有度

| 稀有度 | 获取方式 |
|--------|----------|
| `Starter` | 初始遗物，不在宝箱/精英中出现 |
| `Common` | 普通遗物，可通过宝箱、精英获取 |
| `Uncommon` | 罕见遗物 |
| `Rare` | 稀有遗物 |
| `Shop` | 商店遗物 |
| `Event` | 事件遗物 |
| `Ancient` | 先古之民给予的遗物 |

### 2.3 常用事件钩子

```csharp
// 回合开始时
public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)

// 卡牌打出后
public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)

// 受到伤害前
public override async Task BeforeTakeDamage(...)

// 敌人死亡时
public override async Task AfterMonsterKilled(...)

// 战斗结束时
public override async Task AfterCombatEnd(...)
```

### 2.4 注册到遗物池

```csharp
// 在ModInitializer中
ModHelper.AddModelToPool(typeof(IroncladRelicPool), typeof(MyCustomRelic));
```

**常见遗物池：**
- `IroncladRelicPool`, `SilentRelicPool` - 角色专属
- `SharedRelicPool` - 公共遗物池（精英、商店、宝箱）
- `EventRelicPool` - 事件遗物
- `FallbackRelicPool` - 兜底池

### 2.5 修改初始遗物（HarmonyPatch）

```csharp
[HarmonyPatch(typeof(Ironclad), nameof(Ironclad.StartingRelics), MethodType.Getter)]
public static class IroncladStartingRelicsPatch
{
    static void Postfix(ref IReadOnlyList<RelicModel> __result)
    {
        var customRelic = ModelDb.Relic<MyCustomRelic>();
        if (__result.Any(r => r.Id == customRelic.Id)) return;
        var list = __result.ToList();
        list.Add(customRelic);
        __result = list;
    }
}
```

### 2.6 资源路径

```
res://images/relics/my_custom_relic.png              # 大图 (256x256)
res://images/relics/my_custom_relic_outline.png      # 描边图
res://images/atlases/relic_atlas.sprites/my_custom_relic.tres          # 裁切纹理
res://images/atlases/relic_outline_atlas.sprites/my_custom_relic.tres  # 描边裁切
```

### 2.7 本地化文本

`res://<ModID>/localization/zhs/relics.json`:
```json
{
  "MY_CUSTOM_RELIC.title": "瓶装能量",
  "MY_CUSTOM_RELIC.description": "每场战斗开始时，获得 {Energy} 点能量。",
  "MY_CUSTOM_RELIC.flavor": "这个瓶子中蕴含着无尽的力量"
}
```

---

## 3. 自定义卡牌

### 3.1 卡牌构造函数

```csharp
public MyCustomCard() : base(energyCost, cardType, cardRarity, targetType, shouldShowInCardLibrary)
```

**参数说明：**

| 参数 | 类型 | 说明 |
|------|------|------|
| energyCost | int | 基础能量消耗 |
| cardType | CardType | Attack, Skill, Power, Status, Curse, Quest |
| cardRarity | CardRarity | 决定卡牌稀有度和出现逻辑 |
| targetType | TargetType | Self, AnyEnemy, AllEnemies, RandomEnemy |
| shouldShowInCardLibrary | bool | 是否在图鉴显示（默认true） |

### 3.2 卡牌稀有度（CardRarity）

`CardRarity` 属性不仅决定卡牌的边框样式，还直接影响卡牌的获取逻辑和商店售价：

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

### 3.3 动态变量（CanonicalVars）

```csharp
protected override List<DynamicVar> CanonicalVars => new List<DynamicVar>
{
    new DamageVar(2m, ValueProp.Move),   // 基础伤害2点
    new BlockVar(5m)                     // 基础格挡5点
};
```

### 3.3 核心回调方法

#### OnPlay - 打出时触发
```csharp
protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
{
    await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
        .FromCard(this)
        .Targeting(cardPlay.Target)
        .WithHitFx("vfx/vfx_attack_slash")
        .Execute(choiceContext);
}
```

**UI刷新注意事项**：当卡牌打出后需要向手牌添加新卡牌时（如基地车选择建筑后），可能会出现卡牌卡在画面中央的情况。此时需要在添加卡牌后调用 `CardPileCmd.Draw(ctx, 0, Owner)` 触发UI刷新：

```csharp
protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
{
    // ... 选择建筑逻辑 ...
    
    // 将选择的卡牌加入手牌
    await CardPileCmd.AddGeneratedCardToCombat(selectedCard, PileType.Hand, Owner);
    
    // 触发UI刷新：抽0张牌（仅触发刷新机制）
    await CardPileCmd.Draw(ctx, 0, Owner);
}
```

**适用场景**：基地车卡牌、集结卡牌、伞兵卡牌等需要在打出后向手牌添加卡牌的场景。

#### OnUpgrade - 升级时触发
```csharp
protected override void OnUpgrade()
{
    DynamicVars.Damage.UpgradeValueBy(2m);
    DynamicVars.Block.UpgradeValueBy(3m);
}
```

#### OnTurnEndInHand - 回合结束手牌中触发
```csharp
public override async Task OnTurnEndInHand(PlayerChoiceContext choiceContext)
{
    await CreatureCmd.Damage(choiceContext, Owner.Creature, 3m, ValueProp.Unblockable, null);
}
```

### 3.4 卡牌标记与关键词

```csharp
// 卡牌标记
protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { CardTag.Strike };

// 卡牌关键词
protected override HashSet<CardKeyword> CanonicalKeywords => new HashSet<CardKeyword>
{
    CardKeyword.Exhaust    // 消耗
    // CardKeyword.Ethereal  // 虚无
    // CardKeyword.Innate    // 固有
};
```

### 3.5 注册到卡池

```csharp
ModHelper.AddModelToPool(typeof(IroncladCardPool), typeof(MyCustomCard));
```

**常见卡池：**
- `IroncladCardPool`, `SilentCardPool` - 角色专属
- `ColorlessCardPool` - 无色卡池
- `TokenCardPool` - 衍生卡牌
- `StatusCardPool`, `CurseCardPool` - 状态和诅咒

### 3.6 资源路径

```
res://images/atlases/card_atlas.sprites/<卡池名称>/<卡牌ID小写>.tres
res://images/packed/card_portraits/<卡池名称>/<卡牌ID小写>.png
```

**卡池名称对应：**
- `ironclad`, `silent`, `defect`, `necrobinder`, `regent`
- `colorless`, `curse`, `event`, `quest`, `status`, `token`

### 3.7 本地化文本

`res://<ModID>/localization/zhs/cards.json`:
```json
{
  "MY_CUSTOM_CARD.title": "飞刀",
  "MY_CUSTOM_CARD.description": "对指定敌人造成 {Damage} 点伤害。"
}
```

- `{Damage}` 会被动态变量的当前值替换
- `{Damage:diff()}` 显示升级后的差值（如"造成 2→4 点伤害"）

### 3.8 公共卡牌架构（多阵营共享）

当你的Mod包含多个阵营/角色，且某些卡牌在多个阵营中逻辑完全相同时，可以使用公共卡牌架构避免代码重复。

#### 方案一：继承分离模式（传统方案）

采用"公共基类 + 阵营子类"的架构：

```csharp
// Common/Cards/GoldMineCard.cs - 公共基类
public class GoldMineCard : CardModel
{
    public override string PortraitPath => "res://.../gold_mine.png";
    
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        // 完整逻辑
    }
}

// Allies/Cards/AlliesGoldMineCard.cs - 盟军子类
public sealed class AlliesGoldMineCard : GoldMineCard { }

// Soviet/Cards/SovietGoldMineCard.cs - 苏军子类
public sealed class SovietGoldMineCard : GoldMineCard { }
```

**本地化**（需要两份）：
```json
{
    "ALLIES_GOLD_MINE_CARD.title": "黄金矿",
    "SOVIET_GOLD_MINE_CARD.title": "黄金矿"
}
```

#### 方案二：Pool动态切换模式（推荐方案）

通过重写 `Pool` 和 `VisualCardPool` 属性，让同一卡牌实例根据持有者动态切换阵营颜色：

```csharp
using MegaCrit.Sts2.Core.Models.CardPools;

public class GoldMineCard : CardModel
{
    public override string PortraitPath => "res://.../gold_mine.png";

    public override CardPoolModel Pool => IsMutable && Owner != null
        ? Owner.Character.CardPool
        : ModelDb.CardPool<TokenCardPool>();

    public override CardPoolModel VisualCardPool => Pool;

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        // 完整逻辑
    }
}
```

**核心原理**：

| 属性 | 说明 |
|------|------|
| `Pool` | 卡牌所属卡池，决定卡框颜色 |
| `VisualCardPool` | UI显示时使用的卡池 |
| `IsMutable` | 是否为战斗实例（战斗实例才有Owner） |
| `Owner.Character.CardPool` | 当前持有者的阵营卡池 |
| `TokenCardPool` | 无主卡牌使用的卡池（白色/无色） |

**颜色显示逻辑**：

| 场景 | 条件 | 显示颜色 |
|------|------|---------|
| 百科中 | `IsMutable == false` 或 `Owner == null` | 白色/无色 |
| 游戏中-盟军 | `Owner.Character.CardPool` 返回盟军卡池 | 蓝色 |
| 游戏中-苏军 | `Owner.Character.CardPool` 返回苏军卡池 | 红色 |

**本地化**（仅需一份）：
```json
{
    "GOLD_MINE_CARD.title": "黄金矿"
}
```

**注册**（两个阵营注册同一个类）：
```csharp
// AlliedCardRegistry.cs 和 SovietCardRegistry.cs
cards.Add(() => ModelDb.Card<GoldMineCard>());
```

**方案对比**：

| 对比项 | 方案一：继承分离 | 方案二：Pool动态切换 |
|--------|----------------|-------------------|
| 代码量 | 多（每个卡牌需要3个文件） | 少（每个卡牌只需要1个文件） |
| 本地化 | 需要两份（带阵营前缀） | 需要一份（无阵营前缀） |
| 百科显示 | 显示阵营颜色 | 显示白色/无色（符合预期） |
| 适用场景 | 需要独立定制描述 | 逻辑完全相同的公共卡牌 |

---

## 3.9 自定义词条（Custom Keywords）

Mod可以添加自定义词条来增强卡牌的视觉效果和交互体验。词条会在卡牌描述下方显示金色文本，鼠标悬停时显示详细描述。

### 设计理念

自定义词条适用于需要特殊条件或限制的卡牌，例如：
- 需要特定能力才能打出的卡牌（如"建造厂"词条）
- 具有特殊使用条件的卡牌
- 增强卡牌的视觉效果和提示

### 实现步骤

#### 第一步：创建词条定义类

在 `Utils/` 目录下创建 `CustomKeyword.cs`：

```csharp
using System.Collections.Generic;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;

namespace RedAlert2ModCode.Utils;

/// <summary>
/// 自定义词条定义
/// </summary>
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

#### 第二步：在卡牌中使用 ExtraHoverTips

在需要添加词条的卡牌类中重写 `ExtraHoverTips` 属性：

```csharp
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

#### 第三步：添加本地化文本

创建或更新 `localization/zhs/card_keywords.json`：

```json
{
    "mcv.title": "建造厂",
    "mcv.description": "拥有建造厂才能打出建筑卡牌。"
}
```

#### 第四步：在卡牌描述中显示词条文本

在 `cards.json` 的卡牌描述中添加金色格式化的词条文本：

```json
{
    "ALLIED_MC_V.title": "盟军基地车",
    "ALLIED_MC_V.description": "[gold]建造厂. [/gold]\n展开：从当前建筑中选择一张加入手牌。"
}
```

### 效果说明

- **卡牌显示**：在描述下方显示金色的"建造厂."文本
- **悬停提示**：鼠标悬停在词条上时显示详细描述"拥有建造厂才能打出建筑卡牌。"

### 扩展更多词条

在 `ModCardKeywords` 类中添加更多词条：

```csharp
public static class ModCardKeywords
{
    public static readonly CustomKeyword Mcv = new(...);
    
    // 添加新词条
    public static readonly CustomKeyword MyNewKeyword = new(
        "MY_NEW_KEYWORD",
        new LocString("card_keywords", "my_new_keyword.title"),
        new LocString("card_keywords", "my_new_keyword.description")
    );
}
```

然后在需要使用该词条的卡牌中添加到 `ExtraHoverTips`：

```csharp
protected override IEnumerable<IHoverTip> ExtraHoverTips =>
[
    ModCardKeywords.Mcv.CreateHoverTip(),
    ModCardKeywords.MyNewKeyword.CreateHoverTip()
];
```

---

## 3.10 卡牌悬浮提示（HoverTip）

### 3.10.1 核心原理

卡牌上展示悬浮的其他卡牌和能力，是通过重写 `CardModel` 类的 **`ExtraHoverTips`** 属性实现的。游戏引擎会自动将这些提示显示在卡牌描述下方，当玩家将鼠标悬浮在卡牌上时，会显示对应的卡牌或能力的详细信息。

### 3.10.2 HoverTipFactory 工具类

游戏提供了 `MegaCrit.Sts2.Core.HoverTips.HoverTipFactory` 静态类来生成各种悬浮提示：

| 方法 | 作用 | 示例 |
|------|------|------|
| `FromCard<T>(bool upgrade = false)` | 生成卡牌预览 | `HoverTipFactory.FromCard<Shiv>()` |
| `FromCardWithCardHoverTips<T>()` | 生成卡牌预览 + 卡牌附带的所有悬浮提示 | `HoverTipFactory.FromCardWithCardHoverTips<SovereignBlade>()` |
| `FromPower<T>(int? amount = null)` | 生成能力预览 | `HoverTipFactory.FromPower<PoisonPower>()` |
| `FromPowerWithPowerHoverTips<T>()` | 生成能力预览 + 能力附带的所有悬浮提示 | - |
| `FromOrb<T>()` | 生成球体预览 | `HoverTipFactory.FromOrb<LightningOrb>()` |
| `FromRelic<T>()` | 生成遗物预览 | - |
| `Static(StaticHoverTip tip, params DynamicVar[] vars)` | 生成静态文本提示 | - |

### 3.10.3 游戏原版示例

**Accuracy 卡牌**（展示 Shiv 卡牌预览）：

```csharp
using MegaCrit.Sts2.Core.HoverTips;

public sealed class Accuracy : CardModel
{
    // ... 其他代码 ...

    protected override IEnumerable<IHoverTip> ExtraHoverTips => 
        [HoverTipFactory.FromCard<Shiv>()];
}
```

**Abrasive 卡牌**（展示多个能力预览）：

```csharp
using MegaCrit.Sts2.Core.HoverTips;

public sealed class Abrasive : CardModel
{
    // ... 其他代码 ...

    protected override IEnumerable<IHoverTip> ExtraHoverTips => 
    [
        HoverTipFactory.FromPower<DexterityPower>(),
        HoverTipFactory.FromPower<ThornsPower>()
    ];
}
```

**SovereignBlade 卡牌**（展示卡牌及其附带的所有提示）：

```csharp
using MegaCrit.Sts2.Core.HoverTips;

public sealed class SovereignBlade : CardModel
{
    // ... 其他代码 ...

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        HoverTipFactory.FromCardWithCardHoverTips<SovereignBlade>();
}
```

### 3.10.4 自定义实现示例

**示例1：展示升级后的卡牌预览**

```csharp
using MegaCrit.Sts2.Core.HoverTips;

public sealed class MyCard : CardModel
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        // 展示基础版卡牌
        HoverTipFactory.FromCard<SovietEngineer>(),
        // 展示升级后的卡牌（upgrade: true）
        HoverTipFactory.FromCard<SovietEngineer>(upgrade: true)
    ];
}
```

**示例2：展示能力预览**

```csharp
using MegaCrit.Sts2.Core.HoverTips;

public sealed class PoisonStab : CardModel
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        // 展示毒药能力，指定层数
        HoverTipFactory.FromPower<PoisonPower>(3)
    ];
}
```

**示例3：混合展示卡牌和能力**

```csharp
using MegaCrit.Sts2.Core.HoverTips;

public sealed class BladeOfInk : CardModel
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        // 展示卡牌预览
        HoverTipFactory.FromCard<InkyShiv>(),
        // 展示能力预览
        HoverTipFactory.FromPower<WeakPower>()
    ];
}
```

### 3.10.5 能力中的悬浮提示

能力类也可以通过重写 `ExtraHoverTips` 属性来展示其他能力或卡牌的预览：

```csharp
using MegaCrit.Sts2.Core.HoverTips;

public sealed class MyBuff : PowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<DexterityPower>(),
        HoverTipFactory.FromPower<ThornsPower>()
    ];
}
```

### 3.10.6 效果说明

当玩家将鼠标悬浮在卡牌上时，这些悬浮提示会自动显示在卡牌描述的下方，展示对应的卡牌或能力的详细信息。以游戏中的 "Blade of Ink" 为例，它展示了 "Inky Shivs" 卡牌和 "Weak" 能力的悬浮提示，这相当于：

```csharp
protected override IEnumerable<IHoverTip> ExtraHoverTips =>
[
    HoverTipFactory.FromCard<InkyShiv>(),
    HoverTipFactory.FromPower<WeakPower>()
];
```

### 3.10.7 动态悬浮Tip升级机制（HoverTipHelper）

当卡牌的衍生卡效果会随升级而变化时，使用 `HoverTipHelper` 可以根据源卡牌的升级状态动态显示对应版本的衍生卡牌。

#### 核心原理

传统的 `HoverTipFactory.FromCard<T>()` 只能显示固定版本的卡牌预览，无法根据源卡牌的升级状态动态调整。`HoverTipHelper` 通过传入一个 `Func<bool>` 委托来判断当前卡牌是否已升级，从而生成对应版本的悬浮提示。

#### 使用示例

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

#### HoverTipHelper 工具类实现

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

#### 使用场景

| 场景 | 说明 |
|------|------|
| 建筑卡生产单位卡 | 矿场升级后，生产的矿车也会升级 |
| 超级武器建筑 | 升级后冷却回合减少，产生的超级武器卡牌效果增强 |
| 能力卡衍生效果 | 能力卡升级后，衍生卡牌的数值或效果发生变化 |

#### 注意事项

- 使用前需要添加引用：`using RedAlert2ModCode.Common.Utils;`
- 泛型参数必须是已注册的卡牌类型
- 委托 `() => IsUpgraded` 使用了卡牌的 `IsUpgraded` 属性，可以替换为自定义的升级判断逻辑

### 3.10.8 注意事项

1. **using 引用**：使用 `HoverTipFactory` 前需要添加 `using MegaCrit.Sts2.Core.HoverTips;`
2. **泛型类型**：`FromCard<T>`、`FromPower<T>` 中的泛型参数必须是已注册的卡牌或能力类型
3. **升级参数**：`FromCard<T>(upgrade: true)` 会生成升级后的卡牌预览
4. **层数参数**：`FromPower<T>(amount)` 可以指定能力的层数显示
5. **动态升级机制**：使用 `HoverTipHelper.FromCardWithUpgrade<T>()` 实现随升级状态变化的悬浮提示

---

## 4. 自定义药水

### 4.1 药水核心属性

```csharp
public sealed class MyAoEPotion : PotionModel
{
    public override PotionRarity Rarity => PotionRarity.Rare;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.AllEnemies;
    
    protected override List<DynamicVar> CanonicalVars => new() { new DamageVar(30m, ValueProp.Unpowered) };
    
    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        await CreatureCmd.Damage(choiceContext, Owner.Creature.CombatState.HittableEnemies,
            DynamicVars.Damage.BaseValue, DynamicVars.Damage.Props, Owner.Creature, null);
    }
}
```

**使用时机（PotionUsage）：**
- `CombatOnly` - 只能在战斗中使用
- `AnyTime` - 可在战斗外任意时机使用
- `Automatic` - 不能主动使用，由游戏自动触发（如复活药水）

### 4.2 自动触发药水示例

```csharp
public sealed class MyRevivePotion : PotionModel
{
    public override PotionRarity Rarity => PotionRarity.Rare;
    public override PotionUsage Usage => PotionUsage.Automatic;
    public override TargetType TargetType => TargetType.Self;
    public override bool CanBeGeneratedInCombat => false;
    
    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        await CreatureCmd.Heal(target, 10m);
    }
    
    public override bool ShouldDie(Creature creature) => creature != Owner.Creature;
    
    public override async Task AfterPreventingDeath(Creature creature)
    {
        await OnUseWrapper(new ThrowingPlayerChoiceContext(), creature);
    }
}
```

### 4.3 注册到药水池

```csharp
ModHelper.AddModelToPool(typeof(SharedPotionPool), typeof(MyAoEPotion));
```

**常见药水池：**
- `SharedPotionPool` - 所有角色共享
- `IroncladPotionPool`, `SilentPotionPool` - 角色专属
- `EventPotionPool` - 事件药水
- `TokenPotionPool` - 衍生药水

### 4.4 资源路径

```
res://images/potions/<药水ID小写>.png
res://images/atlases/potion_atlas.sprites/<药水ID小写>.tres
res://images/atlases/potion_outline_atlas.sprites/<药水ID小写>.tres
```

### 4.5 本地化文本

`res://<ModID>/localization/zhs/potions.json`:
```json
{
  "MY_AOE_POTION.title": "手雷",
  "MY_AOE_POTION.description": "对所有敌人造成 {Damage} 点伤害。"
}
```

---

## 5. 卡牌附魔

### 5.1 附魔基类

```csharp
public sealed class MyCustomEnchantment : EnchantmentModel
{
    public override bool ShowAmount => true;
    public override bool HasExtraCardText => true;
    
    protected override List<DynamicVar> CanonicalVars => new()
    {
        new DamageVar(0m, ValueProp.Move),
        new BlockVar(0m, ValueProp.Move)
    };
    
    public override bool CanEnchantCardType(CardType cardType)
    {
        return cardType == CardType.Attack;  // 只能附魔攻击牌
    }
    
    public override void RecalculateValues()
    {
        DynamicVars.Damage.BaseValue = Amount;
        DynamicVars.Block.BaseValue = Amount;
    }
    
    public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
    {
        await CreatureCmd.GainBlock(Card.Owner.Creature, DynamicVars.Block, cardPlay);
    }
    
    public override decimal EnchantDamageAdditive(decimal originalDamage, ValueProp props)
    {
        if (Status == EnchantmentStatus.Disabled) return 0m;
        bool isPoweredAttack = props.HasFlag(ValueProp.Move) && !props.HasFlag(ValueProp.Unpowered);
        return isPoweredAttack ? DynamicVars.Damage.BaseValue : 0m;
    }
}
```

### 5.2 关键方法

| 方法 | 说明 |
|------|------|
| `CanEnchantCardType` | 限制可附魔的卡牌类型 |
| `OnEnchant` | 附魔被添加时触发 |
| `RecalculateValues` | 层数变化时重新计算数值 |
| `OnPlay` | 被附魔卡牌打出时触发 |
| `EnchantDamageAdditive` | 提供额外伤害 |
| `EnchantBlockAdditive` | 提供额外格挡 |

### 5.3 给卡牌添加附魔

```csharp
CardCmd.Enchant<MyCustomEnchantment>(card, 1m);  // 层数为1
```

### 5.4 资源路径

```
res://images/enchantments/<附魔ID小写>.png
```

### 5.5 本地化文本

`res://<ModID>/localization/zhs/enchantments.json`:
```json
{
  "MY_CUSTOM_ENCHANTMENT.title": "谨慎",
  "MY_CUSTOM_ENCHANTMENT.description": "这张牌额外造成{Damage}点[gold]伤害[/gold]。\n打出这张牌时，提供{Block}点[gold]格挡[/gold]。",
  "MY_CUSTOM_ENCHANTMENT.extraCardText": "增加 {Amount} 伤害"
}
```

---

## 6. 自定义能力（Buff）

### 6.1 能力基类

```csharp
public sealed class MyBlockOnPlayBuff : PowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool IsInstanced => false;
    public override bool AllowNegative => false;
    
    protected override List<IHoverTip> ExtraHoverTips => new()
    {
        HoverTipFactory.Static(StaticHoverTip.Block)
    };
    
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != Owner) return;
        if (Amount <= 0) return;
        
        Flash();
        await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null, fast: true);
    }
    
    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side == Owner.Side)
        {
            await PowerCmd.Decrement(this);
        }
    }
}
```

### 6.2 能力属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `Type` | `PowerType` | Buff（增益）/ Debuff（减益） |
| `StackType` | `PowerStackType` | Counter（右下角显示层数数值，如力量）/ Single（无层数只显示图标，如虚弱） |
| `InstanceType` | `PowerInstanceType`（枚举，推荐使用） | 重复施加时的实例策略，见下表详解 |
| `AllowNegative` | `bool` | 是否允许 Amount 为负数 |
| `IsInstanced`（旧） | `bool` | 过时API，等效于 `InstanceType = Instanced` / `None`，建议改用枚举版 |

#### PowerInstanceType 枚举详解（解包 `sts2.dll` 源码确认）

控制 `PowerCmd.Apply` 时是"叠加 Amount 到已有实例"还是"新建独立实例"，直接决定能力"会不会叠层"。

| 枚举值 | PowerCmd.Apply 内部行为（PowerCmd.cs:167-173） | 含义与适用场景 | 红警Mod示例 |
|--------|-----------------------------------------------|---------------|------------|
| `None`（默认） | 用 `target.GetPower(Id)` 查找同ID实例 → 找到就 ModifyAmount 叠加 Amount，找不到才新建。Creature.AddPower 还会校验非 Instanced 类型不许重复添加。 | 再打一张 = 在同一份效果上"加数值"，**不需要每个实例独立状态**。 | DollarPower（资金）、TechPointPower（科技点）、Strength/Dexterity/Vulnerable 等纯数值 Buff/Debuff |
| `Instanced` | **查找 existing 直接返回 null → 每次 Apply 都新建独立实例**，每个实例 Amount 独立。 | 再打一张 = "又多了一个独立单元"，每个实例需要**独立的自定义字段/状态/倒计时**。 | 核电站（独立血量）、宝石矿/黄金矿（独立储备）、飞鹰500kg/闪电风暴（独立战备触发计数）、作战实验室（独立生产序列）、自爆卡车（独立爆炸触发器） |
| `InstancedPerApplier` | 按 `Applier`（施放者）匹配实例 → 同一施放者叠加 Amount，不同施放者新建实例。 | 多人联机场景下按玩家区分效果。 | 联机模式下不同玩家分别施加的削弱类效果。 |

> **⚠️ 最常见的坑**：`InstanceType` 已经设为 `Instanced`（要独立实例），但在卡牌 OnPlay 里又手写 `owner.Powers.OfType<YourPower>().FirstOrDefault() → ModifyAmount`，这段手动查找和叠加会完全绕过框架的 InstanceType 机制，导致永远叠不上独立实例。正确做法：**`Instanced` + 直接 Apply，让框架自己每次新建。**

#### 场景速选："我这能力到底要叠层（Amount）还是不叠层（独立实例）？"

| 场景问题 | 选择 | 参数配置 |
|---------|------|---------|
| 再打一张力量卡，是"力量+2"，还是又多了一个"力量图标"？ | 数值相加，**叠层** | `InstanceType = None` + `StackType = Counter` |
| 再打一张核电站卡，是"核电站血量更厚"，还是又多了一座独立的核电站？ | 多一座独立建筑，**不叠层（独立实例）** | `InstanceType = Instanced` + `StackType = Counter/Single` |
| 再打一张宝石矿卡，是"合并到同一个储备值"，还是"每座矿独立记储备"？ | 每座矿独立 → **Instanced**（当前红警Mod实现）<br>合并储备 → **None** + 手动 AddReserve | 根据玩法选 |
| 战备类技能：再打一张飞鹰500kg，是"伤害加倍"还是"每回合多触发一次空袭"？ | 独立触发 → **Instanced**<br>数值加倍 → **None** | 根据玩法选 |

#### 红警Mod全能力默认配置速查表

| 能力类型 | InstanceType | 理由 |
|---------|-------------|------|
| 建筑类（核电站、矿场、雷达、作战实验室、重工、兵营、碉堡、线圈、光棱塔等） | `Instanced` | 每座建筑独立，出售时需要逐个确认 |
| 资源计数（资金 DollarPower、能量、科技点） | `None` | 所有来源合并一个数值 |
| 单位计数（单位血量、单位面板实例） | 对应单位实例独立管理 | 不走 Power 叠层 |
| 战斗状态（中毒 Poison、虚弱 Weak、力量 Strength、格挡 Block） | `None` | 原版机制，纯数值 |
| 战备/回合触发（飞鹰500kg、闪电风暴、核弹冷却等） | `Instanced` | 多战备独立触发，互不干扰 |

### 6.3 常用事件钩子

```csharp
OnApplied          // 能力被施加时
OnRemoved          // 能力被移除时
BeforeTakeDamage   // 受到伤害前
AfterTakeDamage    // 受到伤害后
OnTurnStart        // 回合开始时
OnCardDrawn        // 抽牌时
ModifyDamage       // 修改伤害数值
ModifyBlock        // 修改格挡数值
```

### 6.4 施加与移除能力

```csharp
// 施加
await PowerCmd.Apply<MyBlockOnPlayBuff>(target, amount, source, sourceCard);

// 移除
await PowerCmd.Remove(powerInstance);
```

---

### 6.5 Power 高级模式（推荐）：不借助遗物，Power 自监听「未格挡伤害」+ 动态状态描述注入

#### 旧模式（自爆卡车）vs 新模式（核电站）对比

这是以后"带独立状态 + 受击触发"类能力（地雷、炮塔、自爆单位、护盾装置、可破坏建筑等）**一定会反复遇到**的设计选择。先把两种方案的差异一次性讲透，避免走弯路。

| 维度 | 旧模式：遗物中写监听（如自爆卡车） | **新模式：Harmony 广播补丁 + Power 自监听（如核电站） ⭐ 推荐** |
|------|----------------------------------|-------------------------------------------------------------|
| **耦合结构** | 能力逻辑 + 遗物逻辑两个类，必须先有遗物才能触发监听 | **只有 Power 一个类**，Harmony 补丁充当"事件总线"，不产生任何游戏内实体 |
| **遗物实例必须存在？** | ✅ 是。玩家没有该遗物 → 能力根本无法响应伤害事件 | ❌ **不需要**。Postfix 挂的是 `RelicModel.AfterDamageReceived` 的「方法签名」→ 所有 HookListener 经过的广播出口，不要求任何遗物存在 |
| **UnblockedDamage 精度** | ✅ 同精度（都是同一个 Hook 点） | ✅ 同精度（都是同一个 Hook 点） |
| **多个同类能力（多座核电站 / 多辆自爆卡车）** | 需要遗物内部遍历 `OfType<YourPower>()` 逐个处理，还要处理"多遗物叠加 + 多实例"的双重去重 | **天然解耦**：补丁统一做一次外层去重，然后遍历 powers.ForEach(p.Receive())，每个 Power 独立去重 |
| **触发者身份** | 怪物/中立生物受击时遗物不监听（遗物只在玩家身上）→ 怪物带能力（如 Boss 身上有核装置）无法触发 | **任何 Creature 受击都能触发**（Postfix 参数 target 是受伤者，不区分玩家/怪物），对 Boss 战设计非常友好 |
| **代码复用性** | 每个需要监听伤害的能力要配一个遗物类 + 遗物注册 + 遗物获取条件（卡牌 OnPlay 中给玩家加遗物） | **一个补丁支持 N 种 Power**：只需在 Postfix 的 OfType 行里 `switch` 或调用多个 `DispatchToPower<T>()` 即可 |
| **反模式风险** | 玩家身上会有大量"看不见但实际存在的监听遗物"，战斗结束时如果没正确清理，可能泄漏状态 | 零额外游戏实体。补丁只做转发，不持有任何玩家状态 |

#### 新模式五步实现（完整工程落地）

和 API 快速参考中的完整模板一致，这里再按"完整教程"节奏拆解知识点：

##### 第 1 步：理解「为什么 RelicModel.AfterDamageReceived 不需要遗物存在」

sts2.dll 的 CreatureCmd.Damage 结算完格挡后，会执行一段固定的广播链：

```csharp
// 伪代码： CreatureCmd.Damage 内部
foreach (var listener in combatState.IterateCombatHookListeners())
{
    if (listener is RelicModel relic)
        relic.AfterDamageReceived(ctx, target, result, props, dealer, cardSource);  // ★我们 Postfix 这个
    else if (listener is PowerModel power)
        power.AfterTakeDamage(ctx, dealer, target, result, props, cardSource);       // 不稳定，不推荐
    else if (listener is CardModel card)
        card.AfterDamageReceived(ctx, target, result, props, dealer, cardSource);
}
```

`Harmony Postfix` 的本质是：**在 RelicModel.AfterDamageReceived 这个方法「每次被调用完」之后，插入我们自己的代码**。

- 它不关心调用者是 Relic1 还是 Relic5，也不关心玩家身上有没有 Relic
- 只要框架"准备/正在"广播（即任何 Relic 实例将要/已经执行 AfterDamageReceived 方法体）→ 我们的 Postfix 就能偷听到参数
- 所以 Postfix 被触发 N 次 = 本次战斗有 N 个 Relic/其他 HookListener 在接收广播 = 需要外层 `_processedGlobalEvents` 去重 N→1

##### 第 2 步：补丁只做三件事（单一职责）

不要在补丁里写具体业务逻辑（比如爆炸、施加 Poison）。补丁只负责：

```
过滤（95% 快速失败 return） → 去重（N次广播变1次） → 分发 target.Powers.OfType<YourPower>().Each(p.Receive())
```

##### 第 3 步：Power 自接收 — 每个实例独立处理自己的状态

`ReceiveUnblockedDamage(int unblockedDamage, int eventHashCode)` 是 Power 自己的 public 方法，做三件事：

```
_isExploding 防连锁 → _processedDamageEventIds 内层去重 → CurrentHealth -= unblocked → 阈值判断
```

去重为什么要两层？用表格讲透：

| 层次 | 发生时机 | 解决什么问题 |
|------|---------|-------------|
| 外层 `_processedGlobalEvents`（补丁静态字段） | 补丁 Postfix 入口（**所有 Power 共用同一个 hashset**） | 5 个 Relic 收到同一次伤害 → Postfix 触发 5 次 → 外层 hash 命中 4 次 return，1 次继续分发 |
| 内层 `_processedDamageEventIds`（Power 实例字段） | Power.Receive() 内部（**每个 Power 实例各有自己的 hashset**） | 极端情况：前一次 hash 刚好和新一次碰撞 + 怪物恰好有 2 座核电站实例 A/B → 外层 hash 碰撞认为是同一次只发 1 次 → 内层每个实例自己记录，A 命中跳过 B 也命中跳过 = 零概率错漏 + 也兜住"前一次没清干净 4096 重置边界"bug |

##### 第 4 步：Description getter 动态注入（状态显示 = 零手动刷新）

这一步的原理和"为什么 `{CurrentHealth}` 能实时显示"是同一个知识点：

- SlayTheSpire 2 的 `LocString` 不是"存了值的字符串"，是"值绑定方案 + 访问时求值"
- 框架每次需要渲染能力悬浮 tip（鼠标移上去、战斗结束结算、过场动画展示等）都会调一次 `power.Description` → **触发 getter**
- 所以在 getter 里写 `locString.Add("CurrentHealth", CurrentHealth)` → 注入的是那个瞬间字段的当前值 → 玩家看到的永远是最新的

```csharp
// 正确写法（每次 get 重新 new + 注入当前字段值）
public override LocString Description
{
    get
    {
        var locString = new LocString("powers", Id.Entry + ".description");
        locString.Add("CurrentHealth", CurrentHealth);  // 字段，不是常量
        return locString;
    }
}

// 错误写法 1：构造函数里 new 一次存字段，后续永远显示初始值
private LocString _cachedDesc;  // ← 字段缓存，永远不刷新
public MyPower() { _cachedDesc = new LocString(...).Add("CurrentHealth", Values.Damage); }
public override LocString Description => _cachedDesc;  // ← 永远是初始值

// 错误写法 2：Description.get 里注入常量，虽然每次 new 但显示旧值
locString.Add("CurrentHealth", Values.Damage);  // ← Values 是常量，不是 CurrentHealth 字段
```

##### 第 5 步：防重入（爆炸引起的 Poison 再触发爆炸必须拦住）

这是"带副作用的伤害型效果"最容易炸的点。用状态机理解：

```
正常状态: _isExploding = false
   ↓ 受击
触发爆炸: _isExploding = true;  // 先设标志，再做任何副作用
   ↓ 施加 Poison（PowerCmd.Apply<PoisonPower>）
   ↓ Poison 立即结算回合内 tick（CreatureCmd.Damage）
   ↓ CreatureCmd.Damage 又广播 RelicModel.AfterDamageReceived
   ↓ 补丁又 Postfix，又遍历 OfType<NuclearReactorCorePower>，又调 Receive()
   ↓ Receive() 第一行：
     if (_isExploding) { GD.Print("爆炸进行中，忽略"); return; }  // ★就拦在这里！
   ↓ 不会再扣血，不会连锁触发第二次、第三次爆炸
   ↓ 施加完 Poison，回到 TriggerEffectAsync 尾部
   ↓ PowerCmd.Remove(this) 移除能力实例
```

如果没这个标志，典型日志会这样（本次调试早期真的出现过）：

```
受 11 点未格挡伤害，血量 -7 → 爆炸音效 → 受 11 点（再次扣血）→ 爆炸音效 → 受 11 点
（连锁 30+ 次，最后才打印"赋予中毒"，因为 Poison 一直在结算里排队）
```

---

#### 何时用旧模式（遗物监听）？

只有一种情况可以接受遗物模式：**监听逻辑天然和玩家身份绑定，且需要在多次战斗间持续生效**（比如"所有建筑获得 +2 血量上限"这种遗物效果）。除此之外，所有"战斗内、能力本身需要响应伤害"的情况，一律用核电站模式——代码更少、耦合更低、精度更高、不污染遗物池。

### 6.6 能力图标配置（原 6.5，序号顺延）

由于 `PowerModel.Icon` 属性不是 `virtual` 的，无法通过重写来设置自定义图标。需要使用 `PowerIconPatch` 来拦截图标获取：

#### 实现步骤

1. **创建 Harmony 补丁类**：

```csharp
[HarmonyPatch]
public static class PowerIconPatch
{
    // 能力类型到图标路径的映射字典
    private static readonly Dictionary<Type, string> _customIconPaths = new()
    {
        { typeof(MyBlockOnPlayBuff), "res://images/powers/my_block_on_play_buff.png" },
        { typeof(TransportShipPower), "res://RedAlert2ModResources/images/packed/card_portraits/allies/landicon.png" },
        { typeof(Eagle500kgPower), "res://RedAlert2ModResources/images/packed/powers/Eagle500kgPower.png" },
        // 添加更多能力类型和图标路径
    };

    // 拦截 Icon 属性（战斗界面底部状态栏显示的小图标）
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

    // 拦截 PackedIconPath 属性
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

    // 拦截 BigIcon 属性（悬停提示时显示的大图标）
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

1. **新增能力类型后，必须将其添加到 `_customIconPaths` 字典中**，否则图标将无法正常显示。例如添加 `Eagle500kgPower` 后：
```csharp
{ typeof(Eagle500kgPower), "res://RedAlert2ModResources/images/packed/powers/Eagle500kgPower.png" },
```

2. **图标文件存放位置**：建议将能力图标放在 `RedAlert2ModResources/images/packed/powers/` 目录下。

3. **图标路径格式**：`res://RedAlert2ModResources/images/packed/powers/<能力名称>Power.png`

**常见问题排查**：

如果图标不显示，按以下顺序检查：

| 检查项 | 说明 |
|--------|------|
| `_customIconPaths` 注册 | 确认能力类型已添加到字典中 |
| 图标文件路径 | 确认路径拼写正确，区分大小写 |
| 文件存在性 | 确认图标文件确实存在于指定位置 |
| 图标格式 | 确保是有效的 PNG 格式图片 |
| 缓存问题 | 尝试清理游戏缓存后重新测试 |

**调试技巧**：可以在 `IconPrefix` 方法中添加日志输出来验证是否正确拦截了图标获取：
```csharp
GD.Print($"[PowerIconPatch] 拦截能力图标: {type.FullName}, 路径: {iconPath}");
```

### 6.6 资源路径

```
res://images/powers/<能力ID小写>.png
res://images/atlases/power_atlas.sprites/<能力ID小写>.tres
```

### 6.7 本地化文本

`res://<ModID>/localization/zhs/powers.json`:
```json
{
  "MY_BLOCK_ON_PLAY_BUFF.title": "格挡精进",
  "MY_BLOCK_ON_PLAY_BUFF.description": "打出牌时会提供格挡。",
  "MY_BLOCK_ON_PLAY_BUFF.smartDescription": "每当你打出一张牌，获得 {Amount} 点[gold]格挡[/gold]。"
}
```

### 6.8 数值可变能力的叠加逻辑

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

---

## 7. 自定义事件

### 7.1 基础事件

```csharp
public class MyCustomEvent : EventModel
{
    protected override List<DynamicVar> CanonicalVars => new()
    {
        new StringVar("MyCustomCard", ModelDb.Card<MyCustomCard>().Title),
        new GoldVar(50)
    };
    
    public override bool IsAllowed(RunState runState) => true;
    
    protected override List<EventOption> GenerateInitialOptions()
    {
        return new List<EventOption>
        {
            new EventOption(this, ActMeditation, InitialOptionKey("MEDITATION")),
            new EventOption(this, ActRecharge, InitialOptionKey("LEAVE"))
        };
    }
    
    private async Task ActMeditation()
    {
        CardModel card = Owner.RunState.CreateCard<MyCustomCard>(Owner);
        await CardPileCmd.Add(card, PileType.Deck);
        SetEventFinished(L10NLookup("MY_CUSTOM_EVENT.pages.MEDITATION.description"));
    }
    
    private Task ActRecharge()
    {
        PlayerCmd.GainGold(DynamicVars.Gold.BaseValue, Owner);
        SetEventFinished(L10NLookup("MY_CUSTOM_EVENT.pages.LEAVE.description"));
        return Task.CompletedTask;
    }
}
```

### 7.2 多页选项

```csharp
private Task ActGaze()
{
    SetEventState(
        L10NLookup("MY_CUSTOM_EVENT.pages.GAZE.description"),
        new List<EventOption>
        {
            new EventOption(this, async () => {
                await RelicCmd.Obtain(ModelDb.Relic<Pear>().ToMutable(), Owner);
                SetEventFinished(L10NLookup("MY_CUSTOM_EVENT.pages.GAZE_PEAR.description"));
            }, "MY_CUSTOM_EVENT.pages.GAZE.options.PEAR", HoverTipFactory.FromRelic<Pear>())
        }
    );
    return Task.CompletedTask;
}
```

### 7.3 添加到游戏（HarmonyPatch）

```csharp
[HarmonyPatch(typeof(Overgrowth), nameof(Overgrowth.AllEvents), MethodType.Getter)]
public static class OvergrowthAllEventsPatch
{
    static void Postfix(ref IEnumerable<EventModel> __result)
    {
        __result = __result.Concat(new[] { ModelDb.Event<MyCustomEvent>() }).Distinct();
    }
}
```

**章节类对应：**
- `Overgrowth` - 第一层（密林）
- `Hive` - 第二层
- `Beyond` - 第三层

### 7.4 资源路径

```
res://images/events/<事件ID小写>.png  # 背景图 (3440×1613)
```

### 7.5 本地化文本

`res://<ModID>/localization/zhs/events.json`:
```json
{
  "MY_CUSTOM_EVENT.title": "幻境苹果树",
  "MY_CUSTOM_EVENT.pages.INITIAL.description": "你的眼前出现一棵结满苹果的苹果树...",
  "MY_CUSTOM_EVENT.pages.INITIAL.options.MEDITATION.title": "沉思",
  "MY_CUSTOM_EVENT.pages.INITIAL.options.MEDITATION.description": "将一张 {MyCustomCard} 放入手牌。"
}
```

### 7.6 先古之民事件（AncientEventModel）

```csharp
public sealed class MyAncient : AncientEventModel
{
    public override List<EventOption> AllPossibleOptions => new()
    {
        new EventOption(this, TakeGold, InitialOptionKey("TAKE_GOLD"))
    };
    
    protected override AncientDialogueSet DefineDialogues()
    {
        return new AncientDialogueSet
        {
            FirstVisitEverDialogue = new AncientDialogue("第一次见面的对话文本"),
            CharacterDialogues = new Dictionary<string, IReadOnlyList<AncientDialogue>>
            {
                [CharKey<Ironclad>()] = new[] { new AncientDialogue("铁甲战士专属对话") { VisitIndex = 0 } }
            },
            AgnosticDialogues = new[] { new AncientDialogue("通用对话") }
        };
    }
    
    protected override List<EventOption> GenerateInitialOptions() => AllPossibleOptions.ToList();
    
    private async Task TakeGold() { await PlayerCmd.GainGold(30, Owner); Done(); }
}
```

**先古之民本地化键格式：**
```
<先古ID>.talk.firstVisitEver.<对话组索引>-<台词行索引>.ancient
<先古ID>.talk.<角色ID>.<索引>.ancient
<先古ID>.talk.ANY.<索引>r.ancient  # 注意带r
```

---

## 8. 自定义角色

### 8.1 角色基类

```csharp
public sealed class Watcher : CharacterModel
{
    public override int StartingHp => 72;
    public override int StartingGold => 99;
    public override CardPoolModel CardPool => ModelDb.CardPool<WatcherCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<WatcherRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<WatcherPotionPool>();
    public override CharacterModel? UnlocksAfterRunAs => null;  // null表示初始可用
    
    // UI颜色
    public override Color NameColor => Colors.Purple;
    public override Color DialogueColor => Colors.LightPurple;
    public override Color MapDrawingColor => Colors.Purple;
}
```

### 8.2 角色关联池

#### 卡池（CardPoolModel）
```csharp
public class WatcherCardPool : CardPoolModel
{
    public override string Title => "Watcher";  // 影响卡牌图标路径
    public override string EnergyColorName => "purple";
    public override string CardFrameMaterialPath => "materials/card_frame_watcher.tres";
    public override Color DeckEntryCardColor => Colors.Purple;
    public override Color EnergyOutlineColor => Colors.DarkPurple;
    
    public override List<CardModel> GenerateAllCards()
    {
        return new List<CardModel>
        {
            // 返回该角色卡池中的所有卡牌
        };
    }
}
```

#### 遗物池（RelicPoolModel）
```csharp
public class WatcherRelicPool : RelicPoolModel
{
    public override List<RelicModel> GenerateAllRelics()
    {
        return new List<RelicModel>
        {
            // 返回该角色专属遗物
        };
    }
}
```

#### 药水池（PotionPoolModel）
```csharp
public class WatcherPotionPool : PotionPoolModel
{
    public override List<PotionModel> GenerateAllPotions()
    {
        return new List<PotionModel>
        {
            // 返回该角色专属药水
        };
    }
}
```

### 8.3 必需资源列表

| 资源类型 | 路径 |
|---------|------|
| 待机动画场景 | `res://scenes/creature_visuals/<角色ID>.tscn` |
| 头像图标场景 | `res://scenes/ui/character_icons/<角色ID>_icon.tscn` |
| 能量计数器场景 | `res://scenes/combat/energy_counters/<角色ID>_energy_counter.tscn` |
| 商店待机动画 | `res://scenes/merchant/characters/<角色ID>_merchant.tscn` |
| 篝火休息动画 | `res://scenes/rest_site/characters/<角色ID>_rest_site.tscn` |
| 头像纹理 | `res://images/ui/top_panel/character_icon_<角色ID>.png` |
| 角色选择背景图 | `res://images/packed/character_select/char_select_<角色ID>.png` |
| 卡牌拖尾特效 | `res://scenes/vfx/card_trail_<角色ID>.tscn` |

**场景结构要点：**
所有角色的视觉场景根节点必须是挂载了特殊脚本（如 `NCreatureVisuals`）的 `Node2D`，内部必须包含名为 `%Visuals`, `%Bounds`, `%IntentPos`, `%CenterPos` 等特定子节点。

### 8.4 注册角色（HarmonyPatch）

```csharp
// 添加候选角色
[HarmonyPatch(typeof(ModelDb), nameof(ModelDb.AllCharacters), MethodType.Getter)]
public static class AllCharactersPatch
{
    static void Postfix(ref IEnumerable<CharacterModel> __result)
    {
        __result = __result.Append(new Watcher()).Distinct();
    }
}

// 添加关联池
[HarmonyPatch(typeof(ModelDb), nameof(ModelDb.AllCardPools), MethodType.Getter)]
public static class AllCardPoolsPatch
{
    static void Postfix(ref IEnumerable<CardPoolModel> __result)
    {
        __result = __result.Append(ModelDb.CardPool<WatcherCardPool>()).Distinct();
    }
}

// 同理添加 AllRelicPools 和 AllPotionPools
```

### 8.5 本地化文本

`res://<ModID>/localization/zhs/characters.json`:
```json
{
  "WATCHER.title": "观者",
  "WATCHER.description": "一名目盲的修行者...",
  "WATCHER.pronounObject": "她"
}
```

### 8.6 重要注意事项

**Spine动画导入：**
- Godot不直接支持Spine动画
- 需要下载Godot-Spine的GDExtension插件并放置在 `bin/` 文件夹下
- Spine的JSON文件后缀需改为 `.spine-json`

**脚本检索问题：**
```csharp
// 在ModInitializer中调用
Godot.Bridge.ScriptManagerBridge.LookupScriptsInAssembly(Assembly.GetExecutingAssembly());
```

**音效配置：**
- 固定FMOD路径格式：`event:/sfx/characters/<角色ID>/...`
- 可重写 `CharacterSelectSfx` 等属性借用其他角色音效

---

## 9. 自定义敌怪

### 9.1 怪物基类

```csharp
public sealed class MyCustomMonster : MonsterModel
{
    public override int MinInitialHp => 30;
    public override int MaxInitialHp => 34;
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState attack = new MoveState(
            "ATTACK_STATE",
            AttackMove,
            new SingleAttackIntent(8)
        );
        attack.FollowUpState = attack;  // 循环攻击
        
        return new MonsterMoveStateMachine(new List<MonsterState> { attack }, attack);
    }
    
    private async Task AttackMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(8)
            .FromMonster(this)
            .WithHitFx("vfx/vfx_attack_blunt")  // 添加攻击特效
            .Execute(null);
    }
}
```

### 9.2 怪物意图
```csharp
// 单体攻击意图
new SingleAttackIntent(damage)

// 群体攻击意图
new AoeAttackIntent(damage)

// 防御意图
new DefendIntent(block)

// 增益意图
new BuffIntent(buffAmount)
```

### 9.3 怪物遭遇
```csharp
public sealed class MyCustomEncounter : EncounterModel
{
    public override RoomType RoomType => RoomType.Monster;
    public override bool IsWeak => true;
    
    public override List<MonsterModel> AllPossibleMonsters => new()
    {
        ModelDb.Monster<MyCustomMonster>()
    };
    
    protected override List<(MonsterModel, string?)> GenerateMonsters()
    {
        return new() { (ModelDb.Monster<MyCustomMonster>().ToMutable(), null) };
    }
}
```

### 9.4 注册遭遇
```csharp
[HarmonyPatch(typeof(Overgrowth), nameof(Overgrowth.GenerateAllEncounters))]
public static class EncountersPatch
{
    static void Postfix(ref IEnumerable<EncounterModel> __result)
    {
        __result = __result.Concat(new[] { ModelDb.Encounter<MyCustomEncounter>() }).Distinct();
    }
}
```

### 9.5 资源路径
```
res://images/monsters/<怪物ID>/<怪物ID>_000.png
res://images/monsters/<怪物ID>/<怪物ID>_attack_000.png
```

---

## 10. 攻击特效（VFX）

### 10.1 特效概述

攻击特效是增强战斗视觉体验的重要组成部分。游戏提供了多种内置特效，同时也支持自定义特效。

### 10.2 内置特效类型

游戏解包资源中包含丰富的攻击特效：

| 特效名称 | 路径 | 适用场景 |
|---------|------|---------|
| `vfx_attack_slash` | `res://scenes/vfx/vfx_attack_slash.tscn` | 斩击类攻击 |
| `vfx_attack_blunt` | `res://scenes/vfx/vfx_attack_blunt.tscn` | 钝器类攻击 |
| `vfx_attack_stab` | `res://scenes/vfx/vfx_attack_stab.tscn` | 突刺类攻击 |
| `vfx_attack_lightning` | `res://scenes/vfx/vfx_attack_lightning.tscn` | 闪电类攻击 |
| `vfx_attack_fire` | `res://scenes/vfx/vfx_attack_fire.tscn` | 火焰类攻击 |
| `vfx_attack_frost` | `res://scenes/vfx/vfx_attack_frost.tscn` | 冰霜类攻击 |
| `vfx_attack_poison` | `res://scenes/vfx/vfx_attack_poison.tscn` | 毒素类攻击 |
| `vfx_smoke_puff` | `res://scenes/vfx/vfx_smoke_puff.tscn` | 烟雾效果 |

### 10.3 在伤害命令中使用特效

最常见的使用方式是在 `DamageCmd` 中通过 `WithHitFx()` 方法添加特效：

```csharp
protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
{
    await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
        .FromCard(this)
        .Targeting(cardPlay.Target)
        .WithHitFx("vfx/vfx_attack_slash")  // 指定特效路径
        .Execute(choiceContext);
}
```

### 10.4 使用VFX节点类创建特效

除了通过 `DamageCmd`，还可以直接使用VFX节点类手动创建特效：

```csharp
// 创建刺击特效
var stabVfx = NStabVfx.Create(target, goingRight: true);
NCombatRoom.Instance?.CombatVfxContainer.AddChild(stabVfx);

// 创建斩击特效
var slashVfx = NSlashVfx.Create(target, goingRight: true);
NCombatRoom.Instance?.CombatVfxContainer.AddChild(slashVfx);

// 创建火焰燃烧特效（带持续时间）
var fireVfx = NFireBurningVfx.Create(target, duration: 1.5f, goingRight: true);
NCombatRoom.Instance?.CombatVfxContainer.AddChild(fireVfx);

// 创建毒药冲击特效
var poisonVfx = NPoisonImpactVfx.Create(target, goingRight: true);
NCombatRoom.Instance?.CombatVfxContainer.AddChild(poisonVfx);
```

### 10.5 常用VFX节点类速查

| 节点类 | 说明 | 参数 |
|-------|------|------|
| `NStabVfx` | 刺击特效 | target, goingRight |
| `NSlashVfx` | 斩击特效 | target, goingRight |
| `NFireBurningVfx` | 火焰燃烧特效 | target, duration, goingRight |
| `NPoisonImpactVfx` | 毒药冲击特效 | target, goingRight |
| `NSmokePuffVfx` | 烟雾特效 | position |

### 10.6 创建自定义特效场景

#### 步骤1：准备特效图片

创建帧序列图片，命名格式为 `vfx_my_effect_00-03.png`（00到03为帧索引）。

#### 步骤2：创建场景文件

```gdscript
# res://scenes/vfx/vfx_my_custom_attack.tscn
[gd_scene load_steps=3 format=3]

[ext_resource type="Texture2D" path="res://images/vfx/vfx_my_custom_attack_00-03.png" id="1"]
[ext_resource type="Script" path="res://scripts/vfx/my_custom_vfx.cs" id="2"]

[node name="MyCustomVfx" type="Node2D"]
script = ExtResource("2")

[node name="Sprite" type="Sprite2D" parent="."]
texture = ExtResource("1")
centered = false

[node name="AnimationPlayer" type="AnimationPlayer" parent="."]
```

#### 步骤3：创建C#脚本

```csharp
public class MyCustomVfx : Node2D
{
    [Export] public Sprite2D Sprite;
    [Export] public AnimationPlayer AnimationPlayer;
    
    public static MyCustomVfx Create(Creature target, bool goingRight = true)
    {
        var scene = GD.Load<PackedScene>("res://scenes/vfx/vfx_my_custom_attack.tscn");
        var instance = scene.Instantiate<MyCustomVfx>();
        
        // 设置位置
        instance.Position = target.Position;
        instance.Scale = new Vector2(goingRight ? 1 : -1, 1);
        
        return instance;
    }
    
    public override void _Ready()
    {
        // 播放动画后自动销毁
        AnimationPlayer.Play("attack");
        AnimationPlayer.AnimationFinished += (animName) => QueueFree();
    }
}
```

### 10.7 特效资源路径规范

```
res://scenes/vfx/vfx_<特效名称>.tscn        # 场景文件
res://images/vfx/vfx_<特效名称>_00-03.png   # 帧序列图片
res://images/atlases/vfx_atlas.sprites/<特效名称>.tres  # 裁切纹理
res://scripts/vfx/<特效名称>.cs             # C#脚本（可选）
```

### 10.8 实战示例：组合特效

在 `Eagle500kgPower` 中使用组合特效：

```csharp
// 播放轰击特效
var hitVfx = NStabVfx.Create(target, goingRight: true);
if (hitVfx != null)
{
    NCombatRoom.Instance?.CombatVfxContainer.AddChild(hitVfx);
}

// 播放火焰燃烧特效
var fireVfx = NFireBurningVfx.Create(target, 1.5f, goingRight: true);
if (fireVfx != null)
{
    NCombatRoom.Instance?.CombatVfxContainer.AddChild(fireVfx);
}
```

### 10.9 特效性能优化

| 优化策略 | 说明 |
|---------|------|
| 复用场景 | 使用 `PackedScene` 复用而不是每次创建新场景 |
| 限制数量 | 避免同时播放过多特效 |
| 及时销毁 | 使用 `QueueFree()` 在动画结束后销毁节点 |

---

## 11. 联机同步（Multiplayer Sync）

### 11.1 设计理念

自定义UI面板（如卡牌选择、建筑出售、工程师选择等）在联机模式下必须确保同步，否则会导致客户端状态不一致（StateDivergence）。核心原则是：**仅本地玩家显示和操作面板，其他玩家等待结果同步**。

### 11.2 MultiplayerSyncHelper 工具类

该工具类封装了联机同步的核心逻辑，提供统一的同步接口：

```csharp
public static class MultiplayerSyncHelper
{
    // 判断是否为联机游戏
    public static bool IsMultiplayerGame()
    
    // 判断玩家是否为本地玩家
    public static bool IsLocalPlayer(Player player)
    
    // 单选同步：返回选中项的索引（null表示取消）
    public static Task<int?> ExecuteSyncChoice(Player player, Func<Task<int?>> localChoiceFunc)
    
    // 多选同步：返回选中项的索引列表
    public static Task<List<int>> ExecuteSyncMultiChoice(Player player, Func<Task<List<int>?>> localChoiceFunc)
}
```

**核心工作原理**：

1. **单机模式**：直接执行本地选择函数
2. **联机模式-本地玩家**：显示面板，获取选择结果，同步给其他玩家
3. **联机模式-远程玩家**：等待本地玩家的选择结果，不显示面板

### 11.3 UI面板设计模式

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

### 11.4 单选同步示例（工程师选择）

适用于只需选择一个选项的场景：

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

**实现要点**：
1. 创建数据副本 `choicesCopy`，避免并发修改
2. 在本地选择函数中，通过 `FindIndex` 将对象转换为索引
3. 在同步方法返回后，通过索引从副本中恢复选中对象

### 11.5 多选同步示例（出售建筑）

适用于可选择多个选项的场景：

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

**实现要点**：
1. 创建数据副本 `itemsCopy`
2. 本地选择函数直接返回索引列表（因为 `ShowSelection` 已经返回索引）
3. `ExecuteSyncMultiChoice` 返回 `List<int>` 类型的索引列表

### 11.6 在卡牌中使用同步方法

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

### 11.7 关键注意事项

| 注意事项 | 说明 |
|---------|------|
| **数据复制** | 在同步方法中必须创建数据副本，避免并发修改导致的状态不一致 |
| **索引传递** | 通过索引而非对象引用传递选择结果，确保不同客户端间的一致性 |
| **Close方法** | 所有自定义UI面板必须实现 `public void Close()` 方法，用于清理非本地玩家的面板 |
| **IsLocalPlayer检查** | 在 `ShowSelection` 方法开头检查，非本地玩家立即关闭面板 |
| **单例面板** | 同一类型的面板在同步时应保证只有一个实例 |
| **取消处理** | 当用户取消选择时，应返回 `null` 或空列表，调用方需处理此情况 |

### 11.8 已实现同步的面板

| 面板类 | 同步方法 | 同步类型 | 用途 |
|--------|---------|---------|------|
| `CardSelectionScreen` | `ShowSelectionWithSync` | 单选 | 单张卡牌选择（如基地车展开） |
| `CardSelectionSyncHelper` | `ShowMultiSelectionWithSync` | 多选 | 多张卡牌选择（如集结） |
| `SellBuildingScreen` | `ShowSelectionWithSync` | 多选 | 出售建筑 |
| `ProductionQueueSelectionScreen` | `ShowSelectionWithSync` | 多选 | 生产序列管理 |
| `EngineerChoiceScreen` | `ShowSelectionWithSync` | 单选 | 工程师选择 |
| `DeployChoiceScreen` | `ShowSelectionWithSync` | 单选 | 部署选择（如防空履带车） |
| `ChronoWarpScreen` | `ShowPileSelectionWithSync` | 单选 | 超时空传送选择 |

### 11.9 常见错误与排查

#### 错误1：联机模式下面板不显示

**原因**：`ShowSelection` 方法中缺少 `IsLocalPlayer` 检查

**解决**：在 `Push` 面板后立即检查 `IsLocalPlayer`，非本地玩家调用 `Close()`

#### 错误2：StateDivergence 状态不一致

**原因**：未使用 `ShowSelectionWithSync` 方法，或选择结果未正确同步

**解决**：确保所有自定义UI面板的调用都使用 `ShowSelectionWithSync` 方法

#### 错误3：远程玩家无法获取选择结果

**原因**：未正确传递索引，或数据在不同客户端间不一致

**解决**：
1. 使用索引传递选择结果，而非对象引用
2. 在同步方法中创建数据副本
3. 确保数据在所有客户端上完全一致

#### 错误4：Close() 方法未实现

**原因**：自定义面板缺少 `Close()` 方法

**解决**：为所有自定义UI面板实现 `Close()` 方法，确保正确清理资源

---
| 使用对象池 | 对于频繁使用的特效，考虑使用对象池模式 |

---

## 附录：游戏解包资源结构

游戏解包目录 `D:\RedAlert2Project\SlayTheSpire2Export\` 包含以下与特效相关的资源：

```
SlayTheSpire2Export/
├── resources/
│   ├── scenes/
│   │   └── vfx/                    # 特效场景
│   │       ├── vfx_attack_slash.tscn
│   │       ├── vfx_attack_blunt.tscn
│   │       └── ...
│   └── images/
│       └── vfx/                    # 特效图片
│           ├── vfx_attack_slash_00-03.png
│           └── ...
└── src/
    └── Core/
        └── Nodes/
            └── Vfx/                # VFX节点类
                ├── NStabVfx.cs
                ├── NSlashVfx.cs
                ├── NFireBurningVfx.cs
                └── ...
```

---

## 自定义敌怪（续）

### 9.1 怪物基类（完整示例）

```csharp
public sealed class MyCustomMonster : MonsterModel
{
    public override int MinInitialHp => 30;
    public override int MaxInitialHp => 34;
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState attack = new MoveState(
            "ATTACK_STATE",
            AttackMove,
            new SingleAttackIntent(8)
        );
        attack.FollowUpState = attack;  // 循环攻击
        
        return new MonsterMoveStateMachine(new List<MonsterState> { attack }, attack);
    }
    
    private async Task AttackMove(IReadOnlyList<Creature> targets)
            .WithAttackerAnim("Attack", 0.2f)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(null);
    }
}
```

### 9.2 AI行为模式

#### 简单循环
```csharp
MoveState attack = new MoveState("ATTACK", AttackMove, new SingleAttackIntent(8));
MoveState buff = new MoveState("BUFF", BuffMove, new BuffIntent());
attack.FollowUpState = buff;
buff.FollowUpState = attack;  // 攻击→强化→攻击→强化...
```

#### 随机分支
```csharp
RandomBranchState random = new RandomBranchState("RANDOM");
random.AddBranch(attack, MoveRepeatType.CannotRepeat, 0.7f);   // 70%概率攻击
random.AddBranch(defend, MoveRepeatType.CannotRepeat, 0.3f);   // 30%概率防御
```

#### 条件分支
```csharp
ConditionalBranchState condition = new ConditionalBranchState("CHECK_HP");
condition.AddState(bigAttack, () => Creature.CurrentHp <= 10);  // 生命≤10时强力攻击
condition.AddState(normalAttack, () => true);                   // 否则普通攻击
```

### 9.3 常用怪物意图

| 意图类 | 说明 |
|--------|------|
| `SingleAttackIntent(int damage)` | 单次攻击 |
| `MultiAttackIntent(int damage, int repeat)` | 多次攻击 |
| `DefendIntent()` | 防御 |
| `BuffIntent()` | 强化 |
| `DebuffIntent()` | 削弱 |
| `StatusIntent(int count)` | 添加状态牌 |
| `SummonIntent()` | 召唤其他怪物 |
| `EscapeIntent()` | 逃跑 |
| `StunIntent()` | 眩晕 |

### 9.4 遭遇类（EncounterModel）

```csharp
public sealed class MyCustomEncounter : EncounterModel
{
    public override RoomType RoomType => RoomType.Monster;  // Monster/Elite/Boss
    public override bool IsWeak => true;                    // 弱怪池
    
    public override List<MonsterModel> AllPossibleMonsters => new()
    {
        ModelDb.Monster<MyCustomMonster>()
    };
    
    protected override List<(MonsterModel, string?)> GenerateMonsters()
    {
        return new List<(MonsterModel, string?)>
        {
            (ModelDb.Monster<MyCustomMonster>().ToMutable(), null)  // null使用默认站位
        };
    }
}
```

### 9.5 注册遭遇（HarmonyPatch）

```csharp
[HarmonyPatch(typeof(Overgrowth), nameof(Overgrowth.GenerateAllEncounters))]
public static class OvergrowthGenerateAllEncountersPatch
{
    static void Postfix(ref IEnumerable<EncounterModel> __result)
    {
        __result = __result.Concat(new[] { ModelDb.Encounter<MyCustomEncounter>() }).Distinct();
    }
}
```

### 9.6 资源路径

```
res://scenes/creature_visuals/<怪物ID小写>.tscn
res://scenes/encounters/<遭遇ID小写>.tscn  # 可选，自定义站位
```

**场景结构：**
```
NCreatureVisuals : Node2D
⨽ Node2D(%Visuals)
⨽ Control(%Bounds)
⨽ Marker2D(%IntentPos)
⨽ Marker2D(%CenterPos)
```

### 9.7 本地化文本

`res://<ModID>/localization/zhs/monsters.json`:
```json
{
  "MY_CUSTOM_MONSTER.name": "建筑师",
  "MY_CUSTOM_MONSTER.moves.ATTACK_STATE.title": "攻击"
}
```

`res://<ModID>/localization/zhs/encounters.json`:
```json
{
  "MY_CUSTOM_ENCOUNTER.title": "神秘角色",
  "MY_CUSTOM_ENCOUNTER.loss": "{character}被{encounter}解决掉了。"
}
```

---

## 🔧 通用工具与命令

### 控制台测试命令

| 命令 | 说明 |
|------|------|
| `card <CardID>` | 直接获得卡牌 |
| `addcard <CardID>` | 添加到卡组 |
| `relic <RelicID>` | 获得遗物 |
| `potion <PotionID>` | 获得药水 |
| `power <PowerID> <层数> <目标>` | 施加能力（0=玩家，1+=敌人） |
| `enchant <EnchantmentID> <层数> <手牌索引>` | 为手牌附魔 |
| `event <EventID>` | 触发自定义事件 |
| `ancient <AncientID>` | 触发先古之民 |
| `fight <EncounterID>` | 进入自定义遭遇战 |

### ID命名规则

类名自动转换为大写加下划线格式：
- `MyCustomCard` → `MY_CUSTOM_CARD`
- `MyCustomRelic` → `MY_CUSTOM_RELIC`
- `MyCustomMonster` → `MY_CUSTOM_MONSTER`

---

## 📝 开发最佳实践

1. **保留解包的游戏源代码**，方便查阅API和资源路径
2. **使用Harmony时注意版本兼容性**，确保与.NET 9.0兼容
3. **资源路径严格区分大小写**，与游戏原版保持一致
4. **每次修改代码后重新构建DLL**
5. **每次修改资源后重新导出PCK**
6. **美化包可将 `affects_gameplay` 设为 `false`**，允许联机使用
7. **使用 `Log.Info()` 或 `GD.Print()` 输出调试信息**

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

## 联机模式同步机制

### 多人同步随机数生成

在联机模式下，使用普通的随机数生成器（如 `GD.RandRange()` 或 `new Random()`）会导致不同客户端之间的随机结果不一致，从而引发 `StateDivergence` 错误。游戏提供了同步的随机数生成器来解决这个问题。

**正确用法**：
```csharp
var rng = Owner?.Player?.RunState?.Rng?.CombatCardSelection;
if (rng != null)
{
    var randomIndex = rng.NextInt(enemies.Count);
    // 使用同步的随机数
}
else
{
    // 单机模式下的回退方案
    var randomIndex = GD.RandRange(0, enemies.Count - 1);
}
```

**常用的同步随机数方法**：
| 方法 | 说明 |
|------|------|
| `NextInt(int max)` | 返回 [0, max) 范围内的随机整数 |
| `NextInt(int min, int max)` | 返回 [min, max) 范围内的随机整数 |
| `NextDouble()` | 返回 [0.0, 1.0) 范围内的随机双精度数 |
| `NextBool()` | 返回随机布尔值 |

**使用场景**：
- 防御塔攻击多目标时随机选择目标
- 工程师选项的随机排序
- 任何需要在多人模式下保持一致的随机操作

**错误示例**（会导致联机不同步）：
```csharp
// ❌ 错误：使用非同步随机数
var randomIndex = GD.RandRange(0, enemies.Count - 1);

// ❌ 错误：使用非同步随机数
var rand = new Random();
var randomIndex = rand.Next(enemies.Count);
```

### DamageVar攻击类型与增伤机制

游戏中的伤害数值通过 `DamageVar` 定义，其第二个参数 `ValueProp` 决定了伤害是否能受到增益效果（如力量加成、迟缓debuff增伤等）的影响。

#### ValueProp枚举类型

| 枚举值 | 说明 | 是否受增伤buff影响 |
|--------|------|------------------|
| `ValueProp.Move` | 攻击卡牌造成的伤害 | ✅ 是 |
| `ValueProp.Unpowered` | 能力/遗物/药水造成的伤害 | ❌ 否 |

#### 使用示例

**攻击卡牌（受增伤buff影响）**：
```csharp
// 攻击卡牌使用 ValueProp.Move，伤害会受到力量等buff加成
protected override List<DynamicVar> CanonicalVars => new List<DynamicVar>
{
    new DamageVar(6m, ValueProp.Move)
};
```

**能力卡牌（不受增伤buff影响）**：
```csharp
// 防御塔能力使用 ValueProp.Unpowered，伤害不受力量等buff加成
protected override List<DynamicVar> CanonicalVars => new List<DynamicVar>
{
    new DamageVar(8m, ValueProp.Unpowered)
};
```

#### 红警Mod增伤规则

对于红警2 Mod，伤害能否受增伤buff影响的规则如下：

| 卡牌类型 | 是否受增伤buff | ValueProp | 示例 |
|---------|--------------|-----------|------|
| 攻击卡（单位卡、武器卡） | ✅ 是 | `ValueProp.Move` | 动员兵、灰熊坦克、核弹 |
| 技能卡（非能力类） | ✅ 是 | `ValueProp.Move` | 飞鹰空袭、闪电风暴 |
| 能力卡（防御塔等Power卡） | ❌ 否 | `ValueProp.Unpowered` | 哨戒炮、磁暴线圈、光棱塔 |
| 遗物/药水伤害 | ❌ 否 | `ValueProp.Unpowered` | 各种遗物效果、药水效果 |

**关键原则**：
- 所有通过打出卡牌直接造成的伤害（攻击卡、技能卡）应使用 `ValueProp.Move`
- 所有通过能力(Power)回合触发造成的伤害应使用 `ValueProp.Unpowered`
- 防御塔其伤害是通过能力触发的，因此使用 `ValueProp.Unpowered`

---

## 🎯 快速开始检查清单

- [ ] 安装Megadot编辑器
- [ ] 配置.NET 9.0环境
- [ ] 创建Godot项目
- [ ] 添加sts2.dll和0Harmony.dll引用
- [ ] 创建ModInitializer入口类
- [ ] 编写JSON配置文件
- [ ] 实现第一个功能（遗物/卡牌/角色等）
- [ ] 导出PCK和DLL
- [ ] 放入mods文件夹测试
- [ ] 使用控制台命令验证功能

---

*本教程基于《杀戮尖塔2》官方Mod开发指南整理，适用于Godot引擎 和C#语言开发。*
