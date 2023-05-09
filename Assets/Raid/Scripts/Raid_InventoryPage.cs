using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Raid_InventoryPage : MonoBehaviour
{
    [SerializeField]
    private Raid_InventoryItem ItemPrefab;

    [SerializeField]
    private RectTransform ContentPanel;

    List<Raid_InventoryItem> ListOfUIItems = new List<Raid_InventoryItem>();

    public void InitializeInventoryUI (int InventorySize)
    {
        for (int i = 0; i < InventorySize; i++)
        {
            Raid_InventoryItem UIItem = Instantiate(ItemPrefab, Vector3.zero, Quaternion.identity);
            UIItem.transform.SetParent(ContentPanel);
            UIItem.transform.localScale = new Vector3(1, 1, 0);
            ListOfUIItems.Add(UIItem);
            UIItem.OnItemClicked += HandleItemLooting;
        }
    }

    private void HandleItemLooting(Raid_InventoryItem obj)
    {
        Debug.Log(obj.name);
    }
}
