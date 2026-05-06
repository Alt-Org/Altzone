using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Photon.Pun;
using Random = UnityEngine.Random;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.ReferenceSheets;
using UnityEngine.UI;
using System.Linq;
using Photon.Realtime;
//using static MenuUI.Scripts.Lobby.InRoom.RoomSetupManager;

public class Raid_InventoryPage : MonoBehaviour
{
    [SerializeField, Header("Assigned scripts")] private Raid_InventoryHandler raid_InventoryHandler;
    [SerializeField] private Raid_References raid_References;
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

    List<GameFurniture> ListOfFurniture = new List<GameFurniture>();

    //public PhotonView _photonView { get; private set; }
    private void Awake()
    {
        LootTracker.ResetLootCount();
        //_photonView = gameObject.AddComponent<PhotonView>();
        //_photonView.ViewID = 2;
        //if ((PlayerRole)PhotonNetwork.LocalPlayer.CustomProperties["Role"] == PlayerRole.Spectator)
        {
            spectator = false;
        }

    }

    public List<GameFurniture> GetGameFurniture()
    {   
        List<GameFurniture> furnitures = null;
        // (Todo)GetGameFurniture Altzone.Scripts.ReferenceSheets
        StorageFurnitureReference storageFurnitureReference = StorageFurnitureReference.Instance;
        furnitures = storageFurnitureReference.GetAllGameFurniture();
        
        foreach (GameFurniture furniture in furnitures)
        {
            Debug.Log("(RAID) Nimi : "+furniture.ToString());
        }

        return furnitures;
    }

    public void InitializeInventoryUI(int InventorySize)
    {
        ListOfFurniture = GetGameFurniture();

        for (int i = 0; i < InventorySize; i++)
        {
            Raid_InventoryItem UIItem = Instantiate(ItemPrefab, Vector3.zero, Quaternion.identity);
            UIItem.transform.SetParent(ContentPanel);
            UIItem.transform.localScale = new Vector3(1, 1, 0);
            ListOfUIItems.Add(UIItem);
            UIItem.OnItemClicked += HandleItemLooting;
        }

        /*
        if (PhotonNetwork.IsMasterClient)
        {
            RandomizeBombs();
            string jsonBombs = JsonUtility.ToJson(Bombs);
            _photonView.RPC("SendBombLocationsRPC", RpcTarget.Others, jsonBombs);
        }
        */

        RandomizeBombs();
        //string jsonBombs = JsonUtility.ToJson(Bombs);
        //SendBombLocationsRPC(jsonBombs);

        for (int j = 0; j < Bombs.Length; j++)
        {
            Debug.Log("bombIndex: " + Bombs[j].bombIndex + " Bombs.Length: " + Bombs.Length);
            ListOfUIItems[Bombs[j].bombIndex].GetComponent<Raid_InventoryItem>().SetBomb(Bombs[j].bombType);
        }
    }

    public void HandleItemLooting(Raid_InventoryItem inventoryItem)
    {
        int index = ListOfUIItems.IndexOf(inventoryItem);
        //_photonView.RPC(nameof(HandleItemLootingRPC), RpcTarget.All, index, inventoryItem.ItemWeight);
        HandleItemLootingRPC(index,inventoryItem.ItemWeight);
    }

    /*[PunRPC]*/
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
            Raid_InventoryItem item = ListOfUIItems[index];
            if (item.GetComponent<Raid_InventoryItem>().bomb)
            {
                item.RemoveData();
                item.GetComponent<Raid_InventoryItem>().TriggerBomb();
                if (item.GetComponent<Raid_InventoryItem>()._bombType == 1)
                {
                    LockItems(index);
                }
                return;
            }
            item.LaunchBall();

            if (itemWeight == item.ItemWeight && itemWeight != 0)
            {
                LootTracker.SetLootCount(item.furnitureData,LootTracker.MaxLootWeight);
                item.RemoveData();
            } else
            {
                Debug.Log("This inventory slot has already been looted!");
            }
            ListOfUIItems[index].ItemWeight = 0;
        }
    }

    public List<GameFurniture> WeightQuery(float LowestWeight, float HighestWeight)
    {
        return ListOfFurniture.Where(f => f.Weight >= LowestWeight && f.Weight <= HighestWeight).ToList();
    } 

    public void SetInventorySlotData(int InventorySize)
    {
        // InventorySize = raid_InventoryHandler.InventorySize;

        for (int i = 0; i < InventorySize; i++)
        {
            // int RandomFurniture = Random.Range(0, 12);
            int RandomFurniture = 0;
            //_photonView.RPC(nameof(SetInventorySlotDataRPC), RpcTarget.All, i, RandomFurniture);
            List<GameFurniture> SmallItemList = WeightQuery(0f,50f);
            List<GameFurniture> MediumItemList = WeightQuery(50f,80f);
            List<GameFurniture> LargeItemList = WeightQuery(80.1f,999f); 
            // Ainuttakaan yli 80 painavaa huonekkalua ei ole niin en oikeen tiedä mitä largella sitten ajetaan takaa

            if (LargeItemList.Count == 0)
            {
                Debug.Log("HUOM LargeItemList on tyhjä!"); 
            } if (MediumItemList.Count == 0)
            {
                Debug.Log("HUOM MediumItemList on tyhjä!");
            }if (SmallItemList.Count == 0)
            {
                Debug.Log("HUOM SmallItemList on tyhjä!");
            }
            int x = Random.Range(0, 3);
            switch (x)
            {
                case 0:
                    if (raid_InventoryHandler.LargeItemMaxAmount != 0 && LargeItemList.Count != 0)
                    {
                        RandomFurniture = Random.Range(0,LargeItemList.Count);
                        SetInventorySlotDataRPC(LargeItemList,i,RandomFurniture);
                        raid_InventoryHandler.LargeItemMaxAmount--;
                    } else {goto case 1;} 
                    break;
                case 1:
                    if (raid_InventoryHandler.MediumItemMaxAmount != 0 && MediumItemList.Count != 0)
                    {
                        RandomFurniture = Random.Range(0,MediumItemList.Count);
                        SetInventorySlotDataRPC(MediumItemList,i,RandomFurniture);
                        raid_InventoryHandler.MediumItemMaxAmount--;
                    } else {goto case 2;}
                    break;
                case 2:
                    RandomFurniture = Random.Range(0,SmallItemList.Count);
                    SetInventorySlotDataRPC(SmallItemList,i,RandomFurniture);
                    break;
            }
        }

    }
    public void RandomizeBombs()
    {
        Bombs[0].bombIndex = Random.Range(0, (ListOfUIItems.Count / 3));
        Bombs[1].bombIndex = Random.Range((ListOfUIItems.Count / 3) + 1, (ListOfUIItems.Count / 3) * 2);
        Bombs[2].bombIndex = Random.Range(((ListOfUIItems.Count / 3) * 2) + 1, ListOfUIItems.Count);
    }
    /*[PunRPC]*/
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
    
    public void SetInventorySlotDataRPC(List<GameFurniture> furnitureSet,int Index, int RandomFurniture)
    {
        GameFurniture furniture = furnitureSet[RandomFurniture];
        ListOfUIItems[Index].SetData(furniture);
        return;
    }
}