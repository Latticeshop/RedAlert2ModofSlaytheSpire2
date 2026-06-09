# 杀戮尖塔2 Mod开发 - AI快速参考手册

> 精简版，便于AI快速检索关键API和代码模式

---

## 📂 项目路径配置

### 当前项目路径
```
项目根目录: D:\RedAlert2Project\red-alert-2-mod
游戏解包目录: D:\RedAlert2Project\SlayTheSpire2Mod
Godot引擎: Godot_v4.5.1-stable_mono_win64
红警2图标: D:\RedAlert2Project\icons\红警2图标PNG\
```

### 游戏解包资源结构（参考）
```
D:\RedAlert2Project\SlayTheSpire2Mod\
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
```csharp
ModHelper.AddModelToPool(typeof(IroncladCardPool), typeof(MyCard));
```

### 资源路径
```
res://images/atlases/card_atlas.sprites/<pool>/<card_id>.tres
res://images/packed/card_portraits/<pool>/<card_id>.png
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

*本手册为快速参考，详细教程请查看完整文档。*
