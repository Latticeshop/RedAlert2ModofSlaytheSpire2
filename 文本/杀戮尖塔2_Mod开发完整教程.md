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

---

## 3.8 自定义词条（Custom Keywords）

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

| 属性 | 说明 |
|------|------|
| `Type` | Buff / Debuff |
| `StackType` | Counter(有层数) / Single(无层数) |
| `IsInstanced` | 重复施加时是否创建独立实例 |
| `AllowNegative` | 是否允许层数为负数 |

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

### 6.5 能力图标配置

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
        // 添加更多能力类型和图标路径
    };

    // 拦截 Icon 属性
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

**重要提示**：新增能力类型后，必须将其添加到 `_customIconPaths` 字典中，否则图标将无法正常显示。例如添加 `TransportShipPower` 后：

```csharp
{ typeof(TransportShipPower), "res://RedAlert2ModResources/images/packed/card_portraits/allies/landicon.png" },
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

## �🎯 快速开始检查清单

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
