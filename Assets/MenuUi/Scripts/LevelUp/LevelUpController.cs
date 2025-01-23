using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.LowLevel;

public class LevelUpController : MonoBehaviour
{
    // The panel that appears when the player levels up
    public GameObject LevelUpPanel;

    // The image element where the reward sprite will be displayed
    public Image RewardCharacterImage;
    public Image RewardFurnitureImage;
    public Image RewardCoinsImage;
    public Image RewardDiamondsImage;

    public Image Reward5Image;

    // An array of possible reward sprites
    public Sprite[] CharacterSprites;
    public Sprite[] FurnitureSprites;
    public Sprite[] CoinsSprites;
    public Sprite[] DiamondsSprites;

    public Sprite[] Reward5Sprites;

    // Index to track the randomly selected reward
    public int rewardIndex; 

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

        CharacterReward();
        FurnitureReward();
        CoinsReward();
        DiamondsReward();
    }

    private void CharacterReward()
    {
        // Generate a random index to select a reward from the sprite array
        rewardIndex = Random.Range(0, 3);

        // Assign the randomly selected sprite to the reward image
        RewardCharacterImage.sprite = CharacterSprites[rewardIndex];
    }

    private void FurnitureReward()
    {
        // Generate a random index to select a reward from the sprite array
        rewardIndex = Random.Range(0, 3);

        // Assign the randomly selected sprite to the reward image
        RewardFurnitureImage.sprite = FurnitureSprites[rewardIndex];
    }

    private void CoinsReward()
    {
        // Generate a random index to select a reward from the sprite array
        rewardIndex = Random.Range(0, 3);

        // Assign the randomly selected sprite to the reward image
        RewardCoinsImage.sprite = CoinsSprites[rewardIndex];
    }

    private void DiamondsReward()
    {
        // Generate a random index to select a reward from the sprite array
        rewardIndex = Random.Range(0, 2);

        // Assign the randomly selected sprite to the reward image
        RewardDiamondsImage.sprite = DiamondsSprites[rewardIndex];
    }
}

