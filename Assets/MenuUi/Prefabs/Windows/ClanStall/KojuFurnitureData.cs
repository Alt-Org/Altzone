using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MenuUi.Scripts.Storage;
using Altzone.Scripts.ReferenceSheets;

public class KojuFurnitureData : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image furnitureImage;
    [SerializeField] private TMP_Text furnitureName;
    [SerializeField] private TMP_Text furnitureRarity;
    [SerializeField] private TMP_Text furnitureValue;

    private StorageFurniture storageFurniture;

    // Called to initialize this UI card
    public void SetFurniture(StorageFurniture furniture)
    {
        storageFurniture = furniture;

        if (furnitureImage != null)
            furnitureImage.sprite = storageFurniture.Sprite;

        if (furnitureName != null)
            furnitureName.text = storageFurniture.VisibleName;

        if (furnitureRarity != null)
            furnitureRarity.text = storageFurniture.Rarity.ToString();

        if (furnitureValue != null)
            furnitureValue.text = $"Value: {storageFurniture.Value}";
    }

    public StorageFurniture GetFurnitureData()
    {
        return storageFurniture;
    }
}
