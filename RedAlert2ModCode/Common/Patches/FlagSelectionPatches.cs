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
using RedAlert2ModCode.DeckConfig;
using RedAlert2ModCode.Common.UI;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Common.Patches;

[HarmonyPatch]
public static class FlagSelectionPatches
{
	private static bool _selectionInProgress;
	// 本局已授予的国旗（玩家NetId, 阵营），防止重复轮/误判导致同一阵营国旗发两遍
	private static readonly HashSet<(ulong PlayerId, FlagManager.Faction Faction)> _grantedFlagsThisRun = new();

	private readonly record struct PendingFlagSelection(Player Player, List<RelicModel> Options, uint ChoiceId, bool IsLocal, FlagManager.Faction Faction);

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
		_grantedFlagsThisRun.Clear();
		try
		{
			NetGameType gameType = RunManager.Instance?.NetService?.Type ?? NetGameType.Singleplayer;
			GD.Print($"[RedAlert2Mod] Flag selection: gameType={gameType}, players={runState.Players.Count}");
			if (gameType is NetGameType.Singleplayer or NetGameType.None)
			{
				foreach (Player player in runState.Players)
				{
					await EnsureFlagSelected(player);
					// 基地车模式只自动加入对应阵营随机国旗，不插入第二个选择事件。
					await EnsureBaseCarFlagRandomly(player);
				}
			}
			else
			{
				// 强制房主配置：先等待房主整套配置同步到位（最多约 20 秒），
				// 否则客机在基地车模式随机国旗时拿不到房主配置。
				if (ModConfigManager.IsForceHostConfigEnabled)
				{
					await WaitForForcedHostConfigsAsync();
				}

				GD.Print("[RedAlert2Mod] Multiplayer native flag round...");
				await EnsureFlagsSelectedMultiplayer(runState.Players.ToList());
				// 基地车模式不再打开第二轮选择界面；各端按本局种子加入同一随机旗帜。
				GD.Print("[RedAlert2Mod] Multiplayer base-car random flag round...");
				foreach (Player player in runState.Players.OrderBy(static player => player.NetId))
					await EnsureBaseCarFlagRandomly(player);
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

	private static async Task WaitForForcedHostConfigsAsync()
	{
		DateTimeOffset deadline = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(20);
		while (DateTimeOffset.UtcNow < deadline)
		{
			if (ModConfigManager.HasForcedHostConfigs())
			{
				GD.Print("[RedAlert2Mod] 强制房主配置已同步，开始国旗选择");
				return;
			}
			await Task.Delay(100);
		}
		GD.Print("[RedAlert2Mod] 等待强制房主配置超时，国旗选择按当前可用配置进行");
	}

	/// <summary>
	/// 基地车模式：自动加入一张对应阵营的随机国旗。
	/// 不打开基地车专用选择面板，交给开局遗物队列串行加入，避免与 Neow 同时修改遗物栏。
	/// </summary>
	private static Task<bool> EnsureBaseCarFlagRandomly(Player player)
	{
		try
		{
			string? characterId = player?.Character?.Id?.Entry;
			if (string.IsNullOrEmpty(characterId)) return Task.FromResult(false);

			var baseFaction = GetBaseCarFaction(player);
			if (baseFaction == FlagManager.Faction.None) return Task.FromResult(false);

			// 跨阵营且已经拥有该阵营国旗时跳过；同阵营从剩余旗帜中随机一张。
			bool sameFaction = baseFaction == FlagManager.GetNativePlayerFaction(player);
			if (!sameFaction && FlagManager.PlayerHasFlag(player, baseFaction))
				return Task.FromResult(false);

			RelicModel? selected = FlagManager.GetRandomFlag(player, baseFaction);
			if (selected == null)
			{
				GD.Print($"[RedAlert2Mod] 基地车模式没有可加入的 {baseFaction} 国旗: player={player.NetId}");
				return Task.FromResult(false);
			}

			RelicModel mutable = selected.ToMutable();
			mutable.FloorAddedToDeck = 1;
			try { SaveManager.Instance.MarkRelicAsSeen(mutable); } catch { }
			StartingRelicPickupQueue.Enqueue(player, mutable);
			GD.Print($"[RedAlert2Mod] 基地车模式已排队加入随机国旗: player={player.NetId}, faction={baseFaction}, flag={selected.Title.GetFormattedText()}");
			return Task.FromResult(true);
		}
		catch (Exception ex)
		{
			GD.PrintErr($"[RedAlert2Mod] EnsureBaseCarFlagRandomly error: {ex}");
			return Task.FromResult(false);
		}
	}

	/// <summary>
	/// 获取基地车模式对应的阵营（None 表示未配置基地车）。
	/// </summary>
	private static FlagManager.Faction GetBaseCarFaction(Player player)
	{
		try
		{
			string? characterId = player?.Character?.Id?.Entry;
			if (string.IsNullOrEmpty(characterId)) return FlagManager.Faction.None;
			// 必须走 GetConfigForPlayer：强制房主配置开启时，客机也要用房主同步的基地车配置，
			// 而不是客机本地配置（否则客机本地未配置基地车时缺国旗选项事件）。
			var config = ModConfigManager.GetConfigForPlayer(player);
			if (config == null) return FlagManager.Faction.None;
			return config.BaseCarMode switch
			{
				BaseCarMode.Allied => FlagManager.Faction.Allies,
				BaseCarMode.Soviet => FlagManager.Faction.Soviet,
				BaseCarMode.Yuri => FlagManager.Faction.Yuri,
				_ => FlagManager.Faction.None,
			};
		}
		catch { return FlagManager.Faction.None; }
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

		FlagManager.Faction faction = FlagManager.GetNativePlayerFaction(player);
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
			if (FlagManager.PlayerHasAnyFlag(player)) continue;
			FlagManager.Faction faction = FlagManager.GetNativePlayerFaction(player);
			if (faction == FlagManager.Faction.None)
			{
				GD.Print($"[RedAlert2Mod] Multiplayer: player {player.NetId} is not a RA2 character, skipping.");
				continue;
			}

			// 本局同一阵营国旗只授一次。
			if (_grantedFlagsThisRun.Contains((player.NetId, faction)))
			{
				GD.Print($"[RedAlert2Mod] Multiplayer: player {player.NetId} already granted {faction} this run, skipping.");
				continue;
			}

			// 尤里阵营自动获得尤里国旗
			if (faction == FlagManager.Faction.Yuri)
			{
				if (FlagManager.PlayerHasFlag(player, faction)) continue;
				RelicModel yuriFlag = FlagManager.GetAllFlags(FlagManager.Faction.Yuri)[0];
				await RelicCmd.Obtain(yuriFlag.ToMutable(), player);
				_grantedFlagsThisRun.Add((player.NetId, faction));
				changed = true;
				continue;
			}

			List<RelicModel> options = FlagManager.GetAllFlags(faction);
			uint choiceId = synchronizer.ReserveChoiceId(player);
			bool isLocal = IsLocalPlayer(runManager, player);
			pendingSelections.Add(new PendingFlagSelection(player, options, choiceId, isLocal, faction));
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
					_grantedFlagsThisRun.Add((selection.Player.NetId, selection.Faction));
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
			// 必须使用 selection.Faction（可能是基地车阵营），不能取玩家原生阵营
			FlagSelectionScreen screen = await CreateFlagSelectionScreenAsync(selection.Faction);
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
