using Godot;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using RedAlert2ModCode.Common.Utils;
using System;
using System.Collections;
using System.Reflection;
using System.Threading.Tasks;

namespace RedAlert2ModCode.UI;

internal static class CardSelectionSyncHelper
{
    public static async Task<CardModel?> ShowSelectionWithSync(List<CardModel> cards, Player player, Dictionary<string, CardValueStore.CardValues>? cardValuesMap = null, FactionType faction = FactionType.Allied)
    {
        CardModel? selectedCard = null;
        
        object? runManager = GetRunManager();
        if (runManager == null)
        {
            selectedCard = await CardSelectionScreen.ShowSelection(cards, cardValuesMap, faction);
            return selectedCard;
        }

        if (!IsMultiplayerGame(runManager))
        {
            selectedCard = await CardSelectionScreen.ShowSelection(cards, cardValuesMap, faction);
            return selectedCard;
        }

        object? synchronizer = await WaitForPlayerChoiceSynchronizerAsync(runManager);
        if (synchronizer == null)
        {
            selectedCard = await CardSelectionScreen.ShowSelection(cards, cardValuesMap, faction);
            return selectedCard;
        }

        uint choiceId = ReserveChoiceId(synchronizer, player);
        
        if (IsLocalPlayer(runManager, player))
        {
            selectedCard = await CardSelectionScreen.ShowSelection(cards, cardValuesMap, faction);
            
            if (TrySyncLocalCardChoice(synchronizer, player, choiceId, selectedCard, cards, "card-selection", out uint sentChoiceId))
            {
                GD.Print($"[CardSelectionSync] 同步本地选择: player={player.NetId} choiceId={sentChoiceId} selected={(selectedCard != null ? selectedCard.Id.Entry : "null")}");
            }
            else
            {
                GD.PrintErr($"[CardSelectionSync] 同步本地选择失败: player={player.NetId} choiceId={choiceId}");
            }
            
            return selectedCard;
        }

        GD.Print($"[CardSelectionSync] 等待远程选择: player={player.NetId} choiceId={choiceId}");
        (PlayerChoiceResult remoteChoice, uint receivedChoiceId)? received = await TryWaitForRemoteCardChoice(
            synchronizer,
            (RunState)player.RunState,
            player,
            choiceId,
            "card-selection");
        
        if (!received.HasValue)
        {
            GD.PrintErr($"[CardSelectionSync] 等待远程选择超时: player={player.NetId} choiceId={choiceId}");
            return cards.FirstOrDefault();
        }

        (PlayerChoiceResult remoteChoice, uint receivedChoiceId) = received.Value;
        GD.Print($"[CardSelectionSync] 收到远程选择: player={player.NetId} choiceId={receivedChoiceId}");
        
        selectedCard = ResolveRemoteCardChoice(cards, remoteChoice);
        GD.Print($"[CardSelectionSync] 解析远程选择: {(selectedCard != null ? selectedCard.Id.Entry : "null")}");
        
        return selectedCard;
    }

    public static async Task<List<CardModel>?> ShowMultiSelectionWithSync(List<CardModel> cards, int maxSelect, int minSelect, Player player)
    {
        List<CardModel>? selectedCards = null;
        
        object? runManager = GetRunManager();
        if (runManager == null)
        {
            selectedCards = await CardSelectionScreen.ShowMultiSelection(cards, maxSelect, minSelect);
            return selectedCards;
        }

        if (!IsMultiplayerGame(runManager))
        {
            selectedCards = await CardSelectionScreen.ShowMultiSelection(cards, maxSelect, minSelect);
            return selectedCards;
        }

        object? synchronizer = await WaitForPlayerChoiceSynchronizerAsync(runManager);
        if (synchronizer == null)
        {
            selectedCards = await CardSelectionScreen.ShowMultiSelection(cards, maxSelect, minSelect);
            return selectedCards;
        }

        uint choiceId = ReserveChoiceId(synchronizer, player);
        
        if (IsLocalPlayer(runManager, player))
        {
            selectedCards = await CardSelectionScreen.ShowMultiSelection(cards, maxSelect, minSelect);
            
            if (TrySyncLocalMultiCardChoice(synchronizer, player, choiceId, selectedCards, cards, "multi-card-selection", out uint sentChoiceId))
            {
                GD.Print($"[CardSelectionSync] 同步本地多选: player={player.NetId} choiceId={sentChoiceId} selected={selectedCards?.Count ?? 0}");
            }
            else
            {
                GD.PrintErr($"[CardSelectionSync] 同步本地多选失败: player={player.NetId} choiceId={choiceId}");
            }
            
            return selectedCards;
        }

        GD.Print($"[CardSelectionSync] 等待远程多选: player={player.NetId} choiceId={choiceId}");
        (PlayerChoiceResult remoteChoice, uint receivedChoiceId)? received = await TryWaitForRemoteMultiCardChoice(
            synchronizer,
            (RunState)player.RunState,
            player,
            choiceId,
            "multi-card-selection");
        
        if (!received.HasValue)
        {
            GD.PrintErr($"[CardSelectionSync] 等待远程多选超时: player={player.NetId} choiceId={choiceId}");
            return cards.Count > 0 ? new List<CardModel> { cards[0] } : null;
        }

        (PlayerChoiceResult remoteChoice, uint receivedChoiceId) = received.Value;
        GD.Print($"[CardSelectionSync] 收到远程多选: player={player.NetId} choiceId={receivedChoiceId}");
        
        selectedCards = ResolveRemoteMultiCardChoice(cards, remoteChoice);
        GD.Print($"[CardSelectionSync] 解析远程多选: {selectedCards?.Count ?? 0} 张");
        
        return selectedCards;
    }

