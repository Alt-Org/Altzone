using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts.ReferenceSheets;
using UnityEngine.UI;
using Photon.Realtime;
using MenuUI.Scripts.SoulHome;

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
    [SerializeField] private bool firstItem = true;
    [SerializeField, Min(1)] private int trapAmount = 3;
    [SerializeField, Min(0f)] private float freezeDuration = 10f;
    [SerializeField, Min(1f)] private float doubleWeightMultiplier = 2f;
    [SerializeField, Header("Trap settings")] private bool showTrapIndicators = true;

    [System.Serializable]
    public class BombData
    {
        public int bombIndex;
        [Tooltip("Trap type: 0 = end raid, 1 = freeze, 2 = double next loot weight.")]
        public int bombType;
    }
    [SerializeField] BombData[] Bombs;

    List<Raid_InventoryItem> ListOfUIItems = new List<Raid_InventoryItem>();

    List<GameFurniture> ListOfFurniture = new List<GameFurniture>();
    private bool inventoryFrozen;
    private bool nextLootWeightDoubled;
    private Coroutine freezeRoutine;

    private void Awake()
    {
        ResolveReferences();
        if (LootTracker != null)
        {
            LootTracker.ResetLootCount();
        }

        eventLog = eventLog != null ? eventLog : Raid_EventLog.FindForInventory(transform);
        eventLog?.Clear();
    }

    public List<GameFurniture> GetGameFurniture()
    {
        StorageFurnitureReference storageFurnitureReference = StorageFurnitureReference.Instance;
        return storageFurnitureReference != null
            ? storageFurnitureReference.GetAllGameFurniture()
            : new List<GameFurniture>();
    }

    public void InitializeInventoryUI(int InventorySize, RaidPhotonRoom.TrapData[] trapData = null)
    {
        ResolveReferences();
        eventLog = eventLog != null ? eventLog : Raid_EventLog.FindForInventory(transform);
        eventLog?.Clear();
        ClearInventoryUI();
        ListOfFurniture = GetGameFurniture();

        if (ItemPrefab == null || ContentPanel == null)
        {
            Debug.LogError("Raid inventory cannot initialize because item prefab or content panel is missing.", this);
            return;
        }

        for (int i = 0; i < InventorySize; i++)
        {
            Raid_InventoryItem UIItem = Instantiate(ItemPrefab, ContentPanel);
            UIItem.transform.localScale = new Vector3(1, 1, 0);
            UIItem.SetTrapIndicatorVisible(showTrapIndicators);
            ListOfUIItems.Add(UIItem);
            UIItem.OnItemClicked += HandleItemLooting;
        }

        if (trapData == null)
        {
            RandomizeBombs();
        }
        else
        {
            SetBombsFromTrapData(trapData);
        }
        ApplyBombsToInventory();
    }

    private void ApplyBombsToInventory()
    {
        if (Bombs == null)
        {
            return;
        }

        for (int j = 0; j < Bombs.Length; j++)
        {
            BombData bombData = Bombs[j];
            if (bombData != null && bombData.bombIndex >= 0 && bombData.bombIndex < ListOfUIItems.Count)
            {
                ListOfUIItems[bombData.bombIndex].SetTrap(bombData.bombType);
            }
        }
    }

    public void HandleItemLooting(Raid_InventoryItem inventoryItem)
    {
        if (inventoryItem == null)
        {
            return;
        }

        int index = ListOfUIItems.IndexOf(inventoryItem);
        if (RaidMatchmakingController.Instance != null && RaidMatchmakingController.Instance.ControlsInventorySetup)
        {
            RaidMatchmakingController.Instance.RequestLoot(index);
            return;
        }

        HandleItemLootingRPC(index, inventoryItem.ItemWeight);
    }

    public void HandleItemLootingRPC(int index, float itemWeight)
    {
        ResolveReferences();
        TryStartTimerOnFirstLoot();

        LootActorContext actorContext = new LootActorContext(
            GetLocalEventPlayerName(),
            GetLocalEventCharacterId(),
            GetLocalEventAvatarData());

        ProcessLoot(index, itemWeight, 1f, true, actorContext, true, false);
    }

    public void HandleNetworkLootAccepted(int index, int actorNumber, float lootWeightMultiplier, bool triggeredByLocalPlayer, string playerName = null, CharacterID actorCharacterId = CharacterID.None, AvatarData actorAvatarData = null)
    {
        ResolveReferences();

        LootActorContext actorContext = new LootActorContext(playerName, actorCharacterId, actorAvatarData);
        ProcessLoot(index, 0f, lootWeightMultiplier, triggeredByLocalPlayer, actorContext, triggeredByLocalPlayer, true);
    }

    private void ProcessLoot(int index, float expectedItemWeight, float networkLootWeightMultiplier, bool applyTrapEffect, LootActorContext actorContext, bool addToLootTracker, bool skipWeightValidation)
    {
        if (!CanLoot(index, out Raid_InventoryItem item))
        {
            return;
        }

        if (!skipWeightValidation && !Mathf.Approximately(expectedItemWeight, item.ItemWeight))
        {
            return;
        }

        GameFurniture furniture = item.furnitureData;
        int trapType = item._bombType;
        bool isTrap = item.bomb;
        float lootWeightMultiplier = skipWeightValidation
            ? networkLootWeightMultiplier
            : ResolveLocalLootWeightMultiplier(isTrap, trapType);

        if (isTrap)
        {
            if (applyTrapEffect)
            {
                item.TriggerTrap();
            }

            eventLog?.LogTrapTriggered(actorContext.PlayerName, trapType, actorContext.CharacterId, actorContext.AvatarData);
        }

        if (LootItem(item, lootWeightMultiplier, addToLootTracker))
        {
            eventLog?.LogLootTaken(actorContext.PlayerName, furniture, lootWeightMultiplier, actorContext.CharacterId, actorContext.AvatarData);
        }

        item.ItemWeight = 0f;

        if (isTrap && applyTrapEffect)
        {
            ApplyTrapEffect(trapType);
        }
    }

    private bool LootItem(Raid_InventoryItem item, float lootWeightMultiplier, bool addToLootTracker)
    {
        GameFurniture furniture = item.furnitureData;
        if (LootTracker == null || furniture == null)
        {
            if (LootTracker == null)
            {
                Debug.LogError("Cannot loot item because Raid_InventoryPage is missing Raid_LootTracking reference.", this);
            }

            return false;
        }

        Sprite lootSprite = item.CurrentItemSprite != null ? item.CurrentItemSprite : furniture?.FurnitureInfo?.Image;
        item.LaunchBall(lootSprite);

        if (addToLootTracker)
        {
            LootTracker.SetLootCount(furniture, lootWeightMultiplier);
            nextLootWeightDoubled = false;
        }

        item.RemoveData();

        return true;
    }

    public List<GameFurniture> WeightQuery(List<GameFurniture> furnitureList, float LowestWeight, float HighestWeight)
    {
        List<GameFurniture> results = new List<GameFurniture>();
        if (furnitureList == null)
        {
            return results;
        }

        for (int i = 0; i < furnitureList.Count; i++)
        {
            GameFurniture furniture = furnitureList[i];
            if (furniture != null && furniture.Weight >= LowestWeight && furniture.Weight <= HighestWeight)
            {
                results.Add(furniture);
            }
        }

        return results;
    } 


    public void RandomizeInventoryContent(int InventorySize)
    {
        List<GameFurniture> SmallItemList = WeightQuery(ListOfFurniture,0f,50f);
        List<GameFurniture> MediumItemList = WeightQuery(ListOfFurniture,50f,80f);
        List<GameFurniture> LargeItemList = WeightQuery(ListOfFurniture,80.1f,999f);

        if (SmallItemList.Count == 0 && MediumItemList.Count == 0 && LargeItemList.Count == 0)
        {
            Debug.LogError("Raid inventory cannot be generated because no furniture was found.");
            return;
        }

        for (int i = 0; i < InventorySize; i++)
        {
            int RandomFurniture;
            int x = Random.Range(0, 3);
            switch (x)
            {
                case 0:
                    if (raid_InventoryHandler != null && raid_InventoryHandler.LargeItemMaxAmount != 0 && LargeItemList.Count != 0)
                    {
                        RandomFurniture = Random.Range(0,LargeItemList.Count);
                        SetInventorySlotDataRPC(LargeItemList,i,RandomFurniture);
                        raid_InventoryHandler.LargeItemMaxAmount--;
                    } else {goto case 1;}
                    break;
                case 1:
                    if (raid_InventoryHandler != null && raid_InventoryHandler.MediumItemMaxAmount != 0 && MediumItemList.Count != 0)
                    {
                        RandomFurniture = Random.Range(0,MediumItemList.Count);
                        SetInventorySlotDataRPC(MediumItemList,i,RandomFurniture);
                        raid_InventoryHandler.MediumItemMaxAmount--;
                    } else {goto case 2;}
                    break;
                case 2:
                    if (SmallItemList.Count != 0)
                    {
                        RandomFurniture = Random.Range(0,SmallItemList.Count);
                        SetInventorySlotDataRPC(SmallItemList,i,RandomFurniture);
                    }
                    else if (!TrySetRandomInventorySlotData(MediumItemList, i) && !TrySetRandomInventorySlotData(LargeItemList, i))
                    {
                        return;
                    }
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
        if (ListOfUIItems.Count == 0)
        {
            Bombs = Array.Empty<BombData>();
            return;
        }

        int amount = Mathf.Clamp(trapAmount, 1, ListOfUIItems.Count);
        Bombs = new BombData[amount];

        List<int> availableIndices = BuildSequentialIndexList(ListOfUIItems.Count);
        ShuffleWithUnityRandom(availableIndices);

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
        if (inventorySize <= 0)
        {
            return Array.Empty<RaidPhotonRoom.TrapData>();
        }

        int amount = Mathf.Clamp(trapAmount, 1, inventorySize);
        System.Random rng = new System.Random(seed);
        List<int> availableIndices = BuildSequentialIndexList(inventorySize);

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
    public void SendBombLocationsRPC(string jsonBombs)
    {
        Bombs = string.IsNullOrWhiteSpace(jsonBombs)
            ? Array.Empty<BombData>()
            : JsonUtility.FromJson<BombData[]>(jsonBombs);
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
    
    public void SetInventorySlotDataRPC(List<GameFurniture> furnitureSet,int UiIndex, int FurnitureIndex)
    {
        if (furnitureSet == null
            || UiIndex < 0
            || UiIndex >= ListOfUIItems.Count
            || FurnitureIndex < 0
            || FurnitureIndex >= furnitureSet.Count
            || ListOfUIItems[UiIndex] == null)
        {
            return;
        }

        GameFurniture furniture = furnitureSet[FurnitureIndex];
        ListOfUIItems[UiIndex].SetData(furniture);
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

    private bool TrySetRandomInventorySlotData(List<GameFurniture> furnitureSet, int uiIndex)
    {
        if (furnitureSet == null || furnitureSet.Count == 0 || uiIndex < 0 || uiIndex >= ListOfUIItems.Count)
        {
            return false;
        }

        SetInventorySlotDataRPC(furnitureSet, uiIndex, Random.Range(0, furnitureSet.Count));
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
            raid_References = Raid_References.Instance;
        }

        if (LootTracker == null)
        {
            LootTracker = raid_References != null
                ? raid_References.LootTracking
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

    }

    private void SetBombsFromTrapData(RaidPhotonRoom.TrapData[] trapData)
    {
        if (trapData == null)
        {
            Bombs = Array.Empty<BombData>();
            return;
        }

        List<BombData> bombs = new List<BombData>(trapData.Length);
        for (int i = 0; i < trapData.Length; i++)
        {
            RaidPhotonRoom.TrapData trap = trapData[i];
            if (trap.Index >= 0 && trap.Index < ListOfUIItems.Count)
            {
                bombs.Add(new BombData { bombIndex = trap.Index, bombType = trap.Type });
            }
        }

        Bombs = bombs.ToArray();
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

    private bool CanLoot(int index, out Raid_InventoryItem item)
    {
        item = GetInventoryItem(index);
        if (item == null || inventoryFrozen)
        {
            return false;
        }

        if (raid_Timer == null || LootTracker == null || exitraid == null)
        {
            return false;
        }

        if (raid_Timer.CurrentTime <= 0f || LootTracker.CurrentLootWeight > LootTracker.MaxLootWeight || exitraid.raidEnded)
        {
            return false;
        }

        return item.ItemWeight > 0f && item.furnitureData != null;
    }

    private void TryStartTimerOnFirstLoot()
    {
        if (!firstItem || raid_Timer == null)
        {
            return;
        }

        raid_Timer.StartTimer();
        firstItem = false;
    }

    private float ResolveLocalLootWeightMultiplier(bool isTrap, int trapType)
    {
        if (isTrap && trapType == 2)
        {
            nextLootWeightDoubled = true;
        }

        return nextLootWeightDoubled ? doubleWeightMultiplier : 1f;
    }

    private void ApplyTrapEffect(int trapType)
    {
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
    }

    private readonly struct LootActorContext
    {
        public LootActorContext(string playerName, CharacterID characterId, AvatarData avatarData)
        {
            PlayerName = playerName;
            CharacterId = characterId;
            AvatarData = avatarData;
        }

        public string PlayerName { get; }
        public CharacterID CharacterId { get; }
        public AvatarData AvatarData { get; }
    }

    private static List<int> BuildSequentialIndexList(int count)
    {
        List<int> indices = new List<int>(Mathf.Max(0, count));
        for (int i = 0; i < count; i++)
        {
            indices.Add(i);
        }

        return indices;
    }

    private static void ShuffleWithUnityRandom(List<int> values)
    {
        for (int i = values.Count - 1; i > 0; i--)
        {
            int swapIndex = Random.Range(0, i + 1);
            (values[i], values[swapIndex]) = (values[swapIndex], values[i]);
        }
    }
}
