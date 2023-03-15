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

        switch (sortingBy)
        {
            case 0:
                sortText.text = "Sorted by: Alphabet";
                //slotsList.OrderBy(x => x.GetComponent<InvSlot>().contains.Name);
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
                //slotsList.OrderBy(x => x.GetComponent<InvSlot>().contains.Weight);
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
                //slotsList.OrderBy(x => x.GetComponent<InvSlot>().contains.Material);
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

        for (int i = 0; i < forVal; ++i)
        {
            Undo.SetTransformParent(reSlots[i].transform, reSlots[i].transform.parent, "Sort Children");
            reSlots[i].transform.SetSiblingIndex(i);
        }
    }

    public void SlotInformation()
    {
        try
        {
            //IInventoryItem item = 
        }
        catch { /* Slot is empty */ }
    }

    //private void SortByString()
    //{
    //    List<Transform> reSlots = ToTransforms(slotsList);
    //    int forVal = reSlots.Count;
    //    for (int a = 0; a < forVal; a++) { reSlots.Sort((Transform t1, Transform t2) => { return t1.name.CompareTo(t2.name); }); }
    //    for (int i = 0; i < forVal; ++i)
    //    {
    //        Undo.SetTransformParent(reSlots[i], reSlots[i].parent, "Sort Children");
    //        reSlots[i].SetSiblingIndex(i);
    //    }
    //}

    //private List<Transform> ToTransforms(List<GameObject> objects)
    //{
    //    List<Transform> result = new List<Transform>();
    //    foreach(GameObject obj in objects)
    //    {
    //        result.Add(obj.transform);
    //    }
    //    return result;
    //}

    //public void UnInform() { infoScreen.SetActive(false); invScreen.SetActive(true); }

    // Task List
    // - Visible Inventory (Done)
    // - Sorting (Done)
    // - Infinite capacity possibility (Scroll down)
    // - Reactive Scaling (Done)
    // - Information Panel Instantiation
    // - Information Panel information

}
