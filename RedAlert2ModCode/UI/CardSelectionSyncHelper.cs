using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.UI;

internal static class CardSelectionSyncHelper
{
    public static async Task<CardModel?> ShowSelectionWithSync(PlayerChoiceContext context, List<CardModel> cards, Player player, Dictionary<string, CardValueStore.CardValues>? cardValuesMap = null, FactionType faction = FactionType.Allied)
    {
        List<CardModel> cardsCopy = new(cards);

        int? selectedIndex = await MultiplayerSyncHelper.ExecuteSyncChoice(context, player, async () =>
        {
            CardModel? card;
            if (cardValuesMap != null)
            {
                card = await CardSelectionScreen.ShowSelection(cardsCopy, player, cardValuesMap, faction);
            }
            else
            {
                card = await CardSelectionScreen.ShowSelection(cardsCopy, player, faction);
            }
            return card != null ? cardsCopy.FindIndex(c => c == card) : null;
        });

        if (selectedIndex.HasValue && selectedIndex.Value >= 0 && selectedIndex.Value < cardsCopy.Count)
        {
            return cardsCopy[selectedIndex.Value];
        }

        return null;
    }

    public static async Task<List<CardModel>?> ShowMultiSelectionWithSync(PlayerChoiceContext context, List<CardModel> cards, int maxSelect, int minSelect, Player player)
    {
        List<CardModel> cardsCopy = new(cards);

        List<int> selectedIndices = await MultiplayerSyncHelper.ExecuteSyncMultiChoice(context, player, async () =>
        {
            List<CardModel>? selected = await CardSelectionScreen.ShowMultiSelection(cardsCopy, maxSelect, minSelect, player);
            if (selected == null) return null;

            List<int> indices = new();
            foreach (var card in selected)
            {
                int index = cardsCopy.FindIndex(c => c == card);
                if (index >= 0)
                    indices.Add(index);
            }
            return indices;
        });

        if (selectedIndices.Count > 0)
        {
            List<CardModel> result = new();
            foreach (int index in selectedIndices)
            {
                if (index >= 0 && index < cardsCopy.Count)
                    result.Add(cardsCopy[index]);
            }
            return result.Count > 0 ? result : null;
        }

        return null;
    }

    public static async Task<List<CardSelectionResult>?> ShowSelectionWithQuantitySync(PlayerChoiceContext context, List<CardModel> cards, Player player, Dictionary<string, CardValueStore.CardValues> cardValuesMap, FactionType faction = FactionType.Allied)
    {
        List<CardModel> cardsCopy = new(cards);

        // 将选择结果编码为 [cardIndex1, count1, cardIndex2, count2, ...] 的整数列表
        // 使用特殊标记区分取消(-2)和空选确认(-1)
        List<int> encodedSelection = await MultiplayerSyncHelper.ExecuteSyncMultiChoice(context, player, async () =>
        {
            List<CardSelectionResult>? selected = await CardSelectionScreen.ShowSelectionWithQuantity(cardsCopy, player, cardValuesMap, faction);
            if (selected == null)
            {
                // 取消操作：返回包含-2的列表
                return new List<int> { -2 };
            }

            if (selected.Count == 0)
            {
                // 空选确认：返回包含-1的列表
                return new List<int> { -1 };
            }

            // 正常选择：编码为 [cardIndex1, count1, cardIndex2, count2, ...]
            List<int> encoded = new();
            foreach (var result in selected)
            {
                int index = cardsCopy.FindIndex(c => c == result.Card);
                if (index >= 0)
                {
                    encoded.Add(index);
                    encoded.Add(result.Count);
                }
            }
            return encoded;
        });

        if (encodedSelection != null && encodedSelection.Count > 0)
        {
            // 检查特殊标记
            if (encodedSelection[0] == -2)
            {
                // 取消操作：返回null
                return null;
            }
            
            if (encodedSelection[0] == -1)
            {
                // 空选确认：返回空列表表示确认但未选择任何卡牌
                return new List<CardSelectionResult>();
            }
            
            // 正常选择时，解码结果
            if (encodedSelection.Count >= 2 && encodedSelection.Count % 2 == 0)
            {
                List<CardSelectionResult> result = new();
                for (int i = 0; i < encodedSelection.Count; i += 2)
                {
                    int index = encodedSelection[i];
                    int count = encodedSelection[i + 1];
                    if (index >= 0 && index < cardsCopy.Count && count > 0)
                        result.Add(new CardSelectionResult { Card = cardsCopy[index], Count = count });
                }
                return result;
            }
        }

        return null;
    }
}
