// 小格子铺 | Latticeshop
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;

namespace RedAlert2ModCode.Allies;

/// <summary>
/// 盟军角色 - 使用RitsuLib的ModCharacterTemplate
/// 覆盖所有资源路径以使用自定义资源
/// </summary>
[RegisterCharacter]
public sealed class Allies : ModCharacterTemplate<AlliesCardPool, AlliesRelicPool, AlliesPotionPool>
{
    public const string CharacterId = "Allies";
    
    // 借用故障机器人的基础场景（使用蓝色能量指示器），然后用自定义资源覆盖
    public override string PlaceholderCharacterId => "defect";
    
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
    
    // 自定义资源路径（使用RitsuLib的AssetProfile）
    public override string CustomVisualsPath => "res://RedAlert2ModResources/scenes/creature_visuals/allies.tscn";
    public override string CustomEnergyCounterPath => "";
    public override string CustomMerchantAnimPath => "res://RedAlert2ModResources/scenes/creature_visuals/allies_shop.tscn";
    public override string CustomRestSiteAnimPath => "res://RedAlert2ModResources/scenes/rest_site/characters/allies_rest_site.tscn";
    public override string CustomIconTexturePath => "res://RedAlert2ModResources/images/ui/allies_icon.png";
    public override string CustomIconOutlineTexturePath => "";
    public override string CustomIconPath => "res://RedAlert2ModResources/scenes/ui/character_icons/allies_icon.tscn";
    public override string CustomCharacterSelectBgPath => "res://RedAlert2ModResources/scenes/allies_bg.tscn";
    public override string CustomCharacterSelectIconPath => "res://RedAlert2ModResources/images/charui/allies_character_select.png";
    public override string CustomCharacterSelectLockedIconPath => "";
    public override string CustomCharacterSelectTransitionPath => "";
    public override string CustomMapMarkerPath => "";
    public override string CustomTrailPath => "";
    
    // 起始卡组（通过StartingDeckTypes配置，或者使用属性注册）
    // 暂保持原有StartingDeck实现，后续逐步迁移到StartingDeckTypes
}