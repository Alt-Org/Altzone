using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts.Model.Poco.Game;

public class KojuFurnitureData : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image furnitureImage;
    [SerializeField] private TMP_Text furnitureName;
    [SerializeField] private TMP_Text furnitureValue;

    private float basePrice;
    private float price;

    public void SetFurniture(GameFurniture data)
    {
        if (data == null)
        {
            Debug.LogError("GameFurniture data is null");
            return;
        }

        // Set base price and initialize current price
        basePrice = data.Value;
        price = basePrice;

       // Update visuals
        if (data.FurnitureInfo != null)
        {
            furnitureImage.sprite = data.FurnitureInfo.Image;
            furnitureName.text = data.FurnitureInfo.VisibleName;
        }

        RefreshUI();
    }

    private void RefreshUI()
    {
        furnitureValue.text = $"Value: {price:0.0}";
    }

    public void SetPrice(float newPrice)
    {
        price = newPrice;
        RefreshUI();
    }

    public float GetPrice()
    {
        return price;
    }

    public void ResetPrice()
    {
        price = basePrice;
        RefreshUI();
        Debug.Log($"Price reset to base price: {basePrice}");
    }
}
