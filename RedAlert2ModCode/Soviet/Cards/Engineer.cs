using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using RedAlert2ModCode.Allies.Cards;
using RedAlert2ModCode.UI;
using RedAlert2ModCode.Utils;

using EngineerChoice = RedAlert2ModCode.UI.EngineerChoiceScreen.EngineerChoice;

namespace RedAlert2ModCode.Soviet.Cards;

/// <summary>
/// 苏军工程师 - 苏军士兵单位卡
/// 1费，common蓝卡
/// 效果：从2(升级为3)个选项中选择一个指令执行
/// 对应盟军的工程师
/// </summary>
public sealed class Engineer : CardModel
{
	// 数值配置
	private const int COST = 1;
	private const int BASE_CHOICE_COUNT = 2;
	private const int UPGRADED_CHOICE_COUNT = 1;

	public Engineer() : base(COST, CardType.Skill, CardRarity.Token, TargetType.Self) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/soviet/engnicon.png";

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new IntVar("ChoiceCount", BASE_CHOICE_COUNT)
	};

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		UnitVoiceHelper.PlayUnitVoice(this.GetType(), "Soviet");
		await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

		// 生成随机选项
		List<EngineerChoiceScreen.EngineerChoice> choices = GenerateRandomChoices();

		// 显示选择界面
		var selectedChoice = await EngineerChoiceScreen.ShowSelection(choices);

		if (selectedChoice != null)
		{
			await ExecuteChoice(ctx, selectedChoice);
		}
	}

	/// <summary>
	/// 生成随机选项列表
	/// </summary>
	private List<EngineerChoiceScreen.EngineerChoice> GenerateRandomChoices()
	{
		// 根据权重随机选择
		int choiceCount = IsUpgraded ? BASE_CHOICE_COUNT + UPGRADED_CHOICE_COUNT : BASE_CHOICE_COUNT;
		var selected = WeightedRandomSelection(RedAlert2ModCode.Allies.Cards.EngineerChoiceValues.AllChoices, choiceCount);

		return selected;
	}

	/// <summary>
	/// 加权随机选择
	/// </summary>
	private List<EngineerChoice> WeightedRandomSelection(
		List<EngineerChoice> choices, int count)
	{
		List<EngineerChoice> result = new();
		List<EngineerChoice> remaining = new List<EngineerChoice>(choices);
		
		Random random = new();

		for (int i = 0; i < count && remaining.Count > 0; i++)
		{
			int totalWeight = remaining.Sum(c => c.Weight);
			int randomValue = random.Next(totalWeight);
			int currentWeight = 0;

			foreach (var choice in remaining)
			{
				currentWeight += choice.Weight;
				if (randomValue < currentWeight)
				{
					result.Add(choice);
					remaining.Remove(choice);
					break;
				}
			}
		}

		return result;
	}

	/// <summary>
	/// 执行选中的选项
	/// </summary>
	private async Task ExecuteChoice(PlayerChoiceContext ctx, EngineerChoice choice)
	{
		switch (choice.Type)
		{
			case EngineerChoiceScreen.ChoiceType.CaptureOilDerrick:
				// 将一张油井加入手牌
				var oilDerrickCard = Owner.Creature.CombatState.CreateCard(ModelDb.Card<OilDerrickCard>(), Owner);
				await CardPileCmd.AddGeneratedCardToCombat(oilDerrickCard, PileType.Hand, Owner);
				break;

			case EngineerChoiceScreen.ChoiceType.RepairBuilding:
				// 获得3点覆甲
				await PowerCmd.Apply<MegaCrit.Sts2.Core.Models.Powers.PlatingPower>(ctx, Owner.Creature, 3, Owner.Creature, this);
				break;

			case EngineerChoiceScreen.ChoiceType.CaptureAirfield:
				// 加入一张伞兵卡牌（使用盟军的伞兵卡）
				var paratrooperCard = Owner.Creature.CombatState.CreateCard(ModelDb.Card<Paratrooper>(), Owner);
				await CardPileCmd.AddGeneratedCardToCombat(paratrooperCard, PileType.Hand, Owner);
				break;

			case EngineerChoiceScreen.ChoiceType.CaptureHospital:
				// 获得1点敏捷
				await PowerCmd.Apply<MegaCrit.Sts2.Core.Models.Powers.DexterityPower>(ctx, Owner.Creature, 1, Owner.Creature, this);
				break;

			case EngineerChoiceScreen.ChoiceType.CaptureWorkshop:
				// 获得1点力量
				await PowerCmd.Apply<MegaCrit.Sts2.Core.Models.Powers.StrengthPower>(ctx, Owner.Creature, 1, Owner.Creature, this);
				break;

			case EngineerChoiceScreen.ChoiceType.CaptureTechOutpost:
				// 获得爱国者飞弹和维修厂能力（复用盟军的）
				await PowerCmd.Apply<RedAlert2ModCode.Allies.Powers.PatriotMissilePower>(ctx, Owner.Creature, 1, Owner.Creature, this);
				await RedAlert2ModCode.Allies.Powers.RepairDepotPower.ApplyRepairDepot(Owner.Creature);
				break;

			case EngineerChoiceScreen.ChoiceType.RepairBridge:
				// 选择消耗一张手牌，抽两张牌（使用自定义UI避免与其他模组冲突）
				var handPile = PileType.Hand.GetPile(Owner);
				var handCards = handPile.Cards.ToList();
				
				if (handCards.Any())
				{
					// 使用现有的 CardSelectionScreen 进行手牌选择
					var selectedCards = await CardSelectionScreen.ShowMultiSelection(handCards, 1, 1);
					
					if (selectedCards != null && selectedCards.Any())
					{
						foreach (var card in selectedCards)
						{
							await CardPileCmd.Add(card, PileType.Exhaust);
						}
						await CardPileCmd.Draw(ctx, 2, Owner);
					}
				}
				break;
		}
	}

	protected override void OnUpgrade()
	{
		// 升级效果：增加可选选项数量
		DynamicVars["ChoiceCount"].UpgradeValueBy(UPGRADED_CHOICE_COUNT);
	}
}