using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Raid_LiveInventory : MonoBehaviour
{
    [SerializeField] private RectTransform content;
    [SerializeField] private Raid_InventoryItem collectedLootItemPrefab;
    [SerializeField] private GameObject menuRoot;
    [SerializeField] private TMP_Text spaceRemainingText;
    [SerializeField] private Button backButton;
    [SerializeField] private Vector3 collectedLootItemScale = Vector3.one;
    [SerializeField] private Vector2 collectedLootGridCellSize = new Vector2(325f, 430f);
    [SerializeField] private Vector2 collectedLootGridSpacing = new Vector2(180f, 310f);
    [SerializeField] private int collectedLootGridPaddingTop = 80;
    [SerializeField] private int collectedLootGridPaddingBottom = 40;
    [SerializeField] private int collectedLootColumnCount = 3;
    [SerializeField] private Raid_References raidReferences;

    private Raid_LootTracking lootTracking;
    private Transform liveTimerPanel;
    private Transform liveExitRaidButton;
    private LiveRaidControlOverlayState liveTimerPanelOverlayState;
    private LiveRaidControlOverlayState liveExitRaidButtonOverlayState;
    private readonly List<Raid_InventoryItem> collectedLootItems = new();
    private bool collectedLootLayoutConfigured;

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
        RestoreLiveRaidControlSiblingOrder();
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
        BringLiveRaidControlsToFront();
    }

    public void Hide()
    {
        UnsubscribeLootTracking();
        RestoreLiveRaidControlSiblingOrder();
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
        ConfigureCollectedLootLayout();

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
        lootItem.transform.localScale = collectedLootItemScale;
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

    private void RemoveLiveInventoryItem(int lootIndex)
    {
        if (lootTracking == null || lootIndex < 0)
        {
            return;
        }

        lootTracking.RemoveCollectedLootAt(lootIndex);
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

    private void ConfigureCollectedLootLayout()
    {
        if (collectedLootLayoutConfigured || content == null)
        {
            return;
        }

        GridLayoutGroup gridLayoutGroup = content.GetComponent<GridLayoutGroup>();
        if (gridLayoutGroup == null)
        {
            return;
        }

        gridLayoutGroup.cellSize = collectedLootGridCellSize;
        gridLayoutGroup.spacing = collectedLootGridSpacing;
        gridLayoutGroup.padding = new RectOffset(0, 0, collectedLootGridPaddingTop, collectedLootGridPaddingBottom);
        gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayoutGroup.constraintCount = collectedLootColumnCount;
        gridLayoutGroup.childAlignment = TextAnchor.UpperCenter;
        collectedLootLayoutConfigured = true;
    }

    private void BringLiveRaidControlsToFront()
    {
        ResolveLiveRaidControls();

        BringLiveRaidControlToFront(liveExitRaidButton, ref liveExitRaidButtonOverlayState, true);
        BringLiveRaidControlToFront(liveTimerPanel, ref liveTimerPanelOverlayState, false);
    }

    private void RestoreLiveRaidControlSiblingOrder()
    {
        RestoreLiveRaidControl(ref liveTimerPanelOverlayState);
        RestoreLiveRaidControl(ref liveExitRaidButtonOverlayState);
    }

    private void BringLiveRaidControlToFront(Transform control, ref LiveRaidControlOverlayState overlayState, bool needsRaycaster)
    {
        if (control == null)
        {
            return;
        }

        control.gameObject.SetActive(true);
        SetCanvasGroupsVisible(control);

        overlayState ??= new LiveRaidControlOverlayState(control, needsRaycaster);
        overlayState.Apply(GetLiveInventorySortingOrder() + 10);
    }

    private void RestoreLiveRaidControl(ref LiveRaidControlOverlayState overlayState)
    {
        if (overlayState == null)
        {
            return;
        }

        overlayState.Restore();
        overlayState = null;
    }

    private int GetLiveInventorySortingOrder()
    {
        Canvas canvas = menuRoot != null ? menuRoot.GetComponentInParent<Canvas>() : null;
        return canvas != null ? canvas.sortingOrder : 0;
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

    private sealed class LiveRaidControlOverlayState
    {
        private readonly Canvas canvas;
        private readonly GraphicRaycaster raycaster;
        private readonly bool addedCanvas;
        private readonly bool addedRaycaster;
        private readonly bool originalOverrideSorting;
        private readonly int originalSortingOrder;
        private readonly int originalSortingLayerID;

        public LiveRaidControlOverlayState(Transform control, bool needsRaycaster)
        {
            canvas = control.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = control.gameObject.AddComponent<Canvas>();
                addedCanvas = true;
            }

            originalOverrideSorting = canvas.overrideSorting;
            originalSortingOrder = canvas.sortingOrder;
            originalSortingLayerID = canvas.sortingLayerID;

            if (needsRaycaster)
            {
                raycaster = control.GetComponent<GraphicRaycaster>();
                if (raycaster == null)
                {
                    raycaster = control.gameObject.AddComponent<GraphicRaycaster>();
                    addedRaycaster = true;
                }
            }
        }

        public void Apply(int sortingOrder)
        {
            canvas.overrideSorting = true;
            canvas.sortingOrder = sortingOrder;
        }

        public void Restore()
        {
            if (canvas == null)
            {
                return;
            }

            canvas.overrideSorting = originalOverrideSorting;
            canvas.sortingOrder = originalSortingOrder;
            canvas.sortingLayerID = originalSortingLayerID;

            if (addedRaycaster && raycaster != null)
            {
                Destroy(raycaster);
            }

            if (addedCanvas)
            {
                Destroy(canvas);
            }
        }
    }

    private void ResolveLiveRaidControls()
    {
        Transform root = menuRoot != null && menuRoot.transform.parent != null
            ? menuRoot.transform.parent
            : null;

        if (root == null)
        {
            return;
        }

        if (liveTimerPanel == null)
        {
            liveTimerPanel = FindChildRecursive(root, "TimerPanel");
        }

        if (liveExitRaidButton == null)
        {
            liveExitRaidButton = FindChildRecursive(root, "ExitRaidButton");
        }
    }

    private static void SetCanvasGroupsVisible(Transform root)
    {
        CanvasGroup[] canvasGroups = root.GetComponentsInChildren<CanvasGroup>(true);
        for (int i = 0; i < canvasGroups.Length; i++)
        {
            canvasGroups[i].alpha = 1f;
            canvasGroups[i].interactable = true;
            canvasGroups[i].blocksRaycasts = true;
        }
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

        removeHandler.Initialize(this, removeButton);
    }

    private static void SetRemoveButton(Raid_InventoryItem lootItem, bool visible, int lootIndex)
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
            removeHandler.SetLootIndex(lootIndex);
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
        private Raid_LiveInventory owner;
        private Button button;
        private int lootIndex = -1;

        public void Initialize(Raid_LiveInventory owner, Button button)
        {
            this.owner = owner;
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

        public void SetLootIndex(int lootIndex)
        {
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
            owner?.RemoveLiveInventoryItem(lootIndex);
        }
    }
}
