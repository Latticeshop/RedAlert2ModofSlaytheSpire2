using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;

namespace RedAlert2ModCode.UI;

public static class MultiplayerSyncHelper
{
    /// <summary>
    /// 本地卡牌效果处理串行化闸门（依次执行，类似排到队列末尾）。
    /// 注意：不能用于“暂停/恢复动作”阶段——那些阶段在客户端上走主机中转握手（客户端→主机→客户端），
    /// 本机闸门会与握手互相等待造成卡死。只用于纯本地的卡牌效果/取消处理（如 HandleCardCancellation）。
    /// </summary>
    private static readonly SemaphoreSlim _executionGate = new(1, 1);

    /// <summary>
    /// 将一次本地卡牌效果/取消处理排入串行队列，同一时刻只执行一个。
    /// </summary>
    public static async Task RunSerialized(Func<Task> action)
    {
        await _executionGate.WaitAsync();
        try
        {
            await action();
        }
        finally
        {
            _executionGate.Release();
        }
    }

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

    /// <summary>
    /// 执行一次同步单选。
    /// 与原版 CardSelectCmd 一致：预留选择ID → SignalPlayerChoiceBegun（创建/暂停动作，只阻塞选择者的队列，其他队友继续）
    /// → 本机显示面板/远端等待 → SignalPlayerChoiceEnded（恢复动作）。
    /// </summary>
    public static async Task<int?> ExecuteSyncChoice(PlayerChoiceContext context, Player player, Func<Task<int?>> localChoiceFunc)
    {
        if (context == null)
        {
            return await localChoiceFunc();
        }

        if (!IsMultiplayerGame())
        {
            // 单机：同样走暂停信号（与原版一致），但无需同步结果
            await context.SignalPlayerChoiceBegun(player, PlayerChoiceOptions.CancelPlayCardActions);
            try
            {
                return await localChoiceFunc();
            }
            finally
            {
                await context.SignalPlayerChoiceEnded();
            }
        }

        var synchronizer = await WaitForSynchronizer();
        if (synchronizer == null)
        {
            await context.SignalPlayerChoiceBegun(player, PlayerChoiceOptions.CancelPlayCardActions);
            try
            {
                return await localChoiceFunc();
            }
            finally
            {
                await context.SignalPlayerChoiceEnded();
            }
        }

        uint choiceId = synchronizer.ReserveChoiceId(player);
        await context.SignalPlayerChoiceBegun(player, PlayerChoiceOptions.CancelPlayCardActions);
        try
        {
            if (IsLocalPlayer(player))
            {
                int? result = await localChoiceFunc();
                SyncLocalChoice(synchronizer, player, choiceId, result);
                return result;
            }

            return await WaitForRemoteChoice(synchronizer, player, choiceId);
        }
        finally
        {
            await context.SignalPlayerChoiceEnded();
        }
    }

    /// <summary>
    /// 执行一次同步多选，机制与 <see cref="ExecuteSyncChoice"/> 相同。
    /// </summary>
    public static async Task<List<int>> ExecuteSyncMultiChoice(PlayerChoiceContext context, Player player, Func<Task<List<int>?>> localChoiceFunc)
    {
        if (context == null)
        {
            return (await localChoiceFunc()) ?? new List<int>();
        }

        if (!IsMultiplayerGame())
        {
            // 单机：同样走暂停信号（与原版一致），但无需同步结果
            await context.SignalPlayerChoiceBegun(player, PlayerChoiceOptions.CancelPlayCardActions);
            try
            {
                return (await localChoiceFunc()) ?? new List<int>();
            }
            finally
            {
                await context.SignalPlayerChoiceEnded();
            }
        }

        var synchronizer = await WaitForSynchronizer();
        if (synchronizer == null)
        {
            await context.SignalPlayerChoiceBegun(player, PlayerChoiceOptions.CancelPlayCardActions);
            try
            {
                return (await localChoiceFunc()) ?? new List<int>();
            }
            finally
            {
                await context.SignalPlayerChoiceEnded();
            }
        }

        uint choiceId = synchronizer.ReserveChoiceId(player);
        await context.SignalPlayerChoiceBegun(player, PlayerChoiceOptions.CancelPlayCardActions);
        try
        {
            if (IsLocalPlayer(player))
            {
                List<int>? result = await localChoiceFunc();
                SyncLocalMultiChoice(synchronizer, player, choiceId, result);
                return result ?? new List<int>();
            }

            return await WaitForRemoteMultiChoice(synchronizer, player, choiceId);
        }
        finally
        {
            await context.SignalPlayerChoiceEnded();
        }
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
