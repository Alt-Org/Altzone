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
    [SerializeField] private List<RectTransform> _group;
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

    private Dictionary<string, Transform> _rarityToParent;
    //private Dictionary<FurnitureRarity, Transform> _rarityToParent;
    private Dictionary<FurnitureRarity, GameFurnitureVisualizer> _rarityToPrefab;

    private List<GameFurniture> gameFurnitures;
    private List<GameFurnitureVisualizer> gameFurnituresOnScene;

    private void Awake()
    {

        // Summary: Assigns avatar parts (hair, eyes...) to the specific slots in the Unity Inspector, e.g. Element 0 is tied to hair.
        //1. Get a list of all avatar parts (hair, eyes etc.) and store them in avatarPartsData
        var avatarPartsData = _avatarPartsReference.AvatarPartData;
        
        //2. Create a new dictionary, store the category name as text (string) and location in the Unity hierarchy (transform), so we can start storing information
        _rarityToParent = new Dictionary<string, Transform>();

        //3. Go through each category found in the avatarPartsData list
        int i = 0;
            foreach ( var part in avatarPartsData)
        {

            //3.1. Create a new pair to the dictionary: category name (e.g. part.setName > "Hair")
            // and its location (_group[i] > _group[0], _group[1], etc.)
            _rarityToParent.Add(part.SetName, _group[i]);

            //3.2. Move to the next group by incrementing the i-counter so the next category gets its own location
            // If there are more categories than there are groups, rest of the categories will be listed under the last group
            // i+1 is used to account for zero-based indexing. (e.g. if _group.Count is 7, 'i' will never exceed 6)
            if (i+1 < _group.Count)
            {
                i++;
            } 

        }




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
                
                if (_rarityToParent.TryGetValue(avatarSectiondata.SetName, out Transform _parent))
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
