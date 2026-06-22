using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;
using UnityEngine.UI;

public class HeartScript : MonoBehaviour
{
    private static readonly Vector2 LossHaloPadding = new Vector2(200f, 200f);
    private static readonly Vector2 LossHaloOffset = Vector2.zero;

    public Raid_LootTracking raid_LootTracking;
    [SerializeField, Min(1)] private int recentLootSlotCount = 3;
    [SerializeField] private Image[] recentLootImages;
    private readonly List<Sprite> recentLootSprites = new List<Sprite>();

    private void Awake()
    {
        ResolveLootTracker();
        ResolveRecentLootImages();
        if (raid_LootTracking != null)
        {
            UpdateRecentLootImages(raid_LootTracking.ListOfCollectedLoot);
        }
        else
        {
            UpdateRecentLootImages(null);
        }
    }

    private void OnEnable()
    {
        ResolveLootTracker();
        if (raid_LootTracking != null)
        {
            raid_LootTracking.CollectedLootChanged -= OnCollectedLootChanged;
            raid_LootTracking.CollectedLootChanged += OnCollectedLootChanged;
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

        ResolveLootTracker();
        Raid_LiveInventory liveInventory = ResolveLiveInventory();
        if (liveInventory != null && raid_LootTracking != null)
        {
            liveInventory.Show(raid_LootTracking);
        }
    }

    public void UpdateRecentLootImages(IReadOnlyList<GameFurniture> collectedLoot)
    {
        ResolveRecentLootImages();

        if (recentLootImages == null)
        {
            return;
        }

        int collectedCount = collectedLoot != null ? collectedLoot.Count : 0;
        for (int i = 0; i < recentLootImages.Length; i++)
        {
            Image recentLootImage = recentLootImages[i];
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

    public void UpdateRecentLootSprites(IReadOnlyList<Sprite> lootSprites)
    {
        ResolveRecentLootImages();
        SyncRecentLootSprites(lootSprites);

        if (recentLootImages == null)
        {
            return;
        }

        int spriteCount = lootSprites != null ? lootSprites.Count : 0;
        for (int i = 0; i < recentLootImages.Length; i++)
        {
            Image recentLootImage = recentLootImages[i];
            if (recentLootImage == null)
            {
                continue;
            }

            int spriteIndex = spriteCount - 1 - i;
            Sprite sprite = spriteIndex >= 0 ? lootSprites[spriteIndex] : null;
            recentLootImage.sprite = sprite;
            recentLootImage.enabled = sprite != null;
            recentLootImage.gameObject.SetActive(sprite != null);
        }
    }

    public void AddRecentLootSprite(Sprite lootSprite)
    {
        if (lootSprite == null)
        {
            return;
        }

        recentLootSprites.Add(lootSprite);
        UpdateRecentLootSprites(recentLootSprites);
    }

    public void SetLossHaloVisible(bool visible)
    {
        Raid_RedHalo.SetVisible(gameObject, visible, LossHaloPadding, LossHaloOffset);
    }

    private void SyncRecentLootSprites(IReadOnlyList<Sprite> lootSprites)
    {
        if (ReferenceEquals(lootSprites, recentLootSprites))
        {
            return;
        }

        recentLootSprites.Clear();
        if (lootSprites == null)
        {
            return;
        }

        for (int i = 0; i < lootSprites.Count; i++)
        {
            recentLootSprites.Add(lootSprites[i]);
        }
    }

    private void OnCollectedLootChanged()
    {
        UpdateRecentLootImages(raid_LootTracking != null ? raid_LootTracking.ListOfCollectedLoot : null);
    }

    private Raid_LiveInventory ResolveLiveInventory()
    {
        Raid_LiveInventory liveInventory = Raid_LiveInventory.Instance;
        if (liveInventory != null)
        {
            return liveInventory;
        }

        liveInventory = FindObjectOfType<Raid_LiveInventory>(true);
        if (liveInventory != null)
        {
            return liveInventory;
        }

        Raid_LiveInventory prefab = Resources.Load<Raid_LiveInventory>("Prefabs/LiveInventory");
        if (prefab == null)
        {
            Debug.LogError("Cannot open raid live inventory because Prefabs/LiveInventory is missing.");
            return null;
        }

        Transform parent = null;
        Raid_References raidReferences = FindObjectOfType<Raid_References>();
        if (raidReferences != null && raidReferences.EndMenu != null)
        {
            parent = raidReferences.EndMenu.transform.parent;
        }

        if (parent == null)
        {
            parent = transform.root;
        }

        liveInventory = Instantiate(prefab, parent, false);
        liveInventory.name = "LiveInventory";
        return liveInventory;
    }

    private void ResolveLootTracker()
    {
        if (raid_LootTracking != null)
        {
            return;
        }

        Raid_References raidReferences = FindObjectOfType<Raid_References>();
        if (raidReferences != null && raidReferences.raid_LootTracking != null)
        {
            raid_LootTracking = raidReferences.raid_LootTracking;
            return;
        }

        raid_LootTracking = FindObjectOfType<Raid_LootTracking>();
    }

    private void ResolveRecentLootImages()
    {
        if (HasRecentLootImages())
        {
            return;
        }

        int slotCount = Mathf.Max(1, recentLootSlotCount);
        List<Image> resolvedImages = new List<Image>();
        for (int i = 1; i <= slotCount; i++)
        {
            Transform slot = transform.Find($"RecentLootImage{i}");
            if (slot != null && slot.TryGetComponent(out Image image))
            {
                resolvedImages.Add(image);
                continue;
            }

            Debug.LogWarning($"Recent loot image slot RecentLootImage{i} is missing from {name}. Add it to the HeartPanel prefab instead of creating it at runtime.", this);
        }

        recentLootImages = resolvedImages.ToArray();
    }

    private bool HasRecentLootImages()
    {
        if (recentLootImages == null || recentLootImages.Length == 0)
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
