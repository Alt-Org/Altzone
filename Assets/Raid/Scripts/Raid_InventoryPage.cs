using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Raid_InventoryPage : MonoBehaviour
{
    [SerializeField, Header("Assigned scripts")] private Raid_InventoryHandler raid_InventoryHandler;
    [SerializeField] private Raid_InventoryItem ItemPrefab;
    [SerializeField] private RectTransform ContentPanel;
    [SerializeField] private Raid_LootTracking LootTracker;
    [SerializeField] private Raid_Timer raid_Timer;

    List<Raid_InventoryItem> ListOfUIItems = new List<Raid_InventoryItem>();

    [SerializeField, Header("Furniture and weight")] private Sprite Image1;
    [SerializeField] private int ItemWeight1;
    [SerializeField] private Sprite Image2;
    [SerializeField] private int ItemWeight2;
    [SerializeField] private Sprite Image3;
    [SerializeField] private int ItemWeight3;

    public event Action<int> OnLootDataRequested;

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

    public void HandleItemLooting(Raid_InventoryItem inventoryItem)
    {
        int index = ListOfUIItems.IndexOf(inventoryItem);
        if(index == -1)
        {
            return;
        }
        if (raid_Timer.CurrentTime <= 0 || LootTracker.CurrentLootWeight > LootTracker.MaxLootWeight)
        {
            return;
        }
        else
        {
            if (inventoryItem.ItemWeight == ItemWeight1)
            {
                LootTracker.SetLootCount(ItemWeight1, LootTracker.MaxLootWeight);
                ListOfUIItems[index].RemoveData();
            }
            else if (inventoryItem.ItemWeight == ItemWeight2)
            {
                LootTracker.SetLootCount(ItemWeight2, LootTracker.MaxLootWeight);
                ListOfUIItems[index].RemoveData();
            }
            else if (inventoryItem.ItemWeight == ItemWeight3)
            {
                LootTracker.SetLootCount(ItemWeight3, LootTracker.MaxLootWeight);
                ListOfUIItems[index].RemoveData();
            }
            else
            {
                Debug.Log("This inventory slot has already been looted!");
            }
            ListOfUIItems[index].ItemWeight = 0;
        }        
    }

    public void SetInventorySlotData(int InventorySize)
    {
        InventorySize = raid_InventoryHandler.InventorySize;
        for (int i = 0; i < InventorySize; i++)
        {
            int RandomFurniture = Random.Range(0, 3);
            switch (RandomFurniture)
            {
                case 0:
                    ListOfUIItems[i].ItemWeight = ItemWeight1;
                    ListOfUIItems[i].SetData(Image1, ListOfUIItems[i].ItemWeight);
                    break;
                case 1:
                    if (raid_InventoryHandler.MediumItemMaxAmount <= 0)
                    {
                        ListOfUIItems[i].ItemWeight = ItemWeight1;
                        ListOfUIItems[i].SetData(Image1, ListOfUIItems[i].ItemWeight);
                    }
                    else
                    {
                        ListOfUIItems[i].ItemWeight = ItemWeight2;
                        ListOfUIItems[i].SetData(Image2, ListOfUIItems[i].ItemWeight);
                        raid_InventoryHandler.MediumItemMaxAmount -= 1;
                    }
                    break;
                case 2:
                    if (raid_InventoryHandler.LargeItemMaxAmount <= 0)
                    {
                        ListOfUIItems[i].ItemWeight = ItemWeight1;
                        ListOfUIItems[i].SetData(Image1, ListOfUIItems[i].ItemWeight);
                    }
                    else
                    {
                        ListOfUIItems[i].ItemWeight = ItemWeight3;
                        ListOfUIItems[i].SetData(Image3, ListOfUIItems[i].ItemWeight);
                        raid_InventoryHandler.LargeItemMaxAmount -= 1;
                    }
                    break;
            }
        }
    }
}
