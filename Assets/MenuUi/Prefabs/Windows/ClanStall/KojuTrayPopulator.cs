using System.Collections;
using System.Collections.ObjectModel;
using UnityEngine;
using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Game;

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

    private void Start()
    {
        StartCoroutine(PopulateTray());
    }

    private IEnumerator PopulateTray()
    {
        var store = Storefront.Get(); // Access the data
        ReadOnlyCollection<GameFurniture> allFurniture = null;

        // Waits for the data to be retrieved
        yield return store.GetAllGameFurnitureYield(result => allFurniture = result);

        // Exits and debugs if fetching of items fail
        if (allFurniture == null || allFurniture.Count == 0)
        {
            Debug.LogWarning("No furniture items found.");
            yield break;
        }

        // Populates the tray by instantiating cards and filling them with data
        foreach (var furniture in allFurniture)
        {
            GameObject cardGO = Instantiate(cardPrefab, trayContent);
            FurnitureCardUI cardUI = cardGO.GetComponent<FurnitureCardUI>();
            KojuFurnitureData data = cardGO.GetComponent<KojuFurnitureData>();
            ItemMover mover = cardGO.GetComponent<ItemMover>();

            if (cardUI != null)
            {
                cardUI.PopulateCard(furniture); // Passes the data onto the cards
            }

            if (data != null)
            {
                // Set the base data and initial price
                data.SetFurniture(furniture); 
            }

            if (mover != null)
            {
                mover.SetParents(trayContent, panelContent);
                mover.SetPopup(popup);
                mover.SetPopulator(this);
            }
        }

        Debug.Log($"Spawned {allFurniture.Count} furniture cards.");
    }

    public void ShowPanelFullWarning()
    {
        if (!isWarningActive)
        {
            StartCoroutine(ShowWarningCoroutine());
        }
    }

    private IEnumerator ShowWarningCoroutine()
    {
        isWarningActive = true;
        panelFullWarningUI.SetActive(true);

        yield return new WaitForSeconds(1f); // How long it's visible
        panelFullWarningUI.SetActive(false);

        yield return new WaitForSeconds(0.5f); // Cooldown to prevent re-triggering too fast
        isWarningActive = false;
    }


}
