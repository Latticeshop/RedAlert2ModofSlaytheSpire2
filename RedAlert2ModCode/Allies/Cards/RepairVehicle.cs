#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.Soviet;
using RedAlert2ModCode.Yuri;

using STS2RitsuLib.Interop.AutoRegistration;

namespace RedAlert2ModCode.Allies.Cards;

[RegisterCard(typeof(AlliesCardPool))]
public sealed class RepairVehicle : IfvVehicleBase
{
	private static readonly CardValueStore.CardValues Values = AlliesCardValues.RepairVehicle;

	public RepairVehicle() : base((int)Values.Cost, CardType.Skill, CardRarity.Token, TargetType.Self) { }

	public override string PortraitPath => "res://RedAlert2ModResources/images/packed/card_portraits/allies/ifv_repair.png";

	protected override string ActionKeyName => "repair";

	protected override string AttackSoundPath => "res://RedAlert2ModResources/audio/AlliedUnits/IFV/Vifvrepa_repair.mp3";

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new IntVar("ReplayCount", Values.MagicNumber),
		new StringVar("StoredCards"),
		new IntVar("StoreCount", 1)
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.TechLevelT1.CreateHoverTip(),
		ModCardKeywords.Vehicle.CreateHoverTip(),
		ModCardKeywords.Unit.CreateHoverTip(),
		ModCardKeywords.Deploy.CreateHoverTip(),
		HoverTipFactory.Static(StaticHoverTip.ReplayStatic)
	];

	protected override async Task ExecuteEffect(PlayerChoiceContext ctx, CardPlay play)
	{
		UnitVoiceHelper.PlaySound(AttackSoundPath);
		UnitVoiceHelper.PlayUnitVoice(this.GetType(), "Allies");

		bool isUpgraded = IsUpgraded;

		var selectPrompt = new MegaCrit.Sts2.Core.Localization.LocString("cards", "RED_ALERT2_MOD_CARD_REPAIR_VEHICLE.repair_select_prompt");
		selectPrompt.Add("0", 0);
		selectPrompt.Add("1", 1);
		var prefs = new CardSelectorPrefs(selectPrompt, 0, 1)
		{
			RequireManualConfirmation = true
		};

		var selectedCards = (await CardSelectCmd.FromHand(
			ctx,
			Owner,
			prefs,
			c => c != this && (isUpgraded || IsUnitCard(c)),
			this
		)).ToList();

		foreach (var card in selectedCards)
		{
			card.BaseReplayCount += DynamicVars["ReplayCount"].IntValue;
			GD.Print($"[RepairVehicle] 为卡牌 {card.Title} 赋予 Replay {DynamicVars["ReplayCount"].IntValue}");
		}

		await ConsumeEffectWithExhaust();
	}

	private static readonly Lazy<HashSet<System.Type>> LazyUnitTypes = new(InitializeUnitTypes);

	private static HashSet<System.Type> UnitTypes => LazyUnitTypes.Value;

	private static HashSet<System.Type> InitializeUnitTypes()
	{
		var set = new HashSet<System.Type>();
		set.UnionWith(AlliedCardRegistry.GetBasicUnitTypes());
		set.UnionWith(AlliedCardRegistry.GetT1UnitTypes());
		set.UnionWith(AlliedCardRegistry.GetT2UnitTypes());
		set.UnionWith(AlliedCardRegistry.GetT3UnitTypes());
		set.UnionWith(SovietCardRegistry.GetBasicUnitTypes());
		set.UnionWith(SovietCardRegistry.GetT3UnitTypes());
		var yuriUnits = YuriCardRegistry.GetAllUnits();
		foreach (var u in yuriUnits)
			set.Add(u.GetType());
		return set;
	}

	public static bool IsUnitCard(CardModel card)
	{
		return UnitTypes.Contains(card.GetType());
	}

	protected override void OnUpgrade()
	{
		DynamicVars["ReplayCount"].UpgradeValueBy(Values.MagicNumberUpgraded);
	}
}
