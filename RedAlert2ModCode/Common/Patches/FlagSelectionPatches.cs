using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
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
		bool changed = false;
		foreach (Player player in players)
		{
			if (FlagManager.PlayerHasAnyFlag(player))
			{
				continue;
			}

			if (!RedAlert2ModCode.UI.MultiplayerSyncHelper.IsLocalPlayer(player))
			{
				continue;
			}

			FlagManager.Faction faction = FlagManager.GetPlayerFaction(player);

			if (faction == FlagManager.Faction.Yuri)
			{
				RelicModel yuriFlag = FlagManager.GetAllFlags(FlagManager.Faction.Yuri)[0];
				await RelicCmd.Obtain(yuriFlag.ToMutable(), player);
				changed = true;
				continue;
			}

			RelicModel? selected = await SelectFlagWithLocalScreen(faction);
			if (selected == null)
			{
				changed = true;
				continue;
			}

			await RelicCmd.Obtain(selected.ToMutable(), player);
			changed = true;
		}

		return changed;
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
