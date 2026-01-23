using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KojuSorting : MonoBehaviour
{
    public enum SortMode
    {
        Nimi,       
        Hinta,     
        Harvinaisuus 
    }

    [SerializeField] private Transform itemsParent;
    [SerializeField] private TMP_Text sortingText;

    private SortMode currentSortMode = SortMode.Nimi;

    private void Start()
    {
        SetSortMode(SortMode.Nimi);
    }

    public void SetSortMode(SortMode mode)
    {
        currentSortMode = mode;
        UpdateSortingText();
        SortItems();
    }

    // Call this from a button
    public void OnKojuSort()
    {
        currentSortMode = (SortMode)(((int)currentSortMode + 1) % System.Enum.GetValues(typeof(SortMode)).Length);

        UpdateSortingText();
        SortItems();
    }

    private void UpdateSortingText()
    {
        var lang = SettingsCarrier.Instance.Language;

        switch (currentSortMode)
        {
            case SortMode.Nimi:
                if (lang == SettingsCarrier.LanguageType.Finnish)
                {
                    sortingText.text = "Järjestetty: Aakkoset";
                }
                else if (lang == SettingsCarrier.LanguageType.English)
                {
                    sortingText.text = "Sorting: Alphabetical";
                }
                break;
            case SortMode.Hinta:
                if (lang == SettingsCarrier.LanguageType.Finnish)
                {
                    sortingText.text = "Järjestetty: Hinta";
                }
                else if (lang == SettingsCarrier.LanguageType.English)
                {
                    sortingText.text = "Sorting: Price";
                }
                break;
            case SortMode.Harvinaisuus:
                if (lang == SettingsCarrier.LanguageType.Finnish)
                {
                    sortingText.text = "Järjestetty: Harvinaisuus";
                }
                else if (lang == SettingsCarrier.LanguageType.English)
                {
                    sortingText.text = "Sorting: Rarity";
                }
                break;
        }
    }

    private void SortItems()
    {
        List<(KojuItemSlot slot, FurnitureCardUI card)> slotCardPairs = new List<(KojuItemSlot, FurnitureCardUI)>();

        // Get all KojuItemSlot components under itemsParent (the panel)
        KojuItemSlot[] slots = itemsParent.GetComponentsInChildren<KojuItemSlot>(true);
        if (slots.Length == 0) return;

        // Slot 0 is reserved and should be skipped from sorting due to the poster card occupying it
        for (int i = 1; i < slots.Length; i++) 
        {
            var slot = slots[i];

            // Look for any card in the slot, ignoring the empty visuals
            FurnitureCardUI cardInSlot = null;
            foreach (Transform child in slot.transform)
            {
                if (child.gameObject != slot.KojuEmptyVisual)  
                {
                    cardInSlot = child.GetComponent<FurnitureCardUI>();
                    if (cardInSlot != null)
                    {
                        break;
                    }
                }
            }

            if (cardInSlot != null)
            {
                slotCardPairs.Add((slot, cardInSlot));
            }
        }

        if (slotCardPairs.Count == 0)
        {
            Debug.LogWarning("No cards found in any KojuItemSlot.");
            return;
        }

        // Sort based on the selected mode
        slotCardPairs.Sort((a, b) =>
        {
            switch (currentSortMode)
            {
                case SortMode.Nimi:
                    return string.Compare(a.card.GetNameText(), b.card.GetNameText());
                case SortMode.Hinta:
                    return a.card.GetValue().CompareTo(b.card.GetValue());
                case SortMode.Harvinaisuus:
                    return a.card.GetFurnitureRarity().CompareTo(b.card.GetFurnitureRarity());
                default:
                    return 0;
            }
        });

        // Assign cards back to slots in sorted order
        for (int i = 0; i < slotCardPairs.Count; i++)
        {
            var targetSlot = slots[i + 1];
            var card = slotCardPairs[i].card;

            card.transform.SetParent(targetSlot.transform, false);

            targetSlot.KojuEmptyVisual.SetActive(false);

            Debug.Log($"Moved card '{card.name}' to slot '{targetSlot.name}' at position {i + 1}");
        }

        // Show empty visuals for any remaining slots
        for (int i = slotCardPairs.Count + 1; i < slots.Length; i++)
        {
            slots[i].KojuEmptyVisual.SetActive(true);
        }
    }
}
