using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Altzone.Scripts;
using Altzone.Scripts.ModelV2;
using System.Collections.ObjectModel;
using System;

[System.Serializable]
public class Reward
{
    public string Name;
    public Sprite Sprite;
}

public class BaseCharacter
{
    public int Id { get; set; }
    public string Name { get; set; }
}
public class PlayerCharacter
{
    public string Name { get; set; }
    public string Image { get; set; }
}
public class StorageFurnitureReference : MonoBehaviour
{
    public List<FurnitureSet> Info;
}
public class FurnitureSet
{
    public string Name;
    public List<FurnitureItem> FurnitureItems;
}
public class FurnitureItem
{
    public string Name;
    public int Id;
}
public class LevelUpController : MonoBehaviour
{
    [Header("LevelUpPanel")]
    // The panel that appears when the player levels up
    public GameObject LevelUpPanel;
    [Header("Character rewards")]
    public Reward[] CharacterRewards;
    public TMP_Text RewardCharacterNameText;
    public Image RewardCharacterImage;

    [Header("Furniture rewards")]
    public Reward[] FurnitureRewards;
    public TMP_Text RewardFurnitureNameText;
    public Image RewardFurnitureImage;

    [Header("Coin rewards")]
    public Reward[] CoinRewards;
    public TMP_Text RewardCoinNameText;
    public Image RewardCoinsImage;

    [Header("Diamond rewards")]
    public Reward[] DiamondRewards;
    public TMP_Text RewardDiamondNameText;
    public Image RewardDiamondsImage;

    [Header("Other rewards")]
    public Reward[] OtherRewardRewards;
    public TMP_Text RewardOtherRewardsNameText;
    public Image RewardOthersImage;

    // Index to track the randomly selected reward
    private int rewardIndex;

    [Header("Confirmation window")]
    public GameObject Confirmation_Window;
    public Image RewardImage;
    public TMP_Text RewardText;
    public TMP_Text RewardTypeText;
    public TMP_Text RewardAmountText;

    [SerializeField] private StorageFurnitureReference _storageFurnitureReference;

    // Start is called before the first frame update
    void Start()
    {
        Action<ReadOnlyCollection<BaseCharacter>> charactersFetchedCallback = OnCharactersFetched;
        Storefront.Get().GetAllBaseCharacterYield(charactersFetchedCallback);
    }

    private void OnCharactersFetched(ReadOnlyCollection<BaseCharacter> characters)
    {
        if (characters == null || characters.Count == 0)
        {
            Debug.LogError("No characters available!");
            return;
        }

        BaseCharacter character = characters[0];
        Debug.Log($"Found character: {character.Name}, ID: {character.Id}");

        string characterId = character.Id.ToString();

        PlayerCharacter playerCharacter = PlayerCharacterPrototypes.GetCharacter(characterId);
        OnCharacterDetailsFetched(playerCharacter);
    }

    private void OnCharacterDetailsFetched(PlayerCharacter character)
    {
        if (character != null)
        {
            Debug.Log($"Character name: {character.Name}");
            Debug.Log($"Character image: {character.Image}");
        }
        else
        {
            Debug.LogError("Failed to fetch character details!");
        }

        FetchFurniture();
    }

    private void FetchFurniture()
    {
        if (_storageFurnitureReference == null)
        {
            Debug.LogError("Furniture not set");
            return;
        }

        var furnitureInfo = _storageFurnitureReference.Info;
        if (furnitureInfo == null || furnitureInfo.Count == 0)
        {
            Debug.LogError("No furniture available!");
            return;
        }

        foreach (var furnitureSet in furnitureInfo)
        {
            Debug.Log($"Funriture Set: {furnitureSet.Name}");

            foreach (var furniture in furnitureSet.FurnitureItems)
            {
                Debug.Log($"Furniture: {furniture.Name}, ID: {furniture.Id}");
            }
        }
    }

    // Method to activate the level-up popup and assign a random reward
    public void OpenPopup()
    {
        //Ensure the level - up panel is active
        if (LevelUpPanel != null)
        {
            LevelUpPanel.SetActive(true);
        }

        //AssignRandomCharacterReward();
        //AssignRandomFurnitureReward();
        //AssignRandomCoinsReward();
        //AssignRandomDiamondsReward();
        //AssignRandomReward5();
    }
    private void AssignRandomCharacterReward()
    {
        rewardIndex = Random.Range(0, CharacterRewards.Length);
        Reward selectedReward = CharacterRewards[rewardIndex];

        RewardCharacterImage.sprite = selectedReward.Sprite;
        RewardCharacterNameText.text = selectedReward.Name;
    }
    private void AssignRandomFurnitureReward()
    {
        rewardIndex = Random.Range(0, FurnitureRewards.Length);
        Reward selectedReward = FurnitureRewards[rewardIndex];

        RewardFurnitureImage.sprite = selectedReward.Sprite;
        RewardFurnitureNameText.text = selectedReward.Name;
    }
    private void AssignRandomCoinsReward()
    {
        rewardIndex = Random.Range(0, CoinRewards.Length);
        Reward selectedReward = CoinRewards[rewardIndex];

        int coinsAmount = Random.Range(100, 1000);
        
    }
    private void AssignRandomDiamondsReward()
    {
        rewardIndex = Random.Range(0, DiamondRewards.Length);
        Reward selectedReward = DiamondRewards[rewardIndex];

        int diamondAmount = Random.Range(100, 1000);
    }
    private void AssignRandomReward5()
    {
        rewardIndex = Random.Range(0, OtherRewardRewards.Length);
        Reward selectedReward = OtherRewardRewards[rewardIndex];

        RewardOthersImage.sprite = selectedReward.Sprite;
        RewardOtherRewardsNameText.text = selectedReward.Name;
    }
}

