// 小格子铺 | Latticeshop
using Godot;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Visuals;

namespace RedAlert2ModCode.Allies;

/// <summary>
/// 盟军角色资源配置 - 使用RitsuLib的CharacterAssetProfile
/// 基于故障机器人(defect)的基础配置，覆盖自定义资源路径
/// </summary>
internal static class AlliesCharacterAssets
{
    // 使用故障机器人(defect)作为基础配置
    private static readonly CharacterAssetProfile BaseProfile = CharacterAssetProfiles.Defect();

    /// <summary>
    /// 盟军角色资源配置文件
    /// </summary>
    internal static CharacterAssetProfile Profile { get; } = BaseProfile
        .WithScenes(BaseProfile.Scenes! with
        {
            VisualsPath = "res://RedAlert2ModResources/scenes/creature_visuals/allies.tscn",
            EnergyCounterPath = "res://scenes/combat/energy_counters/defect_energy_counter.tscn",
            RestSiteAnimPath = "res://RedAlert2ModResources/scenes/rest_site/characters/allies_rest_site.tscn",
            MerchantAnimPath = "res://RedAlert2ModResources/scenes/creature_visuals/allies_shop.tscn",
        })
        .WithUi(new(
            IconTexturePath: "res://RedAlert2ModResources/images/ui/allies_icon.png",
            IconOutlineTexturePath: "",
            IconPath: "res://RedAlert2ModResources/scenes/ui/character_icons/allies_icon.tscn",
            CharacterSelectBgPath: "res://RedAlert2ModResources/scenes/allies_bg.tscn",
            CharacterSelectIconPath: "res://RedAlert2ModResources/images/charui/allies_character_select.png",
            CharacterSelectLockedIconPath: "",
            CharacterSelectTransitionPath: "res://materials/transitions/defect_transition_mat.tres",
            MapMarkerPath: "res://images/packed/map/icons/map_marker_defect.png"
        ))
        .WithVfx(new(
            TrailPath: "res://scenes/vfx/card_trail_defect.tscn",
            TrailStyle: new(
                OuterTrailModulate: new Color(0.125f, 0.376f, 0.627f, 0.55f),
                OuterTrailWidth: 82f,
                InnerTrailModulate: new Color(0.3f, 0.6f, 0.9f, 0.8f),
                InnerTrailWidth: 42f,
                BigSparksColor: new Color(0.6f, 0.8f, 1f, 0.85f),
                LittleSparksColor: new Color(0.9f, 0.95f, 1f, 0.95f),
                PrimarySpriteModulate: new Color(0.2f, 0.5f, 0.8f, 0.55f),
                PrimarySpriteScale: new Vector2(1.05f, 1.0f),
                SecondarySpriteModulate: new Color(0.8f, 0.9f, 1f, 0.9f),
                SecondarySpriteScale: new Vector2(0.82f, 0.82f)
            )
        ))
        .WithAudio(new(
            CharacterSelectSfx: "event:/sfx/characters/defect/defect_select",
            CharacterTransitionSfx: "event:/sfx/ui/wipe_silent",
            AttackSfx: "event:/sfx/characters/silent/silent_attack",
            CastSfx: "event:/sfx/characters/silent/silent_cast",
            DeathSfx: "event:/sfx/characters/silent/silent_die"
        ));
}
