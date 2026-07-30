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
    [SerializeField] private GameObject Heart;
    [SerializeField] private bool firstItem = true;
    [SerializeField, Min(1)] private int trapAmount = 3;
    [SerializeField, Min(0f)] private float freezeDuration = 10f;
    [SerializeField, Min(1f)] private float doubleWeightMultiplier = 2f;
    [SerializeField, Header("Trap settings")] private bool showTrapIndicators = true;

    [SerializeField] BombData[] Bombs;

    List<Raid_InventoryItem> ListOfUIItems = new List<Raid_InventoryItem>();
    private readonly Dictionary<int, int> itemIndicesByInstanceId = new Dictionary<int, int>();

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
            UIItem.ConfigureSharedReferences(raid_References, Heart, raid_Timer);
            UIItem.SetTrapIndicatorVisible(showTrapIndicators);
            ListOfUIItems.Add(UIItem);
            itemIndicesByInstanceId[UIItem.GetInstanceID()] = i;
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

        if (!itemIndicesByInstanceId.TryGetValue(inventoryItem.GetInstanceID(), out int index))
        {
            return;
        }

        if (RaidMatchmakingController.Instance != null && RaidMatchmakingController.Instance.ControlsInventorySetup)
        {
            if (!CanLoot(index, out _))
            {
                return;
            }

            RaidMatchmakingController.Instance.RequestLoot(index);
            return;
        }

        HandleItemLootingRPC(index, inventoryItem.ItemWeight);
    }

    public void HandleItemLootingRPC(int index, float itemWeight)
    {
        ResolveReferences();
        TryStartTimerOnFirstLoot();

        RaidPlayerIconData actorContext = new RaidPlayerIconData(
            GetLocalEventPlayerName(),
            GetLocalEventCharacterId(),
            GetLocalEventAvatarData());

        ProcessLoot(index, itemWeight, 1f, true, actorContext, false);
    }

    public void HandleNetworkLootAccepted(int index, int actorNumber, float lootWeightMultiplier, bool triggeredByLocalPlayer, string playerName = null, CharacterID actorCharacterId = CharacterID.None, AvatarData actorAvatarData = null)
    {
        ResolveReferences();

        RaidPlayerIconData actorContext = new RaidPlayerIconData(playerName, actorCharacterId, actorAvatarData);
        ProcessLoot(index, 0f, lootWeightMultiplier, triggeredByLocalPlayer, actorContext, true, true);
    }

    private void ProcessLoot(int index, float expectedItemWeight, float networkLootWeightMultiplier, bool isLocalLoot, RaidPlayerIconData actorContext, bool skipWeightValidation, bool ignoreFreeze = false)
    {
        if (!CanLoot(index, out Raid_InventoryItem item, ignoreFreeze, isLocalLoot))
        {
            return;
        }

        if (!skipWeightValidation && !Mathf.Approximately(expectedItemWeight, item.ItemWeight))
        {
            return;
        }

        GameFurniture furniture = item.FurnitureData;
        Raid_InventoryItem.TrapType trapType = item.CurrentTrapType;
        int trapTypeValue = (int)trapType;
        bool isTrap = item.HasTrap;
        float lootWeightMultiplier = skipWeightValidation
            ? networkLootWeightMultiplier
            : ResolveLocalLootWeightMultiplier(isTrap, trapType);

        if (isTrap)
        {
            item.TriggerTrap();
            eventLog?.LogTrapTriggered(actorContext.PlayerName, trapTypeValue, actorContext.CharacterId, actorContext.AvatarData);
        }

        if (LootItem(item, lootWeightMultiplier, isLocalLoot))
        {
            eventLog?.LogLootTaken(actorContext.PlayerName, furniture, lootWeightMultiplier, actorContext.CharacterId, actorContext.AvatarData);
        }

        if (isTrap && isLocalLoot)
        {
            ApplyTrapEffect(trapType);
        }
    }

    private bool LootItem(Raid_InventoryItem item, float lootWeightMultiplier, bool addToLootTracker)
    {
        GameFurniture furniture = item.FurnitureData;
        if (furniture == null)
        {
            return false;
        }

        Sprite lootSprite = item.CurrentItemSprite != null ? item.CurrentItemSprite : furniture?.FurnitureInfo?.Image;
        item.LaunchBall(lootSprite);

        if (addToLootTracker)
        {
            if (LootTracker == null)
            {
                Debug.LogError("Cannot loot item because Raid_InventoryPage is missing Raid_LootTracking reference.", this);
                return false;
            }

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
        RandomizeInventoryContent(InventorySize, new UnityInventoryRandom(), true);
    }

    public void RandomizeInventoryContentDeterministic(int InventorySize, int seed)
    {
        RandomizeInventoryContent(InventorySize, new SystemInventoryRandom(seed), false);
    }

    private void RandomizeInventoryContent(int inventorySize, IInventoryRandom random, bool updateInventoryHandlerAmounts)
    {
        CategorizeFurnitureByWeight(
            ListOfFurniture,
            out List<GameFurniture> smallItemList,
            out List<GameFurniture> mediumItemList,
            out List<GameFurniture> largeItemList);

        if (smallItemList.Count == 0 && mediumItemList.Count == 0 && largeItemList.Count == 0)
        {
            Debug.LogError("Raid inventory cannot be generated because no furniture was found.");
            return;
        }

        int largeRemaining = raid_InventoryHandler != null ? raid_InventoryHandler.LargeItemMaxAmount : 0;
        int mediumRemaining = raid_InventoryHandler != null ? raid_InventoryHandler.MediumItemMaxAmount : 0;
        for (int i = 0; i < inventorySize; i++)
        {
            int choice = random.Range(0, 3);
            bool itemSet = false;

            switch (choice)
            {
                case 0:
                    if (largeRemaining > 0 && TrySetInventorySlotData(largeItemList, i, random))
                    {
                        largeRemaining--;
                        itemSet = true;
                    }
                    break;
                case 1:
                    if (mediumRemaining > 0 && TrySetInventorySlotData(mediumItemList, i, random))
                    {
                        mediumRemaining--;
                        itemSet = true;
                    }
                    break;
            }

            if (!itemSet && TrySetInventorySlotData(smallItemList, i, random))
            {
                itemSet = true;
            }

            if (!itemSet && mediumRemaining > 0 && TrySetInventorySlotData(mediumItemList, i, random))
            {
                mediumRemaining--;
                itemSet = true;
            }

            if (!itemSet && largeRemaining > 0 && TrySetInventorySlotData(largeItemList, i, random))
            {
                largeRemaining--;
                itemSet = true;
            }

            if (!itemSet)
            {
                break;
            }
        }

        if (updateInventoryHandlerAmounts && raid_InventoryHandler != null)
        {
            raid_InventoryHandler.LargeItemMaxAmount = largeRemaining;
            raid_InventoryHandler.MediumItemMaxAmount = mediumRemaining;
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
                bombType = (Raid_InventoryItem.TrapType)(i < 3 ? i : Random.Range(0, 3))
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
        return item != null && item.HasTrap && item.CurrentTrapType == Raid_InventoryItem.TrapType.DoubleNextLootWeight ? doubleWeightMultiplier : 1f;
    }

    public bool CanRequestLoot(int index, bool ignoreFreeze = false, bool ignoreRaidEnded = false)
    {
        ResolveReferences();
        return CanLoot(index, out _, ignoreFreeze, true, ignoreRaidEnded);
    }

    public bool HasLootableItem(int index)
    {
        Raid_InventoryItem item = GetInventoryItem(index);
        return item != null && item.ItemWeight > 0f && item.FurnitureData != null;
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

    private bool TrySetInventorySlotData(List<GameFurniture> furnitureSet, int uiIndex, IInventoryRandom random)
    {
        if (furnitureSet == null || furnitureSet.Count == 0 || uiIndex < 0 || uiIndex >= ListOfUIItems.Count)
        {
            return false;
        }

        int furnitureIndex = random.Range(0, furnitureSet.Count);
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

        return null;
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
            raid_Timer = raid_References != null
                ? raid_References.RaidTimer
                : null;
        }

        if (Heart == null && raid_References != null)
        {
            Heart = raid_References.Heart;
        }

        if (Heart == null)
        {
            Heart = GameObject.FindWithTag("Heart");
        }

        if (exitraid == null)
        {
            exitraid = ExitRaid.Instance != null ? ExitRaid.Instance : FindObjectOfType<ExitRaid>();
        }

        if (eventLog == null && raid_References != null)
        {
            eventLog = raid_References.EventLog;
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
                bombs.Add(new BombData { bombIndex = trap.Index, bombType = (Raid_InventoryItem.TrapType)trap.Type });
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
        itemIndicesByInstanceId.Clear();
    }

    private bool CanLoot(int index, out Raid_InventoryItem item, bool ignoreFreeze = false, bool requireLootTracker = true, bool ignoreRaidEnded = false)
    {
        item = GetInventoryItem(index);
        if (item == null || inventoryFrozen && !ignoreFreeze)
        {
            return false;
        }

        if (raid_Timer == null || exitraid == null || requireLootTracker && LootTracker == null)
        {
            return false;
        }

        if (raid_Timer.CurrentTime <= 0f || !ignoreRaidEnded && exitraid.raidEnded)
        {
            return false;
        }

        if (requireLootTracker && LootTracker.CurrentLootWeight > LootTracker.MaxLootWeight)
        {
            return false;
        }

        return item.ItemWeight > 0f && item.FurnitureData != null;
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

    private float ResolveLocalLootWeightMultiplier(bool isTrap, Raid_InventoryItem.TrapType trapType)
    {
        if (isTrap && trapType == Raid_InventoryItem.TrapType.DoubleNextLootWeight)
        {
            nextLootWeightDoubled = true;
        }

        return nextLootWeightDoubled ? doubleWeightMultiplier : 1f;
    }

    private void ApplyTrapEffect(Raid_InventoryItem.TrapType trapType)
    {
        switch (trapType)
        {
            case Raid_InventoryItem.TrapType.EndRaid:
                exitraid.EndRaid();
                break;
            case Raid_InventoryItem.TrapType.Freeze:
                Raid_EventPopup.Show(this, Raid_EventPopup.Scenario.Freeze, freezeDuration);
                StartFreeze();
                break;
            case Raid_InventoryItem.TrapType.DoubleNextLootWeight:
                Raid_EventPopup.Show(this, Raid_EventPopup.Scenario.DoubleWeight);
                break;
        }
    }

    [System.Serializable]
    public class BombData
    {
        public int bombIndex;
        public Raid_InventoryItem.TrapType bombType;
    }

    private interface IInventoryRandom
    {
        int Range(int minInclusive, int maxExclusive);
    }

    private readonly struct UnityInventoryRandom : IInventoryRandom
    {
        public int Range(int minInclusive, int maxExclusive)
        {
            return Random.Range(minInclusive, maxExclusive);
        }
    }

    private readonly struct SystemInventoryRandom : IInventoryRandom
    {
        private readonly System.Random rng;

        public SystemInventoryRandom(int seed)
        {
            rng = new System.Random(seed);
        }

        public int Range(int minInclusive, int maxExclusive)
        {
            return rng.Next(minInclusive, maxExclusive);
        }
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

    private static void CategorizeFurnitureByWeight(
        List<GameFurniture> furnitureList,
        out List<GameFurniture> smallItems,
        out List<GameFurniture> mediumItems,
        out List<GameFurniture> largeItems)
    {
        smallItems = new List<GameFurniture>();
        mediumItems = new List<GameFurniture>();
        largeItems = new List<GameFurniture>();

        if (furnitureList == null)
        {
            return;
        }

        for (int i = 0; i < furnitureList.Count; i++)
        {
            GameFurniture furniture = furnitureList[i];
            if (furniture == null)
            {
                continue;
            }

            if (furniture.Weight >= 0f && furniture.Weight <= 50f)
            {
                smallItems.Add(furniture);
            }

            if (furniture.Weight >= 50f && furniture.Weight <= 80f)
            {
                mediumItems.Add(furniture);
            }

            if (furniture.Weight >= 80.1f && furniture.Weight <= 999f)
            {
                largeItems.Add(furniture);
            }
        }
    }
}
