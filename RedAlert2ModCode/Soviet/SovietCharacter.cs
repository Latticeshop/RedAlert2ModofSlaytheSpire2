// 小格子铺 | Latticeshop
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using System.Collections.Generic;
using RedAlert2ModCode.Soviet.Cards;

namespace RedAlert2ModCode.Soviet;

/// <summary>
/// 苏军角色 - 使用RitsuLib的ModCharacterTemplate
/// 使用CharacterAssetProfile配置所有资源路径
/// </summary>
[RegisterCharacter]
public sealed class Soviet : ModCharacterTemplate<SovietCardPool, SovietRelicPool, SovietPotionPool>
{
    public const string CharacterId = "Soviet";
    
    // 角色颜色配置 - 苏军红色
    public static readonly Color Color = new("a02020");
    
    // 必需属性
    public override Color NameColor => Color;
    public override Color MapDrawingColor => Color;
    public override CharacterGender Gender => CharacterGender.Masculine;
    public override int StartingHp => 85; // 苏军血量
    
    // CharacterModel抽象成员实现
    public override float CastAnimDelay => 0f;
    public override float AttackAnimDelay => 0f;
    public override int StartingGold => 99;
    public override List<string> GetArchitectAttackVfx() => new();
    
    // 使用CharacterAssetProfile配置资源路径（解决地图背景为空等问题）
    public override CharacterAssetProfile AssetProfile => SovietCharacterAssets.Profile;
    
    // 起始卡组配置（使用RitsuLib的StartingDeckEntries）
    protected override IEnumerable<StartingDeckEntry> StartingDeckEntries => new[]
    {
        StartingDeckEntry.Of<Conscript>(5),
        StartingDeckEntry.Of<RhinoTank>(5),
        StartingDeckEntry.Of<SovietMCV>(1),
        StartingDeckEntry.Of<SovietWallCard>(1),
    };
    
    // 起始遗物配置
    protected override IEnumerable<Type> StartingRelicTypes => new[]
    {
        typeof(RedAlert2ModCode.Common.Relics.DollarRelic),
    };
}