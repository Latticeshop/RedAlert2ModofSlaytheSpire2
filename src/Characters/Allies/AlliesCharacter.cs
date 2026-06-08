using BaseLib.Abstracts;
using Ra2Mod.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;

namespace Ra2Mod.Characters.Allies;

/// <summary>
/// 盟军角色 - 使用BaseLib的PlaceholderCharacterModel简化开发
/// PlaceholderID设为"ironclad"，自动借用铁卫的资源（动画、UI等）
/// </summary>
public sealed class Allies : PlaceholderCharacterModel
{
    public const string CharacterId = "Allies";
    
    // 借用铁卫的资源作为占位符
    public override string PlaceholderID => "ironclad";
    
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
