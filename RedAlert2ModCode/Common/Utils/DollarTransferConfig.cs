using Godot;
using System;
using System.IO;

namespace RedAlert2ModCode.Common.Utils;

public class DollarTransferConfig
{
    public bool Enabled { get; set; } = true;
    public bool AutoAccept { get; set; } = false;
    public int TimeoutSeconds { get; set; } = 30;
    public bool RespectEctoplasm { get; set; } = true;
    public bool BypassGoldGainTriggers { get; set; } = true;
    public bool DebugMode { get; set; } = false;

    private static DollarTransferConfig? _instance;

    public static DollarTransferConfig Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = LoadConfig();
            }
            return _instance;
        }
    }

    private static DollarTransferConfig LoadConfig()
    {
        try
        {
            string configPath = Path.Combine(ProjectSettings.GlobalizePath("res://RedAlert2ModResources/config"), "dollar_transfer.json");
            
            if (File.Exists(configPath))
            {
                string content = File.ReadAllText(configPath);
                var config = ParseConfig(content);
                GD.Print("[DollarTransfer] 配置文件加载成功");
                return config;
            }
            
            GD.Print("[DollarTransfer] 使用默认配置");
            return new DollarTransferConfig();
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[DollarTransfer] 加载配置失败: {ex.Message}");
            return new DollarTransferConfig();
        }
    }

    private static DollarTransferConfig ParseConfig(string jsonContent)
    {
        var config = new DollarTransferConfig();
        var json = new Json();
        
        var error = json.Parse(jsonContent);
        if (error != Error.Ok)
        {
            GD.PrintErr($"[DollarTransfer] 配置文件解析失败: {error}");
            return config;
        }

        var data = json.Data;
        if (data.VariantType != Variant.Type.Dictionary)
        {
            return config;
        }

        var dict = new Godot.Collections.Dictionary<string, Variant>(data.AsGodotDictionary());

        if (dict.ContainsKey("enabled"))
            config.Enabled = dict["enabled"].AsBool();
        if (dict.ContainsKey("auto_accept"))
            config.AutoAccept = dict["auto_accept"].AsBool();
        if (dict.ContainsKey("timeout_seconds"))
            config.TimeoutSeconds = dict["timeout_seconds"].AsInt32();
        if (dict.ContainsKey("respect_ectoplasm"))
            config.RespectEctoplasm = dict["respect_ectoplasm"].AsBool();
        if (dict.ContainsKey("bypass_gold_gain_triggers"))
            config.BypassGoldGainTriggers = dict["bypass_gold_gain_triggers"].AsBool();
        if (dict.ContainsKey("debug_mode"))
            config.DebugMode = dict["debug_mode"].AsBool();

        return config;
    }

    public void SaveConfig()
    {
        try
        {
            string configDir = ProjectSettings.GlobalizePath("res://RedAlert2ModResources/config");
            if (!Directory.Exists(configDir))
            {
                Directory.CreateDirectory(configDir);
            }
            
            string configPath = Path.Combine(configDir, "dollar_transfer.json");
            string content = SerializeConfig();
            File.WriteAllText(configPath, content);
            GD.Print("[DollarTransfer] 配置文件保存成功");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[DollarTransfer] 保存配置失败: {ex.Message}");
        }
    }

    private string SerializeConfig()
    {
        var data = new Godot.Collections.Dictionary<string, Variant>
        {
            { "enabled", Enabled },
            { "auto_accept", AutoAccept },
            { "timeout_seconds", TimeoutSeconds },
            { "respect_ectoplasm", RespectEctoplasm },
            { "bypass_gold_gain_triggers", BypassGoldGainTriggers },
            { "debug_mode", DebugMode }
        };

        return Json.Stringify(data);
    }
}