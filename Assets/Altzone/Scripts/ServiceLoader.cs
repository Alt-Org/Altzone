using System.Diagnostics;
using System.Linq;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts.Service.Audio;
using Prg.Scripts.Common.Unity.Localization;
using UnityEngine;
using UnityEngine.Assertions;

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
            CheckDataStoreDataAndState(store);
            ShowDebugGameInfo(store);
        }

        private static void CheckPlayerDataAndState(DataStore store, IGameConfig gameConfig)
        {
            var playerSettings = gameConfig.PlayerSettings;
            if (playerSettings.IsFirstTimePlaying)
            {
                Debug.Log("This Is First Time Playing");
                // TODO: this might have some effect to game server operations to set it up correctly for first time!? 
                playerSettings.IsFirstTimePlaying = false;
            }
            var playerGuid = playerSettings.PlayerGuid;
            store.GetPlayerData(playerGuid, playerData =>
            {
                if (playerData == null)
                {
                    // Create new player for us with first custom character we have - if any.
                    // This is temporary solution until we have something more robust way to create first player.
                    store.GetAllCustomCharactersTest(customCharacters =>
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
            });
        }

        private static void CheckDataStoreDataAndState(DataStore store)
        {
            #region Production data in this section

            if (store.CharacterClassesVersion != CreateDefaultModels.CharacterClassesVersion)
            {
                // Replace default CharacterClass models.
                Debug.LogWarning($"Update CharacterClassesVersion {store.CharacterClassesVersion} <- {CreateDefaultModels.CharacterClassesVersion}");
                store.Set(CreateDefaultModels.CreateCharacterClasses(), success =>
                {
                    if (success)
                    {
                        store.CharacterClassesVersion = CreateDefaultModels.CharacterClassesVersion;
                    }
                });
                
            }
            if (store.GameFurnitureVersion != CreateDefaultModels.GameFurnitureVersion)
            {
                // Replace default CharacterClass models.
                Debug.LogWarning($"Update GameFurniture {store.GameFurnitureVersion} <- {CreateDefaultModels.GameFurnitureVersion}");
                store.Set(CreateDefaultModels.CreateGameFurniture(), success =>
                {
                    if (success)
                    {
                        store.GameFurnitureVersion = CreateDefaultModels.GameFurnitureVersion;
                    }
                });
                
            }
            // No conversion rules for Player or Clan data yet.
            Assert.AreEqual(1, store.PlayerDataVersion);
            Assert.AreEqual(1, store.ClanDataVersion);

            #endregion

            if (!AppPlatform.IsEditor)
            {
                return;
            }

            #region Development data in this section

            if (store.CustomCharactersVersion != CreateDefaultModels.CustomCharactersVersion)
            {
                // Replace default CustomCharacter models.
                Debug.LogWarning($"Update CustomCharactersVersion {store.CustomCharactersVersion} <- {CreateDefaultModels.CustomCharactersVersion}");
                store.Set(CreateDefaultModels.CreateCustomCharacters(), success =>
                {
                    if (success)
                    {
                        store.CustomCharactersVersion = CreateDefaultModels.CustomCharactersVersion;
                    }
                });
            }

            #endregion
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("FORCE_LOG")]
        private static void ShowDebugGameInfo(DataStore store)
        {
            store.GetAllCharacterClasses(characterClasses =>
            {
                store.GetAllCustomCharactersTest(customCharacters =>
                {
                    var playerPrefabs = GameConfig.Get().PlayerPrefabs;
                    var isCustomCharactersValid = true;
                    foreach (var customCharacter in customCharacters)
                    {
                        if (characterClasses.All(x => x.CharacterClassId != customCharacter.CharacterClassId))
                        {
                            Debug.LogWarning($"customCharacter {customCharacter.Id} {customCharacter.Name} " +
                                             $"does not have CharacterModel {customCharacter.CharacterClassId}");
                            isCustomCharactersValid = false;
                        }
                        var prefabIndex = int.Parse(customCharacter.UnityKey);
                        if (playerPrefabs.GetPlayerPrefab(prefabIndex) == null)
                        {
                            if (isCustomCharactersValid)
                            {
                                Debug.Log($"characterClasses {characterClasses.Count} ver {store.CharacterClassesVersion}");
                                Debug.Log($"customCharacters {customCharacters.Count} ver {store.CustomCharactersVersion}");
                            }
                            Debug.LogWarning($"customCharacter {customCharacter.Id} {customCharacter.Name} " +
                                             $"does not have PlayerPrefab {customCharacter.UnityKey}");
                            isCustomCharactersValid = false;
                        }
                    }
                    store.GetAllBattleCharactersTest(battleCharacters =>
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