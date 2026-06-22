using System.Collections.Generic;
using RedAlert2ModCode.UI;

using EngineerChoice = RedAlert2ModCode.UI.EngineerChoiceScreen.EngineerChoice;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 工程师卡牌选项配置存储
/// 统一管理工程师选项的标题、描述和权重，便于本地化和平衡调整
/// </summary>
public static class EngineerChoiceValues
{
    /// <summary>
    /// 占领油井选项
    /// 效果：将一张油井加入手牌
    /// </summary>
    public static EngineerChoice CaptureOilDerrick => new()
    {
        Type = EngineerChoiceScreen.ChoiceType.CaptureOilDerrick,
        Title = "占领油井",
        Description = "将一张「油井」加入手牌",
        Weight = 8
    };

    /// <summary>
    /// 修理建筑选项
    /// 效果：获得3点覆甲
    /// </summary>
    public static EngineerChoice RepairBuilding => new()
    {
        Type = EngineerChoiceScreen.ChoiceType.RepairBuilding,
        Title = "修理建筑",
        Description = "获得3点覆甲",
        Weight = 10
    };

    /// <summary>
    /// 占领机场选项
    /// 效果：加入一张伞兵卡牌
    /// </summary>
    public static EngineerChoice CaptureAirfield => new()
    {
        Type = EngineerChoiceScreen.ChoiceType.CaptureAirfield,
        Title = "占领机场",
        Description = "加入一张卡牌「伞兵」",
        Weight = 5
    };

    /// <summary>
    /// 占领市民医院选项
    /// 效果：获得1点敏捷
    /// </summary>
    public static EngineerChoice CaptureHospital => new()
    {
        Type = EngineerChoiceScreen.ChoiceType.CaptureHospital,
        Title = "占领市民医院",
        Description = "获得1点敏捷",
        Weight = 3
    };

    /// <summary>
    /// 占领机械商店选项
    /// 效果：获得1点力量
    /// </summary>
    public static EngineerChoice CaptureWorkshop => new()
    {
        Type = EngineerChoiceScreen.ChoiceType.CaptureWorkshop,
        Title = "占领机械商店",
        Description = "获得1点力量",
        Weight = 3
    };

    /// <summary>
    /// 占领科技前哨站选项
    /// 效果：获得爱国者飞弹和维修厂能力
    /// </summary>
    public static EngineerChoice CaptureTechOutpost => new()
    {
        Type = EngineerChoiceScreen.ChoiceType.CaptureTechOutpost,
        Title = "占领科技前哨站",
        Description = "获得能力「爱国者飞弹」和「维修厂」",
        Weight = 1
    };

    /// <summary>
    /// 获取所有选项列表
    /// </summary>
    public static List<EngineerChoice> AllChoices => new()
    {
        CaptureOilDerrick,
        RepairBuilding,
        CaptureAirfield,
        CaptureHospital,
        CaptureWorkshop,
        CaptureTechOutpost
    };
}
