using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts;
using System.Collections;
using UnityEngine.UI;

public class InvFront : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text _sortText;
    [SerializeField] private Transform _content;
    [SerializeField] private GameObject _infoSlot;
    [SerializeField] private GameObject _loadingText;
    [SerializeField] private GameObject _topButtons;

    [Header("Placeholders")] // These should not remain to the finalized game
    [SerializeField] private Sprite _furnImagePlaceholder;

    [Header("Non-scene objects")]
    [SerializeField] private GameObject _invSlot;
    [SerializeField] private List<Sprite> _icons; // Place images in this list for use as icons, but also, the exact name of the image must be set in the GameFurniture string Filename

    private List<GameFurniture> _items;
    private List<GameObject> _slotsList = new List<GameObject>();

    bool startCompleted = false;

    private int _maxSortingBy = 2;
    private int _sortingBy = -1; // used as a carrier for info on how to sort

    private void OnEnable()
    {
        if (!startCompleted) { StartCoroutine(Begin()); }
    }

    private IEnumerator Begin()
    {
        yield return Storefront.Get().GetAllGameFurnitureYield(result => _items = result.ToList());

        MakeSlots();
        SortStored(); // Sorting before setting the slots / SetSlots is already in SortStored so no need to do it here

        _loadingText.SetActive(false);
        _topButtons.SetActive(true); // The sorting button should not be available if items are yet to be loaded

        startCompleted = true;
    }

    private void MakeSlots()
    {
        for (int i = 0; i < _items.Count; i++)
        {
            GameObject newSlot = Instantiate(_invSlot, _content);
            var slotVal = i;
            newSlot.GetComponent<Button>().onClick.AddListener(() => OnShowInfo(slotVal));
            _slotsList.Add(newSlot);
        }
    }

    private void SetSlots()
    {
        int i = 0;
        foreach (GameFurniture _furn in _items)
        {
            Transform toSet = _slotsList[i].transform;

            // Icon - Placeholder
            toSet.GetChild(0).GetComponent<Image>().sprite = GetIcon(_furn.Filename);

            // Name
            toSet.GetChild(1).GetComponent<TMP_Text>().text = _furn.Name;

            // Weight
            toSet.GetChild(2).GetComponent<TMP_Text>().text = _furn.Weight + " KG";

            // Shape - Placeholder
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
                _sortText.text = "Jarjestetty : Aakkoset";
                _items.Sort((GameFurniture a, GameFurniture b) => { return a.Name.CompareTo(b.Name); });
                break;
            case 1:
                _sortText.text = "Jarjestetty : Paino";
                _items.Sort((GameFurniture a, GameFurniture b) => { return a.Weight.CompareTo(b.Weight); });
                break;
            case 2:
                _sortText.text = "Jarjestetty : Materiaali";
                _items.Sort((GameFurniture a, GameFurniture b) => { return a.Material.CompareTo(b.Material); });
                break;
        }
        SetSlots();
    }
    
    void OnShowInfo(int slotVal)
    {
        Transform parentSlot = _infoSlot.transform;
        GameFurniture _furn = _items[slotVal];

        // Icon - Placeholder
        parentSlot.GetChild(0).GetComponent<Image>().sprite = GetIcon(_furn.Filename);

        // Name
        parentSlot.GetChild(1).GetComponent<TMP_Text>().text = _furn.Name;

        // Weight
        parentSlot.GetChild(2).GetComponent<TMP_Text>().text = _furn.Weight + " KG";

        // Material text
        parentSlot.GetChild(3).GetComponent<TMP_Text>().text = _furn.Material;

        // Type - Placeholder
        parentSlot.GetChild(4).GetComponent<Image>().sprite = GetIcon(_furn.Shape);

        // Type Text
        parentSlot.GetChild(5).GetComponent<TMP_Text>().text = _furn.Shape;

        _infoSlot.SetActive(true);
    }

    private Sprite GetIcon(string name)
    {
        Sprite returned = _icons.Find(x => x.name == name);

        if (returned == null)
        {
            Debug.LogWarning($"No icon of name '{name}' was found");
            return _furnImagePlaceholder;
        }

        return returned;
    }
}
