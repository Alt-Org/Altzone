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
    [SerializeField] private GameObject loadingText;

    [Header("Placeholders")] // These should not remain in the finalized game
    [SerializeField] private Sprite _furnImagePlaceholder;

    [Header("Prefabs")]
    [SerializeField] private GameObject _invSlot;

    private List<GameFurniture> _items;
    private List<GameObject> _slotsList = new List<GameObject>();

    private int maxSortingBy = 2;
    private int sortingBy; // used as a carrier for info on how to sort

    private IEnumerator Start()
    {
        sortingBy = -1; // So that the first sort style is Alphabet
        yield return Storefront.Get().GetAllGameFurnitureYield(result => _items = result.ToList());
        loadingText.SetActive(false);
        MakeSlots();
        SortStored(); // Sorting before setting the slots / SetSlots is already in SortStored so no need to do it here
    }

    private void MakeSlots()
    {
        for (int i = 0; i < _items.Count; i++)
        {
            GameObject newSlot = Instantiate(_invSlot, _content);

            // Adds an event to the new slot that enables it to show information about the furniture it is showcasing
            var slotVal = i;
            newSlot.GetComponent<Button>().onClick.AddListener(() => OnShowInfo(slotVal));

            _slotsList.Add(newSlot);
        }
    }

    private void SetSlots()
    {
        int i = 0;
        foreach (GameFurniture furn in _items)
        {
            Transform toSet = _slotsList[i].transform;

            // Icon - Placeholder
            toSet.GetChild(0).GetComponent<Image>().sprite = _furnImagePlaceholder;

            // Name
            toSet.GetChild(1).GetComponent<TMP_Text>().text = furn.Name;

            // Weight
            toSet.GetChild(2).GetComponent<TMP_Text>().text = furn.Weight + " KG";

            // Shape - Placeholder
            toSet.GetChild(3).GetComponent<Image>().sprite = _furnImagePlaceholder;

            i++;
        }
    }

    public void SortStored() // A very much hardcoded system for sorting 
    {
        if (sortingBy < maxSortingBy) { sortingBy++; }
        else { sortingBy = 0; }

        switch (sortingBy)
        {
            case 0:
                _sortText.text = "Sorted by: Alphabet";
                _items.Sort((GameFurniture a, GameFurniture b) => { return a.Name.CompareTo(b.Name); });
                break;
            case 1:
                _sortText.text = "Sorted by: Weight";
                _items.Sort((GameFurniture a, GameFurniture b) => { return a.Weight.CompareTo(b.Weight); });
                break;
            case 2:
                _sortText.text = "Sorted by: Material";
                _items.Sort((GameFurniture a, GameFurniture b) => { return a.Material.CompareTo(b.Material); });
                break;
            default: _sortText.text = "Something broke"; break; // Just as a safety measure
        }
        SetSlots();
    }
    
    void OnShowInfo(int slotVal)
    {
        Debug.Log($"Showing slot {slotVal} information =)");

        Transform parentSlot = _infoSlot.transform;

        // Icon - Placeholder
        parentSlot.GetChild(0).GetComponent<Image>().sprite = _furnImagePlaceholder;

        // Name
        parentSlot.GetChild(1).GetComponent<TMP_Text>().text = _items[slotVal].Name;

        // Weight
        parentSlot.GetChild(2).GetComponent<TMP_Text>().text = _items[slotVal].Weight + " KG";

        // Material text
        parentSlot.GetChild(3).GetComponent<TMP_Text>().text = _items[slotVal].Material;

        // Type - Placeholder
        parentSlot.GetChild(4).GetComponent<Image>().sprite = _furnImagePlaceholder;

        // Type Text
        parentSlot.GetChild(5).GetComponent<TMP_Text>().text = _items[slotVal].Shape;

        _infoSlot.SetActive(true);
    }
}
