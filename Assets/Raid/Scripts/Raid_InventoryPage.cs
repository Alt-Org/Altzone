using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
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

    public PhotonView _photonView { get; private set; }
    private void Awake()
    {
        LootTracker.ResetLootCount();
        _photonView = gameObject.AddComponent<PhotonView>();
        _photonView.ViewID = 1;
    }

    public void InitializeInventoryUI(int InventorySize)
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
        _photonView.RPC(nameof(HandleItemLootingRPC), RpcTarget.All, index, inventoryItem.ItemWeight);
    }

    [PunRPC]
    public void HandleItemLootingRPC(int index, int itemWeight)
    {
        if (index == -1)
        {
            return;
        }
        if (raid_Timer.CurrentTime <= 0 || LootTracker.CurrentLootWeight > LootTracker.MaxLootWeight)
        {
            return;
        }
        else
        {
            if (itemWeight == ItemWeight1)
            {
                LootTracker.SetLootCount(ItemWeight1, LootTracker.MaxLootWeight);
                ListOfUIItems[index].RemoveData();
            }
            else if (itemWeight == ItemWeight2)
            {
                LootTracker.SetLootCount(ItemWeight2, LootTracker.MaxLootWeight);
                ListOfUIItems[index].RemoveData();
            }
            else if (itemWeight == ItemWeight3)
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
            _photonView.RPC(nameof(SetInventorySlotDataRPC), RpcTarget.All, i, RandomFurniture);
        }

    }

    [PunRPC]
    public void SetInventorySlotDataRPC(int Index, int RandomFurniture)
    {
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
        }
    }
}
