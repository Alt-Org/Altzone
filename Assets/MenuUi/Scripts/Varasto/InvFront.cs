using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts;
using System.Collections;
using System.Collections.ObjectModel;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Clan;
using UnityEngine.UI;
using static ServerManager;

public class InvFront : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text _sortText;
    [SerializeField] private Transform _content;
    [SerializeField] private BaseScrollRect _scrollRect;
    [SerializeField] private GameObject _infoSlot;
    [SerializeField] private GameObject _loadingText;
    [SerializeField] private GameObject _topButtons;

    [Header("Placeholders")] // These should not remain to the finalized game
    [SerializeField] private Sprite _furnImagePlaceholder;

    [Header("Non-scene objects")]
    [SerializeField] private GameObject _invSlot;
    [SerializeField] private GameObject _buySlot;
    [SerializeField] private List<Sprite> _icons; // Place images in this list for use as icons, but also, the exact name of the image must be set in the GameFurniture string Filename

    [Header("Information GameObject")]
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _name;
    [SerializeField] private TMP_Text _weight;
    [SerializeField] private TMP_Text _material;
    [SerializeField] private Image _type;
    [SerializeField] private TMP_Text _typeText;

    private List<GameFurniture> _items;
    private List<GameObject> _slotsList = new();

    bool _startCompleted = false;
    bool _updatingInventory = false;

    private int _maxSortingBy = 2;
    private int _sortingBy = -1; // used as a carrier for info on how to sort

    private const string INVENTORY_EMPTY_TEXT = "Varasto tyhjä";

    private void OnEnable()
    {
        if (!_startCompleted) { StartCoroutine(Begin()); }

        ServerManager.OnClanInventoryChanged += UpdateInventory;
        UpdateInventory();
    }

    private void OnDisable()
    {
        ServerManager.OnClanInventoryChanged -= UpdateInventory;
    }

    private IEnumerator Begin()
    {
        var gameConfig = GameConfig.Get();
        var playerSettings = gameConfig.PlayerSettings;
        var playerGuid = playerSettings.PlayerGuid;
        yield return StartCoroutine(GetFurnitureFromClanInventory(playerGuid));

        MakeSlots();
        SortStored(); // Sorting before setting the slots / SetSlots is already in SortStored so no need to do it here

        if(_items.Count == 0)
        {
            _loadingText.GetComponent<TextMeshProUGUI>().text = INVENTORY_EMPTY_TEXT;
        }
        else
        {
            _loadingText.SetActive(false);
            _topButtons.SetActive(true); // The sorting button should not be available if items are yet to be loaded
        }


        _startCompleted = true;
    }

    private void Reset()
    {
        foreach (Transform child in _content)
        {
            Destroy(child.gameObject);
        }

         _scrollRect.NormalizedPosition = new Vector2(0, 1);

        _items.Clear();
        _slotsList.Clear();

        _loadingText.SetActive(true);
        _topButtons.SetActive(false); // The sorting button should not be available if items are yet to be loaded
    }

    public void UpdateInventory()
    {
        if (_updatingInventory)
            return;

        _updatingInventory = true;
        StartCoroutine(UpdateInventoryCoroutine());
    }

    private IEnumerator UpdateInventoryCoroutine()
    {
        yield return new WaitUntil(() => _startCompleted);

        Reset();

        if (ServerManager.Instance.Stock != null)
        {
            yield return StartCoroutine(ServerManager.Instance.SaveClanFromServerToDataStorage(ServerManager.Instance.Clan));
        }

        yield return StartCoroutine(Begin());
        _updatingInventory = false;
    }

    private IEnumerator GetFurnitureFromClanInventory(string playerGuid)
    {
        var store = Storefront.Get();

        // Get clan furniture from inventory.
        List<ClanFurniture> clanFurnitureList = null;
        store.GetPlayerData(playerGuid, playerData =>
        {
            if (playerData == null || !playerData.HasClanId)
            {
                clanFurnitureList = new List<ClanFurniture>();
                return;
            }
            store.GetClanData(playerData.ClanId, clanData =>
            {
                clanFurnitureList = clanData?.Inventory.Furniture ?? new List<ClanFurniture>();
            });
        });
        // Wait for list to arrive.
        yield return new WaitUntil(() => clanFurnitureList != null);

        // Create furniture list for UI.
        _items = new List<GameFurniture>();
        if (clanFurnitureList.Count == 0)
        {
            Debug.Log($"found clan items {_items.Count}");
            yield break;
        }

        // Find actual furniture pieces for the UI.
        ReadOnlyCollection<GameFurniture> allItems = null;
        yield return store.GetAllGameFurnitureYield(result => allItems = result);
        Debug.Log($"all items {allItems.Count}");
        foreach (var clanFurniture in clanFurnitureList)
        {
            var gameFurnitureId = clanFurniture.GameFurnitureId;
            var furniture = allItems.FirstOrDefault(x => x.Id == gameFurnitureId);
            if (furniture == null)
            {
                continue;
            }
            _items.Add(furniture);
        }
        Debug.Log($"found clan items {_items.Count}");
    }
    
    private void MakeSlots()
    {
        for (int i = 0; i < _items.Count; i++)
        {
            GameObject newSlot = Instantiate(_invSlot, _content);
            var capturedSlotVal = i;
            newSlot.GetComponent<Button>().onClick.AddListener(() =>
            {
                // C# variable capture in the body of anonymous function!
                OnShowInfo(capturedSlotVal);
            });
            _slotsList.Add(newSlot);
        }
        //Instantiate(_buySlot, _content);
    }

    private void SetSlots()
    {
        int i = 0;
        foreach (GameFurniture _furn in _items)
        {
            Transform toSet = _slotsList[i].transform;

            // Icon
            toSet.GetChild(0).GetComponent<Image>().sprite = GetIcon(_furn.Filename);

            // Name
            toSet.GetChild(1).GetComponent<TMP_Text>().text = _furn.Name;

            // Weight
            toSet.GetChild(2).GetComponent<TMP_Text>().text = _furn.Weight + " KG";

            // Shape
            toSet.GetChild(3).GetComponent<Image>().sprite = GetIcon(_furn.Shape);

            i++;
        }
    }

    public void SortStored() // A very much hardcoded system for sorting 
    {
        if (_sortingBy < _maxSortingBy) { _sortingBy++; }
        else { _sortingBy = 0; }

        switch (_sortingBy)
        {
            case 0:
                _sortText.text = "Jarjestetty\nAakkoset";
                _items.Sort((GameFurniture a, GameFurniture b) => { return a.Name.CompareTo(b.Name); });
                break;
            case 1:
                _sortText.text = "Jarjestetty\nPaino";
                _items.Sort((GameFurniture a, GameFurniture b) => { return a.Weight.CompareTo(b.Weight); });
                break;
            case 2:
                _sortText.text = "Jarjestetty\nMateriaali";
                _items.Sort((GameFurniture a, GameFurniture b) => { return a.Material.CompareTo(b.Material); });
                break;
        }
        SetSlots();
    }
    
    void OnShowInfo(int slotVal)
    {
        Transform parentSlot = _infoSlot.transform;
        GameFurniture _furn = _items[slotVal];

        // Icon
        _icon.sprite = GetIcon(_furn.Filename);

        // Name
        _name.text = _furn.Name;

        // Weight
        _weight.text = _furn.Weight + " KG";

        // Material text
        _material.text = _furn.Material;

        // Type
        _type.sprite = GetIcon(_furn.Shape);

        // Type Text
        _typeText.text = _furn.Shape;

        _infoSlot.SetActive(true);
    }

    private Sprite GetIcon(string name)
    {
        Sprite returned = _icons.Find(x => x.name == name);
        if (returned == null)
        {
            return _furnImagePlaceholder;
        }
        return returned;
    }
}
