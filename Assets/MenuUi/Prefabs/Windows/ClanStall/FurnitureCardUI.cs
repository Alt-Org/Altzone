using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Altzone.Scripts.ReferenceSheets;
using MenuUi.Scripts.Storage;
using Altzone.Scripts.Model.Poco.Game;

public class FurnitureCardUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text setNameText;
    [SerializeField] private TMP_Text weightText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image rarityColorImage;

    [Header("References")]
    [SerializeField] private RarityColourReference rarityColourReference;

    private string creatorName;
    private string description;
    private FurnitureRarity storedRarity;

    private float currentPrice = 0f;

    // Populate from StorageFurniture
    public void PopulateCard(StorageFurniture storageFurniture)
    {
        if (storageFurniture == null)
        {
            Debug.LogWarning("StorageFurniture is null when populating card.");
            return;
        }

        nameText.text = storageFurniture.VisibleName;
        setNameText.text = storageFurniture.SetName;
        weightText.text = $"Weight: {storageFurniture.Weight:0.0}";
        iconImage.sprite = storageFurniture.Sprite;

        description = storageFurniture.Info.ArtisticDescription;
        creatorName = storageFurniture.Info.ArtistName;

        storedRarity = storageFurniture.Rarity;

        if (rarityColourReference != null)
        {
            rarityColorImage.color = rarityColourReference.GetColor(storedRarity);
        }

        currentPrice = storageFurniture.Value;  
    }

    public string GetNameText() => nameText.text;
    public string GetSetNameText() => setNameText.text;
    public string GetWeightText() => weightText.text;
    public Sprite GetIcon() => iconImage.sprite;
    public string GetDescriptionText() => description;
    public string GetCreatorText() => creatorName;

    public FurnitureRarity GetFurnitureRarity() => storedRarity;

    public Color GetRarityColor()
    {
        if (rarityColourReference != null)
            return rarityColourReference.GetColor(storedRarity);
        return Color.white;
    }

    // Get current price value for sorting
    public float GetValue()
    {
        return currentPrice;
    }
}
