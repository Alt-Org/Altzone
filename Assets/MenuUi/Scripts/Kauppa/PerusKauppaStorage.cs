using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;
using System.Linq;

public class PerusKauppaStorage : ShopPanelStorage
{
    [Header("Sorting")]
    [SerializeField] private Transform _commonParent;
    [SerializeField] private Transform _rareParent;
    [SerializeField] private Transform _epicParent;
    [SerializeField] private Transform _antiqueParent;

    [Space(5f)]

    [Header("Prefabs")]
    [SerializeField] private GameFurnitureVisualizer _commonPrefab;
    [SerializeField] private GameFurnitureVisualizer _rarePrefab;
    [SerializeField] private GameFurnitureVisualizer _epicPrefab;
    [SerializeField] private GameFurnitureVisualizer _antiquePrefab;

    private Dictionary<FurnitureRarity, Transform> _rarityToParent;
    private Dictionary<FurnitureRarity, GameFurnitureVisualizer> _rarityToPrefab;

    private List<GameFurniture> gameFurnitures;
    private List<GameFurnitureVisualizer> gameFurnituresOnScene;

    private void Awake()
    {
        _rarityToParent = new Dictionary<FurnitureRarity, Transform>{
            {FurnitureRarity.Common, _commonParent},
            {FurnitureRarity.Rare, _rareParent},
            {FurnitureRarity.Epic, _epicParent},
            {FurnitureRarity.Antique, _antiqueParent}
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
        foreach(GameFurniture furniture in gameFurnitures)
            this.gameFurnitures.Add(furniture);

        ListHelperr.Shuffle(this.gameFurnitures);

        foreach(GameFurniture furniture1 in gameFurnitures)
        {
            if(_rarityToParent.TryGetValue(furniture1.Rarity, out Transform _parent))
            {
                if(_rarityToPrefab.TryGetValue(furniture1.Rarity, out GameFurnitureVisualizer _prefab))
                {
                    var newItem = Instantiate(_prefab, _parent);
                    newItem.Initialize(furniture1);
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
    }
}
