using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace RedAlert2ModCode.Allies.Powers;

/// <summary>
/// 运输船能力
/// 用于存储和释放卡牌，并显示当前存储的卡牌信息
/// </summary>
public sealed class TransportShipPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	public override bool IsInstanced => true;

	/// <summary>
	/// 存储的卡牌列表
	/// </summary>
	private List<CardModel> _storedCards = new();

	/// <summary>
	/// 存储的卡牌名称列表（用于本地化显示）
	/// </summary>
	private List<string> _storedCardNames = new();

	/// <summary>
	/// 是否有存储的卡牌
	/// </summary>
	public bool HasStoredCards => _storedCards.Count > 0;

	/// <summary>
	/// 存储的卡牌数量
	/// </summary>
	public int StoredCount => _storedCards.Count;

	/// <summary>
	/// 本地化描述，显示当前存储的卡牌列表
	/// </summary>
	public override LocString Description
	{
		get
		{
			var locString = new LocString("powers", base.Id.Entry + ".description");
			
			// 添加存储的卡牌名称（确保键名与本地化文件中的占位符匹配）
			for (int i = 0; i < _storedCardNames.Count && i < 5; i++)
			{
				locString.Add($"UnitName{i}", _storedCardNames[i]);
			}
			
			return locString;
		}
	}

	/// <summary>
	/// 存储卡牌（从手牌中移除并存储）
	/// </summary>
	public async Task StoreCards(List<CardModel> cards)
	{
		foreach (var card in cards)
		{
			// 存储卡牌对象
			_storedCards.Add(card);
			
			// 存储卡牌名称（本地化后的显示名称）
			_storedCardNames.Add(card.Title.ToString());
			
			// 从手牌中移除卡牌
			card.RemoveFromCurrentPile();
			GD.Print($"[TransportShipPower] 卡牌 {card.Id.Entry} ({card.Title}) 已从手牌移除并存储");
		}
	}

	/// <summary>
	/// 释放所有存储的卡牌到手牌
	/// </summary>
	public async Task ReleaseCards()
	{
		if (!HasStoredCards) return;

		foreach (var card in _storedCards)
		{
			// 将卡牌添加回玩家手牌
			await CardPileCmd.Add(card, PileType.Hand);
			GD.Print($"[TransportShipPower] 卡牌 {card.Id.Entry} ({card.Title}) 已释放到手牌");
		}

		// 清空存储列表
		_storedCards.Clear();
		_storedCardNames.Clear();
		GD.Print($"[TransportShipPower] 所有存储的卡牌已释放");
	}
}