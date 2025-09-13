using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Player;
using MenuUi.Scripts.Storage;
using Altzone.Scripts.Config;

public class KojuTrayPopulator : MonoBehaviour
{
    [Header("UI Setup")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform trayContent;
    [SerializeField] private Transform panelContent;

    [Header("Popup Reference")]
    [SerializeField] private KojuPopup popup;

    [Header("Panel Warning")]
    [SerializeField] private GameObject panelFullWarningUI;
    private bool isWarningActive = false;

    private DataStore store;
    private PlayerData player;
    private ClanData clan;

    // Track items that are located on the panel instead of the tray to avoid respawning them when repopulating the tray
    private HashSet<string> movedFurnitureIds = new HashSet<string>();

    private void OnEnable()
    {
        // Populate tray and initialize StoreFront
        PollManager.RegisterTrayPopulator(this);
        store = Storefront.Get();
        StartCoroutine(PopulateTray());
    }

    // For refreshing the tray when a poll ends
    public void RefreshTray()
    {
        StartCoroutine(PopulateTray());
    }

    private IEnumerator PopulateTray()
    {
        Debug.Log($"PanelContent has {panelContent.childCount} children.");
        foreach (Transform child in panelContent)
        {
            Debug.Log($"Child name: {child.name}");

            // Reserved slot for the poster
            if (child.GetSiblingIndex() == 0)
            {
                continue;
            }

            KojuFurnitureData data = child.GetComponent<KojuFurnitureData>();
            if (data == null)
            {
                Debug.LogWarning($"Child '{child.name}' has NO KojuFurnitureData component.");
            }
            else if (string.IsNullOrEmpty(data.StorageFurnitureId))
            {
                Debug.LogWarning($"Child '{child.name}' has KojuFurnitureData but StorageFurnitureId is empty.");
            }
            else
            {
                Debug.Log($"Child '{child.name}' StorageFurnitureId: {data.StorageFurnitureId}");
            }
        }

        // Load player data
        bool playerLoaded = false;
        store.GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, data =>
        {
            player = data;
            playerLoaded = true;
        });
        yield return new WaitUntil(() => playerLoaded);

        if (player == null || string.IsNullOrEmpty(player.ClanId))
        {
            Debug.LogWarning("Player data missing or player not in clan.");
            yield break;
        }

        bool clanLoaded = false;
        store.GetClanData(player.ClanId, data =>
        {
            clan = data;
            clanLoaded = true;
        });
        yield return new WaitUntil(() => clanLoaded);

        if (clan == null || clan.Inventory?.Furniture == null || clan.Inventory.Furniture.Count == 0)
        {
            Debug.LogWarning("No clan inventory found.");
            yield break;
        }

        // Filter clan furniture voted to sell, you can remove .Where(f => f.VotedToSell) and .ToList(); to show all clan furniture
        List<ClanFurniture> votedToSellFurniture = clan.Inventory.Furniture
            .Where(f => f.VotedToSell)
            .ToList();

        if (votedToSellFurniture.Count == 0)
        {
            Debug.Log("No furniture voted to sell.");
            yield break;
        }

        ReadOnlyCollection<GameFurniture> allGameFurniture = null;
        yield return store.GetAllGameFurnitureYield(result => allGameFurniture = result);

        if (allGameFurniture == null || allGameFurniture.Count == 0)
        {
            Debug.LogError("GameFurniture definitions missing.");
            yield break;
        }

        foreach (Transform child in trayContent)
        {
            Destroy(child.gameObject);
        }

        HashSet<string> panelFurnitureIds = new HashSet<string>();
        foreach (Transform child in panelContent)
        {
            // Skip the first slot (locked slot)
            if (child.GetSiblingIndex() == 0)
            {
                continue;
            }

            KojuFurnitureData data = child.GetComponent<KojuFurnitureData>();
            if (data != null && !string.IsNullOrEmpty(data.StorageFurnitureId))
            {
                string id = data.StorageFurnitureId.Trim();
                if (!panelFurnitureIds.Contains(id))
                {
                    panelFurnitureIds.Add(id);
                }
            }
        }

        Debug.Log($"Total unique furniture in panel: {panelFurnitureIds.Count}");

        int spawnedCount = 0;

        foreach (var clanFurniture in votedToSellFurniture)
        {
            GameFurniture matchingFurniture = allGameFurniture
                .FirstOrDefault(gf => gf.Name == clanFurniture.GameFurnitureName);

            if (matchingFurniture == null)
            {
                Debug.LogWarning($"GameFurniture not found: {clanFurniture.GameFurnitureName}");
                continue;
            }

            // Create StorageFurniture wrapper, containing both the clan and gamefurniture info
            StorageFurniture storageFurniture = new StorageFurniture(clanFurniture, matchingFurniture);
            string storageId = storageFurniture.Id?.Trim();

            if (string.IsNullOrEmpty(storageId))
            {
                Debug.LogWarning("StorageFurniture.Id is null or empty, skipping furniture.");
                continue;
            }

            Debug.Log($"Checking furniture to spawn with ID: {storageId}");

            if (panelFurnitureIds.Contains(storageId))
            {
                Debug.Log($"Skipping spawn: furniture ID '{storageId}' already in panel.");
                continue;
            }

            if (movedFurnitureIds.Contains(storageId)) // Avoid re-spawning moved items
            {
                Debug.Log($"Skipping spawn: furniture ID '{storageId}' was moved during session.");
                continue;
            }

            // Instantiate card UI
            GameObject cardGO = Instantiate(cardPrefab, trayContent);

            // Fill in the UI components
            FurnitureCardUI cardUI = cardGO.GetComponent<FurnitureCardUI>();
            KojuFurnitureData data = cardGO.GetComponent<KojuFurnitureData>();
            ItemMover mover = cardGO.GetComponent<ItemMover>();

            if (cardUI != null)
                cardUI.PopulateCard(storageFurniture);

            if (data != null)
                data.SetFurniture(storageFurniture);

            if (mover != null)
            {
                mover.SetParents(trayContent, panelContent);
                mover.SetPopup(popup);
                mover.SetPopulator(this);
                mover.SetFurniture(storageFurniture);

                mover.OnItemMovedToPanel += HandleItemMovedToPanel;
            }

            spawnedCount++;
        }

        Debug.Log($"Spawned {spawnedCount} furniture cards in tray.");
    }

    // Shows a popup warning when the panel is full, and the user tries to add more items
    public void ShowPanelFullWarning()
    {
        if (!isWarningActive)
        {
            StartCoroutine(ShowWarningCoroutine());
        }
    }

    // For the full panel warning
    private IEnumerator ShowWarningCoroutine()
    {
        isWarningActive = true;
        panelFullWarningUI.SetActive(true);
        yield return new WaitForSeconds(1f);
        panelFullWarningUI.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        isWarningActive = false;
    }

    // When moving from tray to panel
    private void HandleItemMovedToPanel(StorageFurniture moved)
    {
        if (moved != null && !string.IsNullOrEmpty(moved.Id))
        {
            movedFurnitureIds.Add(moved.Id.Trim());
            Debug.Log($"Furniture ID '{moved.Id}' marked as moved.");
        }
    }

    // When moving from panel to tray
    public void HandleItemReturnedToTray(StorageFurniture returned)
    {
        if (returned != null && !string.IsNullOrEmpty(returned.Id))
        {
            movedFurnitureIds.Remove(returned.Id.Trim());
            Debug.Log($"Furniture ID '{returned.Id}' marked as returned to tray.");
        }
    }

}
