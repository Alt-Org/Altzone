using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts.Config;
using Altzone.Scripts;
using System.Linq;
using Altzone.Scripts.Model.Poco.Game;

[System.Serializable]
public class CharacterData
{
    public CharacterID uniqueID;
    public string characterName;

    public CharacterData(int id, string name)
    {
        uniqueID = (CharacterID)id;
        characterName = name;
    }
}

public class HahmonValinta : MonoBehaviour
{
    [SerializeField] private Button[] characterButtons;
    [SerializeField] private Button lockInButton;
    [SerializeField] private CharacterData[] characterData;
    [SerializeField] private GameObject popupWindow; // Reference to the pop-up window panel
    [SerializeField] private TextMeshProUGUI characterNameText; // Reference to the Text component for character name

    private int selectedCharacterIndex = -1;
    private PlayerData _playerData;
    private List<BattleCharacter> characters;

    void Start()
    {
        // Initialize the pop-up window as inactive
        popupWindow.SetActive(false);

        // Assign onClick events for character buttons
        for (int i = 0; i < characterButtons.Length; i++)
        {
            int characterIndex = i;
            characterButtons[i].onClick.AddListener(() => CharacterSelected(characterIndex));
        }

        // Assign onClick event for lock-in button
        lockInButton.onClick.AddListener(LockInCharacter);
    }

    private void OnEnable()
    {
        Load();
    }

    private void Load()
    {
        Debug.Log("Start");
        var gameConfig = GameConfig.Get();
        var playerSettings = gameConfig.PlayerSettings;
        var playerGuid = playerSettings.PlayerGuid;
        var store = Storefront.Get();
        store.GetPlayerData(playerGuid, playerData =>
        {
            _playerData = playerData;
            characters = playerData.BattleCharacters.ToList();
        });
    }

    void CharacterSelected(int characterIndex)
    {
        // Update the selected character index
        selectedCharacterIndex = characterIndex;

        // Activate the pop-up window
        popupWindow.SetActive(true);

        // Update the character name text
        characterNameText.text = characterData[characterIndex].characterName;

        // Log the selected character's name
        Debug.Log("Selected character: " + characterData[characterIndex].characterName);
    }

    public void LockInCharacter()
    {
        // Check if a character is selected
        if (selectedCharacterIndex != -1)
        {
            // Log the selected character's information
            Debug.Log("Locked in character: " + characterData[selectedCharacterIndex].characterName);

            if ((int)characterData[selectedCharacterIndex].uniqueID != _playerData.SelectedCharacterId)
            {
                _playerData.SelectedCharacterId = (int)characterData[selectedCharacterIndex].uniqueID;
                var store = Storefront.Get();
                store.SavePlayerData(_playerData, null);
            }
            // Reset the selected character index and disable the lock-in button
            selectedCharacterIndex = -1;
            lockInButton.interactable = false;

            // Deactivate the pop-up window
            popupWindow.SetActive(false);
        }
        else
        {
            // No character selected, log a message or handle the case as needed
            Debug.Log("No character selected.");
        }
    }
}
