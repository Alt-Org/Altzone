using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Altzone.Scripts.Model.Poco.Game;

public class FurnitureCardUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text setNameText;
    [SerializeField] private TMP_Text weightText;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private Image rarityColorImage;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text creatorText;

    public string GetNameText() => nameText.text;
    public string GetSetNameText() => setNameText.text;
    public Color GetRarityColor() => rarityColorImage.color;
    public string GetWeightText() => weightText.text;
    public string GetValueText() => valueText.text;
    public Sprite GetIcon() => iconImage.sprite;
    public string GetDescriptionText() => descriptionText != null ? descriptionText.text : "";
    public string GetCreatorText() => creatorText != null ? creatorText.text : "";

    // Populates the card UI with furniture data
    public void PopulateCard(GameFurniture furniture)
    {
        nameText.text = furniture.FurnitureInfo.VisibleName;
        setNameText.text = furniture.FurnitureInfo.SetName;
        iconImage.sprite = furniture.FurnitureInfo.Image;
        weightText.text = $"Weight: {furniture.Weight:0.0}";
        valueText.text = $"Value: {furniture.Value:0.0}";

        descriptionText.text = furniture.FurnitureInfo.ArtisticDescription;
        creatorText.text = $"Creator: {furniture.FurnitureInfo.ArtistName}";

        SetRarityColor(furniture.Rarity);
    }

    // Sets the color of the cards by rarity
    private void SetRarityColor(FurnitureRarity rarity)
    {
        Color color = rarity switch
        {
            FurnitureRarity.Common => Color.gray,
            FurnitureRarity.Rare => Color.cyan,
            FurnitureRarity.Epic => new Color(0.6f, 0.2f, 0.8f),  // Violet
            FurnitureRarity.Antique => new Color(1f, 0.55f, 0f),  // Orange
            _ => Color.white
        };

        if (rarityColorImage != null)
        {
            rarityColorImage.color = color;
        }
    }
}
