# RedAlert2Mod RitsuLib 迁移指南

> **重要警告**：本迁移指南基于 RitsuLib 文档编写。由于 RitsuLib 是从 Steam Workshop 获取的 DLL（非 NuGet 包），在实际执行迁移前，**必须验证文档中引用的 API 在实际 DLL 中是否存在**。

---

阅读官方网站和文档
https://github.com/BAKAOLC/STS2-RitsuLib 
https://sts2-ritsulib.ritsukage.com/

## 目录

1. [迁移概述](#1-迁移概述)
2. [Phase 0: API 验证（关键前置步骤）](#2-phase-0-api-验证关键前置步骤)
3. [Phase 1: 基础设施搭建](#3-phase-1-基础设施搭建)
4. [Phase 2: 角色和卡池迁移](#4-phase-2-角色和卡池迁移)
5. [Phase 3: 卡牌注册迁移](#5-phase-3-卡牌注册迁移)
6. [Phase 4: 卡牌逻辑重写](#6-phase-4-卡牌逻辑重写)
7. [Phase 5: 能力和遗物迁移](#7-phase-5-能力和遗物迁移)
8. [Phase 6: 补丁和自定义UI迁移](#8-phase-6-补丁和自定义ui迁移)
9. [Phase 7: 测试和调试](#9-phase-7-测试和调试)
10. [附录：API 对照表](#10-附录api-对照表)

---

## 1. 迁移概述

### 1.1 当前架构

| 组件 | 当前技术栈 |
|------|------------|
| 框架 | BaseLib（本地 DLL 引用） |
| 角色基类 | `PlaceholderCharacterModel` |
| 卡池基类 | `CardPoolModel`（手动 `GenerateAllCards`） |
| 卡牌注册 | `ModHelper.AddModelToPool()` 手动注册 |
| 卡牌逻辑 | `CardModel.OnPlay(PlayerChoiceContext, CardPlay)` |
| 补丁系统 | Harmony 直接调用 |
| 经济系统 | 自定义 `DollarPower : PowerModel` |

### 1.2 目标架构

| 组件 | RitsuLib 技术栈 |
|------|-----------------|
| 框架 | RitsuLib（Workshop DLL 引用） |
| 角色基类 | `ModCharacterTemplate<TCardPool, TRelicPool, TPotionPool>` |
| 卡池基类 | `TypeListCardPoolModel`（属性自动发现） |
| 卡牌注册 | `[RegisterCard(typeof(CardPool))]` 属性注册 |
| 卡牌逻辑 | `ModCardTemplate.Use(ICombatContext, ICreatureState, ICreatureState?)` |
| 补丁系统 | `RitsuLibFramework.CreatePatcher()` |
| 经济系统 | 保持 `DollarPower` 不变（渐进式迁移） |

### 1.3 迁移策略

采用 **渐进式迁移** 策略，BaseLib 和 RitsuLib 可以共存：
- 先完成基础设施搭建，确保两个框架同时加载
- 逐个模块迁移，迁移完一个模块后立即测试
- 最后移除 BaseLib 依赖

---

## 2. Phase 0: API 验证（关键前置步骤）

### 2.1 定位 RitsuLib DLL

RitsuLib 是一个 Steam Workshop mod，其 DLL 文件位于游戏的 Workshop 目录中。

**步骤**：
1. 打开 Steam，订阅 RitsuLib mod（Workshop ID: 待定）
2. 在游戏目录中查找 RitsuLib DLL：
   ```
   <Steam安装目录>/steamapps/workshop/content/2394650/<WorkshopID>/
   ```
   搜索文件 `STS2-RitsuLib.dll` 或 `RitsuLib.dll`

3. 将找到的 DLL 复制到项目目录的 `RitsuLib` 文件夹中：
   ```
   RedAlert2Mod/
   ├── RitsuLib/
   │   └── STS2-RitsuLib.dll
   └── BaseLib/
       └── BaseLib.dll
   ```

### 2.2 验证 RitsuLib API

**必须执行**：使用 IDE 的对象浏览器或 ILSpy 检查以下类型和方法是否存在于 RitsuLib DLL 中：

| 类型/方法 | 命名空间 | 验证状态 |
|-----------|----------|----------|
| `RitsuLibFramework` | `RitsuLib` | ☐ |
| `ModCharacterTemplate<,,>` | `RitsuLib.Content.Characters` | ☐ |
| `TypeListCardPoolModel` | `RitsuLib.Content.Cards` | ☐ |
| `ModCardTemplate` | `RitsuLib.Content.Cards` | ☐ |
| `[RegisterCard]` 属性 | `RitsuLib.Content.Attributes` | ☐ |
| `[RegisterCharacter]` 属性 | `RitsuLib.Content.Attributes` | ☐ |
| `[RegisterPower]` 属性 | `RitsuLib.Content.Attributes` | ☐ |
| `[RegisterRelic]` 属性 | `RitsuLib.Content.Attributes` | ☐ |
| `ModCardTemplate.Use()` | `RitsuLib.Content.Cards` | ☐ |
| `SecondaryResourceRegistry` | `RitsuLib.Content.SecondaryResources` | ☐ |
| `RitsuLibFramework.CreatePatcher()` | `RitsuLib` | ☐ |
| `RitsuLibFramework.CreateContentPack()` | `RitsuLib` | ☐ |

**只有所有验证通过后，才能继续迁移。**

---

## 3. Phase 1: 基础设施搭建

### 3.1 修改 .csproj 添加 RitsuLib 引用

**文件**：`RedAlert2Mod.csproj`

在现有 BaseLib 引用之后添加 RitsuLib 引用：

```xml
<!-- 引用RitsuLib（添加在BaseLib引用之后） -->
<ItemGroup Condition="Exists('$(RitsuLibDllPath)')">
  <Reference Include="STS2-RitsuLib">
    <HintPath>$(RitsuLibDllPath)</HintPath>
    <Private>true</Private>
  </Reference>
  <!-- 确保RitsuLib.dll被复制到输出目录 -->
  <None Include="$(RitsuLibDllPath)">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

同时在 `PropertyGroup` 中添加 RitsuLib 路径配置：

```xml
<!-- RitsuLib路径配置 -->
<RitsuLibDir>$(MSBuildProjectDirectory)/RitsuLib</RitsuLibDir>
<RitsuLibDllPath>$(RitsuLibDir)/STS2-RitsuLib.dll</RitsuLibDllPath>
```

### 3.2 修改 mod_manifest.json 添加依赖

**文件**：`build/RedAlert2Mod.json`

添加 RitsuLib 依赖（保持 BaseLib 以便渐进式迁移）：

```json
{
    "id": "RedAlert2Mod",
    "name": "红警2mod",
    "author": "小格子铺",
    "description": "关于杀戮尖塔2的红色警戒2游戏mod，主要有盟军，苏军，尤里三个阵营。",
    "version": "1.0.6",
    "min_game_version": "0.109.0",
    "has_pck": true,
    "has_dll": true,
    "dependencies": [
        {
            "id": "BaseLib",
            "min_version": "3.3.0"
        },
        {
            "id": "STS2-RitsuLib",
            "min_version": "1.0.0"
        }
    ],
    "affects_gameplay": true
}
```

### 3.3 创建 RitsuLib 初始化入口

**文件**：`RedAlert2ModCode/RitsuLibInitializer.cs`（新建）

```csharp
using System.Reflection;
using HarmonyLib;
using RitsuLib;
using RitsuLib.Content;

namespace RedAlert2ModCode;

/// <summary>
/// RitsuLib框架初始化入口
/// </summary>
public static class RitsuLibInitializer
{
    public const string ModId = "RedAlert2Mod";
    
    public static void Initialize()
    {
        // 1. 初始化RitsuLib框架
        RitsuLibFramework.CreateLogger(ModId);
        
        // 2. 注册mod程序集
        var assembly = Assembly.GetExecutingAssembly();
        ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly);
        
        // 3. 创建补丁器（后续迁移补丁时使用）
        // var patcher = RitsuLibFramework.CreatePatcher(ModId, "main");
        // patcher.RegisterPatches<AlliedPatches>();
        // RitsuLibFramework.ApplyRequiredPatcher(patcher, DisableMod);
        
        // 4. 应用内容包注册（后续迁移内容时使用）
        // RitsuLibFramework.CreateContentPack("RedAlert2Mod").Apply();
    }
    
    private static void DisableMod()
    {
        // 补丁应用失败时的回调
    }
}
```

### 3.4 修改 ModInitializer 集成 RitsuLib

**文件**：`RedAlert2ModCode/ModInitializer.cs`

在现有初始化逻辑前添加 RitsuLib 初始化：

```csharp
public static void Initialize()
{
    // ========== RitsuLib初始化 ==========
    RitsuLibInitializer.Initialize();
    
    // ========== 原有BaseLib逻辑保持不变 ==========
    var harmony = new Harmony(ModId);
    harmony.PatchAll();
    
    // 注册盟军角色立绘补丁
    Allies.AssetHooks.Install(harmony);
    
    // 注册苏军角色立绘补丁
    Soviet.AssetHooks.Install(harmony);
    
    // ... 其他原有代码保持不变 ...
    
    Logger.Info("红警2Mod加载成功！（RitsuLib集成模式）");
}
```

### 3.5 验证步骤

1. 构建项目：`dotnet build`
2. 确保 RitsuLib.dll 被复制到输出目录
3. 启动游戏，确认 mod 正常加载（控制台应显示 "RitsuLib集成模式"）

---

## 4. Phase 2: 角色和卡池迁移

### 4.1 迁移盟军卡池

**文件**：`RedAlert2ModCode/Allies/AlliesCardPool.cs`

将继承从 `CardPoolModel` 改为 `TypeListCardPoolModel`：

```csharp
using Godot;
using RitsuLib.Content.Cards;

namespace RedAlert2ModCode.Allies;

public sealed class AlliesCardPool : TypeListCardPoolModel
{
    public override string Title => "allies";
    public override string EnergyColorName => "defect";
    public override bool IsColorless => false;
    
    public override string CardFrameMaterialPath => "card_frame_blue";
    
    public static readonly Color Color = new("2060a0");
    public override Color DeckEntryCardColor => Color;
    public override Color EnergyOutlineColor => new("103080");
    
    // 注意：TypeListCardPoolModel 会自动发现 [RegisterCard(typeof(AlliesCardPool))] 标记的卡牌
    // 不再需要 GenerateAllCards() 方法
}
```

### 4.2 迁移盟军遗物池

**文件**：`RedAlert2ModCode/Allies/AlliesRelicPool.cs`

```csharp
using RitsuLib.Content.Relics;

namespace RedAlert2ModCode.Allies;

public sealed class AlliesRelicPool : TypeListRelicPoolModel
{
    // 自动发现 [RegisterRelic(typeof(AlliesRelicPool))] 标记的遗物
}
```

### 4.3 迁移盟军药水池

**文件**：`RedAlert2ModCode/Allies/AlliesPotionPool.cs`

```csharp
using RitsuLib.Content.Potions;

namespace RedAlert2ModCode.Allies;

public sealed class AlliesPotionPool : TypeListPotionPoolModel
{
    // 自动发现 [RegisterPotion(typeof(AlliesPotionPool))] 标记的药水
}
```

### 4.4 迁移盟军角色

**文件**：`RedAlert2ModCode/Allies/AlliesCharacter.cs`

将继承从 `PlaceholderCharacterModel` 改为 `ModCharacterTemplate<,,>`：

```csharp
using Godot;
using MegaCrit.Sts2.Core.Localization;
using RitsuLib.Content;
using RitsuLib.Content.Attributes;
using RitsuLib.Content.Characters;

namespace RedAlert2ModCode.Allies;

[RegisterCharacter]
public sealed class Allies : ModCharacterTemplate<AlliesCardPool, AlliesRelicPool, AlliesPotionPool>
{
    public const string CharacterId = "Allies";
    
    // 角色颜色配置
    public static readonly Color Color = new("2060a0"); // 盟军蓝色
    
    // 必需属性
    public override Color NameColor => Color;
    public override Color MapDrawingColor => Color;
    public override CharacterGender Gender => CharacterGender.Feminine;
    public override int StartingHp => 85;
    
    // 资源路径配置
    public override CharacterAssetProfile AssetProfile => new()
    {
        Ui = new()
        {
            CharacterSelectIconPath = "res://RedAlert2ModResources/images/charui/allies_character_select.png",
            // ... 其他UI资源路径
        },
        Visuals = new()
        {
            CharacterSelectBgPath = "res://RedAlert2ModResources/scenes/allies_bg.tscn",
            CreatureVisualPath = "res://RedAlert2ModResources/scenes/creature_visuals/allies.tscn",
            // ... 其他可视化资源路径
        }
    };
    
    // 起始卡组将通过 [RegisterCharacterStarterCard] 属性注册
    // 起始遗物将通过 [RegisterCharacterStarterRelic] 属性注册
}
```

### 4.5 添加起始卡组和遗物的属性注册

**创建文件**：`RedAlert2ModCode/Allies/AlliesStarterCards.cs`（新建）

```csharp
using RitsuLib.Content.Attributes;
using RedAlert2ModCode.Allies.Cards;

namespace RedAlert2ModCode.Allies;

/// <summary>
/// 盟军起始卡组注册
/// </summary>
public static class AlliesStarterCards
{
    [RegisterCharacterStarterCard(typeof(Allies), 5)]
    public static AmericanSoldier AmericanSoldier { get; } = new();
    
    [RegisterCharacterStarterCard(typeof(Allies), 5)]
    public static GrizzlyTank GrizzlyTank { get; } = new();
    
    [RegisterCharacterStarterCard(typeof(Allies), 1)]
    public static AlliedMCV AlliedMCV { get; } = new();
    
    [RegisterCharacterStarterCard(typeof(Allies), 1)]
    public static AlliedWallCard AlliedWallCard { get; } = new();
}
```

**创建文件**：`RedAlert2ModCode/Allies/AlliesStarterRelics.cs`（新建）

```csharp
using RitsuLib.Content.Attributes;
using RedAlert2ModCode.Common.Relics;

namespace RedAlert2ModCode.Allies;

/// <summary>
/// 盟军起始遗物注册
/// </summary>
public static class AlliesStarterRelics
{
    [RegisterCharacterStarterRelic(typeof(Allies))]
    public static DollarRelic DollarRelic { get; } = new();
}
```

### 4.6 苏军和尤里阵营重复以上步骤

对苏军和尤里阵营执行相同的迁移操作：

- `RedAlert2ModCode/Soviet/SovietCardPool.cs`
- `RedAlert2ModCode/Soviet/SovietRelicPool.cs`
- `RedAlert2ModCode/Soviet/SovietPotionPool.cs`
- `RedAlert2ModCode/Soviet/SovietCharacter.cs`
- `RedAlert2ModCode/Yuri/YuriCardPool.cs`
- `RedAlert2ModCode/Yuri/YuriRelicPool.cs`
- `RedAlert2ModCode/Yuri/YuriPotionPool.cs`
- `RedAlert2ModCode/Yuri/YuriCharacter.cs`

### 4.7 验证步骤

1. 构建项目：`dotnet build`
2. 确认编译通过
3. 启动游戏，确认角色选择界面正常显示

---

## 5. Phase 3: 卡牌注册迁移

### 5.1 为卡牌添加 [RegisterCard] 属性

**示例**：`RedAlert2ModCode/Allies/Cards/AmericanSoldier.cs`

```csharp
using RitsuLib.Content.Attributes;
using RedAlert2ModCode.Allies;

namespace RedAlert2ModCode.Allies.Cards;

[RegisterCard(typeof(AlliesCardPool))]
public sealed class AmericanSoldier : CardModel
{
    // ... 原有代码保持不变 ...
}
```

**需要修改的文件**（约100+个）：

| 目录 | 文件数 |
|------|--------|
| `Allies/Cards/` | ~50 |
| `Soviet/Cards/` | ~50 |
| `Common/Cards/` | ~20 |
| `Yuri/Cards/` | ~20 |

### 5.2 删除 ModHelper.AddModelToPool 调用

**文件**：`RedAlert2ModCode/ModInitializer.cs`

删除以下代码块：

```csharp
// 注册所有盟军卡牌到盟军卡池
ModHelper.AddModelToPool(typeof(AlliesCardPool), typeof(AmericanSoldier));
ModHelper.AddModelToPool(typeof(AlliesCardPool), typeof(GrizzlyTank));
// ... 所有类似的 AddModelToPool 调用
```

### 5.3 更新内容包注册

**文件**：`RedAlert2ModCode/RitsuLibInitializer.cs`

启用内容包注册：

```csharp
public static void Initialize()
{
    // 1. 初始化RitsuLib框架
    RitsuLibFramework.CreateLogger(ModId);
    
    // 2. 注册mod程序集
    var assembly = Assembly.GetExecutingAssembly();
    ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly);
    
    // 3. 应用内容包注册
    RitsuLibFramework.CreateContentPack("RedAlert2Mod").Apply();
}
```

### 5.4 验证步骤

1. 构建项目：`dotnet build`
2. 启动游戏，打开卡牌查看器，确认卡牌正确注册到对应卡池

---

## 6. Phase 4: 卡牌逻辑重写

### 6.1 核心差异对比

| 方面 | BaseLib (CardModel) | RitsuLib (ModCardTemplate) |
|------|---------------------|----------------------------|
| 继承基类 | `CardModel` | `ModCardTemplate(...)` |
| 核心方法 | `OnPlay(PlayerChoiceContext, CardPlay)` | `Use(ICombatContext, ICreatureState, ICreatureState?)` |
| 动态数值 | `DynamicVars` | 构造函数参数 |
| 目标选择 | `play.Target` | `target` 参数 |
| 用户 | 通过 `play.Card.Owner` 获取 | `user` 参数 |

### 6.2 卡牌重写示例

**原代码**（`Allies/Cards/AmericanSoldier.cs`）：

```csharp
public sealed class AmericanSoldier : CardModel
{
    public AmericanSoldier() : base(1, CardType.Attack, CardRarity.Common, TargetType.Enemy) { }
    
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        // 造成6点伤害
        await DamageCmd.Attack(6)
            .FromCard(this)
            .Targeting(play.Target)
            .Execute(ctx);
    }
}
```

**迁移后代码**：

```csharp
using RitsuLib.Content.Attributes;
using RitsuLib.Content.Cards;
using RitsuLib.Content.Characters;
using RedAlert2ModCode.Allies;

namespace RedAlert2ModCode.Allies.Cards;

[RegisterCard(typeof(AlliesCardPool))]
public sealed class AmericanSoldier : ModCardTemplate
{
    // 构造函数参数：费用、类型、稀有度、目标类型、伤害值等
    public AmericanSoldier() 
        : base(
            cost: 1, 
            type: CardType.Attack, 
            rarity: CardRarity.Common, 
            target: TargetType.Enemy,
            damage: 6
        )
    {
    }
    
    public override void Use(ICombatContext ctx, ICreatureState user, ICreatureState? target)
    {
        // 造成6点伤害
        ctx.DealDamage(user, target, Damage);
    }
}
```

### 6.3 复杂卡牌迁移示例

**原代码**（带有选择目标和特殊效果）：

```csharp
protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
{
    // 选择一个敌人
    var selectedEnemy = await ctx.SelectSingleEnemy();
    
    // 造成伤害并施加虚弱
    await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
        .FromCard(this)
        .Targeting(selectedEnemy)
        .Execute(ctx);
    
    await PowerCmd.Apply(
        WeakPower.ID,
        this,
        DynamicVars.Weak.BaseValue,
        selectedEnemy
    ).Execute(ctx);
}
```

**迁移后代码**：

```csharp
public override void Use(ICombatContext ctx, ICreatureState user, ICreatureState? target)
{
    // 获取动态伤害值
    int damage = GetDynamicValue("Damage");
    int weakAmount = GetDynamicValue("Weak");
    
    // 造成伤害
    ctx.DealDamage(user, target, damage);
    
    // 施加虚弱
    ctx.ApplyPower(user, target, WeakPower.ID, weakAmount);
}
```

### 6.4 迁移清单

每个卡牌文件需要检查和修改：

| 检查项 | 描述 |
|--------|------|
| 继承基类 | 从 `CardModel` 改为 `ModCardTemplate` |
| 构造函数 | 改为 `ModCardTemplate` 的构造函数参数 |
| OnPlay方法 | 重写为 `Use(ICombatContext, ICreatureState, ICreatureState?)` |
| DynamicVars | 转换为构造函数参数或 `GetDynamicValue()` |
| Command调用 | 转换为 `ICombatContext` 的方法调用 |
| 异步操作 | 转换为同步操作（RitsuLib 卡牌逻辑为同步） |

### 6.5 验证步骤

1. 逐个迁移卡牌，每迁移一个测试一个
2. 重点测试战斗功能：伤害、能力施加、目标选择等
3. 确保经济系统（DollarPower）正常工作

---

## 7. Phase 5: 能力和遗物迁移

### 7.1 能力迁移

**现状分析**：能力类继承 `PowerModel`，这是游戏核心类型，与 RitsuLib 兼容。

**迁移步骤**：
1. 为每个能力类添加 `[RegisterPower]` 属性
2. 无需修改能力逻辑（除非使用了 BaseLib 特有的扩展方法）

**示例**：`RedAlert2ModCode/Allies/Powers/AlliedBarracksPower.cs`

```csharp
using RitsuLib.Content.Attributes;

namespace RedAlert2ModCode.Allies.Powers;

[RegisterPower]
public sealed class AlliedBarracksPower : PowerModel
{
    // ... 原有代码保持不变 ...
}
```

### 7.2 遗物迁移

**现状分析**：遗物类继承 `RelicModel`，这是游戏核心类型，与 RitsuLib 兼容。

**迁移步骤**：
1. 为每个遗物类添加 `[RegisterRelic]` 属性，并指定所属遗物池
2. 无需修改遗物逻辑

**示例**：`RedAlert2ModCode/Allies/Relics/DollarRelic.cs`

```csharp
using RitsuLib.Content.Attributes;
using RedAlert2ModCode.Allies;

namespace RedAlert2ModCode.Common.Relics;

[RegisterRelic(typeof(AlliesRelicPool))]
public sealed class DollarRelic : RelicModel
{
    // ... 原有代码保持不变 ...
}
```

### 7.3 DollarPower 经济系统

**建议策略**：**暂时保持现状**，不迁移到 RitsuLib 的 SecondaryResourceRegistry。

**理由**：
1. DollarPower 绑定了大量自定义逻辑（VFX、音效、转账系统等）
2. RitsuLib 的 SecondaryResourceRegistry 需要重写整个经济系统
3. 可以在后续版本中逐步迁移

### 7.4 验证步骤

1. 构建项目：`dotnet build`
2. 启动游戏，测试能力和遗物的效果
3. 确保经济系统正常工作

---

## 8. Phase 6: 补丁和自定义UI迁移

### 8.1 补丁迁移

**现状分析**：当前使用 Harmony 直接调用 `harmony.PatchAll()`。

**迁移步骤**：

**创建补丁注册类**：`RedAlert2ModCode/Allies/Patches/AlliedPatches.cs`

```csharp
using HarmonyLib;
using RitsuLib.Patching;

namespace RedAlert2ModCode.Allies.Patches;

public static class AlliedPatches
{
    [HarmonyPatch(typeof(CharacterSelectController), nameof(CharacterSelectController.Setup))]
    [HarmonyPostfix]
    public static void CharacterSelectSetup_Postfix()
    {
        // 原有补丁逻辑
    }
}
```

**修改 RitsuLibInitializer**：

```csharp
public static void Initialize()
{
    // ... 其他初始化代码 ...
    
    // 创建补丁器
    var patcher = RitsuLibFramework.CreatePatcher(ModId, "main");
    
    // 注册补丁
    patcher.RegisterPatches<Allies.Patches.AlliedPatches>();
    patcher.RegisterPatches<Soviet.Patches.SovietPatches>();
    patcher.RegisterPatches<Common.Patches.CommonPatches>();
    
    // 应用补丁
    RitsuLibFramework.ApplyRequiredPatcher(patcher, DisableMod);
}
```

**注意**：可以保持 `harmony.PatchAll()` 和 RitsuLib 补丁器同时运行，逐步迁移。

### 8.2 自定义UI迁移

**现状分析**：项目有多个自定义UI组件：
- `CardSelectionScreen` - 卡牌选择面板
- `FlagSelectionScreen` - 国旗选择面板
- 其他自定义UI

**迁移步骤**：

1. 检查UI组件是否依赖 BaseLib 的特定类型
2. 替换为 RitsuLib 或游戏核心的对应类型
3. 测试UI功能

**注意**：如果UI组件只使用游戏核心类型（Godot控件、CardModel等），则无需修改。

### 8.3 验证步骤

1. 构建项目：`dotnet build`
2. 测试自定义UI功能
3. 测试补丁效果

---

## 9. Phase 7: 测试和调试

### 9.1 测试清单

| 测试项 | 描述 |
|--------|------|
| 角色选择 | 确认三个阵营角色正常显示 |
| 起始卡组 | 确认起始卡组正确生成 |
| 卡牌抽取 | 确认卡牌从卡池正确抽取 |
| 卡牌使用 | 测试每张卡牌的 Use 方法 |
| 能力效果 | 测试能力的施加和效果 |
| 遗物效果 | 测试遗物的效果 |
| 经济系统 | 测试 DollarPower 的增减和转账 |
| 自定义UI | 测试 CardSelectionScreen 等 |
| 事件系统 | 测试自定义事件（如果有） |
| 存档兼容 | 测试存档和读档 |

### 9.2 调试技巧

1. 使用 RitsuLib 的日志系统：
   ```csharp
   var logger = RitsuLibFramework.GetLogger(ModId);
   logger.Info("Debug message");
   ```

2. 使用游戏内控制台查看错误信息

3. 在卡牌 Use 方法中添加日志：
   ```csharp
   public override void Use(ICombatContext ctx, ICreatureState user, ICreatureState? target)
   {
       GD.Print($"[CardUse] {Id.Entry} used by {user.CreatureName}");
       // ...
   }
   ```

---

## 10. 附录：API 对照表

### 10.1 生命周期方法

| BaseLib / 游戏核心 | RitsuLib 等效方法 |
|-------------------|-------------------|
| `CardModel.OnPlay()` | `ModCardTemplate.Use()` |
| `PowerModel.OnApply()` | `ModPowerTemplate.OnApply()` |
| `RelicModel.OnAcquire()` | `ModRelicTemplate.OnAcquire()` |

### 10.2 战斗命令

| BaseLib / 游戏核心 | RitsuLib 等效方法 |
|-------------------|-------------------|
| `DamageCmd.Attack()` | `ICombatContext.DealDamage()` |
| `PowerCmd.Apply()` | `ICombatContext.ApplyPower()` |
| `CardPileCmd.Add()` | `ICombatContext.AddCardToPile()` |
| `PlayerCmd.GainEnergy()` | `ICombatContext.GainEnergy()` |

### 10.3 注册属性

| 注册目标 | 属性 |
|----------|------|
| 角色 | `[RegisterCharacter]` |
| 卡牌 | `[RegisterCard(typeof(CardPool))]` |
| 能力 | `[RegisterPower]` |
| 遗物 | `[RegisterRelic(typeof(RelicPool))]` |
| 药水 | `[RegisterPotion(typeof(PotionPool))]` |
| 角色起始卡牌 | `[RegisterCharacterStarterCard(typeof(Character), count)]` |
| 角色起始遗物 | `[RegisterCharacterStarterRelic(typeof(Character))]` |

### 10.4 基类对照表

| BaseLib / 游戏核心 | RitsuLib 等效基类 |
|-------------------|-------------------|
| `PlaceholderCharacterModel` | `ModCharacterTemplate<,,>` |
| `CardPoolModel` | `TypeListCardPoolModel` |
| `CardModel` | `ModCardTemplate` |
| `PowerModel` | `ModPowerTemplate`（可选） |
| `RelicModel` | `ModRelicTemplate`（可选） |

---

## 11. 最终移除 BaseLib

当所有模块都成功迁移后，执行以下步骤移除 BaseLib 依赖：

1. 从 `.csproj` 删除 BaseLib 引用
2. 删除 `BaseLib` 目录
3. 从 `mod_manifest.json` 删除 BaseLib 依赖
4. 删除所有 BaseLib 特有的代码和引用
5. 构建并测试

---

## 参考资料

- RitsuLib GitHub：https://github.com/BAKAOLC/STS2-RitsuLib
- RitsuLib 文档：https://sts2-ritsulib.ritsukage.com/
- 项目结构：`RedAlert2ModCode/`
