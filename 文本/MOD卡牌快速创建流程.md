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
在 `*CardValues.cs` 中定义数值。

### 3. 动态数值配置
注册 `DynamicVar`（如 `DollarVar`, `IntVar`）并在 `OnUpgrade()` 中更新。

### 4. 创建对应能力
创建 `*Power.cs` 并在 `PowerIconPatch.cs` 中注册图标。

### 5. 注册到建筑列表
在 `*CardRegistry.cs` 的 `BuildingCards` 中注册。

### 6. 科技线配置
在 `*TechTreeConfig.cs` 配置解锁条件。

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
- **选择面板类建筑卡**（重工/兵营/MCV等）：取消选择时调用 `CardUtils.HandleCardCancellation(play, this, Owner)` 即可，两套系统会自动跳过取消的打出
- **禁止**在 `OnPlay` 中硬编码 `CardPileCmd.Draw(ctx, 1, Owner)` 或 `UrbanizationPower.TriggerOnSuccessfulPlay(...)`

> 详细实现请查阅 API 文档「建筑打出系统」章节。

---

## 三、创造战备卡牌流程（以飞鹰500kg为例）

战备卡（飞鹰/轨道）的详细实现（基类、叠层逻辑、触发机制）请参考 API 文档中的「高级战备体系实现模式」章节。

### 核心步骤速览
1.  **创建能力类** (`*Power.cs`)：继承基类或 `PowerModel`，实现 `AfterSideTurnStart` 触发逻辑。
2.  **注册能力图标**：在 `PowerIconPatch.cs` 中添加映射。
3.  **创建卡牌类** (`*Card.cs`)：继承基类或 `CardModel`，实现 `OnPlay` 打出逻辑。
4.  **数值存储**：在 `*PowerValues.cs` / `*CardValues.cs` 中定义。
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
-   **多人限制**：通过重写 `MultiplayerConstraint` 属性设置。
-   **目标类型**：队友使用 `TargetType.AnyAlly` / `AllAllies`。
-   **联机同步**：涉及随机数或 UI 选择时，必须使用同步方法（详见 API 文档）。

---

## 六、常见遗漏检查清单

| 检查项 | 单位卡 | 建筑卡 | 战备卡 | 公共卡 |
|--------|:------:|:------:|:------:|:------:|
| 语音注册 | ✅ | ❌ | ❌ | ❌ |
| 卡牌图标 | ✅ | ✅ | ✅ | ✅ |
| 数值存储 | ✅ | ✅ | ✅ | ✅（Common） |
| 价格映射 | ✅ | ✅ | ❌ | ❌ |
| CardRegistry注册 | ✅ | ✅ | ✅ | ✅（双阵营） |
| 遗物转换注册 | ✅ | ❌ | ❌ | ❌ |
| 能力图标补丁 | ❌ | ✅ | ✅ | ✅（共用） |
| 科技解锁配置 | ✅ | ✅ | ❌ | ❌ |
| 本地化 | ✅ | ✅ | ✅ | ✅（双份） |
| 音效播放 | ✅ | ✅ **强制统一** | ❌ | ❌ |

---

*本文档为快速入门指南。遇到复杂场景或需要了解详细 API 用法，请查阅 [杀戮尖塔2_Mod开发教程API快速参考_Beta版.md](file:///d:/RedAlert2Project/red-alert-2-mod/%E6%96%87%E6%9C%AC/%E6%9D%80%E6%9C%8D%E5%B0%96%E5%A1%942_Mod%E5%BC%80%E5%8F%91%E6%95%99%E7%A8%8BAPI%E5%BF%AB%E9%80%9F%E5%8F%82%E8%80%83_Beta%E7%89%88.md)。*
