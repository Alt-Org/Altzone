using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Omistukset : MonoBehaviour
{
    // TODO:
    // Create a list of all items
    // Create a dictionary of items and their ownership status
    // Load a previous dictionary if it exists
    // Save the new dictionary

    public List<KauppaItems> allItems;
    public Dictionary<KauppaItems, bool> omistukset;

    // Get all the items
    // Load previous dictionary
    // Add new items
    private void Start()
    {
        
    }

    public void AddItem(KauppaItems item, bool value)
    {
        omistukset.Add(item, value)
    }

    public void SaveItems()
    {
        foreach (var KauppaItems in omistukset)
            SetInt(KauppaItems, omistukset(KauppaItems))
    }

    // Save item's ownership status
    // 0: Doesn't own it
    // 1: Owns it
    public void SetInt(string itemName, bool value)
    {
        iValue = 0;
        if (value) iValue = 1; 
        PlayerPrefs.SetInt(itemName, iValue);
    }

    public bool Getint(string itemName)
    {
        if (PlayerPrefs.GetInt(itemName) == 1) return true;
        else return false;
    }
}
