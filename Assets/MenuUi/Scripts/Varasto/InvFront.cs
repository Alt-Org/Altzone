using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts;
using System.Collections;

public class InvFront : MonoBehaviour
{
    [Header("Information")]
    [SerializeField] private TMP_Text sortText;

    [Header("GameObjects")]
    [SerializeField] private Transform content;
    [SerializeField] private GameObject invSlot;
    private List<GameFurniture> items;
    private DataStore _store;

    private List<GameObject> slotsList = new List<GameObject>();

    private int maxSortingBy = 1;
    private int sortingBy; // used as a carrier for info on how to sort
    private void Start()
    {
        _store = Storefront.Get();

        sortingBy = -1; // So that the first sort style is Alphabet
        StartCoroutine(MakeSlots());
    }

    private IEnumerator MakeSlots()
    {
        var isCallbackDone = false;
        _store.GetAllGameFurniture(result =>
        {
            items = result;
            isCallbackDone = true;
        });
        yield return new WaitUntil(() => isCallbackDone);

        foreach (GameFurniture item in items)
        {
            Transform slotMade = Instantiate(invSlot, content).transform;

            // Icon - Not done
            //Image slotIcon = slotMade.GetChild(0).GetComponent<Image>();

            // Name
            slotMade.GetChild(1).GetComponent<TMP_Text>().text = item.Name;

            // Weight
            slotMade.GetChild(2).GetComponent<TMP_Text>().text = item.Weight + "KG";

            // Shape - Not done
            //slotMade.GetChild(3).GetComponent<Image>().sprite = 
        }
    }

    public void SortStored() // A very much hardcoded system for sorting 
    {
        if (sortingBy < maxSortingBy) { sortingBy++; }
        else { sortingBy = 0;}

        switch (sortingBy)
        {
            case 0:
                sortText.text = "Sorted by: Alphabet";
                slotsList.OrderBy(x => x.name);
                break;
            case 1:
                sortText.text = "Sorted by: Nothing functional";
                /* Should be made to order by the value, when that exists */
                break;
            default: sortText.text = "Something broke"; break; // Just as a safety measure
        }
        //FillSlots();
    }

    public void SlotInformation()
    {
        try
        {
            //IInventoryItem item = 
        }
        catch { /* Slot is empty */ }
    }

    //public void UnInform() { infoScreen.SetActive(false); invScreen.SetActive(true); }

    //public void FillSlots()
    //{
    //    // Sets the images of the items to their slots
    //    int i = 0;
    //    //List<IFurnitureModel> models = _storefront.GetAllFurnitureModels();
    //    foreach (GameObject _slot in slots)
    //    {
    //        try
    //        {
    //            GameObject slotImage = _slot.transform.Find("Image").gameObject;
    //            Image furnitureImage = invStored[i].

    //            slotImage.SetActive(true);

    //            if (furnitureImage.size.x > 200 || furnitureImage.size.y > 200) // Limits the size of an image if it is too large
    //            {
    //                Vector2 imageNewSize = furnitureImage.size;
    //                if (furnitureImage.size.x > 200) { imageNewSize.x = 200; }
    //                if (furnitureImage.size.y > 200) { imageNewSize.y = 200; }
    //            }

    //            slotImage.GetComponent<Image>().sprite = furnitureImage.sprite;
    //            slotImage.GetComponent<Image>().color = furnitureImage.color;

    //            i++;
    //        }
    //        catch {  break; }
    //    }
    //}

    // Task List
    // - Visible Inventory (Done)
    // - Sorting (Done)
    // - Infinite capacity possibility (Scroll down)
    // - Reactive Scaling (Done)
    // - Information Panel Instantiation
    // - Information Panel information

}
