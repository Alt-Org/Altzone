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
    [SerializeField] private ExitRaid exitraid;

    [SerializeField] private bool spectator = false;
    [SerializeField] private bool firstItem = true;

    [System.Serializable]
    public class BombData
    {
        public int bombIndex;
        // type 0: default, type 1: lock
        public int bombType;
    }

    [SerializeField] private BombData[] Bombs;

    private List<Raid_InventoryItem> ListOfUIItems = new List<Raid_InventoryItem>();

    [SerializeField, Header("Furniture and weight")] private Sprite Image1;
    [SerializeField] private float ItemWeight1;
    [SerializeField] private Sprite Image2;
    [SerializeField] private float ItemWeight2;
    [SerializeField] private Sprite Image3;
    [SerializeField] private float ItemWeight3;
    [SerializeField] private Sprite Image4;
    [SerializeField] private float ItemWeight4;
    [SerializeField] private Sprite Image5;
    [SerializeField] private float ItemWeight5;
    [SerializeField] private Sprite Image6;
    [SerializeField] private float ItemWeight6;
    [SerializeField] private Sprite Image7;
    [SerializeField] private float ItemWeight7;
    [SerializeField] private Sprite Image8;
    [SerializeField] private float ItemWeight8;
    [SerializeField] private Sprite Image9;
    [SerializeField] private float ItemWeight9;
    [SerializeField] private Sprite Image10;
    [SerializeField] private float ItemWeight10;
    [SerializeField] private Sprite Image11;
    [SerializeField] private float ItemWeight11;
    [SerializeField] private Sprite Image12;
    [SerializeField] private float ItemWeight12;

    private void Awake()
    {
        LootTracker?.ResetLootCount();
    }

    public void InitializeInventoryUI(int InventorySize)
    {
        foreach (var item in ListOfUIItems)
        {
            if (item != null) Destroy(item.gameObject);
        }
        ListOfUIItems.Clear();

        for (int i = 0; i < InventorySize; i++)
        {
            Raid_InventoryItem UIItem = Instantiate(ItemPrefab, Vector3.zero, Quaternion.identity);
            UIItem.transform.SetParent(ContentPanel, false);
            UIItem.transform.localScale = Vector3.one;
            UIItem.OnItemClicked += HandleItemLooting;
            UIItem.SetSpectator(spectator);
            ListOfUIItems.Add(UIItem);
        }

        for (int j = 0; j < Bombs.Length; j++)
        {
            if (Bombs[j] == null) continue;
            if (Bombs[j].bombIndex >= 0 && Bombs[j].bombIndex < ListOfUIItems.Count)
            {
                ListOfUIItems[Bombs[j].bombIndex].SetBomb(Bombs[j].bombType);
            }
        }
    }
    public void HandleItemLooting(Raid_InventoryItem inventoryItem)
    {
        int index = ListOfUIItems.IndexOf(inventoryItem);
        if (index < 0) return;

        HandleItemLootingRPC(index, inventoryItem.ItemWeight);
    }

    public void HandleItemLootingRPC(int index, float itemWeight)
    {
        if (firstItem)
        {
            raid_Timer?.StartTimer();
            firstItem = false;
        }

        if (index < 0 || index >= ListOfUIItems.Count) return;

        if (raid_Timer != null && raid_Timer.CurrentTime <= 0) return;
        if (LootTracker != null && LootTracker.CurrentLootWeight > LootTracker.MaxLootWeight) return;
        if (exitraid != null && exitraid.raidEnded) return;

        var slot = ListOfUIItems[index];
        if (slot == null) return;

        if (slot.bomb)
        {
            slot.RemoveData();
            slot.TriggerBomb();
            if (slot._bombType == 1)
            {
                LockItems(index);
            }
            return;
        }

        slot.LaunchBall();

        if (Mathf.Approximately(itemWeight, ItemWeight1))
        {
            LootTracker?.SetLootCount(ItemWeight1, LootTracker.MaxLootWeight);
            slot.RemoveData();
        }
        else if (Mathf.Approximately(itemWeight, ItemWeight2))
        {
            LootTracker?.SetLootCount(ItemWeight2, LootTracker.MaxLootWeight);
            slot.RemoveData();
        }
        else if (Mathf.Approximately(itemWeight, ItemWeight3))
        {
            LootTracker?.SetLootCount(ItemWeight3, LootTracker.MaxLootWeight);
            slot.RemoveData();
        }
        else if (Mathf.Approximately(itemWeight, ItemWeight4))
        {
            LootTracker?.SetLootCount(ItemWeight4, LootTracker.MaxLootWeight);
            slot.RemoveData();
        }
        else if (Mathf.Approximately(itemWeight, ItemWeight5))
        {
            LootTracker?.SetLootCount(ItemWeight5, LootTracker.MaxLootWeight);
            slot.RemoveData();
        }
        else if (Mathf.Approximately(itemWeight, ItemWeight6))
        {
            LootTracker?.SetLootCount(ItemWeight6, LootTracker.MaxLootWeight);
            slot.RemoveData();
        }
        else if (Mathf.Approximately(itemWeight, ItemWeight7))
        {
            LootTracker?.SetLootCount(ItemWeight7, LootTracker.MaxLootWeight);
            slot.RemoveData();
        }
        else if (Mathf.Approximately(itemWeight, ItemWeight8))
        {
            LootTracker?.SetLootCount(ItemWeight8, LootTracker.MaxLootWeight);
            slot.RemoveData();
        }
        else if (Mathf.Approximately(itemWeight, ItemWeight9))
        {
            LootTracker?.SetLootCount(ItemWeight9, LootTracker.MaxLootWeight);
            slot.RemoveData();
        }
        else if (Mathf.Approximately(itemWeight, ItemWeight10))
        {
            LootTracker?.SetLootCount(ItemWeight10, LootTracker.MaxLootWeight);
            slot.RemoveData();
        }
        else if (Mathf.Approximately(itemWeight, ItemWeight11))
        {
            LootTracker?.SetLootCount(ItemWeight11, LootTracker.MaxLootWeight);
            slot.RemoveData();
        }
        else if (Mathf.Approximately(itemWeight, ItemWeight12))
        {
            LootTracker?.SetLootCount(ItemWeight12, LootTracker.MaxLootWeight);
            slot.RemoveData();
        }
        else
        {
            Debug.Log("This inventory slot has already been looted or weight not recognized!");
        }
        slot.ItemWeight = 0;
    }

    public void SetInventorySlotData()
    {
        if (raid_InventoryHandler == null)
        {
            Debug.LogWarning("raid_InventoryHandler not assigned - can't set inventory slot data.");
            return;
        }

        int InventorySize = raid_InventoryHandler.InventorySize;
        raid_InventoryHandler.MediumItemMaxAmount = raid_InventoryHandler.MediumItemMaxAmount;
        raid_InventoryHandler.LargeItemMaxAmount = raid_InventoryHandler.LargeItemMaxAmount;

        for (int i = 0; i < Mathf.Min(InventorySize, ListOfUIItems.Count); i++)
        {
            int RandomFurniture = Random.Range(0, 12);
            SetInventorySlotDataRPC(i, RandomFurniture);
        }

        if (Bombs == null || Bombs.Length == 0)
        {
            RandomizeBombs();
            for (int j = 0; j < Bombs.Length; j++)
            {
                if (Bombs[j] == null) continue;
                if (Bombs[j].bombIndex >= 0 && Bombs[j].bombIndex < ListOfUIItems.Count)
                {
                    ListOfUIItems[Bombs[j].bombIndex].SetBomb(Bombs[j].bombType);
                }
            }
        }
    }

    public void RandomizeBombs()
    {
        if (ListOfUIItems.Count == 0) return;
        if (Bombs == null || Bombs.Length < 3)
        {
            Bombs = new BombData[3];
            for (int i = 0; i < 3; i++) Bombs[i] = new BombData() { bombType = 0, bombIndex = 0 };
        }

        Bombs[0].bombIndex = Random.Range(0, (ListOfUIItems.Count / 3));
        Bombs[1].bombIndex = Random.Range((ListOfUIItems.Count / 3) + 1, (ListOfUIItems.Count / 3) * 2);
        Bombs[2].bombIndex = Random.Range(((ListOfUIItems.Count / 3) * 2) + 1, ListOfUIItems.Count);
    }
    public void SendBombLocationsRPC(string jsonBombs)
    {
        try
        {
            Bombs = JsonUtility.FromJson<BombData[]>(jsonBombs);
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Failed to parse bombs json: " + ex.Message);
        }
    }

    public void LockItems(int index)
    {
        if (index < 0 || index >= ListOfUIItems.Count) return;

        int column = -1;
        bool topRow = false;

        if (index >= 0 && index <= 3)
        {
            topRow = true;
            column = index;
        }
        if (column == -1)
            column = index % 4;

        if (column != 3 && index + 1 < ListOfUIItems.Count)
            ListOfUIItems[index + 1]?.SetLocked();
        if (column != 0 && index - 1 >= 0)
            ListOfUIItems[index - 1]?.SetLocked();
        if (!topRow && index - 4 >= 0)
            ListOfUIItems[index - 4]?.SetLocked();
        if (index + 4 < ListOfUIItems.Count)
            ListOfUIItems[index + 4]?.SetLocked();
    }

    public void SetInventorySlotDataRPC(int Index, int RandomFurniture)
    {
        if (Index < 0 || Index >= ListOfUIItems.Count) return;

        switch (RandomFurniture)
        {
            case 0:
                ListOfUIItems[Index].ItemWeight = ItemWeight1;
                ListOfUIItems[Index].SetData(Image1, ListOfUIItems[Index].ItemWeight);
                break;
            case 1:
                if (raid_InventoryHandler.MediumItemMaxAmount <= 0)
                {
                    ListOfUIItems[Index].ItemWeight = ItemWeight1;
                    ListOfUIItems[Index].SetData(Image1, ListOfUIItems[Index].ItemWeight);
                }
                else
                {
                    ListOfUIItems[Index].ItemWeight = ItemWeight2;
                    ListOfUIItems[Index].SetData(Image2, ListOfUIItems[Index].ItemWeight);
                    raid_InventoryHandler.MediumItemMaxAmount -= 1;
                }
                break;
            case 2:
                if (raid_InventoryHandler.LargeItemMaxAmount <= 0)
                {
                    ListOfUIItems[Index].ItemWeight = ItemWeight1;
                    ListOfUIItems[Index].SetData(Image1, ListOfUIItems[Index].ItemWeight);
                }
                else
                {
                    ListOfUIItems[Index].ItemWeight = ItemWeight3;
                    ListOfUIItems[Index].SetData(Image3, ListOfUIItems[Index].ItemWeight);
                    raid_InventoryHandler.LargeItemMaxAmount -= 1;
                }
                break;
            case 3:
                if (raid_InventoryHandler.LargeItemMaxAmount <= 0)
                {
                    ListOfUIItems[Index].ItemWeight = ItemWeight1;
                    ListOfUIItems[Index].SetData(Image1, ListOfUIItems[Index].ItemWeight);
                }
                else
                {
                    ListOfUIItems[Index].ItemWeight = ItemWeight4;
                    ListOfUIItems[Index].SetData(Image4, ListOfUIItems[Index].ItemWeight);
                    raid_InventoryHandler.LargeItemMaxAmount -= 1;
                }
                break;
            case 4:
                if (raid_InventoryHandler.LargeItemMaxAmount <= 0)
                {
                    ListOfUIItems[Index].ItemWeight = ItemWeight1;
                    ListOfUIItems[Index].SetData(Image1, ListOfUIItems[Index].ItemWeight);
                }
                else
                {
                    ListOfUIItems[Index].ItemWeight = ItemWeight5;
                    ListOfUIItems[Index].SetData(Image5, ListOfUIItems[Index].ItemWeight);
                    raid_InventoryHandler.LargeItemMaxAmount -= 1;
                }
                break;
            case 5:
                ListOfUIItems[Index].ItemWeight = ItemWeight6;
                ListOfUIItems[Index].SetData(Image6, ListOfUIItems[Index].ItemWeight);
                break;
            case 6:
                ListOfUIItems[Index].ItemWeight = ItemWeight7;
                ListOfUIItems[Index].SetData(Image7, ListOfUIItems[Index].ItemWeight);
                break;
            case 7:
                if (raid_InventoryHandler.MediumItemMaxAmount <= 0)
                {
                    ListOfUIItems[Index].ItemWeight = ItemWeight1;
                    ListOfUIItems[Index].SetData(Image1, ListOfUIItems[Index].ItemWeight);
                }
                else
                {
                    ListOfUIItems[Index].ItemWeight = ItemWeight8;
                    ListOfUIItems[Index].SetData(Image8, ListOfUIItems[Index].ItemWeight);
                    raid_InventoryHandler.MediumItemMaxAmount -= 1;
                }
                break;
            case 8:
                if (raid_InventoryHandler.MediumItemMaxAmount <= 0)
                {
                    ListOfUIItems[Index].ItemWeight = ItemWeight1;
                    ListOfUIItems[Index].SetData(Image1, ListOfUIItems[Index].ItemWeight);
                }
                else
                {
                    ListOfUIItems[Index].ItemWeight = ItemWeight9;
                    ListOfUIItems[Index].SetData(Image9, ListOfUIItems[Index].ItemWeight);
                    raid_InventoryHandler.MediumItemMaxAmount -= 1;
                }
                break;
            case 9:
                ListOfUIItems[Index].ItemWeight = ItemWeight10;
                ListOfUIItems[Index].SetData(Image10, ListOfUIItems[Index].ItemWeight);
                break;
            case 10:
                ListOfUIItems[Index].ItemWeight = ItemWeight11;
                ListOfUIItems[Index].SetData(Image11, ListOfUIItems[Index].ItemWeight);
                break;
            case 11:
                if (raid_InventoryHandler.LargeItemMaxAmount <= 0)
                {
                    ListOfUIItems[Index].ItemWeight = ItemWeight1;
                    ListOfUIItems[Index].SetData(Image1, ListOfUIItems[Index].ItemWeight);
                }
                else
                {
                    ListOfUIItems[Index].ItemWeight = ItemWeight12;
                    ListOfUIItems[Index].SetData(Image12, ListOfUIItems[Index].ItemWeight);
                    raid_InventoryHandler.LargeItemMaxAmount -= 1;
                }
                break;
        }
    }
}
