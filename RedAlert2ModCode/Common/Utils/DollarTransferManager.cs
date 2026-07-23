using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using RedAlert2ModCode.Common.GameActions;
using RedAlert2ModCode.Common.Powers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RedAlert2ModCode.Common.Utils;

public static class DollarTransferManager
{
    private static readonly Dictionary<long, TransferRequest> _pendingTransfers = new();
    private static readonly object _lock = new();
    private static bool _isTransferring = false;

    public class TransferRequest
    {
        public long Id { get; }
        public Player Sender { get; }
        public Player Receiver { get; }
        public int Amount { get; }
        public DateTime CreatedTime { get; }
        public bool Completed { get; set; }

        public TransferRequest(Player sender, Player receiver, int amount)
        {
            Id = DateTime.UtcNow.Ticks;
            Sender = sender;
            Receiver = receiver;
            Amount = amount;
            CreatedTime = DateTime.UtcNow;
            Completed = false;
        }
    }

    public static bool CanTransfer(Player sender, int amount)
    {
        if (sender == null) return false;
        if (amount <= 0) return false;

        var dollarPower = sender.Creature.Powers.OfType<DollarPower>().FirstOrDefault();
        if (dollarPower == null) return false;

        return dollarPower.DollarValue >= amount;
    }

    public static IEnumerable<Player> GetValidTargets(Player sender)
    {
        if (sender == null) return Enumerable.Empty<Player>();

        var combatState = sender.Creature.CombatState;
        if (combatState == null) return Enumerable.Empty<Player>();

        var targets = from c in combatState.GetTeammatesOf(sender.Creature)
                      where c != null && c.IsAlive && c.IsPlayer && c.Player != sender
                      select c.Player;

        return targets;
    }

    public static bool ExecuteTransfer(Player sender, Player receiver, int amount)
    {
        if (!CanTransfer(sender, amount))
        {
            GD.PrintErr($"[DollarTransfer] 转账失败：资金不足或无效参数");
            return false;
        }

        if (sender == receiver)
        {
            GD.PrintErr("[DollarTransfer] 转账失败：不能转给自己");
            return false;
        }

        lock (_lock)
        {
            if (_isTransferring)
            {
                GD.PrintErr("[DollarTransfer] 转账失败：已有转账操作进行中，请稍后再试");
                return false;
            }
            _isTransferring = true;
        }

        var request = CreateTransferRequest(sender, receiver, amount);

        var action = new DollarTransferGameAction(sender, receiver.NetId, amount);
        action.AfterFinished += delegate
        {
            GD.Print("[DollarTransfer] 转账操作完成");
            if (request != null)
            {
                CompleteTransfer(request.Id);
            }
            lock (_lock)
            {
                _isTransferring = false;
            }

            try
            {
                var unlockAction = new DollarTransferUnlockAction(sender);
                RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(unlockAction);
                GD.Print("[DollarTransfer] 转账锁解锁同步已发送");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[DollarTransfer] 发送解锁同步异常：{ex}");
            }
        };

        try
        {
            RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(action);
            GD.Print($"[DollarTransfer] 转账请求已发送：{GetPlayerName(sender)} -> {GetPlayerName(receiver)}, 金额: {amount}");
            return true;
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[DollarTransfer] 转账异常：{ex}");
            lock (_lock)
            {
                _isTransferring = false;
            }
            if (request != null)
            {
                CancelTransfer(request.Id);
            }

            try
            {
                var unlockAction = new DollarTransferUnlockAction(sender);
                RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(unlockAction);
                GD.Print("[DollarTransfer] 转账失败，解锁同步已发送");
            }
            catch (Exception unlockEx)
            {
                GD.PrintErr($"[DollarTransfer] 发送解锁同步异常：{unlockEx}");
            }

            return false;
        }
    }

    public static TransferRequest? CreateTransferRequest(Player sender, Player receiver, int amount)
    {
        if (!CanTransfer(sender, amount)) return null;
        if (sender == receiver) return null;

        var request = new TransferRequest(sender, receiver, amount);

        lock (_lock)
        {
            _pendingTransfers[request.Id] = request;
        }

        GD.Print($"[DollarTransfer] 创建转账请求: {request.Id}, {GetPlayerName(sender)} -> {GetPlayerName(receiver)}, {amount}");
        return request;
    }

    public static void CompleteTransfer(long requestId)
    {
        lock (_lock)
        {
            if (_pendingTransfers.TryGetValue(requestId, out var request))
            {
                request.Completed = true;
                GD.Print($"[DollarTransfer] 转账请求完成: {requestId}");
            }
        }
    }

    public static void CancelTransfer(long requestId)
    {
        lock (_lock)
        {
            if (_pendingTransfers.TryGetValue(requestId, out var request))
            {
                request.Completed = true;
                GD.Print($"[DollarTransfer] 转账请求取消: {requestId}");
            }
        }
    }

    public static bool IsTransferPending(long requestId)
    {
        lock (_lock)
        {
            if (_pendingTransfers.TryGetValue(requestId, out var request))
            {
                return !request.Completed;
            }
            return false;
        }
    }

    public static void CleanupExpiredRequests()
    {
        var timeout = DollarTransferConfig.Instance.TimeoutSeconds;

        lock (_lock)
        {
            var expired = _pendingTransfers.Where(kv => 
                (DateTime.UtcNow - kv.Value.CreatedTime).TotalSeconds > timeout && !kv.Value.Completed
            ).ToList();

            foreach (var item in expired)
            {
                item.Value.Completed = true;
                GD.Print($"[DollarTransfer] 清理过期请求: {item.Key}");
            }
        }
    }

    public static int GetSenderBalance(Player sender)
    {
        var dollarPower = sender.Creature.Powers.OfType<DollarPower>().FirstOrDefault();
        return dollarPower?.DollarValue ?? 0;
    }

    public static void ResetTransferLock()
    {
        lock (_lock)
        {
            _isTransferring = false;
            GD.Print("[DollarTransfer] 转账锁已重置");
        }
    }

    private static string GetPlayerName(Player player)
    {
        return player?.Character?.GetType().Name ?? "Unknown";
    }
}