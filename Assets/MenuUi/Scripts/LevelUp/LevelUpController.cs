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


    [Header("LevelUpPanel")]
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

    // 5 rewards
    //Remember change the name
    public Reward[] Reward5Rewards;
    public TMP_Text Reward5RewardsNameText;
    public Image Reward5Image;

    // Index to track the randomly selected reward
    private int rewardIndex;

    [Header("Confirmation window")]
    public GameObject Confirmation_Window;
    public Image ConfirmationImage;
    public TMP_Text RewardText;
    public TMP_Text ConfirmationTypeText;
    public TMP_Text AmountText;

    private int selectedAmount;
    private string selectedType;
    private bool selectedHasRarity;


    // Start is called before the first frame update
    void Start()
    {
        //OpenPopup();
    }

    // Method to activate the level-up popup and assign a random reward
    public void OpenPopup()
    {
        //Ensure the level - up panel is active
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
        Reward selectedReward = CharacterRewards[rewardIndex];

        RewardCharacterImage.sprite = selectedReward.Sprite;
        RewardCharacterNameText.text = selectedReward.Name;

        ShowConfirmation(selectedReward, "Character", 0, true);
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

        ShowConfirmation(selectedReward, "Coins", coinsAmount, false);
    }
    private void AssignRandomDiamondsReward()
    {
        rewardIndex = Random.Range(0, DiamondRewards.Length);
        Reward selectedReward = DiamondRewards[rewardIndex];

        int diamondAmount = Random.Range(100, 1000);

        ShowConfirmation(selectedReward, "Coins", diamondAmount, false);
    }
    private void AssignRandomReward5()
    {
        rewardIndex = Random.Range(0, Reward5Rewards.Length);
        Reward selectedReward = Reward5Rewards[rewardIndex];

        Reward5Image.sprite = selectedReward.Sprite;
        Reward5RewardsNameText.text = selectedReward.Name;
    }

    public void ShowConfirmation(Reward selectedReward, string type, int amount = 0, bool showRarity =true)
    {

        if (selectedReward == null) return;

        ConfirmationImage.sprite = selectedReward.Sprite;
        RewardText.text = selectedReward.Name;

        if (selectedHasRarity)
        {
            ConfirmationTypeText.text = "Rarity: " + selectedType;
            ConfirmationTypeText.gameObject.SetActive(true);
        }
        else
        {
            ConfirmationTypeText.gameObject.SetActive(false);
        }

        if (amount > 0)
        {
            AmountText.text = "Amount: " + selectedAmount.ToString();
            AmountText.gameObject.SetActive(true);
        }
        else
        {
            AmountText.gameObject.SetActive(false);
        }

        Confirmation_Window.SetActive(true);
    }

    public class RewardObject
    {
        protected string _name;

        public string Name { get => _name; }

        public string Image { get; }

        public string Type { get; }

        protected RewardObject(string name, string image)
        {
            _name = name;
            Image = image;
        }
    }

    //public class CharacterRewardObject : RewardObject
    //{

    //}
}

