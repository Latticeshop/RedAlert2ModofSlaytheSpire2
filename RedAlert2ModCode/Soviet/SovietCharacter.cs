using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;
using RedAlert2ModCode.Soviet.Cards;

namespace RedAlert2ModCode.Soviet;

/// <summary>
/// 苏军角色 - 使用BaseLib的PlaceholderCharacterModel
/// 覆盖所有资源路径以使用自定义资源
/// </summary>
public sealed class Soviet : PlaceholderCharacterModel
{
    public const string CharacterId = "Soviet";
    
    // 借用战士的基础场景（使用红色能量指示器），然后用自定义资源覆盖
    public override string PlaceholderID => "ironclad";
    
    // 自定义资源路径
    public override string CustomIconPath => "res://RedAlert2ModResources/scenes/ui/character_icons/soviet_icon.tscn";
    public override string CustomIconTexturePath => "res://RedAlert2ModResources/images/ui/soviet_icon.png";
    public override string CustomCharacterSelectIconPath => "res://RedAlert2ModResources/images/charui/soviet_character_select.png";
    public override string CustomVisualPath => "res://RedAlert2ModResources/scenes/creature_visuals/soviet.tscn";
    public override string CustomCharacterSelectBg => "res://RedAlert2ModResources/scenes/soviet_bg.tscn";
    
    // 篝火休息场景
    public override string CustomRestSiteAnimPath => "res://RedAlert2ModResources/scenes/rest_site/characters/soviet_rest_site.tscn";
    
    // 商店场景
    public override string CustomMerchantAnimPath => "res://RedAlert2ModResources/scenes/creature_visuals/soviet_shop.tscn";
    
    // 角色颜色配置 - 苏军红色
    public static readonly Color Color = new("a02020");
    
    // 必需属性
    public override Color NameColor => Color;
    public override Color MapDrawingColor => Color;
    public override CharacterGender Gender => CharacterGender.Masculine;
    public override int StartingHp => 90; // 苏军更耐打
    
    // 起始卡组（9张动员兵 + 4张犀牛坦克 + 1张苏军基地车 + 1张苏军围墙）
    public override IEnumerable<CardModel> StartingDeck => new List<CardModel>
    {
        ModelDb.Card<Conscript>(),
        ModelDb.Card<Conscript>(),
        ModelDb.Card<Conscript>(),
        ModelDb.Card<Conscript>(),
        ModelDb.Card<Conscript>(),
        ModelDb.Card<Conscript>(),
        ModelDb.Card<Conscript>(),
        ModelDb.Card<Conscript>(),
        ModelDb.Card<Conscript>(),
        ModelDb.Card<RhinoTank>(),
        ModelDb.Card<RhinoTank>(),
        ModelDb.Card<RhinoTank>(),
        ModelDb.Card<RhinoTank>(),
        ModelDb.Card<SovietMCV>(),
        ModelDb.Card<SovietWallCard>(),
    };
    
    // 起始遗物（刀乐）
    public override IReadOnlyList<RelicModel> StartingRelics => new List<RelicModel>
    {
        ModelDb.Relic<RedAlert2ModCode.Common.Relics.DollarRelic>(),
    };
    
    // 卡池、遗物池、药水池
    public override CardPoolModel CardPool => ModelDb.CardPool<SovietCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<SovietRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<SovietPotionPool>();
}