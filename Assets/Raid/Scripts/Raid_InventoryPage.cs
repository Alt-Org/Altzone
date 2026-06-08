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
using MenuUI.Scripts.SoulHome;
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
    [SerializeField] private bool spectator = false;
    [SerializeField] private bool firstItem = true;
    [SerializeField, Min(1)] private int trapAmount = 3;
    [SerializeField, Min(0f)] private float freezeDuration = 10f;
    [SerializeField, Min(1f)] private float doubleWeightMultiplier = 2f;

    [System.Serializable]
    public class BombData
    {
        public int bombIndex;
         //type 0: end game, type 1: freeze, type 2: double weight
        public int bombType;
    }
    [SerializeField] BombData[] Bombs;

    List<Raid_InventoryItem> ListOfUIItems = new List<Raid_InventoryItem>();

    List<GameFurniture> ListOfFurniture = new List<GameFurniture>();
    private bool inventoryFrozen;
    private bool nextLootWeightDoubled;
    private Coroutine freezeRoutine;

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

    // This function will get all game furniture from the "StorageFurnitureReference"
    public List<GameFurniture> GetGameFurniture()
    {   
        List<GameFurniture> furnitures = null;
        StorageFurnitureReference storageFurnitureReference = StorageFurnitureReference.Instance;
        furnitures = storageFurnitureReference.GetAllGameFurniture();
        
        /* Debug print furnitures
        foreach (GameFurniture furniture in furnitures)
        {
            Debug.Log("(RAID) Nimi : "+furniture.ToString());
        }
        */

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
            ListOfUIItems[Bombs[j].bombIndex].GetComponent<Raid_InventoryItem>().SetTrap(Bombs[j].bombType);
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
        if (inventoryFrozen)
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
                item.GetComponent<Raid_InventoryItem>().TriggerTrap();
                if (item.GetComponent<Raid_InventoryItem>()._bombType == 2)
                {
                    nextLootWeightDoubled = true;
                }
                LootItem(item, itemWeight);
                switch (item.GetComponent<Raid_InventoryItem>()._bombType)
                {
                    case 0:
                        exitraid.EndRaid();
                        break;
                    case 1:
                        Raid_EventPopup.Show(this, Raid_EventPopup.Scenario.Freeze, freezeDuration);
                        StartFreeze();
                        break;
                    case 2:
                        Raid_EventPopup.Show(this, Raid_EventPopup.Scenario.DoubleWeight);
                        break;
                }
                ListOfUIItems[index].ItemWeight = 0;
                return;
            }
            LootItem(item, itemWeight);
            ListOfUIItems[index].ItemWeight = 0;
        }
    }

    private void LootItem(Raid_InventoryItem item, float itemWeight)
    {
        item.LaunchBall();

        if (itemWeight == item.ItemWeight && itemWeight != 0)
        {
            float lootWeightMultiplier = nextLootWeightDoubled ? doubleWeightMultiplier : 1f;
            LootTracker.SetLootCount(item.furnitureData, LootTracker.MaxLootWeight, lootWeightMultiplier);
            nextLootWeightDoubled = false;
            item.RemoveData();
        } else
        {
            Debug.Log("This inventory slot has already been looted!");
        }
    }

    // Get a list of furniture between LowestWeight and HighestWeight, also it accepts any List<GameFurniture> to allow for seasonal furniture sets
    public List<GameFurniture> WeightQuery(List<GameFurniture> furnitureList, float LowestWeight, float HighestWeight)
    {
        return furnitureList.Where(f => f.Weight >= LowestWeight && f.Weight <= HighestWeight).ToList();
    } 


    public void RandomizeInventoryContent(int InventorySize)
    {
        // InventorySize = raid_InventoryHandler.InventorySize;

        // Split all furnitures into lists
        List<GameFurniture> SmallItemList = WeightQuery(ListOfFurniture,0f,50f);
        List<GameFurniture> MediumItemList = WeightQuery(ListOfFurniture,50f,80f);
        List<GameFurniture> LargeItemList = WeightQuery(ListOfFurniture,80.1f,999f); 

        // Just so the next one knows that a list might be empty, the spawning logic should still run unless all lists are empty?
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

        for (int i = 0; i < InventorySize; i++)
        {
            int RandomFurniture;
            //_photonView.RPC(nameof(SetInventorySlotDataRPC), RpcTarget.All, i, RandomFurniture);

            // Randomize inventory content and enforce item limits (TODO) randomize better
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
        int amount = Mathf.Clamp(trapAmount, 1, ListOfUIItems.Count);
        Bombs = new BombData[amount];

        List<int> availableIndices = Enumerable.Range(0, ListOfUIItems.Count).OrderBy(_ => Random.value).Take(amount).ToList();

        for (int i = 0; i < amount; i++)
        {
            Bombs[i] = new BombData
            {
                bombIndex = availableIndices[i],
                bombType = i < 3 ? i : Random.Range(0, 3)
            };
        }
    }
    /*[PunRPC]*/
    public void SendBombLocationsRPC(string jsonBombs)
    {
        Bombs = JsonUtility.FromJson<BombData[]>(jsonBombs);
    }

    // Locks items around passed index.
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

    private void StartFreeze()
    {
        if (freezeRoutine != null)
        {
            StopCoroutine(freezeRoutine);
        }

        freezeRoutine = StartCoroutine(FreezeInventoryRoutine());
    }

    private IEnumerator FreezeInventoryRoutine()
    {
        inventoryFrozen = true;
        yield return new WaitForSeconds(freezeDuration);
        inventoryFrozen = false;
        freezeRoutine = null;
    }
    
    // Sets the UI slot to a choosen item.
    public void SetInventorySlotDataRPC(List<GameFurniture> furnitureSet,int UiIndex, int FurnitureIndex)
    {
        GameFurniture furniture = furnitureSet[FurnitureIndex];
        ListOfUIItems[UiIndex].SetData(furniture);
        return;
    }
}
