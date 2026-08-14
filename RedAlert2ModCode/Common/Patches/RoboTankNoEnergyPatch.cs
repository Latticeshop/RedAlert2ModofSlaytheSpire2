using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;
using RedAlert2ModCode.Allies.Cards;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Common.Patches;

/// <summary>
/// 遥控坦克无能量选中音效：点击手牌中的遥控坦克时，若当前能量为 0（无法打出），
/// 播放"Vrobse2a-无能量选中"语音。
/// </summary>
[HarmonyPatch(typeof(NPlayerHand))]
public static class RoboTankNoEnergyPatch
{
    [HarmonyPrefix]
    [HarmonyPatch("StartCardPlay")]
    public static void StartCardPlayPrefix(NHandCardHolder holder)
    {
        try
        {
            if (holder?.CardNode?.Model is RoboTank card &&
                card.Owner?.PlayerCombatState != null &&
                card.Owner.PlayerCombatState.Energy <= 0)
            {
                UnitVoiceHelper.PlayUnitVoice("RoboTankNoEnergy", "Allied");
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[RoboTankNoEnergy] 播放无能量语音失败: {ex.Message}");
        }
    }
}
