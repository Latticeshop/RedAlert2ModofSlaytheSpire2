using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;

namespace RedAlert2ModCode.UI;

public static class MultiplayerSyncHelper
{
    public static bool IsMultiplayerGame()
    {
        if (RunManager.Instance == null) return false;
        if (RunManager.Instance.NetService == null) return false;

        var type = RunManager.Instance.NetService.Type;
        bool isMultiplayer = type == NetGameType.Host || type == NetGameType.Client;
        GD.Print($"[MultiplayerSync] IsMultiplayerGame - NetServiceType={type}, Result={isMultiplayer}");
        return isMultiplayer;
    }

    /// <summary>
    /// 当前端是否为房主。
    /// </summary>
    public static bool IsHost()
    {
        try
        {
            if (RunManager.Instance == null) return false;
            if (RunManager.Instance.NetService == null) return false;
            return RunManager.Instance.NetService.Type == NetGameType.Host;
        }
        catch { return false; }
    }

    public static bool IsLocalPlayer(Player player)
    {
        if (RunManager.Instance == null) return false;
        if (RunManager.Instance.NetService == null) return false;

        ulong serviceNetId = RunManager.Instance.NetService.NetId;
        ulong playerNetId = player.NetId;

        bool isLocal = playerNetId != 0UL && playerNetId == serviceNetId;
        GD.Print($"[MultiplayerSync] IsLocalPlayer - PlayerNetId={playerNetId}, ServiceNetId={serviceNetId}, Result={isLocal}");
        return isLocal;
    }

    public static async Task<int?> ExecuteSyncChoice(Player player, Func<Task<int?>> localChoiceFunc)
    {
        if (!IsMultiplayerGame())
        {
            return await localChoiceFunc();
        }

        var synchronizer = await WaitForSynchronizer();
        if (synchronizer == null)
        {
            return await localChoiceFunc();
        }

        uint choiceId = synchronizer.ReserveChoiceId(player);

        if (IsLocalPlayer(player))
        {
            int? result = await localChoiceFunc();
            SyncLocalChoice(synchronizer, player, choiceId, result);
            return result;
        }

        return await WaitForRemoteChoice(synchronizer, player, choiceId);
    }

    public static async Task<List<int>> ExecuteSyncMultiChoice(Player player, Func<Task<List<int>?>> localChoiceFunc)
    {
        if (!IsMultiplayerGame())
        {
            return (await localChoiceFunc()) ?? new List<int>();
        }

        var synchronizer = await WaitForSynchronizer();
        if (synchronizer == null)
        {
            return (await localChoiceFunc()) ?? new List<int>();
        }

        uint choiceId = synchronizer.ReserveChoiceId(player);

        if (IsLocalPlayer(player))
        {
            List<int>? result = await localChoiceFunc();
            SyncLocalMultiChoice(synchronizer, player, choiceId, result);
            return result ?? new List<int>();
        }

        return await WaitForRemoteMultiChoice(synchronizer, player, choiceId);
    }

    private static async Task<PlayerChoiceSynchronizer?> WaitForSynchronizer()
    {
        if (RunManager.Instance == null) return null;

        for (int i = 0; i < 60; i++)
        {
            if (RunManager.Instance.PlayerChoiceSynchronizer != null)
                return RunManager.Instance.PlayerChoiceSynchronizer;
            await Task.Yield();
        }

        return RunManager.Instance.PlayerChoiceSynchronizer;
    }

    private static void SyncLocalChoice(PlayerChoiceSynchronizer synchronizer, Player player, uint choiceId, int? selectedIndex)
    {
        try
        {
            PlayerChoiceResult result = PlayerChoiceResult.FromIndex(selectedIndex);
            synchronizer.SyncLocalChoice(player, choiceId, result);
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[MultiplayerSync] 同步选择失败: {ex}");
        }
    }

    private static void SyncLocalMultiChoice(PlayerChoiceSynchronizer synchronizer, Player player, uint choiceId, List<int>? selectedIndexes)
    {
        try
        {
            PlayerChoiceResult result = PlayerChoiceResult.FromIndexes(selectedIndexes ?? new List<int>());
            synchronizer.SyncLocalChoice(player, choiceId, result);
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[MultiplayerSync] 同步多选失败: {ex}");
        }
    }

    private static async Task<int?> WaitForRemoteChoice(PlayerChoiceSynchronizer synchronizer, Player player, uint choiceId)
    {
        try
        {
            PlayerChoiceResult result = await synchronizer.WaitForRemoteChoice(player, choiceId);
            return result.AsIndexOrNull();
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[MultiplayerSync] 等待远程选择失败: {ex}");
        }
        return null;
    }

    private static async Task<List<int>> WaitForRemoteMultiChoice(PlayerChoiceSynchronizer synchronizer, Player player, uint choiceId)
    {
        try
        {
            PlayerChoiceResult result = await synchronizer.WaitForRemoteChoice(player, choiceId);
            return result.AsIndexes();
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[MultiplayerSync] 等待远程多选失败: {ex}");
        }
        return new List<int>();
    }
}
