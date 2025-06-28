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

    private void Start()
    {
        // Populate koju and initialize StoreFront
        store = Storefront.Get();
        StartCoroutine(PopulateTray());
    }

    private IEnumerator PopulateTray()
    {
        // Load player data
        bool playerLoaded = false;
        store.GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, data =>
        {
            player = data;
            playerLoaded = true;
        });
        yield return new WaitUntil(() => playerLoaded);

        if (player == null)
        {
            Debug.LogWarning("Player data not found.");
            yield break;
        }

        if (string.IsNullOrEmpty(player.ClanId))
        {
            Debug.LogWarning("Player is not in a clan.");
            yield break;
        }

        // Load clan data
        bool clanLoaded = false;
        store.GetClanData(player.ClanId, data =>
        {
            clan = data;
            clanLoaded = true;
        });
        yield return new WaitUntil(() => clanLoaded);

        if (clan == null)
        {
            Debug.LogWarning("Clan data not found.");
            yield break;
        }

        if (clan.Inventory == null || clan.Inventory.Furniture == null || clan.Inventory.Furniture.Count == 0)
        {
            Debug.LogWarning("No clan storage furniture found.");
            yield break;
        }

        // Filter clan furniture voted to sell, you can remove .Where(f => f.VotedToSell) and .ToList(); to show all clan furniture
        List<ClanFurniture> votedToSellFurniture = clan.Inventory.Furniture
        .Where(f => f.VotedToSell)
        .ToList();

        if (votedToSellFurniture.Count == 0)
        {
            Debug.LogWarning("No clan furniture voted to sell.");
            yield break;
        }

        // Load gamefurniture
        ReadOnlyCollection<GameFurniture> allGameFurniture = null;
        yield return store.GetAllGameFurnitureYield(result => allGameFurniture = result);

        if (allGameFurniture == null || allGameFurniture.Count == 0)
        {
            Debug.LogWarning("No GameFurniture data found in Storefront.");
            yield break;
        }

        int spawnedCount = 0;

        // Loop through all voted furniture and create cards for them
        foreach (var clanFurniture in votedToSellFurniture)
        {
            // Match clan item with master furniture
            GameFurniture matchingFurniture = allGameFurniture
                .FirstOrDefault(gf => gf.Name == clanFurniture.GameFurnitureName);

            if (matchingFurniture == null)
            {
                Debug.LogWarning($"Matching GameFurniture not found for: {clanFurniture.GameFurnitureName}");
                continue;
            }

            // Create StorageFurniture wrapper, containing both the clan and gamefurniture info
            StorageFurniture storageFurniture = new StorageFurniture(clanFurniture, matchingFurniture);

            // Instantiate card UI
            GameObject cardGO = Instantiate(cardPrefab, trayContent);

            // Fill in the UI components
            FurnitureCardUI cardUI = cardGO.GetComponent<FurnitureCardUI>();
            KojuFurnitureData data = cardGO.GetComponent<KojuFurnitureData>();
            ItemMover mover = cardGO.GetComponent<ItemMover>();

            if (cardUI != null)
            {
                // Use the StorageFurniture to preserve unique ID and metadata
                cardUI.PopulateCard(storageFurniture);
            }

            if (data != null)
            {
                data.SetFurniture(storageFurniture);
            }

            if (mover != null)
            {
                mover.SetParents(trayContent, panelContent);
                mover.SetPopup(popup);
                mover.SetPopulator(this);
            }

            spawnedCount++;
        }

        Debug.Log($"Spawned {spawnedCount} furniture cards voted to sell.");
    }

    // Prompt if the panel is full
    public void ShowPanelFullWarning()
    {
        if (!isWarningActive)
        {
            StartCoroutine(ShowWarningCoroutine());
        }
    }

    // Coroutine to flash the warning, and make the warning non spammable
    private IEnumerator ShowWarningCoroutine()
    {
        isWarningActive = true;
        panelFullWarningUI.SetActive(true);

        yield return new WaitForSeconds(1f); // Visible duration
        panelFullWarningUI.SetActive(false);

        yield return new WaitForSeconds(0.5f); // Cooldown to prevent spamming
        isWarningActive = false;
    }
}
