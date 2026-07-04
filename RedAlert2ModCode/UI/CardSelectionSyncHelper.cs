using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using RedAlert2ModCode.Common.Utils;

namespace RedAlert2ModCode.UI;

internal static class CardSelectionSyncHelper
{
    public static async Task<CardModel?> ShowSelectionWithSync(List<CardModel> cards, Player player, Dictionary<string, CardValueStore.CardValues>? cardValuesMap = null, FactionType faction = FactionType.Allied)
    {
        List<CardModel> cardsCopy = new(cards);

        int? selectedIndex = await MultiplayerSyncHelper.ExecuteSyncChoice(player, async () =>
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

    public static async Task<List<CardModel>?> ShowMultiSelectionWithSync(List<CardModel> cards, int maxSelect, int minSelect, Player player)
    {
        List<CardModel> cardsCopy = new(cards);

        List<int> selectedIndices = await MultiplayerSyncHelper.ExecuteSyncMultiChoice(player, async () =>
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
}