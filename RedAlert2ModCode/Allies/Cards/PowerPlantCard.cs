using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using RedAlert2ModCode.Allies.Powers;
using RedAlert2ModCode.Utils;

namespace RedAlert2ModCode.Allies.Cards;

/// <summary>
/// 发电厂 - 盟军建筑卡
/// 1费能力卡
/// 效果：每抽10张牌获得1点能量（升级后改为7张）
/// 参考游戏原版 Automation 卡牌的效果
/// </summary>
public sealed class PowerPlantCard : CardModel
{
	// 数值引用
	private static readonly CardValueStore.CardValues Values = AlliesCardValues.PowerPlant;
	
	public PowerPlantCard() : base((int)Values.Cost, CardType.Power, CardRarity.Common, TargetType.Self) { }

	public override string PortraitPath => $"res://RedAlert2ModResources/images/packed/card_portraits/allies/powricon.png";

	protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
	{
		await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
		
		// 应用发电厂能力
		var power = await PowerCmd.Apply<PowerPlantPower>(Owner.Creature, 1m, Owner.Creature, this);
		
		if (power != null)
		{
			// 设置触发阈值：升级后7张，未升级10张
			int threshold = IsUpgraded 
				? Values.MagicNumber + Values.MagicNumberUpgraded 
				: Values.MagicNumber;
			power.SetThreshold(threshold);
		}
	}

	protected override void OnUpgrade()
	{
		// 升级效果：触发阈值从10降低到7
	}
}
