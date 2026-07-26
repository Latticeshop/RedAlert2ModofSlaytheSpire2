using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;
using RedAlert2ModCode.Common.UI;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Common.Patches;

[HarmonyPatch]
public static class FlagSelectionPatches
{
	private static bool _selectionInProgress;

	private readonly record struct PendingFlagSelection(Player Player, List<RelicModel> Options, uint ChoiceId, bool IsLocal);

	private static MethodInfo RequireMethod(Type type, string name, BindingFlags flags, params Type[] parameters)
	{
		return type.GetMethod(name, flags, binder: null, parameters, modifiers: null)
			?? throw new InvalidOperationException($"Could not find required method {type.FullName}.{name}.");
	}

	public static void Install(Harmony harmony)
	{
		harmony.Patch(
			RequireMethod(typeof(NGame), "StartRun", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, typeof(RunState)),
			postfix: new HarmonyMethod(typeof(FlagSelectionPatches), nameof(StartRunPostfix)));
	}

	private static void StartRunPostfix(RunState runState, ref Task __result)
	{
		// 不要修改 __result，让原始任务正常完成
		// 将国旗选择作为独立任务运行，避免阻塞游戏开始流程
		_ = StartRunAfterOriginal(__result, runState);
	}

	private static async Task StartRunAfterOriginal(Task original, RunState runState)
	{
		await original;
		await EnsureFlagsSelectedForRun(runState);
	}

	private static async Task EnsureFlagsSelectedForRun(RunState runState)
	{
		if (_selectionInProgress)
		{
			Log.Info("[RedAlert2Mod] Flag selection skipped: another selection is in progress.");
			return;
		}

		_selectionInProgress = true;
		try
		{
			NetGameType gameType = RunManager.Instance?.NetService?.Type ?? NetGameType.Singleplayer;
			GD.Print($"[RedAlert2Mod] Flag selection: gameType={gameType}, players={runState.Players.Count}");
			if (gameType is NetGameType.Singleplayer or NetGameType.None)
			{
				foreach (Player player in runState.Players)
				{
					await EnsureFlagSelected(player);
				}
			}
			else
			{
				await EnsureFlagsSelectedMultiplayer(runState.Players.ToList());
			}
		}
		catch (Exception ex)
		{
			GD.PrintErr($"[RedAlert2Mod] Flag selection error: {ex}");
		}
		finally
		{
			_selectionInProgress = false;
		}
	}

	private static async Task<bool> EnsureFlagSelected(Player player)
	{
		bool hasFlag = FlagManager.PlayerHasAnyFlag(player);
		GD.Print($"[RedAlert2Mod] EnsureFlagSelected: player={player?.Character?.Id?.Entry}, hasAnyFlag={hasFlag}");

		if (hasFlag)
		{
			GD.Print("[RedAlert2Mod] EnsureFlagSelected: player already has a flag, skipping.");
			return false;
		}

		FlagManager.Faction faction = FlagManager.GetPlayerFaction(player);
		GD.Print($"[RedAlert2Mod] EnsureFlagSelected: detected faction={faction}");

		if (faction == FlagManager.Faction.None)
		{
			GD.Print("[RedAlert2Mod] EnsureFlagSelected: not a RA2 character, skipping flag selection.");
			return false;
		}

		if (faction == FlagManager.Faction.Yuri)
		{
			RelicModel yuriFlag = FlagManager.GetAllFlags(FlagManager.Faction.Yuri)[0];
			GD.Print($"[RedAlert2Mod] Yuri faction: obtaining Yuri flag (mutable)...");
			await RelicCmd.Obtain(yuriFlag.ToMutable(), player);
			GD.Print("[RedAlert2Mod] Yuri faction: automatically granted Yuri flag.");
			return true;
		}

		GD.Print($"[RedAlert2Mod] Opening flag selection screen for faction={faction}...");
		RelicModel? selected = await SelectFlagWithLocalScreen(faction);
		GD.Print($"[RedAlert2Mod] Flag selection result: {(selected == null ? "null/skipped" : selected.Title.GetFormattedText())}");

		if (selected == null)
		{
			GD.Print("[RedAlert2Mod] Flag selection skipped.");
			return true;
		}

		GD.Print($"[RedAlert2Mod] About to call RelicCmd.Obtain (mutable) for: {selected.Title.GetFormattedText()}");
		await RelicCmd.Obtain(selected.ToMutable(), player);
		GD.Print($"[RedAlert2Mod] RelicCmd.Obtain completed successfully: {selected.Title.GetFormattedText()}");
		return true;
	}