    private static object? GetRunManager()
    {
        try
        {
            var runManagerType = Type.GetType("MegaCrit.Sts2.Core.Runs.RunManager, MegaCrit.Sts2.Core");
            if (runManagerType == null) return null;
            
            var instanceProp = runManagerType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            if (instanceProp == null) return null;
            
            return instanceProp.GetValue(null);
        }
        catch
        {
            return null;
        }
    }

    private static bool IsMultiplayerGame(object runManager)
    {
        try
        {
            var netServiceProp = runManager.GetType().GetProperty("NetService");
            if (netServiceProp == null) return false;
            
            var netService = netServiceProp.GetValue(runManager);
            if (netService == null) return false;
            
            var typeProp = netService.GetType().GetProperty("Type");
            if (typeProp == null) return false;
            
            var netType = typeProp.GetValue(netService);
            if (netType == null) return false;
            
            string typeName = netType.ToString();
            return typeName == "Host" || typeName == "Client";
        }
        catch
        {
            return false;
        }
    }

    private static async Task<object?> WaitForPlayerChoiceSynchronizerAsync(object runManager)
    {
        try
        {
            for (int i = 0; i < 60; i++)
            {
                var synchronizerProp = runManager.GetType().GetProperty("PlayerChoiceSynchronizer");
                if (synchronizerProp != null)
                {
                    var synchronizer = synchronizerProp.GetValue(runManager);
                    if (synchronizer != null)
                        return synchronizer;
                }
                await Task.Yield();
            }
            
            var finalProp = runManager.GetType().GetProperty("PlayerChoiceSynchronizer");
            if (finalProp != null)
                return finalProp.GetValue(runManager);
        }
        catch
        {
        }
        return null;
    }

    private static bool IsLocalPlayer(object runManager, Player player)
    {
        try
        {
            var netServiceProp = runManager.GetType().GetProperty("NetService");
            if (netServiceProp == null) return true;
            
            var netService = netServiceProp.GetValue(runManager);
            if (netService == null) return true;
            
            var serviceNetIdProp = netService.GetType().GetProperty("NetId");
            if (serviceNetIdProp == null) return true;
            
            ulong serviceNetId = (ulong)serviceNetIdProp.GetValue(netService);
            return player.NetId != 0UL && player.NetId == serviceNetId;
        }
        catch
        {
            return true;
        }
    }

    private static uint ReserveChoiceId(object synchronizer, Player player)
    {
        try
        {
            var reserveMethod = synchronizer.GetType().GetMethod("ReserveChoiceId");
            if (reserveMethod != null)
            {
                return (uint)reserveMethod.Invoke(synchronizer, new[] { player });
            }
        }
        catch
        {
        }
        return uint.MaxValue;
    }

