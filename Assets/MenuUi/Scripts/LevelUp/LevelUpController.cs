using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class Reward
{
    public string Name;
    public Sprite Sprite;

    public Reward(string name, Sprite sprite)
    {
        Name = name;
        Sprite = sprite;
    }
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

    [Header("Reward buttons")]
    public Button CharacterRewardButton;
    public Button FurnitureRewardButton;
    public Button DiamondsRewardButton;
    public Button CoinsRewardButton;
    public Button OtherRewardButton;

    public Transform rewardsContainer;
    public GameObject rewardPrefab;

    // List of rewards for each button
    private List<Reward> characterRewards = new List<Reward>
    { 
        new Reward("Ahmatti", Resources.Load<Sprite>("Images/Reward1")),
        new Reward("Tutkija", Resources.Load<Sprite>("Images/Reward2")),
        new Reward("Tapauskovainen", Resources.Load<Sprite>("Images/Reward3"))
    };

    private List<Reward> furnitureRewards = new List<Reward>
    {
        new Reward("Sohva", Resources.Load<Sprite>("Images/Reward4")),
        new Reward("Kasvi", Resources.Load<Sprite>("Images/Reward5")),
        new Reward("Pöytä", Resources.Load<Sprite>("Images/Reward6"))
    };

    private List<Reward> diamondRewards = new List<Reward>
    {
        new Reward("10 diamonds", Resources.Load<Sprite>("Images/Reward7")),
        new Reward("100 diamonds", Resources.Load<Sprite>("Images/Reward8")),
        new Reward("1000 diamonds", Resources.Load<Sprite>("Images/Reward9"))
    };

    private List<Reward> coinRewards = new List<Reward>
    {
        new Reward("10 coins", Resources.Load<Sprite>("Images/Reward10")),
        new Reward("100 coins", Resources.Load<Sprite>("Images/Reward11")),
        new Reward("1000 coins", Resources.Load<Sprite>("Images/Reward12"))
    };

    private List<Reward> otherRewards = new List<Reward>
    {
        new Reward("1 ticket", Resources.Load<Sprite>("Images/Reward13")),
        new Reward("5 tickets", Resources.Load<Sprite>("Images/Reward14")),
        new Reward("10 tickets", Resources.Load<Sprite>("Images/Reward15"))
    };


    // Start is called before the first frame update
    void Start()
    {
        Confirmation_Window.SetActive(false);

        CharacterRewardButton.onClick.AddListener(() => ShowPopup(characterRewards));
        FurnitureRewardButton.onClick.AddListener(() => ShowPopup(furnitureRewards));
        DiamondsRewardButton.onClick.AddListener(() => ShowPopup(diamondRewards));
        CoinsRewardButton.onClick.AddListener(() => ShowPopup(coinRewards));
        OtherRewardButton.onClick.AddListener(() => ShowPopup(otherRewards));
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

    void ShowPopup(List<Reward> rewardOptions)
    {
        foreach (Transform child in rewardsContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var reward in rewardOptions)
        {
            GameObject rewardEntry = Instantiate(rewardPrefab, rewardsContainer);
            rewardEntry.transform.GetChild(0).GetComponent<Text>().text = reward.Name;
            rewardEntry.transform.GetChild(1).GetComponent<Image>().sprite = reward.Sprite;
        }

        Confirmation_Window.SetActive(true);
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

