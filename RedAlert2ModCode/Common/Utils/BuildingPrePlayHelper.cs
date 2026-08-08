// 小格子铺 | Latticeshop
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Runs;
using RedAlert2ModCode.Allies.Cards;
using RedAlert2ModCode.Common.Cards;
using RedAlert2ModCode.Common.GameActions;
using RedAlert2ModCode.Soviet.Cards;
using RedAlert2ModCode.UI;

namespace RedAlert2ModCode.Common.Utils;

/// <summary>
/// A2 预选模式：打出卡牌时不立即出手，而是先弹本地选择面板；
/// 确认后入队“打出动作 + 结算动作”，取消则什么都不做（卡牌仍在手牌）。
/// 完全绕开原版暂停/恢复与出牌队列视觉舞步，消除取消回手的节点崩溃。
/// </summary>
public static class BuildingPrePlayHelper
{
    /// <summary>
    /// 手动 A2 确认后、结算动作执行前的“待结算”标记（按卡牌实例，仅本机需要）。
    /// OnPlay 读到该标记说明效果已交给结算动作，不再重复开面板。
    /// </summary>
    private static readonly ConditionalWeakTable<CardModel, object> _pendingResolution = new();

    public static void MarkPendingResolution(CardModel card)
    {
        _pendingResolution.Remove(card);
        _pendingResolution.Add(card, new object());
    }

    public static bool TryConsumePendingResolution(CardModel card)
    {
        if (card != null && _pendingResolution.TryGetValue(card, out _))
        {
            _pendingResolution.Remove(card);
            return true;
        }
        return false;
    }

    public static bool IsA2Card(CardModel card)
    {
        return card is AlliesBarracksCard
            or SovietBarracksCard
            or AlliedWarFactory
            or SovietWarFactory
            or AlliesShipyardCard
            or SovietShipyardCard
            or AirForceCommand
            or AlliedMCV
            or SovietMCV
            or SellBuildingCard;
    }

    /// <summary>点击手牌时调用：打开预选面板（fire-and-forget）。</summary>
    public static void OpenPrePlayPanel(CardModel card)
    {
        _ = OpenPanelAsync(card, cardAlreadyPlayed: false);
    }

    /// <summary>
    /// 自动打出兜底：卡牌已被自动打出且没有结算动作，补开预选面板；
    /// 确认后只入队结算动作（不再重复打出）。
    /// </summary>
    public static void OpenAutoPlayPanel(CardModel card)
    {
        _ = OpenPanelAsync(card, cardAlreadyPlayed: true);
    }

