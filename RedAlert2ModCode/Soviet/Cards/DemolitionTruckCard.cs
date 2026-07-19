#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.Soviet.Powers;

namespace RedAlert2ModCode.Soviet.Cards;

public sealed class DemolitionTruckCard : CardModel
{
	private static readonly CardValueStore.CardValues Values = SovietCardValues.DemolitionTruck;

	private bool _hasAttackIntent = false;

	public DemolitionTruckCard() : base((int)Values.Cost, CardType.Attack, CardRarity.Token, TargetType.AllEnemies) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/soviet/trkaicon.png";

	public override HashSet<CardKeyword> CanonicalKeywords => new HashSet<CardKeyword>
	{
		CardKeyword.Exhaust
	};

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new DamageVar(Values.Damage, ValueProp.Move),
		new IntVar("Poison", (int)Values.MagicNumber),
		new IntVar("DollarNumber", (int)Values.DollarValue)
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.TechLevelT2.CreateHoverTip(),
		ModCardKeywords.Vehicle.CreateHoverTip()
	];

	protected override void DeepCloneFields()
	{
		base.DeepCloneFields();
		_hasAttackIntent = false;
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		_hasAttackIntent = HasEnemyAttackIntent();

		UnitVoiceHelper.PlayUnitVoice(this.GetType(), "Soviet");

		var combatState = Owner.Creature.CombatState;
		if (combatState == null)
			return;

		int damage = (int)Values.Damage;
		int poisonAmount = IsUpgraded ? (int)(Values.MagicNumber + Values.MagicNumberUpgraded) : (int)Values.MagicNumber;

		var allEnemies = combatState.HittableEnemies.ToList();

		PlayExplosionSound();

		foreach (var enemy in allEnemies)
		{
			await CreatureCmd.Damage(choiceContext, enemy,
				(decimal)damage, ValueProp.Move, this, cardPlay);

			await PowerCmd.Apply<MegaCrit.Sts2.Core.Models.Powers.PoisonPower>(choiceContext, enemy, (decimal)poisonAmount, Owner.Creature, this);
		}

		if (_hasAttackIntent)
		{
			var allPlayers = combatState.PlayerCreatures.ToList();
			foreach (var player in allPlayers)
			{
				await CreatureCmd.Damage(choiceContext, player,
					(decimal)damage, ValueProp.Move, this, cardPlay);
			}
		}
	}

	private bool HasEnemyAttackIntent()
	{
		var combatState = Owner.Creature.CombatState;
		if (combatState == null)
			return false;

		foreach (var enemy in combatState.Enemies)
		{
			if (enemy.IsAlive && enemy.Monster?.NextMove?.Intents != null)
			{
				foreach (var intent in enemy.Monster.NextMove.Intents)
				{
					if (intent is AttackIntent)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private void PlayExplosionSound()
	{
		try
		{
			var audioPlayer = new AudioStreamPlayer();
			audioPlayer.Name = "DemolitionTruckExplosionPlayer";
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
			GD.PrintErr($"[DemolitionTruckCard] 播放爆炸音效失败: {ex.Message}");
		}
	}

	private void PlayFactorySound()
	{
		try
		{
			var audioPlayer = new AudioStreamPlayer();
			audioPlayer.Name = "DemolitionTruckFactoryPlayer";
			var root = Engine.GetMainLoop() as SceneTree;
			if (root != null)
			{
				root.Root.AddChild(audioPlayer);
				var soundFile = GD.Load<AudioStream>("res://RedAlert2ModResources/audio/SovietUnits/DemolitionTruck/Vdemsea_factory.mp3");
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
			GD.PrintErr($"[DemolitionTruckCard] 播放出厂音效失败: {ex.Message}");
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars["Poison"].UpgradeValueBy((int)Values.MagicNumberUpgraded);
	}
}