	private static async Task<bool> EnsureFlagsSelectedMultiplayer(IReadOnlyList<Player> players)
	{
		RunManager runManager = RunManager.Instance;

		// 按 NetId 排序，确保所有客户端顺序一致
		IReadOnlyList<Player> orderedPlayers = players
			.OrderBy(static player => player.NetId)
			.ToList();

		bool changed = false;

		// 等待 PlayerChoiceSynchronizer 就绪
		PlayerChoiceSynchronizer? synchronizer = await WaitForPlayerChoiceSynchronizerAsync(runManager);
		if (synchronizer == null)
		{
			// 如果没有同步器，退回到单机逻辑
			foreach (Player player in orderedPlayers)
			{
				changed |= await EnsureFlagSelected(player);
			}
			return changed;
		}

		// 收集所有需要选择的玩家
		List<PendingFlagSelection> pendingSelections = new();
		foreach (Player player in orderedPlayers)
		{
			if (FlagManager.PlayerHasAnyFlag(player))
			{
				continue;
			}

			FlagManager.Faction faction = FlagManager.GetPlayerFaction(player);

			if (faction == FlagManager.Faction.None)
			{
				GD.Print($"[RedAlert2Mod] Multiplayer: player {player.NetId} is not a RA2 character, skipping.");
				continue;
			}

			// 尤里阵营自动获得尤里国旗
			if (faction == FlagManager.Faction.Yuri)
			{
				RelicModel yuriFlag = FlagManager.GetAllFlags(FlagManager.Faction.Yuri)[0];
				await RelicCmd.Obtain(yuriFlag.ToMutable(), player);
				changed = true;
				continue;
			}

			List<RelicModel> options = FlagManager.GetAllFlags(faction);
			uint choiceId = synchronizer.ReserveChoiceId(player);
			bool isLocal = IsLocalPlayer(runManager, player);
			pendingSelections.Add(new PendingFlagSelection(player, options, choiceId, isLocal));
			GD.Print($"[RedAlert2Mod] Multiplayer: reserved choiceId={choiceId} for player={player.NetId}, isLocal={isLocal}, faction={faction}");
		}

		if (pendingSelections.Count == 0)
		{
			return changed;
		}

		// 分离本地玩家和远程玩家的选择
		List<PendingFlagSelection> localSelections = pendingSelections.Where(s => s.IsLocal).ToList();
		List<PendingFlagSelection> remoteSelections = pendingSelections.Where(s => !s.IsLocal).ToList();

		GD.Print($"[RedAlert2Mod] Multiplayer: {localSelections.Count} local selections, {remoteSelections.Count} remote selections");

		// 同时启动所有选择任务（本地玩家并行处理UI）
		List<FlagSelectionScreen> localScreens = new();
		try
		{
			Task<(PendingFlagSelection, RelicModel?)>[] selectionTasks = pendingSelections
				.Select(selection => SelectFlagMultiplayer(selection, synchronizer, localScreens, runManager))
				.ToArray();

			// 等待所有玩家选择完成
			(PendingFlagSelection, RelicModel?)[] selectedResults = await Task.WhenAll(selectionTasks);

			for (int i = 0; i < selectedResults.Length; i++)
			{
				var (selection, selectedFlag) = selectedResults[i];

				if (selectedFlag != null)
				{
					await RelicCmd.Obtain(selectedFlag.ToMutable(), selection.Player);
					changed = true;
					GD.Print($"[RedAlert2Mod] Multiplayer flag obtained: player={selection.Player.NetId} flag={selectedFlag.Title.GetFormattedText()}");
				}
				else
				{
					GD.Print($"[RedAlert2Mod] Multiplayer flag selection skipped: player={selection.Player.NetId}");
				}
			}
		}
		finally
		{
			foreach (FlagSelectionScreen screen in localScreens)
			{
				if (GodotObject.IsInstanceValid(screen))
				{
					screen.CloseSelectionScreen();
				}
			}
		}

		return changed;
	}