    private static async Task OpenPanelAsync(CardModel card, bool cardAlreadyPlayed)
    {
        try
        {
            var player = card.Owner;
            if (player == null || player.Creature?.CombatState == null)
                return;
            if (!cardAlreadyPlayed && !IsPlayableNow(card))
                return;

            switch (card)
            {
                case AlliesBarracksCard:
                    await ShowProductionPanel(card, FactionType.Allied,
                        AlliesBarracksCard.GetPrePlayCandidates(player, card.IsUpgraded),
                        AlliesCardValues.CreateSoldierValuesMap(), cardAlreadyPlayed);
                    break;
                case SovietBarracksCard:
                    await ShowProductionPanel(card, FactionType.Soviet,
                        SovietBarracksCard.GetPrePlayCandidates(player, card.IsUpgraded),
                        SovietCardValues.CreateSoldierValuesMap(), cardAlreadyPlayed);
                    break;
                case AlliedWarFactory:
                    await ShowProductionPanel(card, FactionType.Allied,
                        AlliedWarFactory.GetPrePlayCandidates(player, card.IsUpgraded),
                        BuildMergedAlliedVehicleValues(), cardAlreadyPlayed);
                    break;
                case SovietWarFactory:
                    await ShowProductionPanel(card, FactionType.Soviet,
                        SovietWarFactory.GetPrePlayCandidates(player, card.IsUpgraded),
                        SovietCardValues.CreateVehicleValuesMap(), cardAlreadyPlayed);
                    break;
                case AlliesShipyardCard:
                    await ShowProductionPanel(card, FactionType.Allied,
                        AlliesShipyardCard.GetPrePlayCandidates(player, card.IsUpgraded),
                        AlliesCardValues.CreateShipValuesMap(), cardAlreadyPlayed);
                    break;
                case SovietShipyardCard:
                    await ShowProductionPanel(card, FactionType.Soviet,
                        SovietShipyardCard.GetPrePlayCandidates(player, card.IsUpgraded),
                        SovietCardValues.CreateShipValuesMap(), cardAlreadyPlayed);
                    break;
                case AirForceCommand:
                    await ShowProductionPanel(card, FactionType.Allied,
                        AirForceCommand.GetPrePlayCandidates(player, card.IsUpgraded),
                        AlliesCardValues.CreateAircraftValuesMap(), cardAlreadyPlayed);
                    break;
                case AlliedMCV:
                    await ShowSinglePanel(card, FactionType.Allied,
                        AlliedMCV.GetPrePlayCandidates(player, card.IsUpgraded),
                        AlliesCardValues.CreateBuildingValuesMap(), cardAlreadyPlayed);
                    break;
                case SovietMCV:
                    await ShowSinglePanel(card, FactionType.Soviet,
                        SovietMCV.GetPrePlayCandidates(player, card.IsUpgraded),
                        SovietCardValues.CreateBuildingValuesMap(), cardAlreadyPlayed);
                    break;
                case SellBuildingCard:
                    await ShowSellPanel(card, cardAlreadyPlayed);
                    break;
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[BuildingPrePlay] 预选面板失败: {ex}");
        }
    }

    private static async Task ShowProductionPanel(
        CardModel card,
        FactionType faction,
        List<CardModel> candidates,
        Dictionary<string, CardValueStore.CardValues> valuesMap,
        bool cardAlreadyPlayed)
    {
        if (candidates.Count == 0)
            return; // 无可选单位：不出手也不弹空面板

        var selected = await CardSelectionScreen.ShowSelectionWithQuantity(candidates, card.Owner, valuesMap, faction);
        if (selected == null)
            return; // 取消：什么都不做，卡牌留在手牌

        var entries = new List<string>();
        var counts = new List<int>();
        foreach (var r in selected)
        {
            entries.Add(r.Card.Id.Entry);
            counts.Add(r.Count);
        }
        EnqueuePlay(card, entries.ToArray(), counts.ToArray(), cardAlreadyPlayed);
    }

    private static async Task ShowSinglePanel(
        CardModel card,
        FactionType faction,
        List<CardModel> candidates,
        Dictionary<string, CardValueStore.CardValues> valuesMap,
        bool cardAlreadyPlayed)
    {
        if (candidates.Count == 0)
            return;

        var selectedCard = await CardSelectionScreen.ShowSelection(candidates, card.Owner, valuesMap, faction);
        if (selectedCard == null)
            return; // 取消

        EnqueuePlay(card, new[] { selectedCard.Id.Entry }, new[] { 1 }, cardAlreadyPlayed);
    }

    private static async Task ShowSellPanel(CardModel card, bool cardAlreadyPlayed)
    {
        var items = SellBuildingCard.GetPrePlayCandidates(card.Owner);
        if (items.Count == 0)
            return;

        int maxSelection = card.IsUpgraded ? 99 : (int)CommonCardValues.SellBuilding.Repeat;
        FactionType faction = (card.Owner.Character?.Id.Entry?.Contains("SOVIET") ?? false)
            ? FactionType.Soviet
            : FactionType.Allied;

        var result = await SellBuildingScreen.ShowSelection(items, maxSelection, card.Owner, faction);
        if (result == null)
            return; // 取消

        var entries = new List<string>();
        var counts = new List<int>();
        foreach (var item in result.Items)
        {
            entries.Add(item.Power.GetType().Name);
            counts.Add(item.SelectedCount);
        }
        EnqueuePlay(card, entries.ToArray(), counts.ToArray(), cardAlreadyPlayed);
    }

    /// <summary>
    /// 确认后入队：先正常打出（PlayCardAction），再入队结算动作（携带选择结果）。
    /// 两个动作按顺序由主机裁定入队，所有端确定性执行。
    /// </summary>
    private static void EnqueuePlay(CardModel card, string[] entries, int[] counts, bool cardAlreadyPlayed)
    {
        if (RunManager.Instance?.ActionQueueSynchronizer == null)
            return;

        if (!cardAlreadyPlayed)
        {
            if (!IsPlayableNow(card))
                return;

            // 必须先打标记再入队打出动作：动作队列可能同步立刻执行 OnPlay，
            // 若标记晚于 OnPlay 设置，OnPlay 会误走“自动打出兜底”再次开面板，造成重复结算。
            MarkPendingResolution(card);

            if (!card.TryManualPlay(null))
            {
                TryConsumePendingResolution(card);
                GD.PrintErr($"[BuildingPrePlay] 打出动作入队失败: {card.Id.Entry}");
                return;
            }
        }

        RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(
            new BuildingResolutionAction(card.Owner, card.Id.Entry, card.IsUpgraded, entries, counts));
        GD.Print($"[BuildingPrePlay] 已入队 打出+结算: {card.Id.Entry}");
    }

    private static bool IsPlayableNow(CardModel card)
    {
        if (card.Owner == null)
            return false;
        if (!PileType.Hand.GetPile(card.Owner).Cards.Contains(card))
            return false;
        return card.CanPlay(out _, out _);
    }

    private static Dictionary<string, CardValueStore.CardValues> BuildMergedAlliedVehicleValues()
    {
        var map = AlliesCardValues.CreateVehicleValuesMap();
        foreach (var kvp in AlliesCardValues.CreateHighTechValuesMap())
            map[kvp.Key] = kvp.Value;
        return map;
    }
}
