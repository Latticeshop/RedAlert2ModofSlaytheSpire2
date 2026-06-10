using HarmonyLib;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using RedAlert2ModCode.Allies.Cards;

namespace RedAlert2ModCode.Allies;

/// <summary>
/// 为盟军初始卡组添加消耗附魔的补丁
/// </summary>
[HarmonyPatch]
public static class InitialDeckExhaustPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(RunManager), nameof(RunManager.Launch))]
    public static void LaunchPostfix(RunState __result)
    {
        // 获取本地玩家（单人模式下为第一个玩家）
        var localPlayer = __result.Players.FirstOrDefault();
        if (localPlayer == null)
            return;

        // 检查是否是盟军角色
        if (localPlayer.Character is not Allies)
            return;

        // 遍历玩家的卡组，为美国大兵和灰熊坦克添加消耗效果
        foreach (var card in localPlayer.Deck.Cards)
        {
            // 检查卡牌类型
            if (card is AmericanSoldier || card is GrizzlyTank)
            {
                // 添加消耗词条
                card.AddKeyword(CardKeyword.Exhaust);
            }
        }
    }
}