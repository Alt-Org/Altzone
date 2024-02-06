using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.Model.Poco.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClanFurnitureListing : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int _maxFurnitureAmount;

    [SerializeField] private Image _furnitureImage;
    [SerializeField] private TextMeshProUGUI _furnitureName;
    [SerializeField] private TextMeshProUGUI _furnitureAmountText;
    [SerializeField] private Button _addFurnitureButton;
    [SerializeField] private Button _removeFurnitureButton;

    private ClanModifyInventoryView _inventoryView;
    private List<ServerItem> _serverItems;

    private GameFurniture _furniture;

    public Image FurnitureImage { get => _furnitureImage; set => _furnitureImage = value; }
    public string FurnitureName { get => _furnitureName.text; set => _furnitureName.text = value; }
    public GameFurniture Furniture { get => _furniture; set { _furniture = value; FurnitureName = Furniture.Name; } }

    public ClanModifyInventoryView InventoryView { get => _inventoryView; set => _inventoryView = value; }
    public List<ServerItem> ServerItems { get => _serverItems; set => _serverItems = value; }

    private void Awake()
    {
        ServerItems = new List<ServerItem>();
    }

    private void Reset()
    {
        ServerItems.Clear();
    }

    public void UpdateUI()
    {
        _furnitureAmountText.text = ServerItems.Count.ToString();

        if(ServerItems.Count <= 0)
            _removeFurnitureButton.interactable = false;
        else
            _removeFurnitureButton.interactable = true;


        if (ServerItems.Count >= _maxFurnitureAmount)
            _addFurnitureButton.interactable = false;
        else
            _addFurnitureButton.interactable = true;
    }

    private void OnDisable()
    {
        Reset();
    }

    public void AddFurniture()
    {
        InventoryView.AddItemToServer(_furniture, serverItem =>
        {
            if (serverItem != null)
            {
                ServerItems.Add(serverItem);
                UpdateUI();
            }
        });

    }

    public void RemoveFurniture()
    {
        if (ServerItems == null || ServerItems.Count <= 0)
            return;

        ServerItem itemToRemove = ServerItems.Last();

        InventoryView.RemoveItemFromServer(itemToRemove, wasSuccessful =>
        {
            if (wasSuccessful)
            {
                ServerItems.Remove(itemToRemove);
                UpdateUI();
            }
        });
    }
}
