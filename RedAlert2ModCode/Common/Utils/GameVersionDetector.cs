using System;
using System.Reflection;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace RedAlert2ModCode.Common.Utils;

public static class GameVersionDetector
{
	private static bool? _isBetaVersion;
	private static MethodInfo? _giveToAnotherPlayerCmdMethod;
	private static MethodInfo? _giveToAnotherPlayerCardMethod;
	private static MethodInfo? _getResultPileTypeAndPositionMethod;

	public static bool IsBetaVersion
	{
		get
		{
			if (!_isBetaVersion.HasValue)
			{
				_isBetaVersion = DetectBetaVersion();
			}
			return _isBetaVersion.Value;
		}
	}

	public static bool HasGiveToAnotherPlayer => _giveToAnotherPlayerCmdMethod != null;

	public static bool HasGetResultPileTypeAndPosition => _getResultPileTypeAndPositionMethod != null;

	private static bool DetectBetaVersion()
	{
		_giveToAnotherPlayerCmdMethod = typeof(CardPileCmd).GetMethod(
			"GiveToAnotherPlayer",
			BindingFlags.Public | BindingFlags.Static);

		_giveToAnotherPlayerCardMethod = typeof(CardModel).GetMethod(
			"GiveToAnotherPlayer",
			BindingFlags.Public | BindingFlags.Instance);

		_getResultPileTypeAndPositionMethod = typeof(CardModel).GetMethod(
			"GetResultPileTypeAndPositionForCardPlay",
			BindingFlags.NonPublic | BindingFlags.Instance);

		return _giveToAnotherPlayerCmdMethod != null &&
		       _giveToAnotherPlayerCardMethod != null &&
		       _getResultPileTypeAndPositionMethod != null;
	}

	public static async Task CallGiveToAnotherPlayer(
		CardModel card,
		Player player,
		PileType pileType,
		CardPilePosition position)
	{
		if (_giveToAnotherPlayerCmdMethod == null)
		{
			throw new NotSupportedException("GiveToAnotherPlayer is not available in this game version");
		}

		object[] parameters = new object[] { card, player, pileType, position, null };
		var result = _giveToAnotherPlayerCmdMethod.Invoke(null, parameters);

		if (result is Task task)
		{
			await task;
		}
	}

	public static void CallCardGiveToAnotherPlayer(CardModel card, Player player)
	{
		if (_giveToAnotherPlayerCardMethod == null)
		{
			var ownerField = typeof(CardModel).GetField("_owner",
				BindingFlags.NonPublic | BindingFlags.Instance);
			if (ownerField != null)
			{
				ownerField.SetValue(card, null);
			}
			card.Owner = player;
			return;
		}

		_giveToAnotherPlayerCardMethod.Invoke(card, new object[] { player });
	}
}
