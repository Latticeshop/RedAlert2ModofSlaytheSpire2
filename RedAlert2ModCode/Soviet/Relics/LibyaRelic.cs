#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;
using Godot;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.Soviet.Cards;

namespace RedAlert2ModCode.Soviet.Relics;

public sealed class LibyaRelic : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Starter;

	public override bool HasUponPickupEffect => false;

	public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (target != Owner.Creature || result.UnblockedDamage <= 0)
			return;

		var combatState = Owner.Creature.CombatState;
		if (combatState == null)
			return;

		var demolitionTruck = FindDemolitionTruckInPiles();
		if (demolitionTruck == null)
			return;

		await TriggerDemolitionTruck(demolitionTruck, combatState, choiceContext);
	}

	private CardModel? FindDemolitionTruckInPiles()
	{
		var player = Owner;

		var deckCards = PileType.Deck.GetPile(player).Cards;
		var handCards = PileType.Hand.GetPile(player).Cards;
		var discardCards = PileType.Discard.GetPile(player).Cards;

		var allCards = deckCards.Concat(handCards).Concat(discardCards).ToList();

		return allCards.FirstOrDefault(c => c is DemolitionTruckCard);
	}

	private async Task TriggerDemolitionTruck(CardModel truckCard, ICombatState combatState, PlayerChoiceContext choiceContext)
	{
		var player = Owner;

		if (!PileType.Hand.GetPile(player).Cards.Contains(truckCard))
		{
			await CardPileCmd.RemoveFromCombat(truckCard);
			await CardPileCmd.Add(truckCard, PileType.Hand);
			GD.Print($"[LibyaRelic] 自爆卡车从其他牌堆移动到手牌");
		}

		GD.Print($"[LibyaRelic] 受到未格挡伤害，自动打出自爆卡车");

		PlayExplosionSound();
		UnitVoiceHelper.PlayUnitVoice(typeof(DemolitionTruckCard), "Soviet");

		int damage = 5;
		int poisonAmount = truckCard.IsUpgraded ? 15 : 10;

		var allEnemies = combatState.HittableEnemies.ToList();
		foreach (var enemy in allEnemies)
		{
			await CreatureCmd.Damage(choiceContext, new List<Creature> { enemy },
				(decimal)damage, ValueProp.Move, Owner.Creature, truckCard);

			await PowerCmd.Apply<MegaCrit.Sts2.Core.Models.Powers.PoisonPower>(choiceContext, enemy, (decimal)poisonAmount, Owner.Creature, truckCard);
		}

		var allPlayers = combatState.PlayerCreatures.ToList();
		foreach (var playerCreature in allPlayers)
		{
			await CreatureCmd.Damage(choiceContext, new List<Creature> { playerCreature },
				(decimal)damage, ValueProp.Move, Owner.Creature, truckCard);
		}

		await CardPileCmd.Add(truckCard, PileType.Exhaust);
	}

	private void PlayExplosionSound()
	{
		try
		{
			var audioPlayer = new AudioStreamPlayer();
			audioPlayer.Name = "LibyaRelicExplosion";
			var root = Engine.GetMainLoop() as SceneTree;
			if (root != null)
			{
				root.Root.AddChild(audioPlayer);
				var soundFile = GD.Load<AudioStream>("res://RedAlert2ModResources/audio/SovietUnits/DemolitionTruck/Vdemdiea_explosion.mp3");
				if (soundFile != null)
				{
					audioPlayer.Stream = soundFile;
					audioPlayer.VolumeDb = -5;
					audioPlayer.Play();
				}
			}
		}
		catch (Exception ex)
		{
			GD.PrintErr($"[LibyaRelic] 播放爆炸音效失败: {ex.Message}");
		}
	}
}