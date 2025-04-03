using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Altzone.Scripts;
using Altzone.Scripts.ModelV2;
using System.Collections.ObjectModel;
using System;
using Altzone.Scripts.Model.Poco.Game;
using Random = UnityEngine.Random;
using Altzone.Scripts.ReferenceSheets;

[System.Serializable]
public class Reward
{
    public Sprite image;
    public string name;
    public string type;
    public string description;
}

public class LevelUpController : MonoBehaviour
{
    [Header("UI Elements")]
    // The panel that appears when the player levels up
    public GameObject LevelUpPanel;
    public Button[] rewardButtons;
    public GameObject Confirmation_Window;
    public TMP_Text[] rewardNameTexts;

    //[Header("Character rewards")]
    //public Reward[] CharacterRewards;
    //public TMP_Text RewardCharacterNameText;
    //public Image RewardCharacterImage;

    //[Header("Furniture rewards")]
    //public Reward[] FurnitureRewards;
    //public TMP_Text RewardFurnitureNameText;
    //public Image RewardFurnitureImage;

    //[Header("Coin rewards")]
    //public Reward[] CoinRewards;
    //public TMP_Text RewardCoinNameText;
    //public Image RewardCoinsImage;

    //[Header("Diamond rewards")]
    //public Reward[] DiamondRewards;
    //public TMP_Text RewardDiamondNameText;
    //public Image RewardDiamondsImage;

    //[Header("Other rewards")]
    //public Reward[] OtherRewardRewards;
    //public TMP_Text RewardOtherRewardsNameText;
    //public Image RewardOthersImage;

    [Header("Confirmation window")]
    public Image RewardImage;
    public TMP_Text RewardNameText;
    public TMP_Text RewardTypeText;
    public TMP_Text RewardDescriptionText;

    [Header("Databases")]
    [SerializeField] private StorageFurnitureReference _storageFurnitureReference;

    public Sprite coinSprite;
    public Sprite diamondSprite;
    public Sprite otherRewardSprite;

    private List<BaseCharacter> availableCharacters = new();
    private List<Reward> currentRewards = new();

    // Start is called before the first frame update
    void Start()
    {
        InitializeButtons();
        Storefront.Get().GetAllBaseCharacterYield(OnCharactersFetched);

        //Action<ReadOnlyCollection<BaseCharacter>> charactersFetchedCallback = OnCharactersFetched;

    }

    // Method to activate the level-up popup and assign a random reward
    public void OpenPopup()
    {
        //Ensure the level - up panel is active
        if (LevelUpPanel != null)
        {
            LevelUpPanel.SetActive(true);
            GenerateRandomRewards();
        }
    }
    public void ConfirmReward()
    {
        Debug.Log($"Reward confirmed");
        Confirmation_Window.SetActive(false);
    }
    private void InitializeButtons()
    {
        for (int i = 0; i < rewardButtons.Length; i++)
        {
            int index = i;
            rewardButtons[i].onClick.AddListener(() => OnRewardSelected(index));
        }
    }
    private void GenerateRandomRewards()
    {
        currentRewards.Clear();

        if (_storageFurnitureReference != null && _storageFurnitureReference.Info.Count > 0)
        {
            var set = _storageFurnitureReference.Info[Random.Range(0, _storageFurnitureReference.Info.Count)];
            var furniture = set.list[Random.Range(0, set.list.Count)];
            currentRewards.Add(new Reward
            {
                image = furniture.Image,
                name = furniture.VisibleName,
                type = "Fruniture",
                description = $"New furniture: {furniture.VisibleName}"
            });
        }

        if (availableCharacters.Count > 0)
        {
            var character = availableCharacters[Random.Range(0, availableCharacters.Count)];
            var charData = PlayerCharacterPrototypes.GetCharacter(character.Id.ToString());
            if (charData != null)
            {
                currentRewards.Add(new Reward
                {
                    image = charData.GalleryImage,
                    name = charData.Name,
                    type = "Character",
                    description = $"New character: {charData.Name}"
                });
            }
        }

        int coinAmount = Random.Range(100, 1000);
        currentRewards.Add(new Reward
        {
            image = coinSprite,
            name = $"{coinAmount} coins",
            type = "Currency",
            description = $"You got {coinAmount} coins!"
        });

        int diamondAmount = Random.Range(5, 20);
        currentRewards.Add(new Reward
        {
            image = diamondSprite,
            name = $"{diamondAmount} Diamonds",
            type = "Premium",
            description = $"You got {diamondAmount} diamonds!"
        });

        currentRewards.Add(new Reward
        {
            image = otherRewardSprite,
            name = "Erikoispalkinto",
            type = "Erikoinen",
            description = "Mystinen palkinto!"
        });

        ShuffleRewards();
        UpdateButtonImages();
    }

