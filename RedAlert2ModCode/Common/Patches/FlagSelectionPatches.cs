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
		__result = StartRunAfterOriginal(__result, runState);
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
		}

		if (pendingSelections.Count == 0)
		{
			return changed;
		}

		// 同时启动所有选择任务
		List<FlagSelectionScreen> localScreens = new();
		try
		{
			Task<RelicModel?>[] selectionTasks = pendingSelections
				.Select(selection => SelectFlagMultiplayer(selection, synchronizer, localScreens))
				.ToArray();

			// 等待所有玩家选择完成
			RelicModel?[] selectedFlags = await Task.WhenAll(selectionTasks);

			for (int i = 0; i < pendingSelections.Count; i++)
			{
				PendingFlagSelection selection = pendingSelections[i];
				RelicModel? selectedFlag = selectedFlags[i];

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

	private static async Task<RelicModel?> SelectFlagMultiplayer(
		PendingFlagSelection selection,
		PlayerChoiceSynchronizer synchronizer,
		ICollection<FlagSelectionScreen> localScreens)
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

			// 同步选择结果
			synchronizer.SyncLocalChoice(selection.Player, selection.ChoiceId, PlayerChoiceResult.FromIndex(selectedIndex));
			return selectedFlag;
		}
		else
		{
			// 远程玩家：等待同步
			PlayerChoiceResult remoteChoice = await synchronizer.WaitForRemoteChoice(selection.Player, selection.ChoiceId);
			int index = remoteChoice.AsIndex();
			return index >= 0 && index < selection.Options.Count ? selection.Options[index] : null;
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
