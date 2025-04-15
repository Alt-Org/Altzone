using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Altzone.Scripts;
using Altzone.Scripts.ModelV2;
using System.Collections.ObjectModel;
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

public class LevelUpController : AltMonoBehaviour
{
    [Header("UI Elements")]
    // The panel that appears when the player levels up
    public GameObject LevelUpPanel;
    public Button[] rewardButtons;
    public GameObject Confirmation_Window;
    public TMP_Text[] rewardNameTexts;

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
        Debug.Log("LevelUpController started!");
        InitializeButtons();

        if (Storefront.Get() == null)
        {
            Debug.LogError("Storefront instance is null!");
            return;
        }
        Debug.Log("Fetching characters from Storefront...");
        Storefront.Get().GetAllBaseCharacterYield(OnCharactersFetched);
    }

    // Method to activate the level-up popup and assign a random reward
    public void OpenPopup()
    {

        Debug.Log("Opening level up popup");
        //Ensure the level - up panel is active
        if (LevelUpPanel != null)
        {
            LevelUpPanel.SetActive(true);
            GenerateRandomRewards();
        }
        else
        {
            Debug.LogError("LevelUpPanel is not assigned!");
        }
    }
    public void ConfirmReward()
    {
        Debug.Log("Reward confirmed");
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

        // 1. Furnitures
        if (_storageFurnitureReference != null && _storageFurnitureReference.Info.Count > 0)
        {
            var set = _storageFurnitureReference.Info[Random.Range(0, _storageFurnitureReference.Info.Count)];
            var furniture = set.list[Random.Range(0, set.list.Count)];
            currentRewards.Add(new Reward
            {
                image = furniture.Image,
                name = furniture.VisibleName,
                type = "Furniture",
                description = $"New furniture: {furniture.VisibleName}"
            });
        }

        // 2. Characters
        if (availableCharacters.Count > 0)
        {
            var character = availableCharacters[Random.Range(0, availableCharacters.Count)];
            var charData = PlayerCharacterPrototypes.GetCharacter(((int)character.Id).ToString()); 
            if (charData != null)
            {
                Debug.Log($"Adding character reward: {charData.Name}");

                currentRewards.Add(new Reward
                {
                    image = charData.GalleryImage,
                    name = charData.Name,
                    type = "Character",
                    description = $"New character: {charData.Name}"
                });
            }
        }
  
        // 3. Coins
        int coinAmount = Random.Range(100, 1000);
        currentRewards.Add(new Reward
        {
            image = coinSprite,
            name = $"{coinAmount} coins",
            type = "Currency",
            description = $"You got {coinAmount} coins!"
        });

        // 4. Diamonds
        int diamondAmount = Random.Range(5, 20);
        currentRewards.Add(new Reward
        {
            image = diamondSprite,
            name = $"{diamondAmount} Diamonds",
            type = "Premium",
            description = $"You got {diamondAmount} diamonds!"
        });

        // 5. other rewards
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
    private void UpdateButtonImages()
    {
        for (int i = 0; i < rewardButtons.Length && i < currentRewards.Count; i++)
        {
            rewardButtons[i].GetComponent<Image>().sprite = currentRewards[i].image;
            rewardNameTexts[i].text = currentRewards[i].name;
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
        if (characters == null)
        {
            Debug.LogError("Characters fetch returned null!");
            return;
        }
        availableCharacters = new List<BaseCharacter>(characters);
        Debug.Log($"Loaded {availableCharacters.Count} characters");
    }
}

