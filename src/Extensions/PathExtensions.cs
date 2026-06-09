using Godot;

namespace RedAlert2Mod.Extensions;

/// <summary>
/// 路径扩展方法 - 简化资源路径编写
/// </summary>
public static class PathExtensions
{
    private const string ModId = "RedAlert2Mod";
    private const string ResPath = $"res://{ModId}";
    
    public static string ImagePath(this string path)
    {
        return Path.Join(ResPath, "images", path);
    }

    public static string CardImagePath(this string path)
    {
        path = Path.Join(ResPath, "images", "card_portraits", path);
        if (ResourceLoader.Exists(path)) return path;
        
        // 如果找不到，返回默认卡牌图片
        return Path.Join(ResPath, "images", "card_portraits", "card.png");
    }

    public static string BigCardImagePath(this string path)
    {
        path = Path.Join(ResPath, "images", "card_portraits", "big", path);
        if (ResourceLoader.Exists(path)) return path;
        
        return Path.Join(ResPath, "images", "card_portraits", "big", "card.png");
    }

    public static string PowerImagePath(this string path)
    {
        path = Path.Join(ResPath, "images", "powers", path);
        if (ResourceLoader.Exists(path)) return path;
        
        return Path.Join(ResPath, "images", "powers", "power.png");
    }

    public static string BigPowerImagePath(this string path)
    {
        path = Path.Join(ResPath, "images", "powers", "big", path);
        if (ResourceLoader.Exists(path)) return path;
        
        return Path.Join(ResPath, "images", "powers", "big", "power.png");
    }

    public static string RelicImagePath(this string path)
    {
        path = Path.Join(ResPath, "images", "relics", path);
        if (ResourceLoader.Exists(path)) return path;
        
        return Path.Join(ResPath, "images", "relics", "relic.png");
    }

    public static string BigRelicImagePath(this string path)
    {
        path = Path.Join(ResPath, "images", "relics", "big", path);
        if (ResourceLoader.Exists(path)) return path;
        
        return Path.Join(ResPath, "images", "relics", "big", "relic.png");
    }

    public static string CharacterUiPath(this string path)
    {
        return Path.Join(ResPath, "images", "charui", path);
    }
}
