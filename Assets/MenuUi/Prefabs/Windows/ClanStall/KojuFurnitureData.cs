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

    private void Start()
    {
        price = 100f; //for testing purposes, set the price for 100. Should be replaced later with actual data
        RefreshUI();
    }

    public void SetFurniture(StorageFurniture data)
    {
        storageFurniture = data;
        RefreshUI();
    }

    //Updates the item to show the data
    private void RefreshUI()
    {
        if (storageFurniture == null)
        {
            Debug.LogWarning("StorageFurniture not set.");

        }
        else
        {
            furnitureImage.sprite = storageFurniture.Sprite;
            furnitureName.text = storageFurniture.VisibleName;
        }

        furnitureValue.text = $"{price}";
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