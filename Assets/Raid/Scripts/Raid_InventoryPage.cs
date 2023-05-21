using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Raid_InventoryPage : MonoBehaviour
{
    [SerializeField] private Raid_InventoryItem ItemPrefab;
    [SerializeField] private RectTransform ContentPanel;
    [SerializeField] private Raid_LootTracking LootTracker;

    List<Raid_InventoryItem> ListOfUIItems = new List<Raid_InventoryItem>();

    public Sprite TestImage, TestImage2;
    public int TestItemWeight, TestItemWeight2;

    private void Awake()
    {
        LootTracker.ResetLootCount();
    }

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

    private void HandleItemLooting(Raid_InventoryItem inventoryItem)
    {
        int index = ListOfUIItems.IndexOf(inventoryItem);
        if(index == -1)
        {
            return;
        }
        else if(index == 0)
        {
            LootTracker.SetLootCount(TestItemWeight, LootTracker.MaxLootWeight);
            ListOfUIItems[0].RemoveData();
            TestItemWeight = 0;
        }
        else if(index == 1)
        {
            LootTracker.SetLootCount(TestItemWeight2, LootTracker.MaxLootWeight);
            ListOfUIItems[1].RemoveData();
            TestItemWeight2 = 0;
        }
    }

    public void SetInventorySlotData()
    {
        ListOfUIItems[0].SetData(TestImage, TestItemWeight);
        ListOfUIItems[1].SetData(TestImage2, TestItemWeight2);
    }
}
