using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using Altzone.Scripts.AvatarPartsInfo;

public class AvatarShopStorage : ShopPanelStorage
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

    [Header("Avatar Parts Reference")]
    [SerializeField] private AvatarPartsReference _avatarPartsReference;

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
    protected override void Start()
    {
        HandleAvatarPartCreation();
    }

    protected void HandleAvatarPartCreation()
    {
        List<AvatarPartsReference.AvatarPartCategoryInfo> avatarPartsData = _avatarPartsReference.AvatarPartData;

        foreach(AvatarPartsReference.AvatarPartCategoryInfo avatarSectiondata in avatarPartsData)
        {
            if (avatarSectiondata == null || avatarSectiondata.AvatarParts.Count <= 0)
            {
                Debug.LogWarning($"Cannot find avatar parts from section {avatarSectiondata.SetName}.");
                continue;
            }

           
            foreach (AvatarPartInfo avatarpartData in avatarSectiondata.AvatarParts)
            {
                if (avatarpartData == null)
                {
                    continue;
                }

                if (_rarityToParent.TryGetValue(FurnitureRarity.Rare, out Transform _parent))
                {
                    if (_rarityToPrefab.TryGetValue(FurnitureRarity.Rare, out GameFurnitureVisualizer _prefab))
                    {
                        Debug.Log("Furniture of " + avatarpartData.Id + "" + avatarpartData.Name + " is created");
                        var newItem = Instantiate(_prefab, _parent);
                        newItem.Initialize(avatarpartData, _popUp);
                        gameFurnituresOnScene.Add(newItem);
                    }
                    else
                    {
                        //Debug.LogWarning($"Prefab for Rarity {avatarpartData.Rarity} is not defined!");
                    }
                }
                else
                {
                    //Debug.LogWarning($"Parent for Rarity {avatarpartData.Rarity} is not defined!");
                }
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
