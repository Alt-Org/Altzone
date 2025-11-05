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
using Newtonsoft.Json.Linq;
using System;
using Altzone.Scripts.Language;

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

public class HahmonValinta : AltMonoBehaviour
{
    [SerializeField] private Button lockInButton;
    [SerializeField] private CharacterData[] characterData;
    [SerializeField] private GameObject popupWindow; // Reference to the pop-up window panel
    [SerializeField] private TextLanguageSelectorCaller characterNameText; // Reference to the Text component for character name
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
        lockInButton.onClick.AddListener(()=>StartCoroutine(LockInCharacter(data.uniqueID)));

        // Activate the pop-up window
        popupWindow.SetActive(true);

        // Update the character name text
        //characterNameText.text = data.characterName;
        characterNameText.SetText(SettingsCarrier.Instance.Language, new string[1] { data.characterName });

        // Log the selected character's name
        Debug.Log("Selected character: " + data.characterName);
    }

    public IEnumerator LockInCharacter(CharacterID id)
    {
        // Check if a character is selected
        if (id != CharacterID.None)
        {
            // Log the selected character's information
            // Debug.Log("Locked in character: " + characterData[selectedCharacterIndex].characterName);
            bool callFinished = false;
            bool characterAdded = false;
            int i = 0;
            if (ServerManager.Instance.Player.currentAvatarId is null or 0 || !Enum.IsDefined(typeof(CharacterID), ServerManager.Instance.Player.currentAvatarId))
            {
                _playerData.SelectedCharacterId = (int)id;

                string noCharacter = ((int)CharacterID.None).ToString();
                _playerData.SelectedCharacterIds = new CustomCharacterListObject[3] { new(Id: CharacterID.None), new(Id: CharacterID.None), new(Id: CharacterID.None) };

                string body = JObject.FromObject(
                    new
                    {
                        _id = _playerData.Id,
                        currentAvatarId = _playerData.SelectedCharacterId

                    }/*,
                    JsonSerializer.CreateDefault(new JsonSerializerSettings { Converters = { new StringEnumConverter() } })*/
                ).ToString();

                yield return StartCoroutine(ServerManager.Instance.UpdatePlayerToServer(body, callback =>
                {
                    if (callback != null)
                    {
                        Debug.Log("Profile info updated.");
                        var store = Storefront.Get();
                        store.SavePlayerData(_playerData, null);
                    }
                    else
                    {
                        Debug.Log("Profile info update failed.");
                    }
                }));
                new WaitUntil(() => callFinished == true);
            }

            List<CustomCharacter> serverCharacters = null;
            bool gettingCharacter = true;
            yield return StartCoroutine(ServerManager.Instance.GetCustomCharactersFromServer(characterList =>
            {
                if (characterList == null)
                {
                    Debug.LogError("Failed to fetch Custom Characters.");
                    gettingCharacter = false;
                    serverCharacters = new();
                }
                else
                {
                    gettingCharacter = false;
                    serverCharacters = characterList;
                }
            }));
            new WaitUntil(() => gettingCharacter == false);

            if (serverCharacters.Count < 3)
            {
                List<CharacterID> characters = /*SelectStartingCharacter(id);*/ SelectAllCharacters(); // Temporarily overridden.
                foreach (var character in characters)
                {
                    callFinished = false;
                    StartCoroutine(ServerManager.Instance.AddCustomCharactersToServer(character, callback =>
                    {
                        if (callback != null)
                        {
                            Debug.Log("CustomCharacter added: " + character);
                            //if (i < _playerData.SelectedCharacterIds.Length) _playerData.SelectedCharacterIds[i].SetData(callback._id, (CharacterID)int.Parse(callback.characterId));
                            characterAdded = true;
                        }
                        else
                        {
                            Debug.Log("CustomCharacter adding failed.");
                        }
                        callFinished = true;
                    }));
                    yield return new WaitUntil(() => callFinished == true);
                    i++;
                }
                if (characterAdded)
                {
                    callFinished = false;
                    StartCoroutine(ServerManager.Instance.UpdateCustomCharacters((c, list) => callFinished = c, true));
                }
                new WaitUntil(() => callFinished == true);

                var gameConfig = GameConfig.Get();
                var playerSettings = gameConfig.PlayerSettings;
                var playerGuid = playerSettings.PlayerGuid;
                DataStore storefront = Storefront.Get();

                storefront.GetPlayerData(playerGuid, p => _playerData = p);

                string[] serverList = new string[_playerData.SelectedCharacterIds.Length];

                for (i = 0; i < _playerData.SelectedCharacterIds.Length; i++)
                {
                    serverList[i] = _playerData.SelectedCharacterIds[i].ServerID;
                }

                string body = JObject.FromObject(
                    new
                    {
                        _id = _playerData.Id,
                        currentAvatarId = _playerData.SelectedCharacterId,
                        battleCharacter_ids = serverList

                    }).ToString();
                callFinished = false;
                StartCoroutine(ServerManager.Instance.UpdatePlayerToServer(body, callback =>
                {
                    if (callback != null)
                    {
                        Debug.Log("Profile info updated.");
                        var store = Storefront.Get();
                        store.SavePlayerData(_playerData, null);
                    }
                    else
                    {
                        Debug.Log("Profile info update failed.");
                    }
                    callFinished = true;
                }));
                new WaitUntil(() => callFinished == true);

                // Reset the selected character index and disable the lock-in button
                selectedCharacterIndex = -1;
                lockInButton.interactable = false;

                // Deactivate the pop-up window
                popupWindow.SetActive(false);

                StartCoroutine(_windowNavigation.Navigate());
                yield break;
            }
            else
            {
                Debug.LogWarning("Player already had starting characters set.");
                StartCoroutine(_windowNavigation.Navigate());
            }

        }
        else
        {
            // No character selected, log a message or handle the case as needed
            Debug.Log("No character selected.");
        }
    }

    public List<CharacterID> SelectStartingCharacter(CharacterID id)
    {
        var list = new List<CharacterID>();
        switch (id)
        {
            case CharacterID.Bodybuilder:
                list.Add(CharacterID.Bodybuilder);
                list.Add(CharacterID.Conman);
                list.Add(CharacterID.Religious);
                break;
            case CharacterID.Joker:
                list.Add(CharacterID.Joker);
                list.Add(CharacterID.Racist);
                list.Add(CharacterID.Religious);
                break;
            case CharacterID.Religious:
                list.Add(CharacterID.Bodybuilder);
                list.Add(CharacterID.Joker);
                list.Add(CharacterID.Religious);
                break;
            case CharacterID.Artist:
                list.Add(CharacterID.Artist);
                list.Add(CharacterID.Soulsisters);
                list.Add(CharacterID.Religious);
                break;
            case CharacterID.Overeater:
                list.Add(CharacterID.Overeater);
                list.Add(CharacterID.Booksmart);
                list.Add(CharacterID.Religious);
                break;
            case CharacterID.SleepyHead:
                list.Add(CharacterID.SleepyHead);
                list.Add(CharacterID.Artist);
                list.Add(CharacterID.Religious);
                break;
            case CharacterID.Booksmart:
                list.Add(CharacterID.Booksmart);
                list.Add(CharacterID.Alcoholic);
                list.Add(CharacterID.Religious);
                break;
            default:
                break;
        }
        return list;
    }

    public List<CharacterID> SelectAllCharacters()
    {
        var list = new List<CharacterID>();

        var charIds = Enum.GetValues(typeof(CharacterID));
        foreach (CharacterID id in charIds)
        {
            if (!CustomCharacter.IsTestCharacter(id) && id != CharacterID.None)
                list.Add(id);
        }
            return list;
    }
}
