using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts.Model.Poco.Game;
using MenuUi.Scripts.Storage;

public class KojuFurnitureData : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image furnitureImage;
    [SerializeField] private TMP_Text furnitureName;
    [SerializeField] private TMP_Text furnitureValue;

    private float basePrice;
    private float price;

    private StorageFurniture furniture;

    public string StorageFurnitureId => furniture?.Id;

    // Sets data from base GameFurniture
    public void SetFurniture(GameFurniture data)
    {
        if (data == null)
        {
            Debug.LogError("GameFurniture data is null");
            return;
        }

        // Store original price and initialize display price
        basePrice = data.Value;
        price = basePrice;

        // Set visuals from GameFurnitureInfo
        if (data.FurnitureInfo != null)
        {
            furnitureImage.sprite = data.FurnitureInfo.Image;
            furnitureName.text = data.FurnitureInfo.VisibleName;
        }

        RefreshUI();
    }


    // Method for setting data from StorageFurniture
    public void SetFurniture(StorageFurniture storageFurniture)
    {
        if (storageFurniture == null)
        {
            Debug.LogError("StorageFurniture data is null");
            return;
        }

        basePrice = storageFurniture.Value;
        price = basePrice;

        furnitureImage.sprite = storageFurniture.Sprite;
        furnitureName.text = storageFurniture.VisibleName;

        RefreshUI();
    }

    private void RefreshUI()
    {
        furnitureValue.text = $"{(int)price}";
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
