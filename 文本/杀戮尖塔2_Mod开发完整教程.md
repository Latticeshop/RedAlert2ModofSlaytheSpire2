# 杀戮尖塔2 Mod开发完整教程

> 本教程基于《杀戮尖塔2》官方Mod开发指南整理，适用于使用Godot引擎和C#语言开发Mod。

---

## 📚 目录

1. [环境搭建](#1-环境搭建)
2. [自定义遗物](#2-自定义遗物)
3. [自定义卡牌](#3-自定义卡牌)
4. [自定义药水](#4-自定义药水)
5. [卡牌附魔](#5-卡牌附魔)
6. [自定义能力（Buff）](#6-自定义能力buff)
7. [自定义事件](#7-自定义事件)
8. [自定义角色](#8-自定义角色)
9. [自定义敌怪](#9-自定义敌怪)
10. [攻击特效（VFX）](#10-攻击特效vfx)
11. [联机同步（Multiplayer Sync）](#11-联机同步multiplayer-sync)
12. [Beta版 API 变化详解](#12-beta版-api-变化详解)
13. [科技树系统（Tech Tree）](#13-科技树系统tech-tree)
14. [经济系统与建筑打出系统](#14-经济系统与建筑打出系统)
15. [阵营架构设计](#15-阵营架构设计)
16. [音效播放系统](#16-音效播放系统)
17. [高级战备体系实现模式（飞鹰 & 轨道）](#17-高级战备体系实现模式飞鹰--轨道)
18. [卡牌存储与消耗机制（IFV / 步兵车系列）](#18-卡牌存储与消耗机制ifv--步兵车系列)
19. [Mod配置面板与开局方案](#19-mod配置面板与开局方案)
20. [多人联机进阶机制](#20-多人联机进阶机制)
21. [先古之民对话本地化（RitsuLib）](#21-先古之民对话本地化ritsulib)
22. [UI选择页面本地化配置](#22-ui选择页面本地化配置)
23. [本地化键名规则与关键API速查](#23-本地化键名规则与关键api速查)
24. [附录：游戏解包资源结构](#附录游戏解包资源结构)
25. [通用工具与命令](#通用工具与命令)
26. [开发最佳实践](#开发最佳实践)
27. [联机模式注意事项](#联机模式注意事项)
28. [联机模式同步机制](#联机模式同步机制)
29. [快速开始检查清单](#快速开始检查清单)

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

### 1.8 编译与部署

每次代码更新后，需要重新编译生成新的 `RedAlert2Mod.dll` 文件：

```bash
dotnet build RedAlert2Mod.csproj -c Release -o build
```

编译成功后，游戏需要的是 `build/RedAlert2Mod.dll` 文件。确保将以下文件复制到游戏的 `mods/RedAlert2Mod/` 目录：

| 文件 | 说明 |
|------|------|
| `RedAlert2Mod.dll` | 主程序集（必须） |
| `RedAlert2Mod.json` | Mod配置文件（必须） |
| `RedAlert2Mod.pck` | 资源包（如果有资源） |

**部署检查清单**：

1. 三个文件必须同名、同目录，且 JSON 中 `id` 与文件名完全一致；
2. 只修改了 C# 代码 → 重新 `dotnet build` 并替换 DLL；
3. 只修改了资源/本地化 → 重新导出 PCK；
4. 同时改了代码和资源 → DLL 与 PCK 都要替换；
5. 部署后查看 `godot.log` 确认加载成功、无报错。

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

### 2.8 遗物卡牌转换补丁（NewLeaf / LeafyPoultice）

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

#### 本地化 ID 命名规则（`_CARD` 后缀）

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

> **排查技巧**：如果卡牌标题/描述在游戏中显示为原始 key（如 `cards.PSI_COMMANDO.title`），说明本地化 key 不匹配。检查类名是否以 `Card` 结尾，并同步修改所有 4 个语言的 `cards.json`。

#### 百科卡框颜色（`Pool` / `VisualCardPool`）

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

> 不要忘了添加 `using MegaCrit.Sts2.Core.Models.CardPools;`。

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

### 进阶：带行为逻辑的自定义词条（超时空词条案例）

前面介绍的自定义词条只包含视觉效果（金色文本 + 悬停提示）。当词条需要绑定游戏行为时，需要创建基类来封装词条逻辑。

#### 应用场景

"超时空"词条的核心逻辑：
1. 打出卡牌时，卡牌进入摸牌堆而非弃牌堆
2. 当卡牌同时拥有"消耗(Exhaust)"词条时，首次打出进入摸牌堆并移除超时空词条，下次打出正常消耗

#### 实现步骤

##### 第一步：创建词条定义

在 `CustomKeyword.cs` 的 `ModCardKeywords` 类中添加超时空词条：

```csharp
public static readonly CustomKeyword Chrono = new(
    "CHRONO",
    new LocString("card_keywords", "chrono.title"),
    new LocString("card_keywords", "chrono.description")
);
```

##### 第二步：创建词条行为基类

在 `Common/Cards/` 目录下创建 `ChronoCardModel.cs`：

```csharp
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Common.Cards;

/// <summary>
/// 超时空卡牌基类
/// 自动处理超时空词条效果：
/// 1. 打出时卡牌进入摸牌堆而非弃牌堆
/// 2. 当卡牌同时拥有消耗(Exhaust)词条时，本次打出进入摸牌堆并移除超时空词条，下次打出正常消耗
/// 3. 自动添加超时空描述文本和悬停提示
/// </summary>
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

    /// <summary>
    /// 子类重写此方法提供额外的悬停提示
    /// </summary>
    protected abstract List<IHoverTip> GetExtraHoverTips();

    protected override CardLocation GetResultLocationForCardPlay()
    {
        // 如果超时空效果已消耗，走正常流程
        if (_chronoConsumed)
        {
            return base.GetResultLocationForCardPlay();
        }

        bool hasExhaustKeyword = Keywords.Contains(CardKeyword.Exhaust);
        
        if (hasExhaustKeyword)
        {
            // 有消耗词条：执行最后一次超时空，移除超时空效果
            _chronoConsumed = true;
            if (DynamicVars["ChronoTitle"] is StringVar chronoTitleVar)
            {
                chronoTitleVar.StringValue = string.Empty;
            }
            return new CardLocation(Owner, PileType.Draw, CardPilePosition.Bottom);
        }

        // 无消耗词条：正常超时空效果，进入摸牌堆
        return new CardLocation(Owner, PileType.Draw, CardPilePosition.Bottom);
    }
}
```

##### 第三步：卡牌继承基类

改造原有卡牌，继承 `ChronoCardModel` 而非 `CardModel`：

```csharp
public sealed class ChronoMiner : ChronoCardModel
{
    private static readonly CardValueStore.CardValues Values = AlliesCardValues.ChronoMiner;
    
    public ChronoMiner() : base((int)Values.Cost, CardType.Skill, CardRarity.Token, TargetType.Self) { }

    public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/allies/ahrvicon.png";

    protected override List<IHoverTip> GetExtraHoverTips()
    {
        return new List<IHoverTip>
        {
            ModCardKeywords.TechLevelT1.CreateHoverTip(),
            ModCardKeywords.Vehicle.CreateHoverTip()
        };
    }

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new IntVar("DollarValue", Values.DollarValue),
        new StringVar("ChronoTitle", "[gold]超时空.[/gold]\n")
    };

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        // 卡牌特有逻辑...
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["DollarValue"].BaseValue = Values.DollarValue + Values.DollarValueUpgraded;
    }
}
```

##### 第四步：添加本地化文本

**card_keywords.json**：
```json
{
    "chrono.title": "超时空",
    "chrono.description": "打出时进入摸牌堆。与消耗词条共存时，首次打出进入摸牌堆并移除超时空，下次打出正常消耗。"
}
```

**cards.json**（在描述开头添加 `{ChronoTitle}` 动态变量）：
```json
{
    "CHRONO_MINER.description": "{ChronoTitle}获得 {DollarValue} 资金。"
}
```

#### 核心机制详解

| 机制 | 说明 |
|------|------|
| `GetResultLocationForCardPlay()` | Beta版新增方法，控制卡牌打出后的去向 |
| `_chronoConsumed` | 状态标记，控制超时空效果是否已消耗 |
| `StringVar("ChronoTitle")` | 动态变量，控制描述开头的"超时空."文本显示/隐藏 |
| `GetExtraHoverTips()` | 抽象方法，子类返回额外的悬浮提示 |
| `ExtraHoverTips` | 基类重写，根据 `_chronoConsumed` 状态动态添加超时空词条悬浮提示 |

#### 效果流程

```
打出超时空卡牌（无消耗词条）
    ↓
GetResultLocationForCardPlay() 返回 CardLocation(Draw, Bottom)
    ↓
卡牌进入摸牌堆底部，超时空效果保留
    ↓
下次打出重复此流程

打出超时空卡牌（有消耗词条）
    ↓
检测到 CardKeyword.Exhaust
    ↓
_chronoConsumed = true
    ↓
ChronoTitle.StringValue = ""（移除描述中的"超时空."文本）
    ↓
返回 CardLocation(Draw, Bottom)，卡牌进入摸牌堆
    ↓
下次打出时 _chronoConsumed = true
    ↓
走 base.GetResultLocationForCardPlay()，正常消耗
```

#### 优势

1. **代码解耦**：超时空逻辑集中在基类，卡牌只需关注自身特有逻辑
2. **易于维护**：修改超时空规则只需修改基类，影响所有超时空卡牌
3. **一致性**：所有超时空卡牌行为一致，避免遗漏或错误
4. **可扩展性**：新增超时空卡牌只需继承基类，无需重复编写超时空逻辑

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

## 3.11 多人联机卡牌

### 3.11.1 概述

《杀戮尖塔2》支持多人联机模式，Mod可以创建仅在多人模式下可用的卡牌。多人联机卡牌可以实现：
- 给队友添加卡牌或buff
- 将自己的卡牌转移给队友
- 与队友配合的组合效果

### 3.11.2 多人模式限制（CardMultiplayerConstraint）

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
public sealed class MyMultiplayerCard : CardModel
{
    // 此卡仅在多人联机模式下出现
    public override CardMultiplayerConstraint MultiplayerConstraint 
        => CardMultiplayerConstraint.MultiplayerOnly;
}
```

### 3.11.3 多人目标类型（TargetType）

多人联机卡牌可以使用以下目标类型来选择队友：

| 目标类型 | 说明 |
|---------|------|
| `TargetType.AnyAlly` | 选择任意单个队友（可以是自己或其他玩家） |
| `TargetType.AllAllies` | 选择所有队友 |
| `TargetType.AnyPlayer` | 选择任意玩家 |

**使用示例**：

```csharp
// 需要选择一个队友作为目标
public MyMultiplayerCard() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyAlly) { }
```

### 3.11.4 获取队友列表

使用 `CombatState.GetTeammatesOf()` 方法获取所有队友生物：

```csharp
// 获取所有队友（包含自己）
IEnumerable<Creature> allTeammates = base.CombatState.GetTeammatesOf(base.Owner.Creature);

// 过滤：只获取存活的、非自己的队友玩家
var validTeammates = from c in base.CombatState.GetTeammatesOf(base.Owner.Creature)
    where c != null && c.IsAlive && c.IsPlayer && c.Player != base.Owner
    select c;

// 判断是否有有效队友
if (validTeammates.Count() == 0)
{
    // 没有队友，处理单人模式情况
    return;
}
```

### 3.11.5 将卡牌转移给队友（核心API）

使用 `CardPileCmd.GiveToAnotherPlayer()` 方法将卡牌转移给队友（参考beta版"魔球"TheBall）：

```csharp
// 方法签名
public static async Task GiveToAnotherPlayer(
    CardModel card,                    // 要转移的卡牌
    Player player,                     // 目标队友（接收方）
    PileType pileType,                 // 放入目标的哪个牌堆
    CardPilePosition position = CardPilePosition.Bottom,  // 牌堆中的位置
    AbstractModel? clonedBy = null
)
```

**参数说明**：

| 参数 | 说明 |
|------|------|
| `card` | 要转移的卡牌（可以是 `this` 即本卡，也可以是其他卡牌） |
| `player` | 接收卡牌的队友玩家 |
| `pileType` | 放入目标的哪个牌堆 |
| `position` | 在牌堆中的位置 |

**PileType 可选值**：
- `PileType.Hand` - 手牌
- `PileType.Draw` - 抽牌堆
- `PileType.Discard` - 弃牌堆
- `PileType.Exhaust` - 消耗堆

**CardPilePosition 可选值**：
- `CardPilePosition.Top` - 顶部
- `CardPilePosition.Bottom` - 底部（默认）
- `CardPilePosition.Random` - 随机位置

**完整示例：将本卡交给随机队友**

```csharp
protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
{
    // 1. 获取有效队友
    var teammates = from c in base.CombatState.GetTeammatesOf(base.Owner.Creature)
        where c != null && c.IsAlive && c.IsPlayer && c.Player != base.Owner
        select c;

    if (teammates.Count() == 0)
        return;

    // 2. 随机选择一个队友
    Creature randomTeammate = base.Owner.RunState.Rng.CombatTargets.NextItem(teammates);

    // 3. 将本卡转移给队友（放入抽牌堆，随机位置）
    await CardPileCmd.GiveToAnotherPlayer(
        this,
        randomTeammate.Player,
        PileType.Draw,
        CardPilePosition.Random
    );
}
```

### 3.11.6 给队友添加生成的卡牌

使用 `CardFactory.GetDistinctForCombat()` + `CardPileCmd.AddGeneratedCardToCombat()` 给队友添加一张随机卡牌（参考原版"慷慨捐助"Largesse）：

```csharp
protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
{
    // 校验目标
    ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

    // 播放施法动画
    await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

    // 为目标玩家生成一张随机无色牌
    CardModel cardModel = CardFactory.GetDistinctForCombat(
        cardPlay.Target.Player,
        ModelDb.CardPool<ColorlessCardPool>().GetUnlockedCards(
            cardPlay.Target.Player, CardRarity.Common, includeUncollectable: false),
        1,
        Owner.RunState.Rng.CombatCardGeneration
    ).FirstOrDefault();

    // 如果本卡升级了，生成的牌也升级
    if (cardModel != null && IsUpgraded)
        CardCmd.Upgrade(cardModel);

    // 添加到目标玩家手牌（通过 Owner 同步）
    await CardPileCmd.AddGeneratedCardToCombat(cardModel, PileType.Hand, Owner);
}
```

### 3.11.7 给所有队友添加效果

遍历所有队友，给每个队友添加 buff 或其他效果（参考原版"能量涌动"EnergySurge）：

```csharp
protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
{
    // 获取所有存活的队友玩家
    var teammates = from c in base.CombatState.GetTeammatesOf(base.Owner.Creature)
        where c != null && c.IsAlive && c.IsPlayer
        select c;

    // 给每个队友加能量
    foreach (Creature teammate in teammates)
    {
        await PlayerCmd.GainEnergy(base.DynamicVars.Energy.IntValue, teammate.Player);
    }
}
```

### 3.11.8 参考原版实现

| 卡牌 | 功能 | 实现要点 | 适用场景 |
|-----|------|---------|---------|
| **Largesse（慷慨捐助）** | 给队友添加一张随机无色牌 | `CardFactory.GetDistinctForCombat()` + `CardPileCmd.AddGeneratedCardToCombat()` | 给队友随机送牌 |
| **TheBall（魔球，beta版）** | 将本卡交给随机队友 | `CombatState.GetTeammatesOf()` + `CardPileCmd.GiveToAnotherPlayer()` | 传递卡牌/接力效果 |
| **EnergySurge（能量涌动）** | 给所有队友加能量 | 遍历 `GetTeammatesOf()` + `PlayerCmd.GainEnergy()` | 群体增益效果 |

### 3.11.9 常见问题

| 问题 | 原因 | 解决 |
|------|------|------|
| 单人模式下多人卡牌仍出现 | 没有设置 `MultiplayerConstraint` | 设置为 `CardMultiplayerConstraint.MultiplayerOnly` |
| 转移卡牌后自己手牌还有这张牌 | 没有同时移除本卡 | `GiveToAnotherPlayer` 会自动处理，无需手动移除 |
| 队友列表为空导致报错 | 单人模式下没有队友 | 先检查 `teammates.Count() > 0` 再执行 |
| 联机状态不同步 | 使用了非同步的随机数 | 使用 `Owner.RunState.Rng.CombatTargets` 获取战斗随机数 |

---

## 3.12 转账系统（DollarTransfer）

### 3.12.1 概述

转账系统允许玩家在多人联机模式下将资金转移给队友，增强团队协作体验。系统包含完整的并发控制和网络同步机制，确保转账操作的安全性和一致性。

### 3.12.2 核心架构

```
┌─────────────────────────────────────────────────────────────┐
│                     转账系统架构                            │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  DollarTransferScreen (UI层)                                │
│       │                                                     │
│       │ 用户选择目标和金额                                    │
│       ↓                                                     │
│  DollarTransferManager (业务层)                             │
│       │                                                     │
│       ├── 并发控制 (_isTransferring + lock)                 │
│       ├── 转账执行 (ExecuteTransfer)                        │
│       └── 网络同步解锁 (DollarTransferUnlockAction)         │
│                 │                                           │
│                 ↓                                           │
│  DollarTransferGameAction (游戏动作层)                       │
│       │                                                     │
│       ├── 资金扣除 (sender)                                 │
│       └── 资金增加 (receiver)                               │
│                 │                                           │
│                 ↓                                           │
│  NetDollarTransferGameAction (网络同步层)                    │
│       │                                                     │
│       └── 自动注册 (ReflectionHelper.GetSubtypesInMods)     │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### 3.12.3 核心组件详解

#### DollarTransferManager

转账逻辑管理器，负责并发控制和网络同步：

```csharp
// RedAlert2ModCode/Common/Utils/DollarTransferManager.cs
public static class DollarTransferManager
{
    private static readonly Dictionary<long, TransferRequest> _pendingTransfers = new();
    private static readonly object _lock = new();
    private static bool _isTransferring = false;

    // 检查是否可以转账
    public static bool CanTransfer(Player sender, int amount);

    // 获取有效转账目标
    public static IEnumerable<Player> GetValidTargets(Player sender);

    // 执行转账（核心方法）
    public static bool ExecuteTransfer(Player sender, Player receiver, int amount);

    // 获取发送者资金余额
    public static int GetSenderBalance(Player sender);

    // 重置转账锁
    public static void ResetTransferLock();
}
```

#### DollarTransferScreen

转账UI面板，供玩家选择目标和金额：

```csharp
// RedAlert2ModCode/UI/DollarTransferScreen.cs
public sealed partial class DollarTransferScreen : Control, IOverlayScreen
{
    // 显示转账面板
    public static async Task<int?> ShowTransferScreen(Player sender);

    // 关闭面板
    public void Close();

    // 内部方法
    private void OnTargetSelected(int targetIndex);  // 选择目标后触发
    private void ShowError(string message);          // 显示错误提示
}
```

#### DollarTransferGameAction

转账游戏动作，处理实际的资金转移：

```csharp
// RedAlert2ModCode/Common/GameActions/DollarTransferGameAction.cs
public class DollarTransferGameAction : GameAction
{
    public Player Sender { get; }
    public ulong ReceiverNetId { get; }
    public int Amount { get; }

    protected override async Task ExecuteAction()
    {
        // 1. 从发送者扣除资金
        await PowerCmd.ModifyAmount(context, senderPower, -Amount, ...);
        
        // 2. 给接收者增加资金（如果没有DollarPower则创建）
        if (receiverPower == null)
            await PowerCmd.Apply<DollarPower>(context, receiver.Creature, Amount, ...);
        else
            await PowerCmd.ModifyAmount(context, receiverPower, Amount, ...);
    }

    public override INetAction ToNetAction()
    {
        return new NetDollarTransferGameAction { receiverNetId, amount };
    }
}
```

#### DollarTransferUnlockAction

转账锁解锁动作，用于网络同步解锁信号：

```csharp
// RedAlert2ModCode/Common/GameActions/DollarTransferUnlockAction.cs
public class DollarTransferUnlockAction : GameAction
{
    protected override async Task ExecuteAction()
    {
        // 重置转账锁（所有玩家收到后都会执行）
        DollarTransferManager.ResetTransferLock();
    }
}
```

### 3.12.4 并发控制机制

转账系统采用多重并发保护机制，确保多人联机时的安全性：

#### 第一层：_isTransferring 标志

使用静态布尔值防止同时发起多次转账：

```csharp
lock (_lock)
{
    if (_isTransferring)
    {
        // 已有转账进行中，拒绝本次请求
        return false;
    }
    _isTransferring = true;
}
```

#### 第二层：lock 线程安全

使用 `lock(_lock)` 确保多线程环境下的线程安全：

```csharp
private static readonly object _lock = new();

public static void ResetTransferLock()
{
    lock (_lock)
    {
        _isTransferring = false;
    }
}
```

#### 第三层：网络同步解锁

转账完成后发送 `DollarTransferUnlockAction` 同步给所有玩家：

```csharp
action.AfterFinished += delegate
{
    // 本地解锁
    lock (_lock)
    {
        _isTransferring = false;
    }

    // 发送解锁同步给其他玩家
    var unlockAction = new DollarTransferUnlockAction(sender);
    RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(unlockAction);
};
```

#### 第四层：面板打开自动解锁

作为保险机制，重新打开转账面板时自动调用 `ResetTransferLock()`：

```csharp
public static async Task<int?> ShowTransferScreen(Player sender)
{
    // 打开面板时自动解锁
    DollarTransferManager.ResetTransferLock();
    
    var screen = new DollarTransferScreen(sender);
    // ...
}
```

### 3.12.5 完整工作流程

#### 正常转账流程

```
玩家A打开转账面板
    ↓
ResetTransferLock() → _isTransferring = false
    ↓
玩家A选择目标（玩家B）和金额（1000），点击转账
    ↓
ExecuteTransfer(A, B, 1000) 检查 _isTransferring
    ↓
_isTransferring = false → 设置为 true
    ↓
创建 DollarTransferGameAction(A, B.NetId, 1000)
    ↓
RequestEnqueue(action) → 加入动作队列
    ↓
动作执行：A的资金-1000，B的资金+1000
    ↓
AfterFinished 回调：
    ↓
_isTransferring = false（本地解锁）
    ↓
创建 DollarTransferUnlockAction(A)
    ↓
RequestEnqueue(unlockAction) → 同步给所有玩家
    ↓
玩家B收到解锁动作 → ResetTransferLock() → _isTransferring = false
    ↓
所有玩家均可再次发起转账
```

#### 并发冲突流程

```
玩家A和玩家B同时点击转账
    ↓
玩家A的 ExecuteTransfer() 获取锁 → _isTransferring = true
    ↓
玩家B的 ExecuteTransfer() 获取锁 → _isTransferring = true（被拒绝）
    ↓
玩家B收到错误提示："转账失败，请稍后重试"
    ↓
玩家A的转账完成 → 发送解锁同步
    ↓
玩家B收到解锁信号 → _isTransferring = false
    ↓
玩家B可以再次尝试转账
```

### 3.12.6 本地化配置

在 `card_keywords.json` 中添加转账UI的本地化：

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

### 3.12.7 UI组件说明

转账面板包含以下UI组件：

| 组件 | 说明 |
|------|------|
| 标题 | "转账给队友" |
| 余额显示 | 显示当前资金余额 |
| 金额选择 | +/- 按钮和输入框，步长1000 |
| 目标选择 | 显示所有存活队友的按钮 |
| 错误提示 | 红色文本，显示转账失败原因 |
| 取消按钮 | 关闭面板 |

### 3.12.8 关键设计要点

#### NetAction 自动注册

游戏通过反射自动发现并注册所有实现 `INetAction` 接口的类型：

```csharp
// 游戏内部机制，无需手动注册
ReflectionHelper.GetSubtypesInMods<INetAction>();
```

因此 `NetDollarTransferGameAction` 和 `NetDollarTransferUnlockAction` 只需实现接口即可自动注册。

#### 异常处理

转账系统在所有关键路径都添加了异常处理：

```csharp
try
{
    RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(action);
}
catch (Exception ex)
{
    // 异常时也要解锁
    lock (_lock)
    {
        _isTransferring = false;
    }
    // 发送解锁同步
    var unlockAction = new DollarTransferUnlockAction(sender);
    RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(unlockAction);
}
```

#### UI反馈机制

- **转账成功**：关闭面板，玩家知道操作已完成
- **转账失败**：显示红色错误提示，保留面板，玩家可以重试

### 3.12.9 使用示例

#### 打开转账面板

```csharp
// 在卡牌或能力中调用
await DollarTransferScreen.ShowTransferScreen(player);
```

#### 直接执行转账

```csharp
// 在代码中直接执行转账（不需要UI）
bool success = DollarTransferManager.ExecuteTransfer(sender, receiver, amount);
if (success)
{
    // 转账成功
}
else
{
    // 转账失败
}
```

### 3.12.10 常见问题

| 问题 | 原因 | 解决 |
|------|------|------|
| 转账后资金不同步 | 网络延迟或同步失败 | `DollarTransferGameAction` 通过 `INetAction` 自动同步 |
| 并发转账导致卡死 | 未正确处理锁状态 | 多重并发保护机制确保安全 |
| 转账锁永久锁定 | 异常导致解锁逻辑未执行 | 打开面板时自动解锁作为保险 |
| 远程玩家无法转账 | 锁状态未同步 | `DollarTransferUnlockAction` 同步解锁信号 |

### 3.13 卡牌数值存储规范

#### 规则1：数值集中存储

- 任何卡牌的数值信息（费用、伤害、护盾、重复次数等）都必须在数值文件中统一存储
- 推荐使用 `AlliesCardValues.cs` 这样的静态类来管理所有卡牌数值
- 卡牌类中通过引用数值存储类来获取数值，避免硬编码
- **飞鹰/轨道系列**的卡牌数值和能力数值**统一存储在** `CommonCardValues.cs` 和 `CommonPowerValues.cs`，不使用阵营专属文件

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

#### 规则2：资金消耗本地化格式

- 任何需要消耗"资金"的**非单位**卡牌（如建筑卡、技能卡），必须在本地化描述的开头加上"价格：xxx。"
- **单位卡**的价格由生产序列能力在选择时消耗，一般不在本地化描述中展示价格
- 示例：`"ALLIED_WALL_CARD.description": "价格：${DollarNumber}。获得 {Block} 点护盾。将此牌返回你的手牌。"`

#### 规则3：动态数值显示（`diff()` 格式化器，容易遗漏）

- 卡牌描述中的伤害/格挡变量**必须**写成 `{Damage:diff()}`、`{Block:diff()}`、`{DeployDamage:diff()}` 等带 `:diff()` 的格式；
- 若写成裸 `{Damage}`，卡牌只会显示 `BaseValue`（基础/升级值），战斗中力量、易伤、虚弱、敏捷等 buff 对数值的修正**不会显示**在卡牌上（这就是"易伤增伤没显示"的根因）；
- 机制：`DamageVar`/`BlockVar.UpdateCardPreview` 通过 `Hook.ModifyDamage/ModifyBlock` 计算修正后的 `PreviewValue`；`NCard.UpdateVisuals` 在手牌/打出堆中运行全局 hooks（`runGlobalHooks=true`）；悬停/锁定敌人时 `NCardPlay.SetPreviewTarget` 提供目标，目标身上的易伤/虚弱等 debuff 才会参与计算；修正后数值与基础值不同时自动绿色（变高）/红色（变低）高亮，与原版 Strike 行为一致；
- 适用于：`Damage`、`Block`、`DefendDamage`、`DeployDamage`、`DeployVigor` 等攻击/防御类变量；
- 不要乱加：`Repeat`、`Poison`、`StoredCards`、`{0}`/`{1}` 等非 hook 修正值保持 `{Var}` 原样。

#### 价格映射注册（训练UI显示价格）

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

> **⚠️ 先用二分法判断你属于哪种场景！不要误用模式！**
>
> | 你监听伤害之后要触发什么行为？ | 推荐模式 |
> |-----------------------------|---------|
> | **改 Power 自己的状态（扣血/蓄能/护盾计数）+ 触发 Power 自己的效果（爆炸/放技能/护盾破）** | ⭐ 本节新模式：Harmony Postfix + Power 自监听 |
> | **卡牌级操作**（从牌堆/弃牌堆/抽牌堆搜一张卡、自动打出某卡、生成Token卡入手牌）<br>→ 典型例：利比亚"受击→搜牌堆→自动打出自爆卡车" | **遗物模式**（旧模式正确）。遗物天然有玩家 Owner、战斗 combatState、牌堆 DrawPile/DiscardPile/Hand、CardPlay 上下文，框架已经稳定跑了几年。 |
> | **跨多战斗的玩家级永久加成**（全局+力量、+血量上限、开局获得某卡/某单位） | **遗物模式**。遗物是战斗间持久化容器；Power 战斗结束会清空。 |
>
> 一句话口诀：**改Power自己 → Power自监听；动卡牌 → 遗物；跨战斗 → 遗物。**
>
> 典型用例：利比亚（LIBYA_RELIC）"受未格挡伤害时搜牌堆找自爆卡车自动打出" → 遗物模式是**正确选择**，不要硬改成Power自监听。

#### 旧模式（遗物）vs 新模式（Power自监听）适用场景对比

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

### 6.9 动态切换能力类型（Buff/Debuff）

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

## 12. Beta版 API 变化详解（正式版 → Beta版迁移）

> 以下API仅适用于Beta版，正式版使用不同签名。移植代码时需重点关注。

### 12.1 Beta版 vs 正式版 核心差异速览

| 模块 | 正式版 | Beta版 | 注意事项 |
|------|--------|--------|---------|
| 卡牌去向方法 | `GetResultPileTypeForCardPlay()` | `GetResultLocationForCardPlay()` | 返回值从 `PileType` 改为 `CardLocation` |
| 攻击卡FromCard | `FromCard(CardModel)` | `FromCard(CardModel, CardPlay?)` | 新增 `cardPlay` 参数 |
| 跨玩家传牌 | 需反射手写 | `CardPileCmd.GiveToAnotherPlayer()` | Beta版有原生API |
| CardPlay构造 | 可选Player | **必填** `Player` | `CardPlay.Player` 为必填成员 |
| Targeting参数 | 接受 `List<Creature>` | 单个 `Creature` 或 `TargetingAllOpponents()` | 群体攻击需改用新API |

### 12.2 卡牌去向：GetResultLocationForCardPlay

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

### 12.3 攻击卡：FromCard 新增 cardPlay 参数

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

### 12.4 群体攻击：Targeting 参数变化

**Beta版变更**：`Targeting` 不再接受 `List<Creature>`，需改用新API。

```csharp
// 正式版
DamageCmd.Attack(amount).FromCard(this).Targeting(List<Creature>)

// Beta版（二选一）
DamageCmd.Attack(amount).FromCard(this, cardPlay).Targeting(Creature)           // 单个目标
DamageCmd.Attack(amount).FromCard(this, cardPlay).TargetingAllOpponents(CombatState) // 所有敌人
```

### 12.5 CardPlay 构造：Player 为必填成员

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

### 12.6 CardModel 新增方法

| 方法 | 说明 |
|-----|------|
| `GiveToAnotherPlayer(Player)` | 将卡牌所有权移交给另一个玩家（直接设置 `_owner`） |
| `CreateCloneForPlayer(Player)` | 为指定玩家创建卡牌克隆 |

### 12.7 CardPileCmd 新增参数

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

## 13. 科技树系统（Tech Tree）

### 13.1 科技线架构

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

### 13.2 核心机制：BuildingTechTree

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

### 13.3 T1/T2/T3 科技等级规则

| 等级 | 解锁条件 | 解锁内容 | 示例 |
|------|----------|----------|------|
| **T1** | 基地车默认解锁 | 发电厂、兵营、矿场、围墙、机枪碉堡/哨戒炮 | 基础防御塔 |
| **T2** | 矿场解锁核心生产 + 空指部/雷达升级科技等级 | 重工、船厂、空指部/雷达 + 爱国者/磁暴线圈/防空炮/巨炮 + 维修厂 | 进阶防御塔 |
| **T3** | 空指部/雷达升级科技等级 + 作战实验室 | 作战实验室 + 超武（天气控制器/超时空/铁幕/核弹井）+ 矿石精炼器/核电站/工业工厂 | 超级武器 |

### 13.4 科技等级关键字

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

### 13.5 单位卡牌添加科技等级Tip

所有 **Token类型** 的单位卡牌（除围墙外）必须在 `ExtraHoverTips` 的**第一位**添加对应的科技等级关键字：

```csharp
protected override IEnumerable<IHoverTip> ExtraHoverTips =>
[
    ModCardKeywords.TechLevelT2.CreateHoverTip(),  // 科技等级Tip放在第一位
    ModCardKeywords.Vehicle.CreateHoverTip()       // 其他词条放在后面
];
```

### 13.6 本地化配置

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

## 14. 经济系统与建筑打出系统

### 14.1 经济系统（刀乐）

红警2 Mod引入了经济系统，通过"刀乐"能力来管理资金。建筑和单位的生产需要消耗资金，资源采集会增加资金。

**刀乐遗物（DollarRelic）**：初始遗物，战斗开始时赋予刀乐能力并设置启动资金。

**刀乐能力（DollarPower）**：专门用于存储资金数值的能力。

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

### 14.2 建筑打出系统（BuildingDrawPower & UrbanizationPower）

建筑卡牌打出后有两套独立的自动触发逻辑，均通过 `PowerModel.AfterCardPlayed` 钩子集中实现，**建筑卡牌自身无需写任何触发代码**：

| 能力 | 持有者 | 触发条件 | 效果 |
|------|--------|----------|------|
| `BuildingDrawPower`（隐藏） | 所有获得 `DollarPower` 的玩家 | 打出**非围墙且非防御塔**的建筑牌 | 抽1张牌（从抽牌堆顶） |
| `UrbanizationPower`（可见） | 打出 `UrbanizationCard` 的玩家 | 打出**非围墙**的建筑/防御塔牌 | 从牌堆中抽取建筑牌 |

两者在各自的 `AfterCardPlayed` 钩子中独立触发，互不干扰。

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

### 14.3 卡牌类型判断（CardUtils）

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

### 14.4 选择面板类建筑卡的取消处理

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

### 14.5 自动挂载机制（DollarPower.AfterApplied）

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

### 14.6 触发逻辑速查

| 卡牌类型 | BuildingDrawPower（抽1张） | UrbanizationPower（抽建筑牌） |
|---------|:------------------------:|:---------------------------:|
| 围墙 | ❌ | ❌ |
| 防御塔（光棱塔/机枪碉堡等） | ❌ | ✅ |
| 建筑（发电厂/重工/兵营等） | ✅ | ✅ |
| 选择面板类建筑卡（取消选择） | ❌ | ❌ |
| 选择面板类建筑卡（成功选择） | ✅ | ✅ |

### 14.7 新增建筑卡注意事项

1. **无需写任何抽牌/城市化触发代码**：两套系统均通过 `AfterCardPlayed` 钩子自动触发
2. **无需硬编码 `CardPileCmd.Draw(ctx, 1, Owner)`**：建筑抽牌由 `BuildingDrawPower` 统一处理
3. **无需硬编码 `UrbanizationPower.TriggerOnSuccessfulPlay(...)`**：该方法已删除，城市化由 `AfterCardPlayed` 统一处理
4. **选择面板类建筑卡**：实现 `ICancellableCardPlay`，取消时调用 `CardUtils.HandleCardCancellation(play, this, Owner)` 即可，无需额外处理

---

## 15. 阵营架构设计

### 15.1 设计理念

为了管理红警2中大量的单位卡牌，采用阵营分类架构：盟军、苏军、尤里、其他四大阵营，每个阵营包含：
- **单位卡**：士兵、装甲、飞机、船只
- **建筑卡**：兵营、重工、防御建筑等
- **技能卡**：用于卡组构造的特殊卡牌

### 15.2 卡牌注册管理器（CardRegistry）

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

### 15.3 架构优势

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

### 15.4 阵营目录结构

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

### 15.5 卡牌分类层级结构

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

### 15.6 CardRegistry 分类字段对应表

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

### 15.7 公共卡牌架构补充要点

在 3.8 公共卡牌架构基础上，补充以下工程要点：

#### 资源与能力共享

虽然卡牌实例分离，但以下资源和逻辑仍然共用：

| 共享类型 | 说明 |
|---------|------|
| **能力(Power)** | 公共卡牌使用的能力（如 `GoldMinePower`、`OilDerrickPower`）存放在 `Common/Powers/` 目录，两个阵营共用同一份 |
| **资源文件** | 卡牌图片、能力图标等资源文件存放在 `RedAlert2ModResources/`，两个阵营共用同一份 |
| **数值配置** | 卡牌数值存放在 `Common/Cards/CommonCardValues.cs`，两个阵营共用同一份 |
| **逻辑代码** | 所有 `OnPlay`、`OnUpgrade` 等方法在公共基类中实现，子类自动继承 |

#### 新增公共卡牌流程（方案二推荐）

1. **创建公共基类**：在 `Common/Cards/` 目录下创建卡牌类，重写 `Pool` 和 `VisualCardPool` 属性
2. **注册卡牌**：在 `AlliedCardRegistry` 和 `SovietCardRegistry` 中注册同一个公共基类
3. **添加本地化**：在 `cards.json` 中添加一份不带阵营前缀的本地化条目
4. **验证编译**：运行 `dotnet build` 确保没有错误

#### UI刷新注意事项

当卡牌打出后需要向手牌添加新卡牌时（如基地车选择建筑后），可能会出现卡牌卡在画面中央的情况，需要手动刷新游戏UI才能恢复正常。原因是游戏的卡牌堆刷新机制需要通过特定操作触发，单纯调用 `CardPileCmd.AddGeneratedCardToCombat()` 添加卡牌可能不会自动触发UI刷新。

**解决方案**：在添加卡牌到手牌后，调用 `CardPileCmd.Draw(ctx, 0, Owner)` 触发UI刷新。虽然抽0张牌，但会强制更新手牌区域的UI显示。

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

**适用场景**：基地车卡牌、集结卡牌、伞兵卡牌等所有需要在打出后向手牌添加卡牌的场景。

---

## 16. 音效播放系统

### 16.1 建筑音效播放

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

### 16.2 单位语音播放

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

### 16.3 建筑音效播放

`BuildingSoundHelper` 提供建筑放置音效的集中播放接口。

#### 基础用法

```csharp
// 在建筑卡牌的 OnPlay 方法中调用
BuildingSoundHelper.PlayBuildingPlaceSound();
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

## 17. 高级战备体系实现模式（飞鹰 & 轨道）

### 17.1 飞鹰战备体系（盟军）

#### 1. 架构设计：双基类封装

飞鹰战备体系通过两个基类实现了高度的代码复用和统一的行为逻辑：

- **`DesperateMeasureCardBase<TPower>` (卡牌基类)**：统一管理卡牌的打出流程、关键词显示（Pool/VisualCardPool 切换）、升级逻辑等。
- **`DesperateMeasurePowerBase` (能力基类)**：统一管理回合触发流程、目标锁定判定、溅射伤害处理、独立叠层逻辑等。

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

### 17.2 轨道战备体系（苏军）

#### 1. 与飞鹰战备的关键差异

- **前置依赖**：轨道战备卡**必须**在拥有「雷达（Radar）」能力时才能打出或出现在卡池中。
- **实现方式**：无统一的卡牌/能力基类，三张轨道卡（120mm、380mm、毒气）独立实现，但遵循相同的叠层和移除规范。

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

### 17.3 战备体系常见问题排查

| 现象 | 可能原因 | 解决方案 |
|-----|---------|---------|
| 叠层后伤害数值显示异常 | 使用了 `IsUpgraded` 而非实际数值作为叠层判断依据 | 改为按所有自定义数值字段（如 `CurrentDamage`, `CurrentWeak`）进行判断 |
| 战备能力触发后未移除 | `AfterSideTurnStart` 方法末尾漏写移除逻辑 | 检查并添加 `await PowerCmd.Remove(this);` 或调用基类的 `ConsumeOrRemove` |
| 苏军玩家无雷达却在奖励中拿到轨道卡 | 卡池过滤仅在 `CreatePowerCards` 中生效，未覆盖所有卡牌来源 | 在轨道卡的 `OnPlay` 方法开头增加雷达能力检查作为防御性代码 |
| 多张战备卡只触发了一次 | `Amount` 递减和 `Remove` 的逻辑位置不对，导致提前移除实例 | 确保在触发逻辑执行完毕后再消耗 `Amount` 或移除实例 |

---

## 18. 卡牌存储与消耗机制（IFV / 步兵车系列）

### 18.1 IFV 存储普通士兵的消耗逻辑

| IFV 状态 | 操作 | 存储卡牌去向 | IFV 去向 |
|----------|------|-------------|----------|
| 无消耗词条 | 攻击（抽牌弃牌+格挡） | 存储 | 弃牌堆 |
| 无消耗词条 | 部署（释放存储） | 士兵返回手牌 | 弃牌堆 |
| 有消耗词条 | 攻击（抽牌弃牌+格挡） | 无 | 消耗堆 |
| 有消耗词条 | 部署（释放存储） | 士兵返回手牌 | 消耗堆 |

> **关键实现**：通过 `GetPlayTargetPile()` 方法根据 `CardKeyword.Exhaust` 词条动态决定卡牌去向。存储士兵时 `CanonicalKeywords` 动态添加 `CardKeyword.Exhaust`，使卡牌显示消耗视觉指示器。

### 18.2 IFV 存储特殊士兵 → 特殊步兵车的转化机制

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

### 18.3 核心机制详解

#### 1. 消耗词条继承（关键）

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

#### 2. 克隆状态保留

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

#### 3. 双路径消耗逻辑

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

### 18.4 新增步兵车卡牌模板（基类方案）

由于所有步兵车共享相同的存储/消耗继承/双路径释放逻辑，推荐使用**基类模式**消除重复代码。

#### Step 0：创建 IfvVehicleBase 基类

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

#### Step 1：IFV 中添加特殊士兵识别

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

#### Step 2：使用公共转化工具 VehicleDeployHelper

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

#### Step 3：创建新步兵车（继承基类，只需 ~30 行）

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

#### Step 4：注册到卡池

在 `AlliesCardPool.GenerateAllCards()` 或 `AlliedCardRegistry` 中注册新卡牌。

#### Step 5：本地化

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

### 18.5 关键代码位置

| 文件 | 功能 |
|------|------|
| `Allies/Cards/Ifv.cs` | IFV 卡牌逻辑，特殊士兵识别与转化 |
| `Allies/Cards/IfvVehicleBase.cs` | 步兵车基类，封装存储/消耗/释放/双路径逻辑 |
| `Allies/Cards/RepairVehicle.cs` | 维修车（参考模板），继承基类实现特殊效果 |
| `Allies/Cards/AlliesCardValues.cs` | 数值配置 |
| `localization/*/card_keywords.json` | UI 选项本地化 |

### 18.6 新增特殊士兵检查清单

在添加新步兵车前，确保：

- [ ] 特殊士兵类型已注册到对应阵营 `CardRegistry` 的士兵列表（`Soldiers`/`RadarSoldiers`/`HighTechSoldiers`）
- [ ] IFV 的 `GetSoldierCardsFromHand()` 能识别该士兵类型
- [ ] 已创建 `IfvVehicleBase` 子类并注册到卡池
- [ ] 已完成 4 语言本地化（`cards.json` + `card_keywords.json`）
- [ ] 卡牌肖像图片已放置到正确路径

### 18.7 无士兵时的降级行为

当 IFV 选择部署但手牌无士兵卡牌时，卡牌**正常打出**（进弃牌堆/消耗堆）而非返回手牌：

```csharp
if (soldierCards.Count == 0)
{
    await CardPileCmd.Add(this, GetPlayTargetPile(), CardPilePosition.Bottom, this);
    return;
}
```

---

## 19. Mod配置面板与开局方案

本 mod 的配置面板（主菜单按钮进入）提供按角色（Character）独立配置的四个功能：自定义初始卡组、自定义初始遗物、基地车模式、幸运方块模式，以及卡池奖励模式。所有配置以 JSON 保存，**多人模式下按玩家 NetId 独立同步与应用**。

### 19.1 功能总览

| 配置项 | 作用 | 主要实现位置 |
|---|---|---|
| 自定义初始卡组 | 开局用自选卡牌替换默认初始卡组 | `CardLibraryTab.cs` / `InitialDeckPatch.ApplyConfigToPlayer` |
| 自定义初始遗物 | 开局用自选遗物替换默认初始遗物 | `RelicLibraryTab.cs` / `InitialDeckPatch.ApplyCustomRelics` |
| 基地车模式 | 追加基地车卡 + 补刀乐遗物 + 触发对应阵营国旗事件 | `ModConfigPatches.ApplyBaseCarMode` / `FlagSelectionPatches` |
| 幸运方块模式 | 初始卡组替换为箱子卡组（可与自定义卡组叠加） | `InitialDeckPatch.CreateLuckyCrateDeck` |
| 卡池奖励模式 | 控制箱子卡是否进入卡牌奖励 | `CratePoolHelper` / `CardRewardCratePatch` |

### 19.2 配置数据模型（CharacterConfig）

配置按角色 ID 保存于 `RedAlert2ModConfig.json`：

```json
{
  "characters": {
    "RED_ALERT2_MOD_CHARACTER_ALLIES": {
      "customDeckCardTypes": ["Strike", "Defend:U"],
      "enableCustomDeck": true,
      "startingRelicTypes": ["IvoryTile", "OrnamentalFan"],
      "enableCustomRelics": true,
      "baseCarMode": "Soviet",
      "luckyCrateMode": false,
      "cratePoolMode": "None"
    }
  }
}
```

要点：
- `customDeckCardTypes` 存卡牌**类型名**（`Type.Name`）；升级卡以 **`类型名:U`** 后缀标记，与未升级同名卡**分开独立叠加**（各按数量合并）。
- `startingRelicTypes` 存遗物类型名，同样支持 `:U` 无关（遗物没有升级，纯类型名）。
- 互斥规则：`baseCarMode != None` 时自动关闭 `luckyCrateMode`；`luckyCrateMode = true` 时把 `baseCarMode` 置为 `None`（由 `CharacterConfig` 属性 setter 统一保证）。

### 19.3 自定义初始卡组

**卡牌库 UI（`CardLibraryTab.cs`）**：
- 每张卡下方两个并排按钮：`＋ 添加`（左）与 `＋ 升级`（右），点击卡牌本身 = 普通添加。
- 升级条目写入配置时编码为 `类型名:U`。
- 筛选：类型（默认开启攻击/技能/能力）、角色（默认不选中=不过滤；选中后严格过滤）、通用；未选任何项时显示空态提示。
- NCard 从对象池复用时 `Ready` 信号不会再次触发，需在 `IsNodeReady()` 时立即 `UpdateVisuals`，否则残留上一张卡文案。

**开局应用（`InitialDeckPatch` → `ApplyConfigToPlayer`）**：
```csharp
player.Deck.Clear(silent: true);
foreach (CardModel card in replacement) {
    card.FloorAddedToDeck = 1;
    player.Deck.AddInternal(card, -1, true);
    if (runState != null) runState.AddCard(card, player); // 多人延迟应用时注册 Owner，避免 Hook 遍历 NRE
}
```
- 单机：在 `Player.PopulateStartingInventory` 的 Postfix 应用（`CreateShared` 会统一赋 Owner，**不要**预置 Owner）。
- 多人：开局同步完成后由 `RunStartPatch.ApplyConfigsAfterSyncAsync` 统一应用，需用 `runState.AddCard(card, player)` 注册。
- 升级条目在 `CreateCustomDeck` 中通过 `CardCmd.Upgrade(card)` 生成真正升级版卡牌。

### 19.4 自定义初始遗物

**遗物库 UI（`RelicLibraryTab.cs`）**：
- 按原版百科分组：初始/普通/罕见/稀有/商店/先古/事件，外加各 mod 专属池（按遗物池类型分组的 `专属 · xxx` 栏）。
- 点击图标添加/移除（右键取消选中），已添加显示金色边框 + 数量角标；悬停显示遗物 tooltip（挂到共享顶层 CanvasLayer 105）。
- 详情：点击已配置遗物打开游戏原生遗物检视页：`NGame.Instance.GetInspectRelicScreen().Open(relics, relic)`。

**开局应用（`ApplyCustomRelics`）**：清空默认遗物后按配置 `player.AddRelicInternal(relic, -1, true)`（该方法自带 `relic.Owner = player`），并 `MarkRelicAsSeen`。

### 19.5 基地车模式

规则（按角色**原生阵营**判断，`FlagManager.GetNativePlayerFaction`）：

| 角色原生阵营 | 基地车选择 | 效果 |
|---|---|---|
| 盟军 | 无 / 盟军 | 盟军：原生旗 + 同阵营重复一轮旗 + 额外一张盟军MCV + 缺刀乐则补 |
| 盟军 | 苏军 | 原生盟军旗 + 苏军旗 + 苏军MCV + 刀乐 |
| 苏军 | 盟军 | 对称 |
| 任意 | 尤里 | 仅授予尤里旗（不加MCV/刀乐） |
| 战士等非RA2 | 苏军/盟军 | 仅基地车阵营旗一次 |

实现：
- MCV 卡通过 `Deck.AddInternal` 加入（同阵营也加，2 张）；跨阵营 MCV 无本阵营建筑时，打出后自动回手牌（防空选择界面卡死）。
- 刀乐：`!player.Relics.Any(r => r is DollarRelic)` 时补授（自定义遗物替换后仍会补）。
- 国旗事件：单机 = 原生轮 + 基地车轮顺序执行；**多人 = 两轮都走 `PlayerChoiceSynchronizer` 同步**（`EnsureFlagsSelectedMultiplayer`，原生轮 + `baseCarRound: true` 第二轮），并带"每局每玩家每阵营只授一次"守卫（同阵营重复轮除外）。

### 19.6 幸运方块模式

- 初始卡组替换为：随机箱子×5，回血/士兵/载具/海军/空军箱子各×1（共 10 张）。
- 与自定义初始卡组叠加：箱子卡 + 自定义卡。
- 与基地车模式互斥（见配置模型）。

### 19.7 卡池奖励模式（CratePoolMode）

- 箱子卡属于角色默认卡池，`AlliesCardPool` / `SovietCardPool` 的 `AllCards` 始终包含（不再按模式过滤）。
- `AllCrates`（奖励只有箱子）：仅**战斗结束卡牌奖励**（`CardCreationOptions.ForRoom` 的 Encounter 来源）替换为纯箱子卡；商店/事件保持默认池。
- `AddCrates`（奖励加入箱子）：通过补丁 `CardCreationOptions.GetPossibleCards` 在奖励候选中混入箱子卡（`Distinct` 去重），原版角色也生效。

### 19.8 联机同步（配置按 NetId）

- 载荷：`NetConfigSyncAction`（INetAction，游戏自动反射注册）携带 角色ID/卡组/遗物/基地车/幸运方块/奖励模式；`ConfigSyncGameAction.ExecuteAction` 调 `ModConfigManager.SetRemoteCharacterConfig(NetId, config)`。
- 触发：开局 `RunManager.SetUpNewMultiplayer` Postfix 广播本机配置；配置保存时也尝试广播（两局之间动作队列不存在时静默失败，本机保存仍生效，下一局广播带上）。
- 应用：`ApplyConfigsAfterSyncAsync` 持续等待（战斗开始前）各玩家配置到位，**本机玩家用本机最新配置，远端玩家用同步配置**（避免 `_remoteConfigs` 过期副本导致"卡牌生效遗物不生效"）。
- 离开大厅时清空 `_remoteConfigs`，避免跨会话串配置。

### 19.9 初始资源配置 与 开局方案

#### 初始资源配置

`CharacterConfig` 新增 `StartingGold` / `MaxHp`（**0 = 使用角色默认值**）：

- 面板新增第三个功能页「初始资源配置」：两个 `SpinBox` 修改后立即 `UpdateCharacterConfig` 落盘并广播；
- 开局应用在 `InitialDeckPatch.ApplyConfigToPlayer` 末尾：金币直接写 `Player.Gold`，血量用 `Creature.SetMaxHpInternal / SetCurrentHpInternal` 同步（当前血量=配置上限，满血开局）；
- JSON 字段为 `startingGold` / `maxHp`，多人模式随 `ConfigSyncGameAction` 按玩家同步。

#### 开局方案（动态槽位，不设上限）

`ModConfigManager` 新增 `DeckConfigPreset`（`Name` + 全部角色 `CharacterConfig` 快照）：

- **动态列表**：`_presets` 为 `List<DeckConfigPreset?>`，**高亮槽 = 当前方案**（工作配置 `_configs` 绑定其上，索引 `activePresetIndex` 持久化），末尾始终保持一个空槽（`EnsureTrailingEmptySlot`）用于"保存即新建"，**数量不设上限**；
- JSON 根节点 `presets` 数组序列化：当前槽（即使为空）与已保存/已命名槽写入，未命名空槽不写（加载时自动重建）；旧 5 槽配置自动迁移；
- **绑定模型**：`Save()` 时把 `_configs` 深拷贝同步回当前槽（**编辑即同步**，无独立副本）；
- `SavePreset(i)`：遍历 `ModelDb.AllCharacters`，把**全部角色**的配置（未配置过的角色自动补默认项）用 `CharacterConfig.Clone()` 深拷贝写入；空槽 = **新建方案**（保留已命名名字，未命名自动"方案N"）并追加新空槽；已保存槽 = 覆盖内容（保留名字）；当前槽 = 重写内容（**不重命名**）；
- `LoadPreset(i)`：**只移动高亮**（`_activePresetIndex = i`）并载入工作配置，**不覆盖任何槽内容**；随后 `BroadcastAllLocalConfigs()` 重新广播本机配置；
- `DeletePreset(i)`：任意槽可删（含当前槽），删除后 `RecoverActiveSlot` 把高亮回退到最近的非空槽（全部为空则新建当前槽），并保持末尾空槽；
- `RenamePreset(i, name)`：改槽位名字（空槽也可提前命名，保存时名字保留）；
- UI（独立功能页「开局方案存储」）：动态槽位卡片，**高亮 = 当前方案**（金色边框 + 「当前」标签），单击不高亮不切换；卡片左上角「✏」编辑按钮（弹输入框命名）；右上角「✕」删除按钮（任意槽含当前槽，弹窗确认）；下方「保存」按钮 = 弹窗确认后把当前配置复制到该槽（空槽=新建，已占用=覆盖）；「切换」按钮或**双击槽卡片** = 直接加载该槽为当前方案（无确认弹窗，切换只移动高亮、不覆盖，随时可切回；空槽/当前槽无效）。

### 19.10 关键文件索引

```
RedAlert2ModCode/DeckConfig/ModConfigManager.cs   # 配置模型/JSON/同步
RedAlert2ModCode/DeckConfig/ModConfigPanel.cs     # 配置面板 UI + 卡组/遗物编辑器
RedAlert2ModCode/DeckConfig/CardLibraryTab.cs     # 卡牌库
RedAlert2ModCode/DeckConfig/RelicLibraryTab.cs    # 遗物库
RedAlert2ModCode/DeckConfig/ModConfigPatches.cs   # 开局应用/基地车/奖励/联机应用
RedAlert2ModCode/Common/Patches/FlagSelectionPatches.cs # 国旗事件（含多人同步轮）
RedAlert2ModCode/Common/Utils/FlagManager.cs      # 阵营/国旗工具
RedAlert2ModCode/Common/Utils/CratePoolHelper.cs  # 箱子卡池辅助
RedAlert2ModCode/Common/Utils/UiLayers.cs         # 共享顶层 UI 层（悬浮提示等）
RedAlert2ModCode/Common/GameActions/NetConfigSyncAction.cs / ConfigSyncGameAction.cs # 配置同步动作
```

---

## 20. 多人联机进阶机制

### 20.1 多人选择的异步执行机制（握手与约束）

#### 为什么选择要"暂停/恢复"

卡牌 `OnPlay` 中间的选牌发生在动作内部。若直接 await 面板，`PlayCardAction` 一直处于 **Executing**，占死 `ActionExecutor`，队友的卡牌也无法执行。因此需要 `SignalPlayerChoiceBegun` 让动作**暂停**，释放执行器给队友，选择完成后再 `SignalPlayerChoiceEnded` **恢复**。

#### 关键约束：暂停/恢复是主机中转握手

```csharp
// HookPlayerChoiceContext
await context.SignalPlayerChoiceBegun(player, PlayerChoiceOptions.CancelPlayCardActions);
// ... 本机面板 / 远端 WaitForRemoteChoice ...
await context.SignalPlayerChoiceEnded();
```

- **客户端**上，暂停（`RequestEnqueueHookAction`）与恢复（`RequestResumeActionAfterPlayerChoice`）都是"客户端 → 主机 → 客户端"的握手，顺序由主机统一裁定，保证两端校验和一致；
- **绝不能**用本机锁/信号量包住暂停或恢复阶段——本地锁会与握手互相等待造成卡死；
- 只有**纯本地、无握手**的处理（如 `CardUtils.HandleCardCancellation` 的返能/回手）可以用 `MultiplayerSyncHelper.RunSerialized` 串行化。

#### 恢复时的出牌队列视觉舞步

恢复动作会走 `NCardPlayQueue.ReAddCardAfterPlayerChoice`（卡牌重新入队）等视觉流程，该流程假定"同一时间只有一张卡在暂停"。两卡并发暂停/恢复时可能产生节点无父级/双释放，由 `NCardPlayQueueChoiceResumePatch` 防御性兜底（无父级节点先挂回、待删除节点跳过）。

### 20.2 生产建筑 A2 预选模式（先选后打）

兵营/重工/船厂/空指部/MCV/出售建筑已改为 **A2 预选模式**：点击手牌只弹本地面板，确认后才真正打出；取消则零副作用（卡牌留在手牌），彻底绕开"取消回手"与暂停/恢复的视觉竞态。

#### 流程

```
点击手牌（NPlayerHand.StartCardPlay 被拦截）
→ 本地预选面板（卡牌不出手、不扣费、不暂停）
→ 确认 → 入队两个动作：
    ├─ PlayCardAction（正常打出，OnPlay 最小化）
    └─ BuildingResolutionAction（扣费/加能力/生产序列/出售，载荷=选择结果）
→ 取消 → 只关面板，什么都不发生
```

#### 关键组件

| 组件 | 作用 |
|------|------|
| `BuildingPrePlayHelper` | 拦截判断、打开预选面板、入队打出+结算；`RunSerialized` 无握手限制 |
| `BuildingPrePlayInterceptPatch` | Harmony 前缀拦截 `NPlayerHand.StartCardPlay` |
| `BuildingResolutionAction` + `NetBuildingResolutionAction` | 结算动作与跨端序列化（`ActionTypes` 自动注册 Mod 的 `INetAction`） |
| `CardUtils.GetCardModelByEntry` | 结算时按 Entry 重建单位信息 |

#### 新增一张生产建筑卡要做的

1. 在 `*CardValues.cs` 注册数值与映射；
2. 在 `*CardRegistry.cs` 的 `BuildingCards`/`Vehicles` 等注册；
3. **候选构建**：把"可用单位列表 + 国旗/科技过滤 + 升级处理"抽成静态 `GetPrePlayCandidates(Player owner, bool isUpgraded)`；
4. **OnPlay 最小化**：只留音效/动画，不弹面板、不处理取消；
5. 在 `BuildingPrePlayHelper.OpenPanelAsync` 的 switch 中注册该卡的候选与数值映射；
6. 在 `BuildingResolutionAction` 中按 `BuildingEntry` 增加扣费/加能力/生产序列逻辑；
7. 若该卡会被"自动打出"，OnPlay 的兜底会自动补开面板（确认后只入队结算）。

#### 待结算标记（防止重复）

手动 A2 确认时，`EnqueuePlay` **先** `MarkPendingResolution(card)` **再**入队打出动作——因为动作队列可能同步立刻执行 OnPlay；标记必须早于 OnPlay 设置，否则 OnPlay 会误走"自动打出兜底"再开一个面板，导致重复结算。

---

## 21. 先古之民对话本地化（RitsuLib）

### 21.1 核心原理

先古之民（Neow、建筑师 `THE_ARCHITECT`、Darv、Orobas 等）的对话通过 `ancients.json` 的本地化键定义。角色通过 `[RegisterCharacter]` 注册到 RitsuLib 后，RitsuLib 会在 `AncientDialogueSet.PopulateLocKeys` 执行前，自动把本地化表里属于该角色的对话**追加**进对话集——**无需也不应再写 Harmony 补丁硬编码对话**，否则会与 RitsuLib 的追加叠加，导致列表索引错位、生成缺失键。

### 21.2 键名格式

```text
{ANCIENT}.talk.{角色Entry}.{对话序号}-{行序号}[r].{ancient|char}
{ANCIENT}.talk.{角色Entry}.{对话序号}-{行序号}[r].next
```

| 部分 | 说明 |
|------|------|
| `ANCIENT` | 先古 ID，如 `NEOW`、`THE_ARCHITECT` |
| `角色Entry` | 角色的 **ModelId Entry**（如 `RED_ALERT2_MOD_CHARACTER_ALLIES`），不是短别名 |
| `对话序号` | 从 0 开始连续编号；扫描到首个缺失序号即停止 |
| `行序号` | 该段对话内的行，从 0 开始连续编号 |
| `r` | 可选；带 `r` 表示该段对话可重复（进入重复池，多次通关后仍会随机出现） |
| `ancient` / `char` | 发言者：`.ancient` 为先古说，`.char` 为角色说；同一行优先读 `.ancient` |
| `.next` | 除最后一行外，每行必须配"继续"按钮文本；不带 `.ancient/.char` 后缀 |

### 21.3 建筑师（THE_ARCHITECT）特殊规则

- 对话序号即拜访序号：`VisitIndex = dialogueIndex`（RitsuLib 自动解析），角色第 N 次通关显示第 N-1 段（`charVisits = TotalWins`）。
- 超过最后一段后，只从带 `r` 后缀的对话中随机复用；因此建议每段都加 `r`。
- 攻击演出可选键（值为 `None` / `Player` / `Architect` / `Both`）：
  - `{对话序号}-attack`：结束攻击者
  - `{对话序号}-startattack`：开场攻击者
  - `{对话序号}-endattack`：结束攻击者（缺省 `Architect`）

### 21.4 Neow 等其他先古的规则

- 拜访序号映射：`0→0`、`1→1`、`2→4`，之后每段 `+3`（与原版 Neow 一致）。
- 原版 Neow 对话不带 `r`，默认不重复。

### 21.5 完整示例

```json
{
  "NEOW.talk.RED_ALERT2_MOD_CHARACTER_ALLIES.0-0.ancient": "……盟军……指挥官……",
  "NEOW.talk.RED_ALERT2_MOD_CHARACTER_ALLIES.0-0.next": "回应",
  "NEOW.talk.RED_ALERT2_MOD_CHARACTER_ALLIES.0-1.char": "盟军永不退缩。",
  "NEOW.talk.RED_ALERT2_MOD_CHARACTER_ALLIES.0-1.next": "继续",
  "NEOW.talk.RED_ALERT2_MOD_CHARACTER_ALLIES.0-2.ancient": "……很好……去吧……",

  "THE_ARCHITECT.talk.RED_ALERT2_MOD_CHARACTER_ALLIES.0-0r.ancient": "盟军。你的科技令人印象深刻。",
  "THE_ARCHITECT.talk.RED_ALERT2_MOD_CHARACTER_ALLIES.0-0r.next": "回应",
  "THE_ARCHITECT.talk.RED_ALERT2_MOD_CHARACTER_ALLIES.0-1r.char": "不可避免？盟军总能找到出路！",
  "THE_ARCHITECT.talk.RED_ALERT2_MOD_CHARACTER_ALLIES.0-1r.next": "继续",
  "THE_ARCHITECT.talk.RED_ALERT2_MOD_CHARACTER_ALLIES.0-2r.ancient": "没有目的的智慧只是噪音。",
  "THE_ARCHITECT.talk.RED_ALERT2_MOD_CHARACTER_ALLIES.0-2r.next": "继续",
  "THE_ARCHITECT.talk.RED_ALERT2_MOD_CHARACTER_ALLIES.0-3r.char": "我们的目标很明确！",
  "THE_ARCHITECT.talk.RED_ALERT2_MOD_CHARACTER_ALLIES.0-endattack": "Both"
}
```

### 21.6 注意事项

1. **角色 Entry 必须用真实 ID**：代码中通过 `ModelDb.GetId<Allies>().Entry` 获取（例如 `RED_ALERT2_MOD_CHARACTER_ALLIES`），不要写 `ALLIES` 这类短别名，否则键永远不会被读到。
2. **不要同时硬编码 `TheArchitect.DefineDialogues` 补丁**：RitsuLib 会按本地化键再追加一份，两套叠加后列表下标偏移，会出现 `THE_ARCHITECT.talk.XXX.7-0.char` 之类的缺失键（原样显示键名）。
3. 修改 `ancients.json` 后需要重新导出 pck（本地化文件在 pck 内）；若同时改了代码，DLL 也要一起替换。
4. 各语言文件（zhs / eng / jpn / kor 等）的键集合必须保持一致，只翻译值。
5. 若某角色完全没配置对话，RitsuLib 的"空对话回退"需要开启调试兼容总开关 + Ancient/THE_ARCHITECT 兼容设置才会生效（避免 PROCEED 时空引用崩溃）；对话追加本身不依赖该开关。

---

## 22. UI选择页面本地化配置

### 22.1 核心原理

游戏仅加载特定名称的JSON本地化文件，自定义的 `ui_strings.json`、`engineer_choices.json` 等文件**不会被游戏自动识别**。自定义UI文本必须整合到游戏原生支持的本地化文件中，推荐使用 `card_keywords.json`。

### 22.2 游戏支持的本地化文件

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

### 22.3 实现步骤

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

### 22.4 命名空间约定

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

### 22.5 动态变量替换

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

### 22.6 支持的 Add 方法重载

```csharp
locString.Add("name", decimal value);    // 数值
locString.Add("name", bool value);       // 布尔值
locString.Add("name", string value);     // 字符串
locString.Add("name", IList<string> value); // 字符串列表
locString.Add("name", LocString value);  // 嵌套本地化字符串
```

### 22.7 注意事项

1. **不要创建自定义JSON文件**：游戏不会自动加载非标准名称的本地化文件
2. **使用 object 类型**：ChoiceOption 的 Title 和 Description 应定义为 `object` 类型，同时支持 `string` 和 `LocString`
3. **统一使用 GetLocStringText**：所有显示文本的地方都应通过此方法处理
4. **中英文文件同步**：修改中文 `zhs/card_keywords.json` 后，必须同步修改英文 `eng/card_keywords.json`

---

## 23. 本地化键名规则与关键API速查

### 23.1 本地化键名规则汇总

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

### 23.2 关键API速查

#### 伤害命令
```csharp
await DamageCmd.Attack(damage)
    .FromCard(card) / .FromMonster(monster) / .FromOsty(osty)
    .Targeting(creature) / .TargetingAllOpponents(state)
    .WithHitFx("vfx/path")
    .Execute(context);
```

#### 能力命令
```csharp
await PowerCmd.Apply<PowerType>(target, amount, source, sourceCard);
await PowerCmd.Remove(powerInstance);
await PowerCmd.Decrement(powerInstance);
```

#### 玩家命令
```csharp
await PlayerCmd.GainEnergy(amount, owner);
await PlayerCmd.GainGold(amount, owner);
await PlayerCmd.GainBlock(amount, owner);
```

#### 生物命令
```csharp
await CreatureCmd.Damage(context, target, amount, props, source, card);
await CreatureCmd.Heal(target, amount);
await CreatureCmd.GainBlock(target, amount, props, card, fast);
```

#### 卡牌命令
```csharp
await CardPileCmd.Add(card, PileType.Deck);
CardCmd.Enchant<EnchantType>(card, amount);
CardCmd.RemoveKeyword(card, CardKeyword.Exhaust);
```

#### 遗物命令
```csharp
await RelicCmd.Obtain(relic.ToMutable(), owner);
```

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
