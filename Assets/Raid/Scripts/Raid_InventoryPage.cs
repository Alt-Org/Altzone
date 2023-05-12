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

    public Sprite TestImage;
    public int TestItemWeight;

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

    private void HandleItemLooting(Raid_InventoryItem obj)
    {
        LootTracker.SetLootCount(TestItemWeight, 250);
        ListOfUIItems[0].RemoveData();
        TestItemWeight = 0;
    }

    public void TestMethod()
    {
        ListOfUIItems[0].SetData(TestImage, TestItemWeight);
    }
}
