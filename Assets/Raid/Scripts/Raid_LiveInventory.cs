using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Raid_LiveInventory : MonoBehaviour
{
    private delegate bool RemoveCollectedLoot(int lootIndex);

    [SerializeField] private RectTransform content;
    [SerializeField] private Raid_InventoryItem collectedLootItemPrefab;
    [SerializeField] private GameObject menuRoot;
    [SerializeField] private TMP_Text spaceRemainingText;
    [SerializeField] private Button backButton;
    [SerializeField] private Raid_References raidReferences;

    private Raid_LootTracking lootTracking;
    private readonly List<Raid_InventoryItem> collectedLootItems = new();

    public static Raid_LiveInventory Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null || Instance == this)
        {
            Instance = this;
        }

        EnsureInitialized();
        Hide();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        UnsubscribeLootTracking();
    }

    private void OnDisable()
    {
        UnsubscribeLootTracking();
    }

    public void Show()
    {
        Show(ResolveLootTracking());
    }

    public void Show(Raid_LootTracking lootTracking)
    {
        if (lootTracking == null)
        {
            Debug.LogError("Cannot show raid live inventory because loot tracking is missing.");
            return;
        }

        EnsureInitialized();
        UnsubscribeLootTracking();
        this.lootTracking = lootTracking;
        this.lootTracking.CollectedLootChanged += Refresh;

        SetBackButtonVisible();
        Refresh();
        SetVisible(true);
    }

    public void Hide()
    {
        UnsubscribeLootTracking();
        SetVisible(false);
    }

    public void HideLiveInventory()
    {
        Hide();
    }

    private void Refresh()
    {
        if (lootTracking == null)
        {
            return;
        }

        SetCollectedLoot(lootTracking.ListOfCollectedLoot);
        SetSpaceRemainingText(lootTracking.CurrentLootWeight, lootTracking.MaxLootWeight);
    }

    private void SetCollectedLoot(IReadOnlyList<GameFurniture> lootList)
    {
        if (content == null || lootList == null)
        {
            HideUnusedCollectedLootItems(0);
            return;
        }

        int displayedLootCount = 0;
        for (int i = 0; i < lootList.Count; i++)
        {
            if (!TryShowCollectedLootIcon(lootList[i], i, displayedLootCount))
            {
                continue;
            }

            displayedLootCount++;
        }

        HideUnusedCollectedLootItems(displayedLootCount);
    }

    private bool TryShowCollectedLootIcon(GameFurniture furniture, int lootIndex, int itemIndex)
    {
        if (collectedLootItemPrefab == null || furniture?.FurnitureInfo?.Image == null || content == null)
        {
            return false;
        }

        Raid_InventoryItem lootItem = GetOrCreateCollectedLootItem(itemIndex);
        if (lootItem == null)
        {
            return false;
        }

        lootItem.transform.SetSiblingIndex(itemIndex);
        lootItem.SetData(furniture);
        lootItem.SetShowItemWeightText(true);
        lootItem.gameObject.SetActive(true);

        SetRemoveButton(lootItem, true, lootIndex);
        return true;
    }

    private Raid_InventoryItem GetOrCreateCollectedLootItem(int itemIndex)
    {
        if (itemIndex < collectedLootItems.Count)
        {
            return collectedLootItems[itemIndex];
        }

        Raid_InventoryItem lootItem = Instantiate(collectedLootItemPrefab, content);
        lootItem.name = "CollectedLootItem";
        ConfigureRemoveButton(lootItem);
        collectedLootItems.Add(lootItem);
        return lootItem;
    }

    private void HideUnusedCollectedLootItems(int firstUnusedIndex)
    {
        for (int i = firstUnusedIndex; i < collectedLootItems.Count; i++)
        {
            if (collectedLootItems[i] != null)
            {
                SetRemoveButton(collectedLootItems[i], false, -1);
                collectedLootItems[i].gameObject.SetActive(false);
            }
        }
    }

    private void SetSpaceRemainingText(float currentLootWeight, float maxLootWeight)
    {
        if (spaceRemainingText != null)
        {
            spaceRemainingText.text = $"{currentLootWeight:F0} kg\n/{maxLootWeight:F0} kg";
        }
    }

    private void SetBackButtonVisible()
    {
        if (backButton != null)
        {
            backButton.gameObject.SetActive(true);
        }
    }

    private void SetVisible(bool visible)
    {
        EnsureInitialized();

        if (menuRoot != null && menuRoot.activeSelf != visible)
        {
            menuRoot.SetActive(visible);
        }
    }

    private void EnsureInitialized()
    {
        if (menuRoot == null)
        {
            menuRoot = gameObject;
        }

        ResolveBackButton();
        if (backButton != null)
        {
            backButton.onClick.RemoveListener(Hide);
            backButton.onClick.AddListener(Hide);
        }
    }

    private void ResolveBackButton()
    {
        if (backButton != null || menuRoot == null)
        {
            return;
        }

        Transform backTransform = FindChildRecursive(menuRoot.transform, "LiveInventoryBackButton");
        if (backTransform != null)
        {
            backButton = backTransform.GetComponent<Button>();
        }
    }

    private Raid_LootTracking ResolveLootTracking()
    {
        if (raidReferences == null)
        {
            raidReferences = Raid_References.Instance;
        }

        return raidReferences != null
            ? raidReferences.LootTracking
            : FindObjectOfType<Raid_LootTracking>();
    }

    private void UnsubscribeLootTracking()
    {
        if (lootTracking != null)
        {
            lootTracking.CollectedLootChanged -= Refresh;
            lootTracking = null;
        }
    }

    private void ConfigureRemoveButton(Raid_InventoryItem lootItem)
    {
        if (lootItem == null)
        {
            return;
        }

        Button removeButton = FindRemoveButton(lootItem.transform);
        if (removeButton == null)
        {
            return;
        }

        LiveInventoryRemoveButton removeHandler = removeButton.GetComponent<LiveInventoryRemoveButton>();
        if (removeHandler == null)
        {
            removeHandler = removeButton.gameObject.AddComponent<LiveInventoryRemoveButton>();
        }

        removeHandler.Initialize(removeButton);
    }

    private void SetRemoveButton(Raid_InventoryItem lootItem, bool visible, int lootIndex)
    {
        Button removeButton = FindRemoveButton(lootItem != null ? lootItem.transform : null);
        if (removeButton == null)
        {
            return;
        }

        removeButton.gameObject.SetActive(visible);
        LiveInventoryRemoveButton removeHandler = removeButton.GetComponent<LiveInventoryRemoveButton>();
        if (removeHandler != null)
        {
            RemoveCollectedLoot removeCollectedLoot = visible && lootTracking != null
                ? lootTracking.RemoveCollectedLootAt
                : null;
            removeHandler.SetRemoveAction(removeCollectedLoot, lootIndex);
        }
    }

    private static Button FindRemoveButton(Transform parent)
    {
        Transform existing = parent != null ? parent.Find("RemoveCollectedLootButton") : null;
        return existing != null && existing.TryGetComponent(out Button existingButton)
            ? existingButton
            : null;
    }

    private static Transform FindChildRecursive(Transform root, string childName)
    {
        if (root == null)
        {
            return null;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            if (child.name == childName)
            {
                return child;
            }

            Transform nested = FindChildRecursive(child, childName);
            if (nested != null)
            {
                return nested;
            }
        }

        return null;
    }

    private sealed class LiveInventoryRemoveButton : MonoBehaviour
    {
        private Button button;
        private RemoveCollectedLoot removeCollectedLoot;
        private int lootIndex = -1;

        public void Initialize(Button button)
        {
            if (this.button == button)
            {
                return;
            }

            if (this.button != null)
            {
                this.button.onClick.RemoveListener(OnClick);
            }

            this.button = button;
            if (this.button != null)
            {
                this.button.onClick.AddListener(OnClick);
            }
        }

        public void SetRemoveAction(RemoveCollectedLoot removeCollectedLoot, int lootIndex)
        {
            this.removeCollectedLoot = removeCollectedLoot;
            this.lootIndex = lootIndex;
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(OnClick);
            }
        }

        private void OnClick()
        {
            removeCollectedLoot?.Invoke(lootIndex);
        }
    }
}
