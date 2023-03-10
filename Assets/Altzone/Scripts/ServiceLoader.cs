using System.Diagnostics;
using System.Linq;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco;
using Altzone.Scripts.Service.Audio;
using Prg.Scripts.Common.Unity.Localization;
using UnityEngine;

namespace Altzone.Scripts
{
    /// <summary>
    /// Loads all services used by this game.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class ServiceLoader : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log($"{name}");
            Localizer.LoadTranslations(Application.systemLanguage);
            AudioManager.Get();
            var store = Storefront.Get();
            var gameConfig = GameConfig.Get();
            CheckPlayerDataAndState(store, gameConfig);
            ShowDebugGameInfo(store);
        }

        private static void CheckPlayerDataAndState(DataStore store, IGameConfig gameConfig)
        {
            var playerSettings = gameConfig.PlayerSettings;
            var playerGuid = playerSettings.PlayerGuid;
            store.GetPlayerData(playerGuid, playerData =>
            {
                if (playerData == null)
                {
                    // Create new player for us with first custom character we have - if any.
                    store.GetAllCustomCharacters(customCharacters =>
                    {
                        var currentCustomCharacterId = customCharacters.Count == 0 ? 0 : customCharacters[0].Id;
                        playerData = new PlayerData(0, 0, currentCustomCharacterId, "Player", 0, playerGuid);
                        store.SavePlayerData(playerData, updatedPlayerData => { Debug.Log($"Create player {updatedPlayerData}"); });
                    });
                }
                else
                {
                    Debug.Log($"Load player {playerData}");
                }
                if (playerSettings.IsFirstTimePlaying)
                {
                    Debug.Log("This Is First Time Playing");
                }
                gameConfig.PlayerDataModel = playerData;
            });
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("FORCE_LOG")]
        private static void ShowDebugGameInfo(DataStore store)
        {
            store.GetAllCharacterClasses(characterClasses =>
            {
                Debug.Log($"characterClasses {characterClasses.Count}");
                store.GetAllCustomCharacters(customCharacters =>
                {
                    var playerPrefabs = GameConfig.Get().PlayerPrefabs;
                    var isCustomCharactersValid = true;
                    foreach (var customCharacter in customCharacters)
                    {
                        if (characterClasses.All(x => x.Id != customCharacter.CharacterClassId))
                        {
                            Debug.LogWarning($"customCharacter {customCharacter.Id} {customCharacter.Name} " +
                                             $"does not have CharacterModel {customCharacter.CharacterClassId}");
                            isCustomCharactersValid = false;
                        }
                        var prefabIndex = int.Parse(customCharacter.PlayerPrefabKey);
                        if (playerPrefabs.GetPlayerPrefab(prefabIndex) == null)
                        {
                            Debug.LogWarning($"customCharacter {customCharacter.Id} {customCharacter.Name} " +
                                             $"does not have PlayerPrefab {customCharacter.PlayerPrefabKey}");
                            isCustomCharactersValid = false;
                        }
                    }
                    Debug.Log($"customCharacters {customCharacters.Count}");
                    store.GetAllBattleCharacters(battleCharacters =>
                    {
                        Debug.Log($"battleCharacters {battleCharacters.Count}");
                        if (isCustomCharactersValid)
                        {
                            return;
                        }
                        // Dump all battle characters if something is wrong in storage.
                        foreach (var battleCharacter in battleCharacters)
                        {
                            Debug.Log($"battleCharacter {battleCharacter}");
                        }
                    });
                });
            });
        }
    }
}