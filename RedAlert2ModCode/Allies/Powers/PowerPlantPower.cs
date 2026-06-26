using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Allies.Powers;

/// <summary>
/// 发电厂能力 - 每抽一定数量的牌获得能量
/// 参考游戏原版 AutomationPower 的实现
/// </summary>
public sealed class PowerPlantPower : PowerModel
{
	// 数值引用
	private static readonly CardValueStore.CardValues Values = AlliesPowerValues.PowerPlantPower;
	
	private const string _baseCardsKey = "BaseCards";

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	/// <summary>
	/// 当前剩余抽牌数
	/// </summary>
	private int _cardsLeft;

	/// <summary>
	/// 显示剩余抽牌数
	/// </summary>
	public override int DisplayAmount => _cardsLeft;

	public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

	/// <summary>
	/// 当前阈值（未升级10，升级7）
	/// </summary>
	public int CurrentThreshold { get; set; } = Values.MagicNumber;

	/// <summary>
	/// 通过 CanonicalVars 提供动态变量，供 smartDescription 使用
	/// </summary>
	protected override IEnumerable<DynamicVar> CanonicalVars =>
		new DynamicVar[] { new DynamicVar(_baseCardsKey, Values.MagicNumber) };

	/// <summary>
	/// 设置阈值并重置计数
	/// </summary>
	public void SetThreshold(int threshold)
	{
		CurrentThreshold = threshold;
		// 更新动态变量，使 smartDescription 显示正确的数值
		DynamicVars[_baseCardsKey].BaseValue = threshold;
		_cardsLeft = threshold;
		InvokeDisplayAmountChanged();
	}

	/// <summary>
	/// 能力应用时初始化
	/// </summary>
	public override Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		_cardsLeft = CurrentThreshold;
		return Task.CompletedTask;
	}

	/// <summary>
	/// 抽牌后触发
	/// </summary>
	public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
	{
		if (card.Owner == base.Owner.Player && Amount > 0)
		{
			_cardsLeft--;
			InvokeDisplayAmountChanged();
			
			if (_cardsLeft <= 0)
			{
				Flash();
				await PlayerCmd.GainEnergy(1, base.Owner.Player);
				_cardsLeft = CurrentThreshold;
				InvokeDisplayAmountChanged();
			}
		}
	}
}