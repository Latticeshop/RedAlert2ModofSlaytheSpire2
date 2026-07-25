using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.Common.Cards;

/// <summary>
/// 超时空卡牌基类
/// 自动处理超时空词条效果：
/// 1. 打出时卡牌进入摸牌堆而非弃牌堆
/// 2. 当卡牌同时拥有消耗(Exhaust)词条时，本次打出进入摸牌堆并移除超时空词条，下次打出正常消耗
/// 3. 自动添加超时空描述文本和悬停提示
/// </summary>
public abstract class ChronoCardModel : CardModel
{
	private bool _chronoConsumed;

	protected ChronoCardModel(int cost, CardType cardType, CardRarity cardRarity, TargetType targetType)
		: base(cost, cardType, cardRarity, targetType) { }

	/// <summary>
	/// 运行时卡池：当卡牌有所有者时，返回所有者角色的卡池；否则返回TokenCardPool
	/// </summary>
	public override CardPoolModel Pool => IsMutable && Owner != null
		? Owner.Character.CardPool
		: ModelDb.CardPool<TokenCardPool>();

	/// <summary>
	/// 视觉卡池：用于确定卡牌的边框颜色等视觉表现
	/// 运行时与Pool相同，卡池查看器中通过重写AllCards属性实现显示
	/// </summary>
	public override CardPoolModel VisualCardPool => Pool;

	protected override List<DynamicVar> CanonicalVars => new()
	{
		new StringVar("ChronoTitle", "[gold]超时空.[/gold]\n")
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips
	{
		get
		{
			var tips = GetExtraHoverTips();
			
			if (!_chronoConsumed)
			{
				tips.Add(ModCardKeywords.Chrono.CreateHoverTip());
			}
			
			return tips;
		}
	}

	/// <summary>
	/// 子类重写此方法提供额外的悬停提示
	/// </summary>
	protected abstract List<IHoverTip> GetExtraHoverTips();

	protected override CardLocation GetResultLocationForCardPlay()
	{
		// 如果超时空效果已消耗，走正常流程
		if (_chronoConsumed)
		{
			return base.GetResultLocationForCardPlay();
		}

		bool hasExhaustKeyword = Keywords.Contains(CardKeyword.Exhaust);
		
		if (hasExhaustKeyword)
		{
			// 有消耗词条：执行最后一次超时空，移除超时空效果
			_chronoConsumed = true;
			if (DynamicVars["ChronoTitle"] is StringVar chronoTitleVar)
			{
				chronoTitleVar.StringValue = string.Empty;
			}
			GD.Print($"[{GetType().Name}] 检测到消耗关键字，触发最后一次超时空进入摸牌堆，超时空效果已消耗");
			return new CardLocation(Owner, PileType.Draw, CardPilePosition.Bottom);
		}

		// 无消耗词条：正常超时空效果，进入摸牌堆
		GD.Print($"[{GetType().Name}] 超时空效果生效，进入摸牌堆");
		return new CardLocation(Owner, PileType.Draw, CardPilePosition.Bottom);
	}
}
