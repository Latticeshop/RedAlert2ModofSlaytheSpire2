// 小格子铺 | Latticeshop
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using System.Collections.Generic;
using RedAlert2ModCode.Allies.Cards;

namespace RedAlert2ModCode.Allies;

/// <summary>
/// 盟军角色 - 使用RitsuLib的ModCharacterTemplate
/// 使用CharacterAssetProfile配置所有资源路径
/// </summary>
[RegisterCharacter]
public sealed class Allies : ModCharacterTemplate<AlliesCardPool, AlliesRelicPool, AlliesPotionPool>
{
    public const string CharacterId = "Allies";
    
    // 角色颜色配置
    public static readonly Color Color = new("2060a0"); // 盟军蓝色
    
    // 必需属性
    public override Color NameColor => Color;
    public override Color MapDrawingColor => Color;
    public override CharacterGender Gender => CharacterGender.Feminine; // 谭雅是女性角色
    public override int StartingHp => 85;
    
    // CharacterModel抽象成员实现
    public override float CastAnimDelay => 0f;
    public override float AttackAnimDelay => 0f;
    public override int StartingGold => 99;
    public override List<string> GetArchitectAttackVfx() => new();
    
    // 使用CharacterAssetProfile配置资源路径（解决地图背景为空等问题）
    public override CharacterAssetProfile AssetProfile => AlliesCharacterAssets.Profile;
    
    // 起始卡组配置（使用RitsuLib的StartingDeckEntries）
    protected override IEnumerable<StartingDeckEntry> StartingDeckEntries => new[]
    {
        StartingDeckEntry.Of<AmericanSoldier>(5),
        StartingDeckEntry.Of<GrizzlyTank>(5),
        StartingDeckEntry.Of<AlliedMCV>(1),
        StartingDeckEntry.Of<AlliedWallCard>(1),
    };
    
    // 起始遗物配置
    protected override IEnumerable<Type> StartingRelicTypes => new[]
    {
        typeof(RedAlert2ModCode.Common.Relics.DollarRelic),
    };
}