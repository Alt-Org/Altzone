using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;
using UnityEngine.UI;

public class HeartScript : MonoBehaviour
{
    private static readonly Vector2 LossHaloPadding = new Vector2(200f, 200f);
    private static readonly Vector2 LossHaloOffset = Vector2.zero;

    [SerializeField] private Raid_References raidReferences;
    [SerializeField] private Raid_LootTracking raid_LootTracking;
    [SerializeField, Min(1)] private int recentLootSlotCount = 3;
    [SerializeField] private List<Image> recentLootImages = new List<Image>();

    private Raid_References RaidReferences
    {
        get
        {
            ResolveRaidReferences();
            return raidReferences;
        }
    }

    private Raid_LootTracking LootTracking
    {
        get
        {
            ResolveLootTracker();
            return raid_LootTracking;
        }
    }

    private IReadOnlyList<Image> RecentLootImages
    {
        get
        {
            ResolveRecentLootImages();
            return recentLootImages;
        }
    }

    private void Awake()
    {
        Raid_LootTracking lootTracking = LootTracking;
        UpdateRecentLootImages(lootTracking != null ? lootTracking.ListOfCollectedLoot : null);
    }

    private void OnEnable()
    {
        Raid_LootTracking lootTracking = LootTracking;
        if (lootTracking != null)
        {
            lootTracking.CollectedLootChanged += OnCollectedLootChanged;
        }
    }

    private void OnDisable()
    {
        if (raid_LootTracking != null)
        {
            raid_LootTracking.CollectedLootChanged -= OnCollectedLootChanged;
        }
    }

    public void OpenLiveInventory()
    {
        ExitRaid exitRaid = ExitRaid.Instance;
        if (exitRaid != null && exitRaid.raidEnded)
        {
            return;
        }

        Raid_LiveInventory liveInventory = RaidReferences != null ? RaidReferences.LiveInventory : Raid_LiveInventory.Instance;
        if (liveInventory != null)
        {
            liveInventory.Show();
        }
    }

    public void UpdateRecentLootImages(IReadOnlyList<GameFurniture> collectedLoot)
    {
        IReadOnlyList<Image> images = RecentLootImages;

        if (images == null)
        {
            return;
        }

        int collectedCount = collectedLoot != null ? collectedLoot.Count : 0;
        for (int i = 0; i < images.Count; i++)
        {
            Image recentLootImage = images[i];
            if (recentLootImage == null)
            {
                continue;
            }

            int collectedIndex = collectedCount - 1 - i;
            Sprite sprite = collectedIndex >= 0 ? collectedLoot[collectedIndex]?.FurnitureInfo?.Image : null;
            recentLootImage.sprite = sprite;
            recentLootImage.enabled = sprite != null;
            recentLootImage.gameObject.SetActive(sprite != null);
        }
    }

    public void SetLossHaloVisible(bool visible)
    {
        Raid_RedHalo.SetVisible(gameObject, visible, LossHaloPadding, LossHaloOffset);
    }

    private void OnCollectedLootChanged()
    {
        Raid_LootTracking lootTracking = LootTracking;
        UpdateRecentLootImages(lootTracking != null ? lootTracking.ListOfCollectedLoot : null);
    }

    private void ResolveLootTracker()
    {
        if (raid_LootTracking != null)
        {
            return;
        }

        Raid_References references = RaidReferences;
        if (references != null && references.LootTracking != null)
        {
            raid_LootTracking = references.LootTracking;
            return;
        }

        raid_LootTracking = FindObjectOfType<Raid_LootTracking>();
    }

    private void ResolveRaidReferences()
    {
        if (raidReferences != null)
        {
            return;
        }

        raidReferences = Raid_References.Instance;
    }

    private void ResolveRecentLootImages()
    {
        if (HasRecentLootImages())
        {
            return;
        }

        recentLootImages ??= new List<Image>();
        recentLootImages.Clear();
        int slotCount = Mathf.Max(1, recentLootSlotCount);
        for (int i = 1; i <= slotCount; i++)
        {
            Transform slot = transform.Find($"RecentLootImage{i}");
            if (slot != null && slot.TryGetComponent(out Image image))
            {
                recentLootImages.Add(image);
                continue;
            }

            Debug.LogWarning($"Recent loot image slot RecentLootImage{i} is missing from {name}. Add it to the HeartPanel prefab instead of creating it at runtime.", this);
        }
    }

    private bool HasRecentLootImages()
    {
        if (recentLootImages == null || recentLootImages.Count == 0)
        {
            return false;
        }

        foreach (Image image in recentLootImages)
        {
            if (image != null)
            {
                return true;
            }
        }

        return false;
    }
}
