using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace RedAlert2ModCode.Common.Utils;

/// <summary>
/// 透明特效工具 - 统一管理角色/怪物的透明视觉效果
/// 用于"抹除"和"伪装"等能力的透明特效，确保能力移除时正确恢复
/// </summary>
public static class TransparencyHelper
{
    /// <summary>
    /// 设置透明特效（默认 40% 不透明度，与抹除特效一致）
    /// </summary>
    /// <param name="creature">目标生物</param>
    /// <param name="alpha">不透明度（0=完全透明，1=完全不透明），默认 0.4f</param>
    public static void SetTransparency(Creature creature, float alpha = 0.4f)
    {
        if (creature == null) return;

        var nCreature = creature.GetCreatureNode();
        if (nCreature?.Visuals != null)
        {
            var body = nCreature.Visuals.GetCurrentBody();
            body.Modulate = new Color(1f, 1f, 1f, alpha);
        }
    }

    /// <summary>
    /// 移除透明特效（恢复 100% 不透明度）
    /// </summary>
    /// <param name="creature">目标生物</param>
    public static void ResetTransparency(Creature creature)
    {
        if (creature == null) return;

        var nCreature = creature.GetCreatureNode();
        if (nCreature?.Visuals != null)
        {
            var body = nCreature.Visuals.GetCurrentBody();
            body.Modulate = new Color(1f, 1f, 1f, 1f);
        }
    }
}
