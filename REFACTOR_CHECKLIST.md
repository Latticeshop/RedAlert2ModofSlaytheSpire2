# 项目结构重构检查报告

## ✅ 已完成的重构

### 1. 目录结构重组

**新结构**：
```
red-alert-2-mod/
├── RedAlert2ModCode/              # ✅ C#代码
│   ├── Allies/                    # ✅ 盟军代码
│   │   ├── AlliesCharacter.cs
│   │   ├── AlliesCardPool.cs
│   │   ├── AlliesRelicPool.cs
│   │   ├── AlliesPotionPool.cs
│   │   └── AlliesRegistration.cs
│   ├── Extensions/                # ✅ 扩展方法
│   │   ├── PathExtensions.cs
│   │   └── ...
│   └── ModInitializer.cs          # ✅ Mod入口
│
├── RedAlert2ModResources/         # ✅ Godot资源子项目
│   ├── images/                    # ✅ 图片资源
│   │   ├── allies/
│   │   ├── card_portraits/
│   │   ├── charui/
│   │   ├── powers/
│   │   └── relics/
│   ├── scenes/                    # ✅ 场景文件
│   │   ├── creature_visuals/
│   │   ├── ui/
│   │   ├── combat/
│   │   └── allies_bg.tscn
│   ├── localization/zhs/          # ✅ 本地化
│   │   ├── cards.json
│   │   ├── characters.json
│   │   └── ... (7个文件)
│   └── mod_image.png              # ✅ 封面图
│
├── project.godot                  # Godot配置
├── RedAlert2Mod.csproj            # C#项目
├── Directory.Build.props          # 构建配置
└── build/                         # 输出目录
```

**旧目录清理**：
- ✅ `images/` 已删除
- ✅ `scenes/` 已删除
- ✅ `localization/` 已删除
- ✅ `src/` 已删除

---

## ⚠️ 需要修复的问题

### 1. 命名空间不匹配 🔴 高优先级

**问题**：代码中的命名空间还是旧的 `RedAlert2Mod.Characters.Allies`

**当前**：
```csharp
namespace RedAlert2Mod.Characters.Allies;
```

**应该改为**：
```csharp
namespace RedAlert2ModCode.Allies;
```

**需要修改的文件**：
- `RedAlert2ModCode/Allies/AlliesCharacter.cs`
- `RedAlert2ModCode/Allies/AlliesCardPool.cs`
- `RedAlert2ModCode/Allies/AlliesRelicPool.cs`
- `RedAlert2ModCode/Allies/AlliesPotionPool.cs`
- `RedAlert2ModCode/Allies/AlliesRegistration.cs`

---

### 2. using引用需要更新 🟡 中优先级

**问题**：Extensions的命名空间也需要更新

**当前**：
```csharp
using RedAlert2Mod.Extensions;
```

**应该改为**：
```csharp
using RedAlert2ModCode.Extensions;
```

---

### 3. ModInitializer的命名空间 🟡 中优先级

**当前**：
```csharp
namespace RedAlert2Mod;
```

**应该改为**：
```csharp
namespace RedAlert2ModCode;
```

---

### 4. project.godot的icon路径 🟢 低优先级

**当前**：
```gdscript
config/icon="res://icon.svg"
```

**建议改为**（使用mod_image.png）：
```gdscript
config/icon="res://RedAlert2ModResources/mod_image.png"
```

---

### 5. .csproj的Assembly名称 🟢 低优先级

**当前**：
```xml
project/assembly_name="RedAlert2Mod"
```

这个可以保持不变，因为它是程序集名称，不影响目录结构。

---

### 6. 资源路径引用检查 🔴 高优先级

需要检查以下文件中的资源路径是否正确：

**场景文件**：
- `RedAlert2ModResources/scenes/creature_visuals/allies.tscn`
- `RedAlert2ModResources/scenes/ui/character_icons/allies_icon.tscn`
- `RedAlert2ModResources/scenes/allies_bg.tscn`

**代码文件**：
- `RedAlert2ModCode/Allies/AlliesCharacter.cs` 中的CustomXXX路径

**当前路径示例**：
```csharp
public override string CustomIconPath => "res://scenes/ui/character_icons/allies_icon.tscn";
```

**应该改为**：
```csharp
public override string CustomIconPath => "res://RedAlert2ModResources/scenes/ui/character_icons/allies_icon.tscn";
```

---

## 📋 修复清单

### 立即修复（必须）
- [ ] 更新所有C#文件的命名空间
- [ ] 更新所有using引用
- [ ] 更新代码中的资源路径（res://前缀）
- [ ] 验证场景文件中的资源引用

### 优化修复（建议）
- [ ] 更新project.godot的icon路径
- [ ] 测试编译是否成功
- [ ] 在Godot中打开项目验证

---

## 🎯 下一步行动

1. **批量更新命名空间**
   ```bash
   # 查找所有需要修改的文件
   grep -r "namespace RedAlert2Mod" RedAlert2ModCode/
   
   # 批量替换
   find RedAlert2ModCode/ -name "*.cs" -exec sed -i 's/namespace RedAlert2Mod\./namespace RedAlert2ModCode./g' {} \;
   ```

2. **更新资源路径**
   - 在所有.cs文件中添加 `RedAlert2ModResources/` 前缀

3. **测试编译**
   ```bash
   dotnet build -c ExportRelease
   ```

4. **在Godot中验证**
   - 打开project.godot
   - 检查资源是否都能正常加载
   - 导出PCK测试

---

**总结**：目录结构重构基本完成，但需要更新命名空间和资源路径引用才能正常工作。
