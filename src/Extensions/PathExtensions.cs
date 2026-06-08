namespace Ra2Mod.Extensions;

/// <summary>
/// 路径扩展方法 - 简化资源路径编写
/// </summary>
public static class PathExtensions
{
    public static string CardImagePath(this string path) => $"res://images/card_portraits/{path}";
    public static string BigCardImagePath(this string path) => $"res://images/card_portraits/big/{path}";
    public static string CharacterUiPath(this string path) => $"res://images/charui/{path}";
    public static string PowerImagePath(this string path) => $"res://images/powers/{path}";
    public static string ImagePath(this string path) => $"res://images/{path}";
}
