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
using UnityEngine.UI;


public class Kirpputori : MonoBehaviour
{
    //Currently works as a place to instantiate desiarable amounts of "ads".
    //To do:
    //Make it a place to Instantiate and store information about real-time available "ads".

    [Header("Instantiation Parameters")]
    [SerializeField] private int _AdsAmount;
    [SerializeField] private GameObject _AdPrefab;
    [SerializeField] private GameObject _ClanPopup;

    private List<GameObject> _adsInScene;

    [Header("Rect Transforms")]
    [SerializeField] private RectTransform _AdsGroup;
    [SerializeField] private RectTransform _Content;
    private DataStore _store;

    

    private void Awake()
    {
        _adsInScene = new();

        for (int i = 0; i < _AdsAmount; i++)
        {
            GameObject newAd = Instantiate(_AdPrefab, _AdsGroup);
            _adsInScene.Add(newAd);

        }

        //Force content to rebild to avoid UI issues
        LayoutRebuilder.ForceRebuildLayoutImmediate(_Content);

        _store = Storefront.Get();

        StartCoroutine(RandomFurniture());

    }

    public IEnumerator RandomFurniture()
    {
        ReadOnlyCollection<GameFurniture> allGameFurniture = null;
        yield return _store.GetAllGameFurnitureYield(result => allGameFurniture = result);

        foreach (GameObject ad in _adsInScene)
        {
            var furnitureList = new List<StorageFurniture>();

            for (int i = 0; i < 1; i++)
            {
                int randomIndex = UnityEngine.Random.Range(0, allGameFurniture.Count);

                GameFurniture randomFurniture = allGameFurniture[randomIndex];

                var clanFurniture = new ClanFurniture(id: Guid.NewGuid().ToString(), gameFurnitureId: randomFurniture.Id);

                var storageFurniture = new StorageFurniture(clanFurniture, randomFurniture);

                furnitureList.Add(storageFurniture);
            }

            var esineDisplay = ad.GetComponent<EsineDisplay>();
            esineDisplay.AlignFurnitures(furnitureList, this);


            // Trigger randomization for advertisement's frames and background colours once for new items.
            // Results are cached in EsineDisplay during the gaming session.
            var displayAdBackground = ad.GetComponent<EsineDisplay>();
            displayAdBackground.RandomizeAdBackgroundColor();

            var displayAdFrame = ad.GetComponent<EsineDisplay>();
            displayAdFrame.RandomizeAdFrames();
        }
    }




    public void OpenPopup(List<StorageFurniture> furnitureList)
    {
        _ClanPopup.SetActive(true);
        var clanStallPopupHandler = _ClanPopup.GetComponent<ClanStallPopupHandler>();
        clanStallPopupHandler.CreateStalls(furnitureList);
    }
}
