using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    [Header("Game objects")]
    [SerializeField] private GameObject[] slots;
    [SerializeField] private TMP_Text pageText;
    [SerializeField] private TMP_Text sortText;

    private List<GameObject> invStored;
    private int page; // The value that dictates which page of the inventory is shown
    private int maxPage; // The max page that can be entered, dictated by the amount of items owned

    private int maxSortingBy = 1;
    private int sortingBy; // used as a carrier for info on how to sort

    [Header("Test purpose")]
    [SerializeField] private List<GameObject> testStored; // Used to test that the images work correctly, the actual inventory will be taken from elsewhere
    private void Start()
    {
        invStored = testStored;

        sortingBy = -1; // So that the first sort style is Alphabet
        SortStored();

        maxPage = Mathf.CeilToInt(invStored.Count / 20) + 1; // Sets the max pages
        MovePage(0);
    }

    public void MovePage(int by)
    {
        page = Mathf.Clamp(page + by, 1, maxPage); // Sets the next selected page, which is clamped between the min page and max page
        pageText.text = page + " / " + maxPage;
        FillSlots();
    }

    public void SortStored() // A very much hardcoded system for sorting 
    {
        if (sortingBy < maxSortingBy) { sortingBy++; }
        else { sortingBy = 0;}

        switch (sortingBy)
        {
            case 0:
                sortText.text = "Sorted by: Alphabet";
                invStored.OrderBy(x => x.name);
                break;
            case 1:
                sortText.text = "Sorted by: Nothing functional";
                /* Sorts by the value, when that exists */
                break;
            default: sortText.text = "Something broke"; break; // Just as a safety measure
        }
    }

    public void FillSlots()
    {
        // Sets the images of the items to their slots
        int i = 20 * (page - 1);
        foreach (GameObject _slot in slots)
        {
            try
            {
                GameObject slotImage = _slot.transform.Find("Image").gameObject;
                SpriteRenderer furnitureImage = invStored[i].GetComponent<SpriteRenderer>();

                slotImage.SetActive(true);

                slotImage.GetComponent<Image>().sprite = furnitureImage.sprite;
                slotImage.GetComponent<Image>().color = furnitureImage.color;

                i++;
            }
            catch {  break; }
        }
    }

    // Task List
    // - Visible Inventory (Done)
    // - Sorting (Done)
    // - Infinite capacity possibility (Done)
    // - Reactive Scaling
    // - Information Panel Instantiation

}
