using System.Collections;
using System.Collections.ObjectModel;
using UnityEngine;
using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Game;

public class KojuTrayPopulator : MonoBehaviour
{
    [Header("UI Setup")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform contentParent;

    private void Start()
    {
        StartCoroutine(PopulateTray());
    }

    private IEnumerator PopulateTray()
    {
        var store = Storefront.Get(); //Access data
        ReadOnlyCollection<GameFurniture> allFurniture = null;

        //Waits for the data to be retrieved
        yield return store.GetAllGameFurnitureYield(result => allFurniture = result);

        //Exists and debugs if fetching of items fail
        if (allFurniture == null || allFurniture.Count == 0)
        {
            Debug.LogWarning("No furniture items found.");
            yield break;
        }

        //Populates the tray by instantiating a card and filling it with the data
        foreach (var furniture in allFurniture)
        {
            GameObject cardGO = Instantiate(cardPrefab, contentParent);
            FurnitureCardUI cardUI = cardGO.GetComponent<FurnitureCardUI>();
            KojuFurnitureData data = cardGO.GetComponent<KojuFurnitureData>();

            if (cardUI != null)
            {
                cardUI.PopulateCard(furniture); //Passes the data onto the cards
            }

            if (data != null)
            {
                data.SetPrice(furniture.Value);
            }
        }

        Debug.Log($"Spawned {allFurniture.Count} furniture cards.");
    }
}
