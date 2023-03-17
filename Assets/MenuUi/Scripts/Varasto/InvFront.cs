using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts;
using System.Collections;
using UnityEditor;

public class InvFront : MonoBehaviour
{
    [Header("Information")]
    [SerializeField] private TMP_Text sortText;

    [Header("GameObjects")]
    [SerializeField] private Transform content;
    [SerializeField] private GameObject invSlot;
    private DataStore _store;

    private List<GameFurniture> items;
    private List<GameObject> slotsList = new List<GameObject>();

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
            items = result.ToList();
            isCallbackDone = true;
        });
        yield return new WaitUntil(() => isCallbackDone);

        MakeSlots();

        SetSlots();
    }

    private void MakeSlots()
    {
        for (int i = 0; i < items.Count; i++)
        {
            slotsList.Add(Instantiate(invSlot, content));
        }
    }

    private void SetSlots()
    {
        int i = 0;
        foreach (GameFurniture furn in items)
        {
            GameObject toSet = slotsList[i];

            // Icon - Not done
            //toSet.Image slotIcon = transform.GetChild(0).GetComponent<Image>();

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

        List<GameObject> reSlots = slotsList;
        int forVal = reSlots.Count;

        switch (sortingBy)
        {
            case 0:
                sortText.text = "Sorted by: Alphabet";
                items.OrderBy(v => v.Name);
                items.Sort((GameFurniture a, GameFurniture b) => { return a.Name.CompareTo(b.Name); });
                break;
            case 1:
                sortText.text = "Sorted by: Weight";
                items.Sort((GameFurniture a, GameFurniture b) => { return a.Weight.CompareTo(b.Weight); });
                break;
            case 2:
                sortText.text = "Sorted by: Material";
                items.Sort((GameFurniture a, GameFurniture b) => { return a.Material.CompareTo(b.Material); });
                break;
            default: sortText.text = "Something broke"; break; // Just as a safety measure
        }

        SetSlots();
    }

    public void SlotInformation()
    {

    }

    //public void UnInform() { infoScreen.SetActive(false); invScreen.SetActive(true); }

    // Task List
    // - Visible Inventory (Done)
    // - Sorting (Done)
    // - Infinite capacity possibility (Done)
    // - Reactive Scaling (Done?)
    // - Information Panel Instantiation
    // - Information Panel information

}
