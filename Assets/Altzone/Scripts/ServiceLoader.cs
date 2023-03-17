using System.Collections;
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
        public bool isReady;

        private IEnumerator Start()
        {
            Debug.Log($"start");
            Localizer.LoadTranslations(Application.systemLanguage);
            AudioManager.Get();
            var store = Storefront.Get();
            var gameConfig = GameConfig.Get();
            yield return StartCoroutine(CheckDataStoreDataAndState(store));
            yield return StartCoroutine(CheckPlayerDataAndState(store, gameConfig));
            CheckGameInfoDebugOnly(store);
            Debug.Log($"exit");
            isReady = true;
        }

        private static IEnumerator CheckPlayerDataAndState(DataStore store, IGameConfig gameConfig)
        {
            Debug.Log($"start");

            var playerSettings = gameConfig.PlayerSettings;
            if (playerSettings.IsFirstTimePlaying)
            {
                Debug.Log("This Is First Time Playing");
                // TODO: this might have some effect to game server operations to set it up correctly for first time!? 
                playerSettings.IsFirstTimePlaying = false;
            }

            // Get current player.
            var playerGuid = playerSettings.PlayerGuid;
            PlayerData playerData = null;
            var isCallbackDone = false;
            store.GetPlayerData(playerGuid, foundPlayerData =>
            {
                playerData = foundPlayerData;
                isCallbackDone = true;
            });
            yield return new WaitUntil(() => isCallbackDone);

            if (playerData == null || store.PlayerDataVersion != CreateDefaultModels.PlayerDataVersion)
            {
                isCallbackDone = false;
                store.GetAllCustomCharactersTest(customCharacters =>
                {
                    var currentCustomCharacterId = customCharacters.Count == 0 ? 0 : customCharacters[0].Id;
                    if (playerData == null)
                    {
                        playerData = CreateDefaultModels.CreatePlayerData(playerGuid, "abba", currentCustomCharacterId);
                    }
                    else
                    {
                        playerData.CurrentCustomCharacterId = currentCustomCharacterId;
                    }
                    store.SavePlayerData(playerData, updatedPlayerData =>
                    {
                        playerData = updatedPlayerData;
                        store.PlayerDataVersion = CreateDefaultModels.PlayerDataVersion;
                        isCallbackDone = true;
                    });
                });
            }
            yield return new WaitUntil(() => isCallbackDone);
            Debug.Log($"PlayerData {playerData}");
            Assert.AreEqual(CreateDefaultModels.PlayerDataVersion, store.PlayerDataVersion);

            Debug.Log($"exit");
        }

        private static IEnumerator CheckDataStoreDataAndState(DataStore store)
        {
            Debug.Log($"start");
            var isCallbackDone = true;

            #region Production data in this section

            if (store.CharacterClassesVersion != CreateDefaultModels.CharacterClassesVersion)
            {
                isCallbackDone = false;
                // Replace default CharacterClass models.
                Debug.LogWarning($"Update CharacterClassesVersion {store.CharacterClassesVersion} <- {CreateDefaultModels.CharacterClassesVersion}");
                store.Set(CreateDefaultModels.CreateCharacterClasses(), success =>
                {
                    if (success)
                    {
                        store.CharacterClassesVersion = CreateDefaultModels.CharacterClassesVersion;
                    }
                    isCallbackDone = true;
                });
            }
            yield return new WaitUntil(() => isCallbackDone);
            Assert.AreEqual(CreateDefaultModels.CharacterClassesVersion, store.CharacterClassesVersion);

            if (store.GameFurnitureVersion != CreateDefaultModels.GameFurnitureVersion)
            {
                isCallbackDone = false;
                // Replace default CharacterClass models.
                Debug.LogWarning($"Update GameFurniture {store.GameFurnitureVersion} <- {CreateDefaultModels.GameFurnitureVersion}");
                store.Set(CreateDefaultModels.CreateGameFurniture(), success =>
                {
                    if (success)
                    {
                        store.GameFurnitureVersion = CreateDefaultModels.GameFurnitureVersion;
                    }
                    isCallbackDone = true;
                });
            }
            yield return new WaitUntil(() => isCallbackDone);
            Assert.AreEqual(CreateDefaultModels.GameFurnitureVersion, store.GameFurnitureVersion);

            #endregion

            #region Development data in this section

            if (store.ClanDataVersion != CreateDefaultModels.ClanDataVersion)
            {
                isCallbackDone = false;
                store.GetAllGameFurniture(furniture =>
                {
                    var clanData = CreateDefaultModels.CreateClanData("abba", furniture);
                    store.SaveClanData(clanData, updatedClanData => { Debug.Log($"Create clan {updatedClanData}"); });
                    store.ClanDataVersion = CreateDefaultModels.ClanDataVersion;
                    isCallbackDone = true;
                });
            }
            yield return new WaitUntil(() => isCallbackDone);
            Assert.AreEqual(CreateDefaultModels.ClanDataVersion, store.ClanDataVersion);

            if (store.CustomCharactersVersion != CreateDefaultModels.CustomCharactersVersion)
            {
                isCallbackDone = false;
                // Replace default CustomCharacter models.
                Debug.LogWarning($"Update CustomCharactersVersion {store.CustomCharactersVersion} <- {CreateDefaultModels.CustomCharactersVersion}");
                store.Set(CreateDefaultModels.CreateCustomCharacters(), success =>
                {
                    if (success)
                    {
                        store.CustomCharactersVersion = CreateDefaultModels.CustomCharactersVersion;
                    }
                    isCallbackDone = true;
                });
            }
            yield return new WaitUntil(() => isCallbackDone);
            Assert.AreEqual(CreateDefaultModels.CustomCharactersVersion, store.CustomCharactersVersion);

            #endregion

            Debug.Log($"exit");
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void CheckGameInfoDebugOnly(DataStore store)
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