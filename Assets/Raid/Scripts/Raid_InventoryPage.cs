using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Photon.Pun;
using Random = UnityEngine.Random;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
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
    [SerializeField] private Raid_EventLog eventLog;
    private HeartScript heartScript;
    [SerializeField] private bool spectator = false;
    [SerializeField] private bool firstItem = true;
    [SerializeField, Min(1)] private int trapAmount = 3;
    [SerializeField, Min(0f)] private float freezeDuration = 10f;
    [SerializeField, Min(1f)] private float doubleWeightMultiplier = 2f;
    [SerializeField, Header("Trap settings")] private bool showTrapIndicators = true;

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
        ResolveReferences();
        if (LootTracker != null)
        {
            LootTracker.ResetLootCount();
        }

        eventLog = eventLog != null ? eventLog : Raid_EventLog.FindForInventory(transform);
        eventLog?.Clear();
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

    public void InitializeInventoryUI(int InventorySize, RaidPhotonRoom.TrapData[] trapData = null)
    {
        eventLog = eventLog != null ? eventLog : Raid_EventLog.FindForInventory(transform);
        eventLog?.Clear();
        ClearInventoryUI();
        ListOfFurniture = GetGameFurniture();

        for (int i = 0; i < InventorySize; i++)
        {
            Raid_InventoryItem UIItem = Instantiate(ItemPrefab, Vector3.zero, Quaternion.identity);
            UIItem.transform.SetParent(ContentPanel);
            UIItem.transform.localScale = new Vector3(1, 1, 0);
            UIItem.SetTrapIndicatorVisible(showTrapIndicators);
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

        if (trapData == null)
        {
            RandomizeBombs();
        }
        else
        {
            SetBombsFromTrapData(trapData);
        }
        //string jsonBombs = JsonUtility.ToJson(Bombs);
        //SendBombLocationsRPC(jsonBombs);

        for (int j = 0; j < Bombs.Length; j++)
        {
            Debug.Log("bombIndex: " + Bombs[j].bombIndex + " Bombs.Length: " + Bombs.Length);
            if (Bombs[j].bombIndex >= 0 && Bombs[j].bombIndex < ListOfUIItems.Count)
            {
                ListOfUIItems[Bombs[j].bombIndex].GetComponent<Raid_InventoryItem>().SetTrap(Bombs[j].bombType);
            }
        }
    }

    public void HandleItemLooting(Raid_InventoryItem inventoryItem)
    {
        int index = ListOfUIItems.IndexOf(inventoryItem);
        if (RaidMatchmakingController.Instance != null && RaidMatchmakingController.Instance.ControlsInventorySetup)
        {
            RaidMatchmakingController.Instance.RequestLoot(index);
            return;
        }

        //_photonView.RPC(nameof(HandleItemLootingRPC), RpcTarget.All, index, inventoryItem.ItemWeight);
        HandleItemLootingRPC(index,inventoryItem.ItemWeight);
    }

    /*[PunRPC]*/
    public void HandleItemLootingRPC(int index, float itemWeight)
    {
        ResolveReferences();

        if (firstItem)
        {
            if (raid_Timer == null)
            {
                return;
            }

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
        if (raid_Timer == null || LootTracker == null || exitraid == null
            || raid_Timer.CurrentTime <= 0 || LootTracker.CurrentLootWeight > LootTracker.MaxLootWeight || exitraid.raidEnded)
        {
            return;
        }
        else
        {
            Raid_InventoryItem item = ListOfUIItems[index];
            string playerName = GetLocalEventPlayerName();
            CharacterID characterId = GetLocalEventCharacterId();
            AvatarData avatarData = GetLocalEventAvatarData();
            if (item.GetComponent<Raid_InventoryItem>().bomb)
            {
                int trapType = item.GetComponent<Raid_InventoryItem>()._bombType;
                GameFurniture furniture = item.furnitureData;
                item.GetComponent<Raid_InventoryItem>().TriggerTrap();
                eventLog?.LogTrapTriggered(playerName, trapType, characterId, avatarData);
                if (trapType == 2)
                {
                    nextLootWeightDoubled = true;
                }
                float lootWeightMultiplier = nextLootWeightDoubled ? doubleWeightMultiplier : 1f;
                if (LootItem(item, itemWeight))
                {
                    eventLog?.LogLootTaken(playerName, furniture, lootWeightMultiplier, characterId, avatarData);
                }
                switch (trapType)
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
            GameFurniture furnitureData = item.furnitureData;
            if (LootItem(item, itemWeight))
            {
                eventLog?.LogLootTaken(playerName, furnitureData, 1f, characterId, avatarData);
            }
            ListOfUIItems[index].ItemWeight = 0;
        }
    }

    public void HandleNetworkLootAccepted(int index, int actorNumber, string lootOwnerId, float lootWeightMultiplier, bool triggeredByLocalPlayer, string playerName = null, CharacterID actorCharacterId = CharacterID.None, AvatarData actorAvatarData = null)
    {
        ResolveReferences();

        if (index < 0 || index >= ListOfUIItems.Count || exitraid != null && exitraid.raidEnded)
        {
            return;
        }

        Raid_InventoryItem item = ListOfUIItems[index];
        if (item == null || item.ItemWeight <= 0f || item.furnitureData == null)
        {
            return;
        }

        if (item.bomb)
        {
            int trapType = item._bombType;
            GameFurniture furniture = item.furnitureData;
            if (triggeredByLocalPlayer)
            {
                item.TriggerTrap();
            }

            eventLog?.LogTrapTriggered(playerName, trapType, actorCharacterId, actorAvatarData);
            if (LootItemForOwner(item, item.ItemWeight, lootOwnerId, lootWeightMultiplier))
            {
                eventLog?.LogLootTaken(playerName, furniture, lootWeightMultiplier, actorCharacterId, actorAvatarData);
            }

            if (!triggeredByLocalPlayer)
            {
                ListOfUIItems[index].ItemWeight = 0;
                return;
            }

            switch (trapType)
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

        GameFurniture furnitureData = item.furnitureData;
        if (LootItemForOwner(item, item.ItemWeight, lootOwnerId, lootWeightMultiplier))
        {
            eventLog?.LogLootTaken(playerName, furnitureData, lootWeightMultiplier, actorCharacterId, actorAvatarData);
        }
        ListOfUIItems[index].ItemWeight = 0;
    }

    private bool LootItem(Raid_InventoryItem item, float itemWeight)
    {
        ResolveReferences();

        if (item == null || itemWeight != item.ItemWeight || itemWeight == 0)
        {
            Debug.Log("This inventory slot has already been looted!");
            return false;
        }

        if (LootTracker == null)
        {
            Debug.LogError("Cannot loot item because Raid_InventoryPage is missing Raid_LootTracking reference.", this);
            return false;
        }

        GameFurniture furniture = item.furnitureData;
        Sprite lootSprite = item.CurrentItemSprite != null ? item.CurrentItemSprite : furniture?.FurnitureInfo?.Image;
        float lootWeightMultiplier = nextLootWeightDoubled ? doubleWeightMultiplier : 1f;
        item.LaunchBall(lootSprite, UpdateHeartRecentLootSprite);
        LootTracker.SetLootCount(furniture, LootTracker.MaxLootWeight, lootWeightMultiplier);
        nextLootWeightDoubled = false;
        item.RemoveData();
        return true;
    }

    private bool LootItemForOwner(Raid_InventoryItem item, float itemWeight, string lootOwnerId, float lootWeightMultiplier)
    {
        ResolveReferences();

        if (item == null || itemWeight != item.ItemWeight || itemWeight == 0)
        {
            Debug.Log("This inventory slot has already been looted!");
            return false;
        }

        if (LootTracker == null)
        {
            Debug.LogError("Cannot loot item because Raid_InventoryPage is missing Raid_LootTracking reference.", this);
            return false;
        }

        GameFurniture furniture = item.furnitureData;
        Sprite lootSprite = item.CurrentItemSprite != null ? item.CurrentItemSprite : furniture?.FurnitureInfo?.Image;
        Action<Sprite> updateRecentLootImage = LootTracker.IsDisplayedLootOwner(lootOwnerId)
            ? UpdateHeartRecentLootSprite
            : null;
        item.LaunchBall(lootSprite, updateRecentLootImage);
        LootTracker.SetLootOwnerLootCount(lootOwnerId, furniture, LootTracker.MaxLootWeight, lootWeightMultiplier);
        item.RemoveData();

        return true;
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

    public void RandomizeInventoryContentDeterministic(int InventorySize, int seed)
    {
        List<GameFurniture> smallItemList = WeightQuery(ListOfFurniture, 0f, 50f);
        List<GameFurniture> mediumItemList = WeightQuery(ListOfFurniture, 50f, 80f);
        List<GameFurniture> largeItemList = WeightQuery(ListOfFurniture, 80.1f, 999f);

        if (smallItemList.Count == 0 && mediumItemList.Count == 0 && largeItemList.Count == 0)
        {
            Debug.LogError("Raid inventory cannot be generated because no furniture was found.");
            return;
        }

        int largeRemaining = raid_InventoryHandler != null ? raid_InventoryHandler.LargeItemMaxAmount : 0;
        int mediumRemaining = raid_InventoryHandler != null ? raid_InventoryHandler.MediumItemMaxAmount : 0;
        System.Random rng = new System.Random(seed);

        for (int i = 0; i < InventorySize; i++)
        {
            int choice = rng.Next(0, 3);
            bool itemSet = false;

            switch (choice)
            {
                case 0:
                    if (largeRemaining > 0 && TrySetInventorySlotData(largeItemList, i, rng))
                    {
                        largeRemaining--;
                        itemSet = true;
                    }
                    break;
                case 1:
                    if (mediumRemaining > 0 && TrySetInventorySlotData(mediumItemList, i, rng))
                    {
                        mediumRemaining--;
                        itemSet = true;
                    }
                    break;
            }

            if (!itemSet && TrySetInventorySlotData(smallItemList, i, rng))
            {
                itemSet = true;
            }

            if (!itemSet && mediumRemaining > 0 && TrySetInventorySlotData(mediumItemList, i, rng))
            {
                mediumRemaining--;
                itemSet = true;
            }

            if (!itemSet && largeRemaining > 0 && TrySetInventorySlotData(largeItemList, i, rng))
            {
                largeRemaining--;
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

    public RaidPhotonRoom.TrapData[] BuildDeterministicTrapData(int inventorySize, int seed)
    {
        int amount = Mathf.Clamp(trapAmount, 1, inventorySize);
        System.Random rng = new System.Random(seed);
        List<int> availableIndices = Enumerable.Range(0, inventorySize).ToList();

        for (int i = availableIndices.Count - 1; i > 0; i--)
        {
            int swapIndex = rng.Next(i + 1);
            (availableIndices[i], availableIndices[swapIndex]) = (availableIndices[swapIndex], availableIndices[i]);
        }

        RaidPhotonRoom.TrapData[] traps = new RaidPhotonRoom.TrapData[amount];
        for (int i = 0; i < amount; i++)
        {
            int trapType = i < 3 ? i : rng.Next(0, 3);
            traps[i] = new RaidPhotonRoom.TrapData(availableIndices[i], trapType);
        }

        return traps;
    }

    public Raid_InventoryItem GetInventoryItem(int index)
    {
        if (index < 0 || index >= ListOfUIItems.Count)
        {
            return null;
        }

        return ListOfUIItems[index];
    }

    public float GetNetworkLootWeightMultiplier(int index)
    {
        Raid_InventoryItem item = GetInventoryItem(index);
        return item != null && item.bomb && item._bombType == 2 ? doubleWeightMultiplier : 1f;
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

    private bool TrySetInventorySlotData(List<GameFurniture> furnitureSet, int uiIndex, System.Random rng)
    {
        if (furnitureSet == null || furnitureSet.Count == 0 || uiIndex < 0 || uiIndex >= ListOfUIItems.Count)
        {
            return false;
        }

        int furnitureIndex = rng.Next(0, furnitureSet.Count);
        SetInventorySlotDataRPC(furnitureSet, uiIndex, furnitureIndex);
        return true;
    }

    private string GetLocalEventPlayerName()
    {
        string playerName = ServerManager.Instance?.Clan?.name;
        if (string.IsNullOrWhiteSpace(playerName))
        {
            playerName = ServerManager.Instance?.Player?.name;
        }

        return string.IsNullOrWhiteSpace(playerName) ? "Player" : playerName;
    }

    private CharacterID GetLocalEventCharacterId()
    {
        int? currentAvatarId = ServerManager.Instance?.Player?.currentAvatarId;
        if (currentAvatarId.HasValue && Enum.IsDefined(typeof(CharacterID), currentAvatarId.Value))
        {
            return (CharacterID)currentAvatarId.Value;
        }

        return CharacterID.None;
    }

    private AvatarData GetLocalEventAvatarData()
    {
        ServerPlayer serverPlayer = ServerManager.Instance?.Player;
        if (serverPlayer?.avatar != null)
        {
            return new AvatarData(serverPlayer.name, serverPlayer.avatar);
        }

        PlayerData playerData = null;
        string playerGuid = GameConfig.Get().PlayerSettings.PlayerGuid;
        if (!string.IsNullOrWhiteSpace(playerGuid))
        {
            Storefront.Get().GetPlayerData(playerGuid, data => playerData = data);
        }

        return playerData?.AvatarData;
    }

    private void ResolveReferences()
    {
        if (raid_References == null)
        {
            raid_References = FindObjectOfType<Raid_References>();
        }

        if (LootTracker == null)
        {
            LootTracker = raid_References != null && raid_References.raid_LootTracking != null
                ? raid_References.raid_LootTracking
                : null;
        }

        if (LootTracker == null)
        {
            LootTracker = FindObjectOfType<Raid_LootTracking>();
        }

        if (raid_Timer == null)
        {
            raid_Timer = FindObjectOfType<Raid_Timer>();
        }

        if (exitraid == null)
        {
            exitraid = ExitRaid.Instance != null ? ExitRaid.Instance : FindObjectOfType<ExitRaid>();
        }

        if (heartScript == null)
        {
            heartScript = raid_References != null && raid_References.Heart != null
                ? raid_References.Heart.GetComponent<HeartScript>()
                : null;
        }

        if (heartScript == null)
        {
            GameObject heart = GameObject.FindWithTag("Heart");
            if (heart != null)
            {
                heartScript = heart.GetComponent<HeartScript>();
            }
        }

        if (heartScript == null)
        {
            heartScript = FindObjectOfType<HeartScript>();
        }
    }

    private void UpdateHeartRecentLootSprite(Sprite lootSprite)
    {
        if (lootSprite == null)
        {
            return;
        }

        ResolveReferences();
        if (heartScript != null)
        {
            heartScript.AddRecentLootSprite(lootSprite);
        }
    }

    private void SetBombsFromTrapData(RaidPhotonRoom.TrapData[] trapData)
    {
        Bombs = trapData
            .Where(trap => trap.Index >= 0 && trap.Index < ListOfUIItems.Count)
            .Select(trap => new BombData { bombIndex = trap.Index, bombType = trap.Type })
            .ToArray();
    }

    private void ClearInventoryUI()
    {
        foreach (Raid_InventoryItem item in ListOfUIItems)
        {
            if (item != null)
            {
                item.OnItemClicked -= HandleItemLooting;
                Destroy(item.gameObject);
            }
        }

        ListOfUIItems.Clear();
    }
}
