using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.LowLevel;

[System.Serializable]
public class Reward
{
    public string Name;
    public Sprite Sprite;
}

public class LevelUpController : MonoBehaviour
{
    // The panel that appears when the player levels up
    public GameObject LevelUpPanel;

    // The image element where the reward sprite will be displayed

    // Character rewards
    public Reward[] CharacterRewards;
    public TMP_Text RewardCharacterNameText;
    public Image RewardCharacterImage;

    // Furniture rewards
    public Reward[] FurnitureRewards;
    public TMP_Text RewardFurnitureNameText;
    public Image RewardFurnitureImage;

    // Coin rewards
    public Reward[] CoinRewards;
    public TMP_Text RewardCoinNameText;
    public Image RewardCoinsImage;

    // Diamond rewards
    public Reward[] DiamondRewards;
    public TMP_Text RewardDiamondNameText;
    public Image RewardDiamondsImage;

    // 5 rewards
    //Remember change the name
    public Reward[] Reward5Rewards;
    public TMP_Text Reward5RewardsNameText;
    public Image Reward5Image;

    // Index to track the randomly selected reward
    private int rewardIndex; 

    // Start is called before the first frame update
    void Start()
    {
        OpenPopup();
    }

    // Method to activate the level-up popup and assign a random reward
    public void OpenPopup()
    {
        // Ensure the level-up panel is active
        if (LevelUpPanel != null)
        {
            LevelUpPanel.SetActive(true);
        }

        AssignRandomCharacterReward();
        AssignRandomFurnitureReward();
        AssignRandomCoinsReward();
        AssignRandomDiamondsReward();
        AssignRandomReward5();
    }

    private void AssignRandomCharacterReward()
    {
        rewardIndex = Random.Range(0, CharacterRewards.Length);
        RewardCharacterImage.sprite = CharacterRewards[rewardIndex].Sprite;
        RewardCharacterNameText.text = CharacterRewards[rewardIndex].Name;
    }
    private void AssignRandomFurnitureReward()
    {
        rewardIndex = Random.Range(0, FurnitureRewards.Length);
        RewardFurnitureImage.sprite = FurnitureRewards[rewardIndex].Sprite;
        RewardFurnitureNameText.text = FurnitureRewards[rewardIndex].Name;
    }
    private void AssignRandomCoinsReward()
    {
        rewardIndex = Random.Range(0, CoinRewards.Length);
        RewardCoinsImage.sprite = CoinRewards[rewardIndex].Sprite;
        RewardCoinNameText.text= CoinRewards[rewardIndex].Name;
    }
    private void AssignRandomDiamondsReward()
    {
        rewardIndex = Random.Range(0, DiamondRewards.Length);
        RewardDiamondsImage.sprite = DiamondRewards[rewardIndex].Sprite;
        RewardDiamondNameText.text = DiamondRewards[rewardIndex].Name;
    }
    private void AssignRandomReward5()
    {
        rewardIndex = Random.Range(0, Reward5Rewards.Length);
        Reward5Image.sprite = Reward5Rewards[rewardIndex].Sprite;
        Reward5RewardsNameText.text = Reward5Rewards[rewardIndex].Name; 
    }
}