    private static bool TrySyncLocalCardChoice(
        object synchronizer,
        Player player,
        uint choiceId,
        CardModel? selectedCard,
        List<CardModel> cards,
        string context,
        out uint sentChoiceId)
    {
        sentChoiceId = choiceId;
        try
        {
            int selectedIndex = selectedCard != null ? cards.FindIndex(c => c == selectedCard) : -1;
            PlayerChoiceResult result = CreateCardSelectionResult(selectedIndex);
            var syncMethod = synchronizer.GetType().GetMethod("SyncLocalChoice");
            if (syncMethod != null)
            {
                syncMethod.Invoke(synchronizer, new object[] { player, choiceId, result });
                return true;
            }
        }
        catch (InvalidOperationException ex)
        {
            try
            {
                uint retryChoiceId = ReserveChoiceId(synchronizer, player);
                GD.Print($"[CardSelectionSync] 重试同步: context={context} player={player.NetId} staleChoiceId={choiceId} retryChoiceId={retryChoiceId} error={ex.Message}");
                
                int selectedIndex = selectedCard != null ? cards.FindIndex(c => c == selectedCard) : -1;
                PlayerChoiceResult result = CreateCardSelectionResult(selectedIndex);
                var syncMethod = synchronizer.GetType().GetMethod("SyncLocalChoice");
                if (syncMethod != null)
                {
                    syncMethod.Invoke(synchronizer, new object[] { player, retryChoiceId, result });
                    sentChoiceId = retryChoiceId;
                    return true;
                }
            }
            catch (Exception retryEx)
            {
                GD.PrintErr($"[CardSelectionSync] 同步失败: context={context} player={player.NetId} choiceId={sentChoiceId} error={retryEx}");
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[CardSelectionSync] 同步失败: context={context} player={player.NetId} choiceId={choiceId} error={ex}");
        }
        return false;
    }

    private static bool TrySyncLocalMultiCardChoice(
        object synchronizer,
        Player player,
        uint choiceId,
        List<CardModel>? selectedCards,
        List<CardModel> cards,
        string context,
        out uint sentChoiceId)
    {
        sentChoiceId = choiceId;
        try
        {
            List<int> selectedIndices = new();
            if (selectedCards != null)
            {
                foreach (var card in selectedCards)
                {
                    int index = cards.FindIndex(c => c == card);
                    if (index >= 0)
                        selectedIndices.Add(index);
                }
            }
            PlayerChoiceResult result = CreateMultiCardSelectionResult(selectedIndices);
            var syncMethod = synchronizer.GetType().GetMethod("SyncLocalChoice");
            if (syncMethod != null)
            {
                syncMethod.Invoke(synchronizer, new object[] { player, choiceId, result });
                return true;
            }
        }
        catch (InvalidOperationException ex)
        {
            try
            {
                uint retryChoiceId = ReserveChoiceId(synchronizer, player);
                GD.Print($"[CardSelectionSync] 重试同步多选: context={context} player={player.NetId} staleChoiceId={choiceId} retryChoiceId={retryChoiceId} error={ex.Message}");
                
                List<int> selectedIndices = new();
                if (selectedCards != null)
                {
                    foreach (var card in selectedCards)
                    {
                        int index = cards.FindIndex(c => c == card);
                        if (index >= 0)
                            selectedIndices.Add(index);
                    }
                }
                PlayerChoiceResult result = CreateMultiCardSelectionResult(selectedIndices);
                var syncMethod = synchronizer.GetType().GetMethod("SyncLocalChoice");
                if (syncMethod != null)
                {
                    syncMethod.Invoke(synchronizer, new object[] { player, retryChoiceId, result });
                    sentChoiceId = retryChoiceId;
                    return true;
                }
            }
            catch (Exception retryEx)
            {
                GD.PrintErr($"[CardSelectionSync] 同步多选失败: context={context} player={player.NetId} choiceId={sentChoiceId} error={retryEx}");
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[CardSelectionSync] 同步多选失败: context={context} player={player.NetId} choiceId={choiceId} error={ex}");
        }
        return false;
    }

    private static async Task<(PlayerChoiceResult Result, uint ChoiceId)?> TryWaitForRemoteCardChoice(
        object synchronizer,
        RunState runState,
        Player player,
        uint choiceId,
        string context)
    {
        return await TryWaitForRemoteChoice(synchronizer, runState, player, choiceId, IsCardSelectionResult, context);
    }

    private static async Task<(PlayerChoiceResult Result, uint ChoiceId)?> TryWaitForRemoteMultiCardChoice(
        object synchronizer,
        RunState runState,
        Player player,
        uint choiceId,
        string context)
    {
        return await TryWaitForRemoteChoice(synchronizer, runState, player, choiceId, IsMultiCardSelectionResult, context);
    }

    private static async Task<(PlayerChoiceResult Result, uint ChoiceId)?> TryWaitForRemoteChoice(
        object synchronizer,
        RunState runState,
        Player player,
        uint choiceId,
        Func<PlayerChoiceResult, bool> isExpected,
        string context)
    {
        uint currentChoiceId = choiceId;
        while (true)
        {
            (PlayerChoiceResult Result, uint ChoiceId)? remote = await WaitForRemoteChoiceByEvent(
                synchronizer, runState, player, currentChoiceId, isExpected, context);
            
            if (!remote.HasValue)
            {
                GD.Print($"[CardSelectionSync] 等待远程选择超时: context={context} player={player.NetId} choiceId={currentChoiceId}");
                return null;
            }

            PlayerChoiceResult remoteChoice = remote.Value.Result;
            uint receivedChoiceId = remote.Value.ChoiceId;
            
            if (isExpected(remoteChoice))
            {
                return (remoteChoice, receivedChoiceId);
            }

            GD.Print($"[CardSelectionSync] 跳过非预期选择: context={context} player={player.NetId} expectedChoiceId={currentChoiceId} receivedChoiceId={receivedChoiceId}");
            currentChoiceId = ReserveChoiceId(synchronizer, player);
        }
    }

    private static async Task<(PlayerChoiceResult Result, uint ChoiceId)?> WaitForRemoteChoiceByEvent(
        object synchronizer,
        RunState runState,
        Player player,
        uint choiceId,
        Func<PlayerChoiceResult, bool> isExpected,
        string context)
    {
        if (TryTakeBufferedExpectedRemoteChoice(synchronizer, runState, player, isExpected, out PlayerChoiceResult expectedBufferedResult, out uint expectedBufferedChoiceId))
        {
            GD.Print($"[CardSelectionSync] 使用缓存的预期选择: context={context} player={player.NetId} choiceId={expectedBufferedChoiceId}");
            return (expectedBufferedResult, expectedBufferedChoiceId);
        }

        if (TryTakeBufferedRemoteChoice(synchronizer, player, choiceId, out NetPlayerChoiceResult bufferedResult))
        {
            GD.Print($"[CardSelectionSync] 使用缓存的选择: context={context} player={player.NetId} choiceId={choiceId}");
            return (PlayerChoiceResult.FromNetData(player, runState, bufferedResult), choiceId);
        }

        TaskCompletionSource<(uint ChoiceId, NetPlayerChoiceResult Result)> completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        
        EventInfo? eventInfo = synchronizer.GetType().GetEvent("PlayerChoiceReceived");
        Delegate? handler = null;
        
        try
        {
            if (eventInfo != null)
            {
                var handlerInstance = new PlayerChoiceReceivedHandler(player.NetId, choiceId, isExpected, player, runState, completion);
                handler = Delegate.CreateDelegate(eventInfo.EventHandlerType, handlerInstance, "OnReceived");
                eventInfo.AddEventHandler(synchronizer, handler);
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[CardSelectionSync] 注册事件处理器失败: {ex}");
        }
        
        try
        {
            if (TryTakeBufferedExpectedRemoteChoice(synchronizer, runState, player, isExpected, out PlayerChoiceResult lateExpectedBufferedResult, out uint lateExpectedBufferedChoiceId))
            {
                GD.Print($"[CardSelectionSync] 使用延迟缓存的预期选择: context={context} player={player.NetId} choiceId={lateExpectedBufferedChoiceId}");
                return (lateExpectedBufferedResult, lateExpectedBufferedChoiceId);
            }

            if (TryTakeBufferedRemoteChoice(synchronizer, player, choiceId, out NetPlayerChoiceResult lateBufferedResult))
            {
                GD.Print($"[CardSelectionSync] 使用延迟缓存的选择: context={context} player={player.NetId} choiceId={choiceId}");
                return (PlayerChoiceResult.FromNetData(player, runState, lateBufferedResult), choiceId);
            }

            Task<(uint ChoiceId, NetPlayerChoiceResult Result)> waitTask = completion.Task;
            Task timeout = WaitForFramesAsync(1800);
            
            if (await Task.WhenAny(waitTask, timeout) != waitTask)
            {
                return null;
            }

            (uint receivedChoiceId, NetPlayerChoiceResult result) = await waitTask;
            TryTakeBufferedRemoteChoice(synchronizer, player, receivedChoiceId, out _);
            GD.Print($"[CardSelectionSync] 收到选择: context={context} player={player.NetId} expectedChoiceId={choiceId} receivedChoiceId={receivedChoiceId}");
            
            return (PlayerChoiceResult.FromNetData(player, runState, result), receivedChoiceId);
        }
        finally
        {
            if (eventInfo != null && handler != null)
            {
                eventInfo.RemoveEventHandler(synchronizer, handler);
            }
        }
    }

    private static async Task WaitForFramesAsync(int frameCount)
    {
        TimeSpan timeout = TimeSpan.FromSeconds(frameCount / 60.0d);
        if (timeout <= TimeSpan.Zero)
            return;

        DateTimeOffset deadline = DateTimeOffset.UtcNow + timeout;
        while (DateTimeOffset.UtcNow < deadline)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(16));
        }
    }

    private static bool TryTakeBufferedRemoteChoice(
        object synchronizer,
        Player player,
        uint choiceId,
        out NetPlayerChoiceResult result)
    {
        result = default;
        try
        {
            FieldInfo? receivedChoicesField = synchronizer.GetType().GetField("_receivedChoices", BindingFlags.Instance | BindingFlags.NonPublic);
            if (receivedChoicesField?.GetValue(synchronizer) is not IList receivedChoices)
                return false;

            for (int i = 0; i < receivedChoices.Count; i++)
            {
                object? entry = receivedChoices[i];
                if (entry == null)
                    continue;

                Type entryType = entry.GetType();
                ulong senderId = (ulong)(entryType.GetField("senderId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(entry) ?? 0UL);
                uint bufferedChoiceId = (uint)(entryType.GetField("choiceId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(entry) ?? uint.MaxValue);
                
                if (senderId != player.NetId || bufferedChoiceId != choiceId)
                    continue;

                object? completionSource = entryType.GetField("completionSource", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(entry);
                if (completionSource?.GetType().GetProperty("Task")?.GetValue(completionSource) is not Task<NetPlayerChoiceResult> task
                    || !task.IsCompletedSuccessfully)
                    continue;

                result = task.Result;
                receivedChoices.RemoveAt(i);
                return true;
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[CardSelectionSync] 读取缓存选择失败: player={player.NetId} choiceId={choiceId} error={ex}");
        }
        return false;
    }

    private static bool TryTakeBufferedExpectedRemoteChoice(
        object synchronizer,
        RunState runState,
        Player player,
        Func<PlayerChoiceResult, bool> isExpected,
        out PlayerChoiceResult result,
        out uint choiceId)
    {
        result = null!;
        choiceId = uint.MaxValue;
        try
        {
            FieldInfo? receivedChoicesField = synchronizer.GetType().GetField("_receivedChoices", BindingFlags.Instance | BindingFlags.NonPublic);
            if (receivedChoicesField?.GetValue(synchronizer) is not IList receivedChoices)
                return false;

            for (int i = 0; i < receivedChoices.Count; i++)
            {
                object? entry = receivedChoices[i];
                if (entry == null)
                    continue;

                Type entryType = entry.GetType();
                ulong senderId = (ulong)(entryType.GetField("senderId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(entry) ?? 0UL);
                
                if (senderId != player.NetId)
                    continue;

                object? completionSource = entryType.GetField("completionSource", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(entry);
                if (completionSource?.GetType().GetProperty("Task")?.GetValue(completionSource) is not Task<NetPlayerChoiceResult> task
                    || !task.IsCompletedSuccessfully)
                    continue;

                PlayerChoiceResult candidate = PlayerChoiceResult.FromNetData(player, runState, task.Result);
                if (!isExpected(candidate))
                    continue;

                choiceId = (uint)(entryType.GetField("choiceId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(entry) ?? uint.MaxValue);
                result = candidate;
                receivedChoices.RemoveAt(i);
                return true;
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[CardSelectionSync] 读取缓存预期选择失败: player={player.NetId} error={ex}");
        }
        return false;
    }

    private static bool IsExpectedNetChoice(Player player, RunState runState, NetPlayerChoiceResult netResult, Func<PlayerChoiceResult, bool> isExpected)
    {
        try
        {
            return isExpected(PlayerChoiceResult.FromNetData(player, runState, netResult));
        }
        catch
        {
            return false;
        }
    }

    private static PlayerChoiceResult CreateCardSelectionResult(int selectedIndex)
    {
        var result = new PlayerChoiceResult();
        var choiceTypeField = typeof(PlayerChoiceResult).GetField("_choiceType", BindingFlags.Instance | BindingFlags.NonPublic);
        var payloadField = typeof(PlayerChoiceResult).GetField("_payload", BindingFlags.Instance | BindingFlags.NonPublic);
        
        if (choiceTypeField != null)
            choiceTypeField.SetValue(result, "RedAlert2ModCardSelection");
        
        if (payloadField != null)
            payloadField.SetValue(result, selectedIndex.ToString());
        
        return result;
    }

    private static PlayerChoiceResult CreateMultiCardSelectionResult(List<int> selectedIndices)
    {
        var result = new PlayerChoiceResult();
        var choiceTypeField = typeof(PlayerChoiceResult).GetField("_choiceType", BindingFlags.Instance | BindingFlags.NonPublic);
        var payloadField = typeof(PlayerChoiceResult).GetField("_payload", BindingFlags.Instance | BindingFlags.NonPublic);
        
        if (choiceTypeField != null)
            choiceTypeField.SetValue(result, "RedAlert2ModMultiCardSelection");
        
        if (payloadField != null)
            payloadField.SetValue(result, string.Join(",", selectedIndices));
        
        return result;
    }

    private static bool IsCardSelectionResult(PlayerChoiceResult result)
    {
        var choiceTypeField = typeof(PlayerChoiceResult).GetField("_choiceType", BindingFlags.Instance | BindingFlags.NonPublic);
        var choiceType = choiceTypeField?.GetValue(result) as string;
        return choiceType == "RedAlert2ModCardSelection";
    }

    private static bool IsMultiCardSelectionResult(PlayerChoiceResult result)
    {
        var choiceTypeField = typeof(PlayerChoiceResult).GetField("_choiceType", BindingFlags.Instance | BindingFlags.NonPublic);
        var choiceType = choiceTypeField?.GetValue(result) as string;
        return choiceType == "RedAlert2ModMultiCardSelection";
    }

    private static CardModel? ResolveRemoteCardChoice(List<CardModel> cards, PlayerChoiceResult remoteChoice)
    {
        try
        {
            var payloadField = typeof(PlayerChoiceResult).GetField("_payload", BindingFlags.Instance | BindingFlags.NonPublic);
            var payload = payloadField?.GetValue(remoteChoice) as string;
            
            if (int.TryParse(payload, out int selectedIndex) && selectedIndex >= 0 && selectedIndex < cards.Count)
            {
                return cards[selectedIndex];
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[CardSelectionSync] 解析远程选择失败: error={ex}");
        }
        return cards.FirstOrDefault();
    }

    private static List<CardModel> ResolveRemoteMultiCardChoice(List<CardModel> cards, PlayerChoiceResult remoteChoice)
    {
        List<CardModel> result = new();
        try
        {
            var payloadField = typeof(PlayerChoiceResult).GetField("_payload", BindingFlags.Instance | BindingFlags.NonPublic);
            var payload = payloadField?.GetValue(remoteChoice) as string;
            
            if (!string.IsNullOrEmpty(payload))
            {
                var indices = payload.Split(',')
                    .Select(s => int.TryParse(s, out int i) ? i : -1)
                    .Where(i => i >= 0 && i < cards.Count)
                    .Distinct();
                
                foreach (int index in indices)
                {
                    result.Add(cards[index]);
                }
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[CardSelectionSync] 解析远程多选失败: error={ex}");
        }
        
        if (result.Count == 0 && cards.Count > 0)
            result.Add(cards[0]);
        
        return result;
    }

    private class PlayerChoiceReceivedHandler
    {
        private readonly ulong _expectedPlayerNetId;
        private readonly uint _expectedChoiceId;
        private readonly Func<PlayerChoiceResult, bool> _isExpected;
        private readonly Player _player;
        private readonly RunState _runState;
        private readonly TaskCompletionSource<(uint ChoiceId, NetPlayerChoiceResult Result)> _completion;

        public PlayerChoiceReceivedHandler(ulong expectedPlayerNetId, uint expectedChoiceId, 
            Func<PlayerChoiceResult, bool> isExpected, Player player, RunState runState,
            TaskCompletionSource<(uint ChoiceId, NetPlayerChoiceResult Result)> completion)
        {
            _expectedPlayerNetId = expectedPlayerNetId;
            _expectedChoiceId = expectedChoiceId;
            _isExpected = isExpected;
            _player = player;
            _runState = runState;
            _completion = completion;
        }

        public void OnReceived(object receivedPlayer, uint receivedChoiceId, NetPlayerChoiceResult result)
        {
            if (receivedPlayer is Player p && p.NetId != _expectedPlayerNetId)
                return;
            
            if (receivedChoiceId == _expectedChoiceId || IsExpectedNetChoice(_player, _runState, result, _isExpected))
            {
                _completion.TrySetResult((receivedChoiceId, result));
            }
        }
    }
}
