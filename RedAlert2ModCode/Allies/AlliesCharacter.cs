using BaseLib.Abstracts;
using RedAlert2ModCode.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;

namespace RedAlert2ModCode.Allies;

/// <summary>
/// 盟军角色 - 使用BaseLib的PlaceholderCharacterModel
/// 覆盖所有资源路径以使用自定义资源
/// </summary>
public sealed class Allies : PlaceholderCharacterModel
{
    public const string CharacterId = "Allies";
    
    // 借用铁卫的基础场景，然后用自定义资源覆盖
    public override string PlaceholderID => "ironclad";
    
    // 自定义资源路径
    public override string CustomIconPath => "res://RedAlert2ModResources/scenes/ui/character_icons/allies_icon.tscn";
    public override string CustomCharacterSelectIconPath => "res://RedAlert2ModResources/images/packed/character_select/char_select_allies.png";
    public override string CustomVisualPath => "res://RedAlert2ModResources/scenes/creature_visuals/allies.tscn";
    public override string CustomCharacterSelectBg => "res://RedAlert2ModResources/scenes/allies_bg.tscn";
    
    // 角色颜色配置
    public static readonly Color Color = new("2060a0"); // 盟军蓝色
    
    // 必需属性
    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Masculine;
    public override int StartingHp => 80;
    
    // 起始卡组（暂时为空，后续添加自定义卡牌）
    public override IEnumerable<CardModel> StartingDeck => [];
    
    // 起始遗物（暂时为空）
    public override IReadOnlyList<RelicModel> StartingRelics => [];
    
    // 卡池、遗物池、药水池
    public override CardPoolModel CardPool => ModelDb.CardPool<AlliesCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<AlliesRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<AlliesPotionPool>();
}
