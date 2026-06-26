using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.Common.Powers;
using RedAlert2ModCode.Soviet.Powers;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Soviet.Cards;

/// <summary>
/// 磁能反应堆 - 苏军建筑卡
/// 1费能力卡
/// 效果：每抽10张牌获得1点能量（升级后改为7张）
/// 与盟军发电厂效果一致
/// </summary>
public sealed class NuclearReactor : CardModel
{
	// 数值引用
	private static readonly CardValueStore.CardValues Values = SovietCardValues.NuclearReactor;
	
	public NuclearReactor() : base((int)Values.Cost, CardType.Power, CardRarity.Common, TargetType.Self) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/soviet/npwricon.png";
	
	protected override List<DynamicVar> CanonicalVars => new()
	{
		new IntVar("DollarNumber", Values.DollarValue),
		new IntVar("MagicNumber", Values.MagicNumber)
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		ModCardKeywords.Building.CreateHoverTip()
	];

	protected override bool IsPlayable
	{
		get
		{
			if (!base.IsPlayable)
				return false;

			// 检查是否拥有MCV能力（建造厂）
			if (!CardUtils.HasMcvPower(Owner.Creature))
				return false;

			var dollarPower = Owner.Creature.Powers.OfType<Common.Powers.DollarPower>().FirstOrDefault();
			if (dollarPower == null || dollarPower.DollarValue < Values.DollarValue)
				return false;

			return true;
		}
	}

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		// 播放建筑释放音效
		BuildingSoundHelper.PlayBuildingPlaceSound();
		
		// 扣除资金
		var dollarPower = Owner.Creature.Powers.OfType<Common.Powers.DollarPower>().FirstOrDefault();
		if (dollarPower != null)
		{
			dollarPower.AddDollar(-(int)Values.DollarValue);
			GD.Print($"[NuclearReactor] 扣除资金 {Values.DollarValue}");
		}

		await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
		
		// 应用苏联磁能反应堆能力
		var power = await PowerCmd.Apply<SovietPowerPlantPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, 1m, Owner.Creature, this);
		
		if (power != null)
		{
			// 设置触发阈值：升级后7张，未升级10张
			int threshold = IsUpgraded 
				? Values.MagicNumber + Values.MagicNumberUpgraded 
				: Values.MagicNumber;
			power.SetThreshold(threshold);
		}

		// 打出后抽一张牌
		await CardPileCmd.Draw(ctx, 1, Owner);
	}

	protected override void OnUpgrade()
	{
		// 升级效果：触发阈值从10降低到7
	}
}