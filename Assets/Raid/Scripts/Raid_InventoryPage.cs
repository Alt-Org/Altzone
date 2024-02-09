using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Random = UnityEngine.Random;
using static Battle0.Scripts.Lobby.InRoom.RoomSetupManager;

public class Raid_InventoryPage : MonoBehaviour
{
    [SerializeField, Header("Assigned scripts")] private Raid_InventoryHandler raid_InventoryHandler;
    [SerializeField] private Raid_InventoryItem ItemPrefab;
    [SerializeField] private RectTransform ContentPanel;
    [SerializeField] private Raid_LootTracking LootTracker;
    [SerializeField] private Raid_Timer raid_Timer;
    [SerializeField] private ExitRaid exitraid;
    [SerializeField] private bool spectator;
    [SerializeField] private bool firstItem = true;

    [System.Serializable]
    public class BombData
    {
        public int bombIndex;
         //type 0: default, type 1: lock
        public int bombType;
    }
    [SerializeField] BombData[] Bombs;

    List<Raid_InventoryItem> ListOfUIItems = new List<Raid_InventoryItem>();
    

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

    public PhotonView _photonView { get; private set; }
    private void Awake()
    {
        LootTracker.ResetLootCount();
        _photonView = gameObject.AddComponent<PhotonView>();
        _photonView.ViewID = 2;
        if ((PlayerRole)PhotonNetwork.LocalPlayer.CustomProperties["Role"] == PlayerRole.Spectator)
        {
            spectator = true;
        }

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

        if (PhotonNetwork.IsMasterClient)
        {
            //RandomizeBombs();
            //string jsonBombs = JsonUtility.ToJson(Bombs);
            //_photonView.RPC("SendBombLocationsRPC", RpcTarget.Others, jsonBombs);
        }
        for (int j = 0; j < Bombs.Length; j++)
        {
            Debug.Log("bombIndex: " + Bombs[j].bombIndex + " Bombs.Length: " + Bombs.Length);
            ListOfUIItems[Bombs[j].bombIndex].GetComponent<Raid_InventoryItem>().SetBomb(Bombs[j].bombType);
        }
    }

    public void HandleItemLooting(Raid_InventoryItem inventoryItem)
    {
        int index = ListOfUIItems.IndexOf(inventoryItem);
        _photonView.RPC(nameof(HandleItemLootingRPC), RpcTarget.All, index, inventoryItem.ItemWeight);
    }

    [PunRPC]
    public void HandleItemLootingRPC(int index, float itemWeight)
    {
        if (firstItem)
        {
            raid_Timer.StartTimer();
            firstItem = false;
        }
        if (index == -1)
        {
            return;
        }
        if (raid_Timer.CurrentTime <= 0 || LootTracker.CurrentLootWeight > LootTracker.MaxLootWeight || exitraid.raidEnded)
        {
            return;
        }
        else
        {
            if (ListOfUIItems[index].GetComponent<Raid_InventoryItem>().bomb)
            {
                ListOfUIItems[index].RemoveData();
                ListOfUIItems[index].GetComponent<Raid_InventoryItem>().TriggerBomb();
                if (ListOfUIItems[index].GetComponent<Raid_InventoryItem>()._bombType == 1)
                {
                    LockItems(index);
                }
                return;
            }
            ListOfUIItems[index].LaunchBall();

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
            else if (itemWeight == ItemWeight4)
            {
                LootTracker.SetLootCount(ItemWeight4, LootTracker.MaxLootWeight);
                ListOfUIItems[index].RemoveData();
            }
            else if (itemWeight == ItemWeight5)
            {
                LootTracker.SetLootCount(ItemWeight5, LootTracker.MaxLootWeight);
                ListOfUIItems[index].RemoveData();
            }
            else if (itemWeight == ItemWeight6)
            {
                LootTracker.SetLootCount(ItemWeight6, LootTracker.MaxLootWeight);
                ListOfUIItems[index].RemoveData();
            }
            else if (itemWeight == ItemWeight7)
            {
                LootTracker.SetLootCount(ItemWeight7, LootTracker.MaxLootWeight);
                ListOfUIItems[index].RemoveData();
            }
            else if (itemWeight == ItemWeight8)
            {
                LootTracker.SetLootCount(ItemWeight8, LootTracker.MaxLootWeight);
                ListOfUIItems[index].RemoveData();
            }
            else if (itemWeight == ItemWeight9)
            {
                LootTracker.SetLootCount(ItemWeight9, LootTracker.MaxLootWeight);
                ListOfUIItems[index].RemoveData();
            }
            else if (itemWeight == ItemWeight10)
            {
                LootTracker.SetLootCount(ItemWeight10, LootTracker.MaxLootWeight);
                ListOfUIItems[index].RemoveData();
            }
            else if (itemWeight == ItemWeight11)
            {
                LootTracker.SetLootCount(ItemWeight11, LootTracker.MaxLootWeight);
                ListOfUIItems[index].RemoveData();
            }
            else if (itemWeight == ItemWeight12)
            {
                LootTracker.SetLootCount(ItemWeight12, LootTracker.MaxLootWeight);
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
            int RandomFurniture = Random.Range(0, 12);
            _photonView.RPC(nameof(SetInventorySlotDataRPC), RpcTarget.All, i, RandomFurniture);
        }

    }
    public void RandomizeBombs()
    {
        Bombs[0].bombIndex = Random.Range(0, (ListOfUIItems.Count / 3));
        Bombs[1].bombIndex = Random.Range((ListOfUIItems.Count / 3) + 1, (ListOfUIItems.Count / 3) * 2);
        Bombs[2].bombIndex = Random.Range(((ListOfUIItems.Count / 3) * 2) + 1, ListOfUIItems.Count);
    }
    [PunRPC]
    public void SendBombLocationsRPC(string jsonBombs)
    {
        Bombs = JsonUtility.FromJson<BombData[]>(jsonBombs);
    }
    public void LockItems(int index)
    {
        int column = -1;
        bool topRow = false;

        if (index == 0 || index == 1 || index == 2 || index == 3)
        {
            topRow = true;
            column = index;
        }
        if (column == -1)
            column = index % 4;

        if (column != 3)
            ListOfUIItems[index + 1].GetComponent<Raid_InventoryItem>().SetLocked();
        if (column != 0)
            ListOfUIItems[index - 1].GetComponent<Raid_InventoryItem>().SetLocked();
        if (!topRow)
            ListOfUIItems[index - 4].GetComponent<Raid_InventoryItem>().SetLocked();
        if (ListOfUIItems[index + 4])
            ListOfUIItems[index + 4].GetComponent<Raid_InventoryItem>().SetLocked();
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
