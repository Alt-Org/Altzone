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
    private const int DefaultRecentLootSlotCount = 3;
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

        int slotCount = recentLootSlotCount > 0 ? recentLootSlotCount : DefaultRecentLootSlotCount;
        List<Image> resolvedImages = new List<Image>();
        for (int i = 1; i <= slotCount; i++)
        {
            Transform slot = transform.Find($"RecentLootImage{i}");
            if (slot != null && slot.TryGetComponent(out Image image))
            {
                resolvedImages.Add(image);
                continue;
            }

            resolvedImages.Add(CreateRecentLootImage(i));
        }

        recentLootImages = resolvedImages.ToArray();
    }

    private Image CreateRecentLootImage(int slotIndex)
    {
        GameObject imageObject = new GameObject($"RecentLootImage{slotIndex}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(transform, false);
        imageObject.SetActive(false);

        RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
        ApplyRecentLootImageLayout(rectTransform, slotIndex);
        rectTransform.SetSiblingIndex(Mathf.Max(0, slotIndex - 1));

        Image image = imageObject.GetComponent<Image>();
        image.raycastTarget = false;
        image.preserveAspect = true;
        image.enabled = false;
        return image;
    }

    private void ApplyRecentLootImageLayout(RectTransform rectTransform, int slotIndex)
    {
        if (rectTransform == null)
        {
            return;
        }

        switch (slotIndex)
        {
            case 1:
                rectTransform.anchorMin = new Vector2(0.35f, 0.35f);
                rectTransform.anchorMax = new Vector2(0.68f, 0.72f);
                break;
            case 2:
                rectTransform.anchorMin = new Vector2(0.13f, 0.38f);
                rectTransform.anchorMax = new Vector2(0.44f, 0.78f);
                break;
            default:
                rectTransform.anchorMin = new Vector2(0.52f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.78f, 0.86f);
                break;
        }

        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
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
