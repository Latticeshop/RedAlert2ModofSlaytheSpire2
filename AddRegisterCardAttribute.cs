using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

class AddRegisterCardAttribute
{
    static void Main(string[] args)
    {
        string commonCardsPath = @"D:\RedAlert2Project\red-alert-2-mod\RedAlert2ModCode\Common\Cards";
        string[] files = Directory.GetFiles(commonCardsPath, "*.cs");
        
        // 排除不需要注册的文件
        var excludeFiles = new HashSet<string>
        {
            "CommonCardValues.cs",
            "EngineerChoiceValues.cs",
            "ChronoCardModel.cs",
            "TimedBombKeywordCardModel.cs" // 这是基类
        };
        
        foreach (string filePath in files)
        {
            string fileName = Path.GetFileName(filePath);
            if (excludeFiles.Contains(fileName))
            {
                Console.WriteLine($"Skipping base/config class: {fileName}");
                continue;
            }
            
            string content = File.ReadAllText(filePath);
            
            // 检查是否已经有RegisterCard属性
            if (content.Contains("[RegisterCard"))
            {
                Console.WriteLine($"Already has RegisterCard: {fileName}");
                continue;
            }
            
            // 检查是否有命名空间声明
            var namespaceMatch = Regex.Match(content, @"namespace\s+RedAlert2ModCode\.Common\.Cards\s*;");
            if (!namespaceMatch.Success)
            {
                Console.WriteLine($"No namespace found: {fileName}");
                continue;
            }
            
            // 检查类声明
            var classMatch = Regex.Match(content, @"(\n)(public\s+(sealed\s+)?partial\s+|public\s+(sealed\s+)?)class\s+(\w+)\s*:\s*CardModel");
            if (!classMatch.Success)
            {
                Console.WriteLine($"No class declaration found: {fileName}");
                continue;
            }
            
            // 添加using语句
            if (!content.Contains("using STS2RitsuLib.Interop.AutoRegistration;"))
            {
                content = content.Replace(
                    "namespace RedAlert2ModCode.Common.Cards;",
                    "using STS2RitsuLib.Interop.AutoRegistration;\n\nnamespace RedAlert2ModCode.Common.Cards;"
                );
            }
            
            // 添加RegisterCard属性（同时注册到Allies和Soviet卡池）
            content = content.Replace(
                classMatch.Value,
                $"{classMatch.Groups[1]}[RegisterCard(typeof(RedAlert2ModCode.Allies.AlliesCardPool))]\n[RegisterCard(typeof(RedAlert2ModCode.Soviet.SovietCardPool))]\n{classMatch.Groups[2].Value}class {classMatch.Groups[5].Value} : CardModel"
            );
            
            File.WriteAllText(filePath, content);
            Console.WriteLine($"Added RegisterCard attributes to: {fileName}");
        }
        
        Console.WriteLine("\nDone!");
    }
}
