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
    private List<GameFurniture> items;
    private DataStore _store;

    [SerializeField] private List<GameObject> slotsList;

    private int maxSortingBy = 2;
    private int sortingBy; // used as a carrier for info on how to sort
    private void Start()
    {
        _store = Storefront.Get();

        sortingBy = -1; // So that the first sort style is Alphabet
        StartCoroutine(MakeSlots());
    }

    private IEnumerator MakeSlots()
    {
        bool isCallbackDone = false;
        _store.GetAllGameFurniture(result =>
        {
            items = result;
            isCallbackDone = true;
        });
        yield return new WaitUntil(() => isCallbackDone);

        int i = 1;
        foreach (GameFurniture item in items)
        {
            GameObject newObject = Instantiate(invSlot, content);
            newObject.GetComponent<InvSlot>().contains = item;
            newObject.name = item.Name;
            slotsList.Add(newObject);
            i++;
        }

        //SortByString();
    }

    public void SortStored() // A very much hardcoded system for sorting 
    {
        if (sortingBy < maxSortingBy) { sortingBy++; }
        else { sortingBy = 0; }

        List<GameObject> reSlots = slotsList;
        int forVal = reSlots.Count;

        // Depending on the value of sortingBy, orders reSlots to the wanted order which will be fed forward
        switch (sortingBy)
        {
            case 0:
                sortText.text = "Sorted by: Alphabet";
                for (int a = 0; a < forVal; a++)
                {
                    reSlots.Sort((GameObject t1, GameObject t2) =>
                    {
                        return t1.GetComponent<InvSlot>().contains.Name.CompareTo(t2.GetComponent<InvSlot>().contains.Name);
                    });
                }
                break;
            case 1:
                sortText.text = "Sorted by: Weight";
                for (int a = 0; a < forVal; a++)
                {
                    reSlots.Sort((GameObject t1, GameObject t2) =>
                    {
                        return t1.GetComponent<InvSlot>().contains.Weight.CompareTo(t2.GetComponent<InvSlot>().contains.Weight);
                    });
                }
                break;
            case 2:
                sortText.text = "Sorted by: Material?";
                for (int a = 0; a < forVal; a++)
                {
                    reSlots.Sort((GameObject t1, GameObject t2) =>
                    {
                        return t1.GetComponent<InvSlot>().contains.Material.CompareTo(t2.GetComponent<InvSlot>().contains.Material);
                    });
                }
                break;
            default: sortText.text = "Something broke"; break; // Just as a safety measure
        }

        // Gets the order of objects in reSlots and sets the slots to that order
        for (int i = 0; i < forVal; ++i)
        {
            reSlots[i].transform.SetSiblingIndex(i);
        }

        // If more sorts are needed : 
        // for (int a = 0; a < "ListLenghtInt"; a++)
        // {
        //       reSlots.Sort((GameObject t1, GameObject t2) =>
        //       {
        //           return t1.GetComponent<"StringContainingScript">()."StringName".CompareTo(t2.GetComponent<"StringContainingScript">()."StringName");
        //       });
        // }
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
