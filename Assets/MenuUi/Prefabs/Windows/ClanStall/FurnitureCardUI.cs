using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.ReferenceSheets;

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

    public void PopulateCard(GameFurniture furniture)
    {
        nameText.text = furniture.FurnitureInfo.VisibleName;
        setNameText.text = furniture.FurnitureInfo.SetName;
        weightText.text = $"Weight: {furniture.Weight:0.0}";
        iconImage.sprite = furniture.FurnitureInfo.Image;
        description = furniture.FurnitureInfo.ArtisticDescription;
        creatorName = furniture.FurnitureInfo.ArtistName;

        storedRarity = furniture.Rarity;

        // Apply color using rarity reference
        if (rarityColourReference != null)
        {
            rarityColorImage.color = rarityColourReference.GetColor(storedRarity);
        }
    }

    public string GetNameText() => nameText.text;
    public string GetSetNameText() => setNameText.text;
    public string GetWeightText() => weightText.text;
    public Sprite GetIcon() => iconImage.sprite;
    public string GetDescriptionText() => description;
    public string GetCreatorText() => creatorName;

    // Added getter for rarity enum
    public FurnitureRarity GetFurnitureRarity() => storedRarity;

    // Added getter for color
    public Color GetRarityColor()
    {
        if (rarityColourReference != null)
            return rarityColourReference.GetColor(storedRarity);
        return Color.white;
    }
}