	private static async Task<(PendingFlagSelection, RelicModel?)> SelectFlagMultiplayer(
		PendingFlagSelection selection,
		PlayerChoiceSynchronizer synchronizer,
		ICollection<FlagSelectionScreen> localScreens,
		RunManager runManager)
	{
		if (selection.IsLocal)
		{
			// 本地玩家：显示选择UI
			FlagManager.Faction faction = FlagManager.GetPlayerFaction(selection.Player);
			FlagSelectionScreen screen = await CreateFlagSelectionScreenAsync(faction);
			localScreens.Add(screen);

			RelicModel? selectedFlag = await screen.FlagSelected();
			int selectedIndex = selectedFlag != null
				? selection.Options.FindIndex(f => f.Id == selectedFlag.Id)
				: -1;

			// 同步选择结果（带连接状态检查）
			if (IsNetServiceConnected(runManager))
			{
				synchronizer.SyncLocalChoice(selection.Player, selection.ChoiceId, PlayerChoiceResult.FromIndex(selectedIndex));
				GD.Print($"[RedAlert2Mod] Multiplayer: synced choiceId={selection.ChoiceId}, index={selectedIndex} for player={selection.Player.NetId}");
			}
			else
			{
				GD.Print($"[RedAlert2Mod] Multiplayer: skipped syncing choice (not connected) for player={selection.Player.NetId}");
			}
			return (selection, selectedFlag);
		}
		else
		{
			// 远程玩家：等待同步
			try
			{
				PlayerChoiceResult remoteChoice = await synchronizer.WaitForRemoteChoice(selection.Player, selection.ChoiceId);
				int index = remoteChoice.AsIndex();
				RelicModel? result = index >= 0 && index < selection.Options.Count ? selection.Options[index] : null;
				GD.Print($"[RedAlert2Mod] Multiplayer: received remote choice for player={selection.Player.NetId}, choiceId={selection.ChoiceId}, index={index}");
				return (selection, result);
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[RedAlert2Mod] Multiplayer remote selection error for player={selection.Player.NetId}: {ex}");
				return (selection, null);
			}
		}
	}

	private static bool IsNetServiceConnected(RunManager runManager)
	{
		try
		{
			if (runManager.NetService == null)
				return false;

			// 检查 NetService 的连接状态
			PropertyInfo? isConnectedProp = runManager.NetService.GetType().GetProperty("IsConnected");
			if (isConnectedProp != null)
			{
				object? value = isConnectedProp.GetValue(runManager.NetService);
				if (value is bool connected)
					return connected;
			}

			return true;
		}
		catch
		{
			return false;
		}
	}

	private static async Task<PlayerChoiceSynchronizer?> WaitForPlayerChoiceSynchronizerAsync(RunManager runManager)
	{
		for (int i = 0; i < 60; i++)
		{
			if (runManager.PlayerChoiceSynchronizer != null)
			{
				return runManager.PlayerChoiceSynchronizer;
			}
			await Task.Yield();
		}
		return runManager.PlayerChoiceSynchronizer;
	}

	private static bool IsLocalPlayer(RunManager runManager, Player player)
	{
		return player.NetId != 0UL && player.NetId == runManager.NetService.NetId;
	}

	private static async Task<RelicModel?> SelectFlagWithLocalScreen(FlagManager.Faction faction)
	{
		FlagSelectionScreen screen = await CreateFlagSelectionScreenAsync(faction);
		return await screen.FlagSelected();
	}

	private static async Task<FlagSelectionScreen> CreateFlagSelectionScreenAsync(FlagManager.Faction faction)
	{
		for (int i = 0; i < 60; i++)
		{
			if (NOverlayStack.Instance != null)
			{
				break;
			}
			await Task.Yield();
		}

		FlagSelectionScreen selectionScreen = FlagSelectionScreen.Create(faction);

		if (NOverlayStack.Instance == null)
		{
			throw new InvalidOperationException("NOverlayStack is not available for flag selection.");
		}

		NOverlayStack.Instance.Push(selectionScreen);
		return selectionScreen;
	}
}
