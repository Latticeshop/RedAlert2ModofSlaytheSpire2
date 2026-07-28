#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using RedAlert2ModCode.Common.Utils;
using RedAlert2ModCode.UI;

namespace RedAlert2ModCode.Allies.Cards;

public abstract class IfvVehicleBase : CardModel
{
	protected List<CardModel> _storedCards = new();
	protected bool _hasStored;
	protected bool _inheritedExhaust;

	protected IfvVehicleBase(int cost, CardType type, CardRarity rarity, TargetType target)
		: base(cost, type, rarity, target) { }

	public override bool CanBeGeneratedInCombat => false;

	protected string LocKeyPrefix => ToSnakeCase(GetType().Name);
	protected string CardId => GetType().Name.ToUpperInvariant();

	protected virtual string ActionKeyName => "attack";

	protected virtual string DeploySoundPath => "res://RedAlert2ModResources/audio/AlliedUnits/IFV/Vifvtran-deploy.mp3";
	protected virtual string AttackSoundPath => string.Empty;

	protected string UiTitleKey => $"ui.{LocKeyPrefix}.title";
	protected string UiActionTitleKey => $"ui.{LocKeyPrefix}.{ActionKeyName}_title";
	protected string UiActionDescKey => $"ui.{LocKeyPrefix}.{ActionKeyName}_desc";
	protected string UiDeployTitleKey => $"ui.{LocKeyPrefix}.deploy_title";
	protected string UiDeployDescKey => $"ui.{LocKeyPrefix}.stored_deploy_desc";

	public override IEnumerable<CardKeyword> CanonicalKeywords => Array.Empty<CardKeyword>();

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new IntVar("ReplayCount", 1),
		new StringVar("StoredCards"),
		new IntVar("StoreCount", 1)
	};

	protected override void DeepCloneFields()
	{
		base.DeepCloneFields();
		_storedCards = new List<CardModel>(_storedCards);
		_hasStored = false;
		_inheritedExhaust = false;
	}

	public void SetStoredCards(CardModel ifvCard, CardModel soldierCard, bool inheritedExhaust = false)
	{
		_storedCards.Clear();
		_storedCards.Add(ifvCard);
		_storedCards.Add(soldierCard);
		_hasStored = true;
		_inheritedExhaust = inheritedExhaust;

		if (inheritedExhaust)
		{
			AddKeyword(CardKeyword.Exhaust);
		}

		var storedText = new LocString("cards", $"{Id.Entry}.stored_info");
		storedText.Add("0", soldierCard.Title);
		((StringVar)DynamicVars["StoredCards"]).StringValue = GetLocStringText(storedText);
	}

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		var options = new List<DeployChoiceScreen.ChoiceOption>
		{
			new DeployChoiceScreen.ChoiceOption
			{
				Id = "attack",
				Title = new LocString("card_keywords", UiActionTitleKey),
				Description = new LocString("card_keywords", UiActionDescKey),
				IconPath = "res://RedAlert2ModResources/images/ui/attack.png"
			},
			new DeployChoiceScreen.ChoiceOption
			{
				Id = "deploy",
				Title = new LocString("card_keywords", UiDeployTitleKey),
				Description = new LocString("card_keywords", UiDeployDescKey),
				IconPath = "res://RedAlert2ModResources/images/ui/deploy.png"
			}
		};

		var selectedIndex = await DeployChoiceScreen.ShowSelectionWithSync(Owner, new LocString("card_keywords", UiTitleKey), options, FactionType.Allied);

		if (selectedIndex.HasValue)
		{
			if (options[selectedIndex.Value].Id == "attack")
			{
				await ExecuteEffect(ctx, play);
			}
			else
			{
				await ExecuteDeployRelease(ctx, play);
			}
		}
	}

	protected abstract Task ExecuteEffect(PlayerChoiceContext ctx, CardPlay play);

	protected async Task ExecuteDeployRelease(PlayerChoiceContext ctx, CardPlay play)
	{
		if (!_hasStored || _storedCards.Count == 0)
		{
			await CardPileCmd.RemoveFromCombat(this);
			return;
		}

		UnitVoiceHelper.PlaySound(DeploySoundPath);

		await ReleaseStoredCards();
		await CardPileCmd.RemoveFromCombat(this);
	}

	protected async Task ReleaseStoredCards()
	{
		var ifvCard = _storedCards[0];
		var soldierCard = _storedCards[1];

		soldierCard.HasBeenRemovedFromState = false;
		await CardPileCmd.Add(soldierCard, PileType.Hand, CardPilePosition.Bottom, this);

		ifvCard.HasBeenRemovedFromState = false;
		if (_inheritedExhaust)
		{
			await CardPileCmd.Add(ifvCard, PileType.Exhaust, CardPilePosition.Bottom, this);
		}
		else
		{
			await CardPileCmd.Add(ifvCard, PileType.Discard, CardPilePosition.Bottom, this);
		}

		_storedCards.Clear();
		_hasStored = false;
		((StringVar)DynamicVars["StoredCards"]).StringValue = string.Empty;
	}

	protected void ClearStored()
	{
		_storedCards.Clear();
		_hasStored = false;
		((StringVar)DynamicVars["StoredCards"]).StringValue = string.Empty;
	}

	protected async Task ConsumeEffectWithExhaust()
	{
		if (!_inheritedExhaust || !_hasStored)
		{
			await CardPileCmd.Add(this, PileType.Discard, CardPilePosition.Bottom, this);
			return;
		}

		var ifvCard = _storedCards[0];

		ifvCard.HasBeenRemovedFromState = false;
		await CardPileCmd.Add(ifvCard, PileType.Exhaust, CardPilePosition.Bottom, this);

		ClearStored();
		await CardPileCmd.RemoveFromCombat(this);
	}

	protected static string GetLocStringText(object locStringObj)
	{
		if (locStringObj == null) return string.Empty;
		if (locStringObj is string str) return str;
		var method = locStringObj.GetType().GetMethod("GetFormattedText", System.Type.EmptyTypes);
		if (method != null)
		{
			try
			{
				var result = method.Invoke(locStringObj, null);
				if (result is string text && !string.IsNullOrEmpty(text)) return text;
			}
			catch { }
		}
		return string.Empty;
	}

	protected static string ToSnakeCase(string name)
	{
		var result = new System.Text.StringBuilder();
		for (int i = 0; i < name.Length; i++)
		{
			if (i > 0 && char.IsUpper(name[i]))
				result.Append('_');
			result.Append(char.ToLowerInvariant(name[i]));
		}
		return result.ToString();
	}
}
