using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Game;
using MenuUi.Scripts.Storage;
using MenuUI.Scripts.SoulHome;
using UnityEngine;

public class ClanStallPopupHandler : MonoBehaviour
{

    [SerializeField] private GameObject KojuSlot;
    [SerializeField] private GameObject Content;
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

    private IEnumerator RandomFurniture()
    {
        ReadOnlyCollection<GameFurniture> allGameFurniture = null;
        yield return store.GetAllGameFurnitureYield(result => allGameFurniture = result);

        Debug.Log("Furniture count: " + allGameFurniture.Count);

        int randomIndex = UnityEngine.Random.Range(0, allGameFurniture.Count);

        GameFurniture randomFurniture = allGameFurniture[randomIndex];

        Debug.Log($"RandomFurniture Name {randomFurniture.Name}, ID={randomFurniture.Id}");
    }
}
