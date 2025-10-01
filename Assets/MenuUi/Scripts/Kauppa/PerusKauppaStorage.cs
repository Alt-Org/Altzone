using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class PerusKauppaStorage : ShopPanelStorage
{
    [SerializeField] private RectTransform _Content;
    private bool _isInitiallyRebuild = false;

    [Space(5f)]

    [Header("Parents")]
    [SerializeField] private RectTransform _commonGroup;
    [SerializeField] private RectTransform _rareGroup;
    [SerializeField] private RectTransform _epicGroup;
    [SerializeField] private RectTransform _antiqueGroup;

    [Space(5f)]

    [Header("Prefabs")]
    [SerializeField] private GameFurnitureVisualizer _commonPrefab;
    [SerializeField] private GameFurnitureVisualizer _rarePrefab;
    [SerializeField] private GameFurnitureVisualizer _epicPrefab;
    [SerializeField] private GameFurnitureVisualizer _antiquePrefab;

    [Header("PopUp")]
    [SerializeField] private GameObject _popUp;

    private Dictionary<FurnitureRarity, Transform> _rarityToParent;
    private Dictionary<FurnitureRarity, GameFurnitureVisualizer> _rarityToPrefab;

    private List<GameFurniture> gameFurnitures;
    private List<GameFurnitureVisualizer> gameFurnituresOnScene;

    private void Awake()
    {
        _rarityToParent = new Dictionary<FurnitureRarity, Transform>{
            {FurnitureRarity.Common, _commonGroup},
            {FurnitureRarity.Rare, _rareGroup},
            {FurnitureRarity.Epic, _epicGroup},
            {FurnitureRarity.Antique, _antiqueGroup}
        };

        _rarityToPrefab = new Dictionary<FurnitureRarity, GameFurnitureVisualizer>
        {
            {FurnitureRarity.Common, _commonPrefab},
            {FurnitureRarity.Rare, _rarePrefab},
            {FurnitureRarity.Epic, _epicPrefab},
            {FurnitureRarity.Antique, _antiquePrefab}
        };
        gameFurnitures = new();
        gameFurnituresOnScene = new();
    }

    protected override void HandleGameFurnitureCreation(ReadOnlyCollection<GameFurniture> gameFurnitures)
    {
        StartCoroutine(ShopGameFurnitureCreation(gameFurnitures));
    }

    protected IEnumerator ShopGameFurnitureCreation(ReadOnlyCollection<GameFurniture> gameFurnitures)
    {
        List<GameFurniture> furnitures = null;
        bool fetchingfurnitures = true;
        StartCoroutine(ServerManager.Instance.GetClanShopListFromServer(r =>
        {
            furnitures = r;
            fetchingfurnitures = false;
        }));

        yield return new WaitUntil(() => fetchingfurnitures == false);
        foreach (GameFurniture furniture in gameFurnitures)
            this.gameFurnitures.Add(furniture);

        ListHelper.Shuffle(this.gameFurnitures);

        foreach(GameFurniture furniture1 in furnitures)
        {
            if (furniture1 == null)
            {
                Debug.LogError("gameFurniture is null. Ensure it is assigned before calling Initialize.");
                yield break;
            }

            if (_rarityToParent.TryGetValue(furniture1.Rarity, out Transform _parent))
            {
                if (_rarityToPrefab.TryGetValue(furniture1.Rarity, out GameFurnitureVisualizer _prefab))
                {
                    Debug.Log("Furniture of " + furniture1.Name + "" + furniture1.Value +  " is created");
                    var newItem = Instantiate(_prefab, _parent);
                    newItem.Initialize(furniture1, _popUp);
                    gameFurnituresOnScene.Add(newItem);
                }
                else
                {
                    Debug.LogWarning($"Prefab for Rarity {furniture1.Rarity} is not defined!");
                }
            }
            else
            {
                Debug.LogWarning($"Parent for Rarity {furniture1.Rarity} is not defined!");
            }
        }

        //Randomize
        // Sort based on rarity
        // Instantiate on the righfull position

        //Force each content to rebild to avoid UI issues
    }

    private void LateUpdate()
    {
        if(_isInitiallyRebuild)
            return;

        _isInitiallyRebuild = false;
        LayoutRebuilder.ForceRebuildLayoutImmediate(_Content);
        return;
    }
}
