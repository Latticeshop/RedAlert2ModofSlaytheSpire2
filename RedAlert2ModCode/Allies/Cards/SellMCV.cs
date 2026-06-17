using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.Utils;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 卖本 - 运转卡（技能卡）
/// 1费，获得2400资金，消耗
/// 只有拥有基地车能力时才能打出
/// </summary>
public sealed class SellMCV : CardModel
{
	private static readonly CardValueStore.CardValues Values = AlliesCardValues.SellMCV;

	public SellMCV() : base((int)Values.Cost, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

	/// <summary>
	/// 卡牌图片路径（放在上层目录便于多阵营复用）
	/// </summary>
	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/sellmcvicon.png";

	/// <summary>
	/// 消耗词条
	/// </summary>
	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

	/// <summary>
	/// 本地化变量
	/// </summary>
	protected override List<DynamicVar> CanonicalVars => new()
	{
		new IntVar("DollarValue", (int)Values.DollarValue)
	};

	/// <summary>
	/// 检查是否可以打出
	/// 只有拥有基地车能力时才能打出
	/// </summary>
	protected override bool IsPlayable
	{
		get
		{
			if (!base.IsPlayable)
				return false;

			// 检查是否拥有MCV能力（基地车）
			if (!CardUtils.HasMcvPower(Owner.Creature))
				return false;

			return true;
		}
	}

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

		// 移除一层基地车能力（如果只有1层则移除，多层则减少一层）
		var mcvPower = Owner.Creature.Powers.OfType<AlliedMCVPower>().FirstOrDefault();
		if (mcvPower != null)
		{
			// 层数为0时，游戏会自动移除。
            await PowerCmd.ModifyAmount(mcvPower, -1, Owner.Creature, this);
			GD.Print("[SellMCV] 基地车能力减少1层，剩余: " + (mcvPower.Amount - 1));
		}

		// 获得2400资金
		var dollarPower = Owner.Creature.Powers.OfType<Powers.DollarPower>().FirstOrDefault();
		if (dollarPower != null)
		{
			dollarPower.AddDollar((int)Values.DollarValue);
			GD.Print($"[SellMCV] 获得资金 {Values.DollarValue}");
		}

		// 升级效果：将一张工程师卡牌加入手牌
		if (IsUpgraded)
		{
			var engineerCard = Owner.Creature.CombatState.CreateCard(ModelDb.Card<Engineer>(), Owner);
			await CardPileCmd.AddGeneratedCardToCombat(engineerCard, PileType.Hand, addedByPlayer: true);
			GD.Print("[SellMCV] 升级效果：将工程师加入手牌");
		}
	}

	protected override void OnUpgrade()
	{
		// 升级后获得额外效果：将一张工程师卡牌加入手牌
	}
}
