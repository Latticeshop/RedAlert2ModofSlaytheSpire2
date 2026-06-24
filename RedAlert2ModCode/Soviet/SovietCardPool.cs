using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace RedAlert2ModCode.Soviet;

/// <summary>
/// 苏军卡池 - 继承自原版CardPoolModel，手动注册所有卡牌
/// </summary>
public sealed class SovietCardPool : CardPoolModel
{
    public override string Title => "soviet";
    public override string EnergyColorName => "ironclad"; // 使用战士的红色能量图标
    public override bool IsColorless => false;
    
    // 卡牌框材质路径 - 使用红色框
    public override string CardFrameMaterialPath => "card_frame_red";
    
    // 角色颜色配置
    public static readonly Color Color = new("a02020");
    public override Color DeckEntryCardColor => Color;
    public override Color EnergyOutlineColor => new("801010");

    /// <summary>
    /// 注册所有苏军卡牌
    /// 从 SovietCardRegistry 获取，保持单一数据源
    /// </summary>
    protected override CardModel[] GenerateAllCards()
    {
        return SovietCardRegistry.GetAllCards().ToArray();
    }
}