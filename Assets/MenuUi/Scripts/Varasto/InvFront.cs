using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts;
using System.Collections;
using UnityEditor;
using UnityEngine.UI;
using static UnityEngine.UI.Button;
using UnityEngine.EventSystems;

public class InvFront : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text _sortText;
    [SerializeField] private Transform _content;
    [SerializeField] private GameObject _infoSlot;

    [Header("Prefabs")]
    [SerializeField] private GameObject _invSlot;

    private DataStore _store;

    private List<GameFurniture> _items;
    private List<GameObject> _slotsList = new List<GameObject>();

    private int maxSortingBy = 2;
    private int sortingBy; // used as a carrier for info on how to sort
    private void Start()
    {
        _store = Storefront.Get();

        sortingBy = -1; // So that the first sort style is Alphabet
        StartCoroutine(SetInventory());
    }

    private IEnumerator SetInventory()
    {
        bool isCallbackDone = false;
        _store.GetAllGameFurniture(result =>
        {
            _items = result.ToList();
            isCallbackDone = true;
        });
        yield return new WaitUntil(() => isCallbackDone);

        MakeSlots();

        SetSlots();
    }

    private void MakeSlots()
    {
        for (int i = 0; i < _items.Count; i++)
        {
            GameObject newSlot = Instantiate(_invSlot, _content);

            // Adds an event to the new slot that enables it to show information about the furniture it is showcasing
            newSlot.GetComponent<Button>().onClick.AddListener(ShowInfo);

            _slotsList.Add(newSlot);
        }
    }

    private void SetSlots()
    {
        int i = 0;
        foreach (GameFurniture furn in _items)
        { // 1. Furniture (Image) / 2. Name / 3. Weight / 4. Shape Type (Image) 
            GameObject toSet = _slotsList[i];

            // Icon - Not done
            //Image slotIcon = toSet.transform.GetChild(0).GetComponent<Image>();

            // Name
            toSet.transform.GetChild(1).GetComponent<TMP_Text>().text = furn.Name;

            // Weight
            toSet.transform.GetChild(2).GetComponent<TMP_Text>().text = furn.Weight + " KG";

            // Shape - Not done
            //toSet.transform.GetChild(3).GetComponent<Image>().sprite =

            i++;
        }
    }

    public void SortStored() // A very much hardcoded system for sorting 
    {
        if (sortingBy < maxSortingBy) { sortingBy++; }
        else { sortingBy = 0; }

        List<GameObject> reSlots = _slotsList;
        int forVal = reSlots.Count;

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
    
    void ShowInfo()
    { // 1. Icon (Image) / 2. Name / 3. Weight / 4. MaterialText / 5. Type (Image) / 6. TypeText / 7. AddHome (Button) / 8. SuggestSale (Button)
        Debug.Log("Showing some information =)");

        // I know its a bit of a complex way of doing stuff but cant really find workarounds as events cant have arguments, and i know not of how to do it withut the use of events
        int slotVal = _slotsList.IndexOf(EventSystem.current.currentSelectedGameObject);

        // Icon - Still gotta figure out how to get images for these things

        // Name
        _infoSlot.transform.GetChild(1).GetComponent<TMP_Text>().text = _items[slotVal].Name;

        // Weight
        _infoSlot.transform.GetChild(2).GetComponent<TMP_Text>().text = _items[slotVal].Weight + " KG";

        // Material text
        _infoSlot.transform.GetChild(3).GetComponent<TMP_Text>().text = _items[slotVal].Material;

        // Type

        // Type Text
        _infoSlot.transform.GetChild(5).GetComponent<TMP_Text>().text = _items[slotVal].Shape;

        _infoSlot.SetActive(true);
    }

    // Task List
    // - Visible Inventory (Done)
    // - Sorting (Done)
    // - Infinite capacity possibility (Done)
    // - Reactive Scaling (Done?)
    // - Information Panel Instantiation
    // - Information Panel information
}
