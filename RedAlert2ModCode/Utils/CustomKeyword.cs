using System.Collections.Generic;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;

namespace RedAlert2ModCode.Utils;

/// <summary>
/// 自定义词条定义
/// </summary>
public class CustomKeyword
{
    public string Id { get; }
    public LocString Title { get; }
    public LocString Description { get; }

    public CustomKeyword(string id, LocString title, LocString description)
    {
        Id = id;
        Title = title;
        Description = description;
    }

    /// <summary>
    /// 获取卡牌上显示的文本（金色格式化）
    /// </summary>
    public string GetCardText()
    {
        return $"[gold]{Title.GetFormattedText()}.[/gold]";
    }

    /// <summary>
    /// 创建悬停提示
    /// </summary>
    public IHoverTip CreateHoverTip()
    {
        return new HoverTip(Title, Description);
    }
}

/// <summary>
/// 自定义词条管理器
/// </summary>
public static class CustomKeywordManager
{
    private static readonly Dictionary<string, CustomKeyword> _keywords = new();

    /// <summary>
    /// 注册自定义词条
    /// </summary>
    public static void RegisterKeyword(CustomKeyword keyword)
    {
        if (!_keywords.ContainsKey(keyword.Id))
        {
            _keywords[keyword.Id] = keyword;
        }
    }

    /// <summary>
    /// 获取词条
    /// </summary>
    public static CustomKeyword? GetKeyword(string id)
    {
        _keywords.TryGetValue(id, out var keyword);
        return keyword;
    }

    /// <summary>
    /// 所有已注册的词条
    /// </summary>
    public static IEnumerable<CustomKeyword> AllKeywords => _keywords.Values;
}

/// <summary>
/// 预定义的自定义词条
/// </summary>
public static class ModCardKeywords
{
    /// <summary>
    /// MCV词条 - 拥有建造厂才能打出建筑卡牌
    /// </summary>
    public static readonly CustomKeyword Mcv = new(
        "MCV",
        new LocString("card_keywords", "mcv.title"),
        new LocString("card_keywords", "mcv.description")
    );

    /// <summary>
    /// 士兵词条 - 指由个体武装的单位
    /// </summary>
    public static readonly CustomKeyword Soldier = new(
        "SOLDIER",
        new LocString("card_keywords", "soldier.title"),
        new LocString("card_keywords", "soldier.description")
    );

    /// <summary>
    /// 战车词条 - 指陆地装甲的单位
    /// </summary>
    public static readonly CustomKeyword Vehicle = new(
        "VEHICLE",
        new LocString("card_keywords", "vehicle.title"),
        new LocString("card_keywords", "vehicle.description")
    );

    /// <summary>
    /// 空军词条 - 指空中的单位
    /// </summary>
    public static readonly CustomKeyword Aircraft = new(
        "AIRCRAFT",
        new LocString("card_keywords", "aircraft.title"),
        new LocString("card_keywords", "aircraft.description")
    );

    /// <summary>
    /// 海军词条 - 指水里的单位
    /// </summary>
    public static readonly CustomKeyword Navy = new(
        "NAVY",
        new LocString("card_keywords", "navy.title"),
        new LocString("card_keywords", "navy.description")
    );

    /// <summary>
    /// 建筑词条 - 指需要建造厂建造的建筑
    /// </summary>
    public static readonly CustomKeyword Building = new(
        "BUILDING",
        new LocString("card_keywords", "building.title"),
        new LocString("card_keywords", "building.description")
    );

    /// <summary>
    /// 防御塔词条 - 需要建造厂建筑，自动执行的建筑
    /// </summary>
    public static readonly CustomKeyword DefenseTower = new(
        "DEFENSE_TOWER",
        new LocString("card_keywords", "defense_tower.title"),
        new LocString("card_keywords", "defense_tower.description")
    );

    /// <summary>
    /// 生产序列词条 - 每回合自动扣费生产单位
    /// </summary>
    public static readonly CustomKeyword ProductionQueue = new(
        "PRODUCTION_QUEUE",
        new LocString("card_keywords", "production_queue.title"),
        new LocString("card_keywords", "production_queue.description")
    );

    /// <summary>
    /// 初始化所有自定义词条
    /// </summary>
    public static void Initialize()
    {
        CustomKeywordManager.RegisterKeyword(Mcv);
        CustomKeywordManager.RegisterKeyword(Soldier);
        CustomKeywordManager.RegisterKeyword(Vehicle);
        CustomKeywordManager.RegisterKeyword(Aircraft);
        CustomKeywordManager.RegisterKeyword(Navy);
        CustomKeywordManager.RegisterKeyword(Building);
        CustomKeywordManager.RegisterKeyword(DefenseTower);
        CustomKeywordManager.RegisterKeyword(ProductionQueue);
    }
}
