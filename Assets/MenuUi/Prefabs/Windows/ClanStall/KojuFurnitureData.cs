using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MenuUi.Scripts.Storage;

public class KojuFurnitureData : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image furnitureImage;
    [SerializeField] private TMP_Text furnitureName;
    [SerializeField] private TMP_Text furnitureValue;

    private StorageFurniture storageFurniture;
    private float price;

    public void SetFurniture(StorageFurniture data)
    {
        storageFurniture = data;
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (storageFurniture == null)
        {
            // Place debug warning here if problems arise with the UI elements don't properly update
        }
        else
        {
            furnitureImage.sprite = storageFurniture.Sprite;
            furnitureName.text = storageFurniture.VisibleName;
        }

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
}
