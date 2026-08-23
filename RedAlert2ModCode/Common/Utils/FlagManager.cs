using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using RedAlert2ModCode.Allies.Relics;
using RedAlert2ModCode.Common.Relics;
using RedAlert2ModCode.Soviet.Relics;

namespace RedAlert2ModCode.Common.Utils;

public static class FlagManager
{
	public enum Faction
	{
		None,
		Allies,
		Soviet,
		Yuri
	}

	public static readonly List<Type> AlliedFlags = new()
	{
		typeof(USARelic),
		typeof(UKRelic),
		typeof(FranceRelic),
		typeof(GermanyRelic),
		typeof(SouthKoreaRelic),
	};

	public static readonly List<Type> SovietFlags = new()
	{
		typeof(USSRRelic),
		typeof(CubaRelic),
		typeof(IraqRelic),
		typeof(LibyaRelic),
	};

	public static readonly List<Type> YuriFlags = new()
	{
		typeof(YuriRelic),
	};

	public static Faction GetPlayerFaction(Player player)
	{
		return GetNativePlayerFaction(player);
	}

	/// <summary>
	/// 角色原生阵营（仅按角色判断，不受基地车模式覆盖影响）。
	/// 国旗选择等逻辑必须用原生阵营判断"同阵营/跨阵营"，避免 FactionPatch 把非RA2角色误判成基地车阵营。
	/// </summary>
	public static Faction GetNativePlayerFaction(Player player)
	{
		string? charId = player.Character?.Id?.Entry;
		GD.Print($"[RedAlert2Mod] GetPlayerFaction: charId={charId}");
		if (charId == null) return Faction.None;

		if (charId.Equals("Allies", StringComparison.OrdinalIgnoreCase) ||
		    charId.Contains("ALLIES", StringComparison.OrdinalIgnoreCase))
		{
			GD.Print($"[RedAlert2Mod] GetPlayerFaction: detected ALLIES");
			return Faction.Allies;
		}

		if (charId.Equals("Soviet", StringComparison.OrdinalIgnoreCase) ||
		    charId.Contains("SOVIET", StringComparison.OrdinalIgnoreCase))
		{
			GD.Print($"[RedAlert2Mod] GetPlayerFaction: detected SOVIET");
			return Faction.Soviet;
		}

		if (charId.Contains("YURI", StringComparison.OrdinalIgnoreCase))
		{
			GD.Print($"[RedAlert2Mod] GetPlayerFaction: detected YURI");
			return Faction.Yuri;
		}

		GD.Print($"[RedAlert2Mod] GetPlayerFaction: not a RA2 character, returning None");
		return Faction.None;
	}

	public static List<Type> GetFlagsForFaction(Faction faction)
	{
		return faction switch
		{
			Faction.Allies => AlliedFlags,
			Faction.Soviet => SovietFlags,
			Faction.Yuri => YuriFlags,
			_ => new List<Type>(),
		};
	}

	public static List<RelicModel> GetRandomFlags(Faction faction, int count)
	{
		List<Type> allFlags = GetFlagsForFaction(faction);
		List<Type> shuffled = allFlags.OrderBy(_ => Guid.NewGuid()).ToList();
		List<Type> selected = shuffled.Take(Math.Min(count, shuffled.Count)).ToList();

		List<RelicModel> result = new();
		foreach (Type flagType in selected)
		{
			result.Add(GetFlagRelic(flagType));
		}
		return result;
	}

	public static List<RelicModel> GetAllFlags(Faction faction)
	{
		List<Type> allFlags = GetFlagsForFaction(faction);
		List<RelicModel> result = new();
		foreach (Type flagType in allFlags)
		{
			result.Add(GetFlagRelic(flagType));
		}
		return result;
	}

	/// <summary>
	/// 为基地车模式选择一张联机同步的随机国旗。
	/// 使用游戏 RunState.Rng，并排除已拥有的同阵营旗帜。
	/// </summary>
	public static RelicModel? GetRandomFlag(Player player, Faction faction)
	{
		if (player == null) return null;

		List<RelicModel> candidates = GetAllFlags(faction)
			.Where(flag => !player.Relics.Any(existing => existing.GetType() == flag.GetType()))
			.ToList();
		if (candidates.Count == 0) return null;

		// 与教程中的联机随机写法一致：所有客户端使用同一条 RunState RNG 流，
		// 调用方按固定玩家顺序执行，确保各端消耗 RNG 的顺序完全一致。
		var rng = player.RunState?.Rng?.CombatCardSelection;
		if (rng == null) return null;
		return candidates[rng.NextInt(candidates.Count)];
	}

	public static bool PlayerHasAnyFlag(Player player)
	{
		var allFlags = AlliedFlags.Concat(SovietFlags).Concat(YuriFlags);
		return player.Relics.Any(r => allFlags.Contains(r.GetType()));
	}

	/// <summary>
	/// 玩家是否已拥有指定阵营的国旗（用于跨阵营基地车时补授另一阵营国旗）。
	/// </summary>
	public static bool PlayerHasFlag(Player player, Faction faction)
	{
		List<Type> flags = GetFlagsForFaction(faction);
		return player.Relics.Any(r => flags.Contains(r.GetType()));
	}

	public static bool HasFlag(Player player, Type flagType)
	{
		return player.Relics.Any(r => r.GetType() == flagType);
	}

	public static bool HasUSA(Player player) => HasFlag(player, typeof(USARelic));
	public static bool HasUK(Player player) => HasFlag(player, typeof(UKRelic));
	public static bool HasFrance(Player player) => HasFlag(player, typeof(FranceRelic));
	public static bool HasGermany(Player player) => HasFlag(player, typeof(GermanyRelic));
	public static bool HasSouthKorea(Player player) => HasFlag(player, typeof(SouthKoreaRelic));
	public static bool HasUSSR(Player player) => HasFlag(player, typeof(USSRRelic));
	public static bool HasCuba(Player player) => HasFlag(player, typeof(CubaRelic));
	public static bool HasIraq(Player player) => HasFlag(player, typeof(IraqRelic));
	public static bool HasLibya(Player player) => HasFlag(player, typeof(LibyaRelic));
	public static bool HasYuri(Player player) => HasFlag(player, typeof(YuriRelic));

	public static bool IsFlagRelic(RelicModel relic)
	{
		var type = relic.GetType();
		return AlliedFlags.Contains(type) || SovietFlags.Contains(type) || YuriFlags.Contains(type);
	}

	private static readonly MethodInfo _modelDbRelicMethod = typeof(ModelDb).GetMethod("Relic", 1, Type.EmptyTypes)
		?? throw new InvalidOperationException("Could not find ModelDb.Relic<T>() method.");

	public static RelicModel GetFlagRelic(Type flagType)
	{
		MethodInfo generic = _modelDbRelicMethod.MakeGenericMethod(flagType);
		return (RelicModel)generic.Invoke(null, null)!;
	}
}
