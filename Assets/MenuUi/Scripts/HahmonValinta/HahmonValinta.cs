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
using MenuUi.Scripts.Window;

[System.Serializable]
public class CharacterData
{
    public CharacterID uniqueID;
    public string characterName;
    public Button characterButton;

    public CharacterData(int id, string name)
    {
        uniqueID = (CharacterID)id;
        characterName = name;
    }
}

public class HahmonValinta : MonoBehaviour
{
    [SerializeField] private Button lockInButton;
    [SerializeField] private CharacterData[] characterData;
    [SerializeField] private GameObject popupWindow; // Reference to the pop-up window panel
    [SerializeField] private TextMeshProUGUI characterNameText; // Reference to the Text component for character name
    [SerializeField] private WindowNavigation _windowNavigation;

    private int selectedCharacterIndex = -1;
    private PlayerData _playerData;
    private List<BattleCharacter> characters;

    void Start()
    {
        // Initialize the pop-up window as inactive
        popupWindow.SetActive(false);

        // Assign onClick events for character buttons
        for (int i = 0; i < characterData.Length; i++)
        {
            int characterIndex = i;
            characterData[i].characterButton.onClick.AddListener(() => CharacterSelected(characterData[characterIndex]));
        }

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
            //characters = playerData.BattleCharacters.ToList();
        });
    }

    void CharacterSelected(CharacterData data)
    {
        lockInButton.onClick.RemoveAllListeners();
        // 
        lockInButton.onClick.AddListener(()=>LockInCharacter(data.uniqueID));

        // Activate the pop-up window
        popupWindow.SetActive(true);

        // Update the character name text
        characterNameText.text = data.characterName;

        // Log the selected character's name
        Debug.Log("Selected character: " + data.characterName);
    }

    public void LockInCharacter(CharacterID id)
    {
        // Check if a character is selected
        if (id != CharacterID.None)
        {
            // Log the selected character's information
           // Debug.Log("Locked in character: " + characterData[selectedCharacterIndex].characterName);

            if ((int) id != _playerData.SelectedCharacterId)
            {
                _playerData.SelectedCharacterId = (int) id;
                _playerData.SelectedCharacterIds[0] = (int) id;
                var store = Storefront.Get();
                store.SavePlayerData(_playerData, null);
            }
            // Reset the selected character index and disable the lock-in button
            selectedCharacterIndex = -1;
            lockInButton.interactable = false;

            // Deactivate the pop-up window
            popupWindow.SetActive(false);


            StartCoroutine(_windowNavigation.Navigate());
        }
        else
        {
            // No character selected, log a message or handle the case as needed
            Debug.Log("No character selected.");
        }
    }
}
