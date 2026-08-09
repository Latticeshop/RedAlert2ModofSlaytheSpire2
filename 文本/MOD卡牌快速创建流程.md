# MOD卡牌快速创建流程

> 基于红警2 Mod开发经验整理，包含单位卡、建筑卡、战备卡等类型的快速创建流程与核心检查点。
> **详细技术实现、API定义、复杂系统架构请查阅：[杀戮尖塔2_Mod开发教程API快速参考_Beta版.md](file:///d:/RedAlert2Project/red-alert-2-mod/%E6%96%87%E6%9C%AC/%E6%9D%80%E6%9C%8D%E5%B0%96%E5%A1%942_Mod%E5%BC%80%E5%8F%91%E6%95%99%E7%A8%8BAPI%E5%BF%AB%E9%80%9F%E5%8F%82%E8%80%83_Beta%E7%89%88.md)**

---

## 一、创造单位卡牌流程（以V3火箭为例）

### 1. 注册语音
在 `UnitVoiceConfig.cs` 中注册语音路径，并在卡牌 `OnPlay()` 中调用 `UnitVoiceHelper.PlayUnitVoice()`。

### 2. 设置图标
设置 `PortraitPath` 指向卡牌图片资源。

### 3. 数值存储
在 `*CardValues.cs` 中定义数值，并在 `CreateVehicleValuesMap()` (或对应地图) 中注册，映射键必须与类名生成的 ID 完全匹配（如 `V3ROCKET`）。

### 4. 生产序列选项
在 `*CardRegistry.cs` 的对应列表（如 `Vehicles`）中注册卡牌。

### 5. 科技等级悬浮Tip（必须）
在 `ExtraHoverTips` 第一位添加 `ModCardKeywords.TechLevelT1/2/3.CreateHoverTip()`。

### 6. 科技解锁（如需要）
在 `*TechTreeConfig.cs` 配置解锁条件，并在 `*CardRegistry.cs` 的条件列表（如 `RadarVehicles`）中注册。

### 7. 动态数值配置
注册 `DynamicVar` 并在 `OnUpgrade()` 中更新数值。

### 8. 本地化
在 `cards.json` 中添加卡牌 `title` 和 `description`。
- **伤害/格挡变量必须写 `{Damage:diff()}` / `{Block:diff()}`**，不能写裸 `{Damage}`——
  否则战斗中的力量/易伤/虚弱/敏捷修正不会显示在卡牌上（详见 API 文档「规则3」）。

### 9. 遗物转换注册（重要）

新叶（NewLeaf）和树叶膏药（LeafyPoultice）会转换牌组中的单位卡。新增的单位卡**必须注册到对应列表**，否则会走原版随机转换而非 Mod 卡池：

| 单位类型 | 注册列表 | 说明 |
|---------|----------|------|
| 士兵 | `Soldiers` / `RadarSoldiers` / `HighTechSoldiers` / `RelicUnlockedSoldiers` | 按 T1/T2/T3/遗物解锁分类 |
| 装甲 | `Vehicles` / `RadarVehicles` / `HighTechVehicles` | 按 T1/T2/T3 分类 |
| 飞机 | `Aircraft` | — |
| 船只 | `Ships` / `HighTechShips` | 按 T1/T3 分类 |
| **特殊单位卡** | `SpecialUnits` | 如 YuriCard、YuriPrimeCard（**Paratrooper 伞兵和 AirborneDivision 空降师团均不属于单位卡，不注册**） |
| **MCV** | `MobileConstructionVehicles` | 既是装甲单位也是建筑 |

- `GetAllUnits()` 和 `GetAllUnitTypes()` 会自动包含上述所有列表
- 新叶选择面板会**自动排除**围墙和诅咒卡（`CardType.Curse`）
- 非 Mod 角色走原版逻辑，无需处理

> 详细实现请查阅 API 文档「遗物卡牌转换补丁」章节。

---

## 二、创造建筑卡牌流程（以雷达为例）

### 1. 设置图标
设置 `PortraitPath`。

### 2. 数值存储
在 `*CardValues.cs` 中定义数值。同时注册到 `CreateBuildingValuesMap()`（MCV造价显示）和 `BuildingModelMap`（MCV创建卡牌实例）。

### 3. 动态数值配置
注册 `DynamicVar`（如 `DollarVar`, `IntVar`）并在 `OnUpgrade()` 中更新。

### 4. 创建对应能力
创建 `*Power.cs` 并在 `PowerIconPatch.cs` 中注册图标。

### 5. 注册到建筑列表
在 `*CardRegistry.cs` 的 `BuildingCards` 中注册。

### 6. 科技线配置

建筑科技线分为**核心建筑**和**牌组建筑**两类，配置方式不同：

#### 核心建筑（TechTreeConfig）

核心建筑在 MCV 选项中自动显示（无需在牌组中），需在 `*TechTreeConfig.cs` 中配置：

```csharp
// 矿场：生产解锁（解锁T2核心建筑，不升级科技等级）
var refinery = new TechBuildingInfo(typeof(Refinery), TechLevel.T1, 
    powerType: typeof(RefineryPower));
refinery.WithProductionUnlock();

// 空指部/雷达：科技等级升级（CurrentTechLevel → T2）
new(typeof(AirForceCommand), TechLevel.T2, unlocksNextTech: true, 
    powerType: typeof(AirForceCommandPower));
```

| 标记方式 | 效果 | 使用场景 |
|---------|------|----------|
| `WithProductionUnlock()` | 解锁下一级核心建筑的MCV选项，**不升级科技等级** | 矿场 → 重工/空指部/船厂 |
| `unlocksNextTech: true` | 升级 `CurrentTechLevel`，解锁对应等级的牌组建筑 | 空指部/雷达 → T2，作战实验室 → T3 |

#### 牌组建筑（BuildingCardUtils._deckBuildingTechLevelMap）

非核心建筑（防御塔、超武、围墙、维修厂等）需要在**牌组中存在**且**科技等级达标**时才出现在 MCV 选项：

```csharp
// BuildingCardUtils.cs
{ typeof(PrismTowerCard), TechLevel.T2 },      // 光棱塔：T2解锁
{ typeof(AlliesRepairDepot), TechLevel.T2 },  // 维修厂：T2解锁
{ typeof(OreRefineryCard), TechLevel.T3 },    // 矿石精炼器：T3解锁
{ typeof(WeatherController), TechLevel.T3 },   // 天气控制器：T3解锁
```

#### MCV 选项构成

```
MCV选项 = 核心建筑（自动显示） + 牌组建筑（牌组存在 + 科技等级达标）

核心建筑：发电厂 → 兵营 → 矿场 → 重工/空指部/船厂 → 作战实验室
牌组建筑：围墙/碉堡(T1) → 防御塔/维修厂(T2) → 超武/精炼厂(T3)
```

> 详细实现请查阅 API 文档「科技树系统」章节。

### 7. 播放建筑音效
**强制使用** `BuildingSoundHelper.PlayBuildingPlaceSound()`，禁止使用自定义部署音效。

### 8. 本地化
在 `cards.json` (含价格) 和 `powers.json` 中添加条目。

### 9. 建筑打出后自动触发系统（无需手写代码）

建筑卡牌打出后有两套**自动触发**逻辑，均通过 `PowerModel.AfterCardPlayed` 钩子集中实现，**卡牌自身无需写任何触发代码**：

| 能力 | 触发条件 | 效果 |
|------|----------|------|
| `BuildingDrawPower`（隐藏） | 非围墙且非防御塔的建筑 | 抽1张牌 |
| `UrbanizationPower`（需打出城市化卡） | 非围墙的建筑/防御塔 | 从牌堆抽取建筑牌 |

- **建筑抽牌**：通过 `DollarPower.AfterApplied` 自动挂载，所有获得刀乐能力的玩家默认持有
- **防御塔**：只触发城市化，不触发建筑抽牌
- **A2 预选模式建筑卡**（兵营/重工/船厂/空指部/MCV/出售）：取消发生在**打出之前**（见下节），
  不会进入 OnPlay，因此不调用 `CardUtils.HandleCardCancellation`，两套系统天然不触发
- **禁止**在 `OnPlay` 中硬编码 `CardPileCmd.Draw(ctx, 1, Owner)` 或 `UrbanizationPower.TriggerOnSuccessfulPlay(...)`

> 详细实现请查阅 API 文档「建筑打出系统」章节。

### 10. 生产建筑 A2 预选模式（先选后打）

兵营/重工/船厂/空指部/MCV/出售建筑现在采用 **A2 预选模式**：

```
点击手牌（NPlayerHand.StartCardPlay 被拦截）
→ 本地预选面板（卡牌不出手、不扣费、不暂停）
→ 确认 → 入队：PlayCardAction（打出）+ BuildingResolutionAction（结算：扣费/能力/生产序列）
→ 取消 → 只关面板，卡牌留在手牌，零副作用
```

新增一张**生产建筑**卡需要：

1. 把“可用单位列表 + 国旗/科技过滤 + 升级处理”抽成静态
   `GetPrePlayCandidates(Player owner, bool isUpgraded)`（面板与结算共用）；
2. `OnPlay` 最小化（只留音效/动画），不再弹面板、不再处理取消；
3. 在 `BuildingPrePlayHelper.OpenPanelAsync` 的 switch 注册该卡的候选与数值映射；
4. 在 `BuildingResolutionAction` 中按 `BuildingEntry` 增加结算逻辑（扣费/加能力/生产序列）；
5. 若会被自动打出：OnPlay 兜底会自动补开面板（确认后只入队结算）。

> 详细实现请查阅 API 文档「生产建筑 A2 预选模式」章节。

---

### 11. 初始资源配置 与 开局方案（5 槽位）

- **初始资源配置**：`CharacterConfig` 新增 `StartingGold` / `MaxHp`
  （0 = 角色默认值）；面板第三页「初始资源配置」直接改配置，
  开局由 `InitialDeckPatch.ApplyConfigToPlayer` 写入金币并同步血量上限。
- **开局方案**：动态槽位列表（不设上限）——高亮槽 = 当前方案（编辑其他页自动同步，
  无独立副本），末尾始终有一个空槽（保存即新建，自动递增）。
  UI 为独立功能页「开局方案存储」：每槽左上角「✏」命名、右上角「✕」删除、
  「保存」（空槽=新建、已占用=覆盖，弹窗确认）与「切换」（只移高亮、不覆盖任何槽，
  直接生效无确认；双击槽卡片也可直接切换）。

> 详细实现请查阅 API 文档「初始资源配置 与 开局方案」章节。

---

## 三、创造战备卡牌流程（以飞鹰500kg为例）

战备卡（飞鹰/轨道）的详细实现（基类、叠层逻辑、触发机制）请参考 API 文档中的「高级战备体系实现模式」章节。

### 核心步骤速览
1.  **创建能力类** (`*Power.cs`)：继承基类或 `PowerModel`，实现 `AfterSideTurnStart` 触发逻辑。
2.  **注册能力图标**：在 `PowerIconPatch.cs` 中添加映射。
3.  **创建卡牌类** (`*Card.cs`)：继承基类或 `CardModel`，实现 `OnPlay` 打出逻辑。
4.  **数值存储**：飞鹰/轨道系列**统一存储在** `CommonCardValues.cs`（卡牌数值）和 `CommonPowerValues.cs`（能力数值），不使用阵营专属文件。
5.  **动态数值配置**：注册 `DynamicVar` 并在 `OnUpgrade()` 中更新。
6.  **配置攻击动画**：使用 `VfxCmd` 播放特效。
7.  **本地化**：在 `cards.json` 和 `powers.json` 中添加条目。
8.  **注册到卡池**：在 `*CardRegistry.cs` 的对应卡池列表中注册。

---

## 四、创造公共卡牌流程（以黄金矿为例）

公共卡牌需要在两个阵营中独立存在，推荐使用 **Pool动态切换模式**（方案二）。

### 核心步骤速览
1.  **创建公共基类**：继承 `CardModel`，重写 `Pool` 和 `VisualCardPool` 以根据持有者阵营动态切换卡框颜色。
2.  **数值存储**：在 `CommonCardValues.cs` 中定义。
3.  **注册到阵营卡池**：在 `AlliedCardRegistry` 和 `SovietCardRegistry` 中均注册同一个基类。
4.  **创建对应能力**：在 `Common/Powers/` 创建共用能力。
5.  **本地化**：仅需一份 `cards.json` 和 `powers.json` 条目。

---

## 五、重要注意事项

### 1. 资源文件重命名
**强制**：所有资源（语音、图片）文件名必须使用英文，避免 Godot 加载问题。

### 2. 编译与部署
```bash
dotnet build RedAlert2Mod.csproj -c Release -o build
```
将 `build` 目录下的 `RedAlert2Mod.dll`, `RedAlert2Mod.json`, `RedAlert2Mod.pck` 复制到游戏 `mods/RedAlert2Mod/` 目录。

### 3. 多人联机

多人选择走**主机中转握手**（客户端→主机→客户端）保证两端顺序一致：

- 暂停/恢复阶段**不能**用本机锁包住，否则会与握手互相等待造成卡死；
- 只有纯本地的取消/回手处理（`CardUtils.HandleCardCancellation`）可以用
  `MultiplayerSyncHelper.RunSerialized` 串行化；
- A2 生产建筑的选择是**纯本地面板**，结果随 `BuildingResolutionAction` 动作载荷跨端同步，
  不涉及暂停/恢复，因此没有并发视觉竞态。
-   **多人限制**：通过重写 `MultiplayerConstraint` 属性设置。
-   **目标类型**：队友使用 `TargetType.AnyAlly` / `AllAllies`。
-   **联机同步**：涉及随机数或 UI 选择时，必须使用同步方法（详见 API 文档）。

---

## 六、常见遗漏检查清单

| 检查项 | 单位卡 | 建筑卡 | 战备卡 | 公共卡 |
|--------|:------:|:------:|:------:|:------:|
| 语音注册 | ✅ | ❌ | ❌ | ❌ |
| 卡牌图标 | ✅ | ✅ | ✅ | ✅ |
| 数值存储 | ✅ | ✅ | ✅（Common） | ✅（Common） |
| 价格映射 | ✅ | ✅ | ❌ | ❌ |
| CardRegistry注册 | ✅ | ✅ | ✅ | ✅（双阵营） |
| 遗物转换注册 | ✅ | ❌ | ❌ | ❌ |
| 能力图标补丁 | ❌ | ✅ | ✅ | ✅（共用） |
| 科技解锁配置 | ✅ | ✅ | ❌ | ❌ |
| 核心建筑(TechTreeConfig) | — | ✅ 核心建筑 | — | — |
| 牌组建筑(BuildingCardUtils) | — | ✅ 防御塔/超武/围墙 | — | — |
| BuildingModelMap | — | ✅（MCV造价显示） | — | — |
| 数值变量 :diff() 格式化器 | ✅ | ✅ | ✅ | ✅（双份） |
| 本地化 | ✅ | ✅ | ✅ | ✅（双份） |
| 音效播放 | ✅ | ✅ **强制统一** | ❌ | ❌ |

---

*本文档为快速入门指南。遇到复杂场景或需要了解详细 API 用法，请查阅 [杀戮尖塔2_Mod开发教程API快速参考_Beta版.md](file:///d:/RedAlert2Project/red-alert-2-mod/%E6%96%87%E6%9C%AC/%E6%9D%80%E6%9C%8D%E5%B0%96%E5%A1%942_Mod%E5%BC%80%E5%8F%91%E6%95%99%E7%A8%8BAPI%E5%BF%AB%E9%80%9F%E5%8F%82%E8%80%83_Beta%E7%89%88.md)。*
