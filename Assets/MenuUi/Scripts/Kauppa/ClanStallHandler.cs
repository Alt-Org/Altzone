using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Game;
using MenuUi.Scripts.Storage;
using MenuUI.Scripts.SoulHome;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class ClanStallPopupHandler : MonoBehaviour
{

    [SerializeField] private GameObject kojuSlot;
    [SerializeField] private GameObject kojuCard;
    [SerializeField] private Transform content;
    private DataStore store;


    private void OnEnable()
    {
        // Populate tray and initialize StoreFront
        store = Storefront.Get();
        StartCoroutine(RandomFurniture());
    }

    public void ClosePopup()
    {
        gameObject.SetActive(false);
    }

    public void OpenPopup()
    {
        gameObject.SetActive(true);
    }
    /*
    private void OnDisable()
    {
        for (int i = content.transform.childCount; i > 0; i--)
        {
            Destroy(content.transform.GetChild(i - 1).gameObject);
        }
    }
    */
    private IEnumerator RandomFurniture()
    {
        ReadOnlyCollection<GameFurniture> allGameFurniture = null;
        yield return store.GetAllGameFurnitureYield(result => allGameFurniture = result);

        Debug.Log("Furniture count: " + allGameFurniture.Count);

        for (int i = 0; i < 7; i++)
        {
            GameObject slot = Instantiate(kojuSlot, content);
            GameObject card = Instantiate(kojuCard, slot.transform);

            Debug.Log("Created slot = " + slot.name + " | parent = " + slot.transform.parent.name);
            Debug.Log("Created card = " + card.name + " | parent = " + card.transform.parent.name);

            int randomIndex = UnityEngine.Random.Range(0, allGameFurniture.Count);

            GameFurniture randomFurniture = allGameFurniture[randomIndex];

            Debug.Log($"RandomFurniture Name {randomFurniture.Name}, ID={randomFurniture.Id}");

            var clanFurniture = new ClanFurniture(id: Guid.NewGuid().ToString(), gameFurnitureId: randomFurniture.Id);

            var storageFurniture = new StorageFurniture(clanFurniture, randomFurniture);

            card.GetComponent<FurnitureCardUI>().PopulateCard(storageFurniture);

            Debug.Log("Icon sprite: " + storageFurniture.Sprite);
        }
    }
}
