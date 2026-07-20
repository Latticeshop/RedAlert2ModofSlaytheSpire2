using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.UI;
using System;

namespace RedAlert2ModCode.Common.Patches;

[HarmonyPatch(typeof(NPower))]
public static class DollarPowerClickPatch
{
    public static void Install(Harmony harmony)
    {
        var readyMethod = AccessTools.Method(typeof(NPower), nameof(NPower._Ready));
        if (readyMethod != null)
        {
            harmony.Patch(
                original: readyMethod,
                postfix: new HarmonyMethod(typeof(DollarPowerClickPatch), nameof(ReadyPostfix))
            );
            ModInitializer.Logger.Info("[DollarPowerClickPatch] 已安装能力点击补丁 (_Ready)");
        }
        else
        {
            ModInitializer.Logger.Error("[DollarPowerClickPatch] 无法找到 NPower._Ready 方法");
        }
    }

    private static void ReadyPostfix(NPower __instance)
    {
        try
        {
            if (!DollarTransferConfig.Instance.Enabled)
            {
                return;
            }

            var power = __instance.Model;
            if (power is DollarPower)
            {
                __instance.MouseFilter = Control.MouseFilterEnum.Stop;
                
                __instance.GuiInput += (InputEvent @event) =>
                {
                    if (@event is InputEventMouseButton mouseButton)
                    {
                        if (mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed)
                        {
                            try
                            {
                                var combatState = CombatManager.Instance.DebugOnlyGetState();
                                if (combatState != null)
                                {
                                    var player = LocalContext.GetMe(combatState);
                                    if (player != null)
                                    {
                                        _ = DollarTransferScreen.ShowTransferScreen(player);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                GD.PrintErr($"[DollarPowerClickPatch] 点击处理错误: {ex.Message}");
                            }
                        }
                    }
                };
                GD.Print("[DollarPowerClickPatch] 为刀乐能力添加点击事件");
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[DollarPowerClickPatch] ReadyPostfix 错误: {ex.Message}");
        }
    }
}