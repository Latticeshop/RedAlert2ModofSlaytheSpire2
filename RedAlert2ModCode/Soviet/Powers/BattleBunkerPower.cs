#nullable enable

using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RedAlert2ModCode.Soviet.Powers;

public sealed class BattleBunkerPower : PowerModel
{
	private static readonly MethodInfo? CardOnPlayMethod = typeof(CardModel).GetMethod("OnPlay", BindingFlags.NonPublic | BindingFlags.Instance);

	public override PowerType Type => PowerType.Buff;
    
	public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

	public bool IsUpgraded { get; set; } = false;

	private List<CardModel> _storedCards = new();

	private bool _isFirstTurn = true;

	public BattleBunkerPower()
	{
	}

	protected override void DeepCloneFields()
	{
		base.DeepCloneFields();
		_storedCards = new List<CardModel>(_storedCards);
		_isFirstTurn = true;
	}

	public override LocString Description
	{
		get
		{
			var locString = new LocString("powers", base.Id.Entry + ".description");
			if (_storedCards.Count > 0)
			{
				string storedCardNames = string.Join(", ", _storedCards.Select(c => c.Title));
				locString.Add("StoredCards", storedCardNames);
			}
			else
			{
				locString.Add("StoredCards", "-");
			}
			return locString;
		}
	}

	public static async Task ApplyBattleBunker(Creature owner, bool isUpgraded = false, List<CardModel>? storedCards = null)
    {
		var newPower = await PowerCmd.Apply<BattleBunkerPower>(new ThrowingPlayerChoiceContext(), owner, 1m, owner, null);
		if (newPower != null)
		{
			newPower.IsUpgraded = isUpgraded;
			if (storedCards != null)
			{
				foreach (var card in storedCards)
				{
					newPower._storedCards.Add(card);
				}
			}
			newPower._isFirstTurn = false;
		}
	}

	public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		if (side != CombatSide.Player)
			return;

		if (_isFirstTurn)
		{
			_storedCards.Clear();
			_isFirstTurn = false;
			return;
		}

		if (_storedCards == null || _storedCards.Count == 0)
			return;

		if (CardOnPlayMethod == null)
			return;

		var enemies = combatState.Enemies.Where(e => e.Side == CombatSide.Enemy && e.IsAlive).ToList();
		var rng = Owner?.Player?.RunState?.Rng?.CombatCardSelection;

		int stacks = (int)base.Amount;
		for (int i = 0; i < stacks; i++)
		{
			foreach (var storedCard in _storedCards)
			{
				if (storedCard == null)
					continue;

				Creature? target = null;
				if (storedCard.TargetType == TargetType.AnyEnemy)
				{
					if (enemies.Count > 0)
					{
						var randomIndex = rng?.NextInt(enemies.Count) ?? GD.RandRange(0, enemies.Count - 1);
						target = enemies[randomIndex];
					}
					else
					{
						continue;
					}
				}
				else if (storedCard.TargetType == TargetType.AllEnemies)
				{
					if (enemies.Count == 0)
					{
						continue;
					}
				}
				else if (storedCard.TargetType == TargetType.Self)
				{
					target = Owner;
				}

				var cardPlay = new CardPlay
				{
					Card = storedCard,
					Target = target,
					ResultPile = PileType.Discard,
					Resources = new ResourceInfo
					{
						EnergySpent = 0,
						EnergyValue = 0,
						StarsSpent = 0,
						StarValue = 0
					},
					IsAutoPlay = true,
					PlayIndex = 0,
					PlayCount = 1
				};
				var task = (Task)CardOnPlayMethod.Invoke(storedCard, new object[] { new ThrowingPlayerChoiceContext(), cardPlay })!;
				await task;
			}
		}
	}
}