    private void ShuffleRewards()
    {
        for (int i = 0; i < currentRewards.Count; i++)
        {
            int randomIndex = Random.Range(i, currentRewards.Count);
            var temp = currentRewards[i];
            currentRewards[i] = currentRewards[randomIndex];
            currentRewards[randomIndex] = temp;
        }
    }

    private void OnRewardSelected(int rewardIndex)
    {
        if (rewardIndex < 0 || rewardIndex >= currentRewards.Count) return;

        var reward = currentRewards[rewardIndex];
        RewardImage.sprite = reward.image;
        RewardNameText.text = reward.name;
        RewardTypeText.text = reward.type;
        RewardDescriptionText.text = reward.description;

        LevelUpPanel.SetActive(false);
        Confirmation_Window.SetActive(true);
    }
    private void OnCharactersFetched(ReadOnlyCollection<BaseCharacter> characters)
    {
        availableCharacters = new List<BaseCharacter>(characters);
        Debug.Log($"Loaded {availableCharacters.Count} characters");
    }
    private void UpdateButtonImages()
    {
        for (int i = 0; i < rewardButtons.Length && i < currentRewards.Count; i++)
        {
            rewardButtons[i].GetComponent<Image>().sprite = currentRewards[i].image;
            rewardNameTexts[i].text = currentRewards[i].name;
        }
    }

    //private void OnCharacterDetailsFetched(PlayerCharacterPrototype character)
    //{
    //    if (character != null)
    //    {
    //        Debug.Log($"Character name: {character.Name}");
    //        Debug.Log($"Character image: {character.GalleryImage}");
    //    }
    //    else
    //    {
    //        Debug.LogError("Failed to fetch character details!");
    //    }

    //    FetchFurniture();
    //}
    //private void FetchFurniture()
    //{
    //    if (_storageFurnitureReference == null)
    //    {
    //        Debug.LogError("Furniture not set");
    //        return;
    //    }

    //    var furnitureInfo = _storageFurnitureReference.Info;
    //    if (furnitureInfo == null || furnitureInfo.Count == 0)
    //    {
    //        Debug.LogError("No furniture available!");
    //        return;
    //    }

    //    foreach (var furnitureSet in furnitureInfo)
    //    {
    //        Debug.Log($"Funriture Set: {furnitureSet.SetName}");

    //        foreach (var furniture in furnitureSet.list)
    //        {
    //            Debug.Log($"Furniture: {furniture.VisibleName}, ID: {furniture.Name}");
    //        }
    //    }
    //}
    //private void AssignRandomCharacterReward()
    //{
    //    rewardIndex = Random.Range(0, CharacterRewards.Length);
    //    Reward selectedReward = CharacterRewards[rewardIndex];

    //    RewardCharacterImage.sprite = selectedReward.image;
    //    RewardCharacterNameText.text = selectedReward.name;
    //}
    //private void AssignRandomFurnitureReward()
    //{
    //    rewardIndex = Random.Range(0, FurnitureRewards.Length);
    //    Reward selectedReward = FurnitureRewards[rewardIndex];

    //    RewardFurnitureImage.sprite = selectedReward.image;
    //    RewardFurnitureNameText.text = selectedReward.name;
    //}
    //private void AssignRandomCoinsReward()
    //{
    //    rewardIndex = Random.Range(0, CoinRewards.Length);
    //    Reward selectedReward = CoinRewards[rewardIndex];

    //    int coinsAmount = Random.Range(100, 1000);
        
    //}
    //private void AssignRandomDiamondsReward()
    //{
    //    rewardIndex = Random.Range(0, DiamondRewards.Length);
    //    Reward selectedReward = DiamondRewards[rewardIndex];

    //    int diamondAmount = Random.Range(100, 1000);
    //}
    //private void AssignRandomReward5()
    //{
    //    rewardIndex = Random.Range(0, OtherRewardRewards.Length);
    //    Reward selectedReward = OtherRewardRewards[rewardIndex];

    //    RewardOthersImage.sprite = selectedReward.image;
    //    RewardOtherRewardsNameText.text = selectedReward.name;
    //}
}

