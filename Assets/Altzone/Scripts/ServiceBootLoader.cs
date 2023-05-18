using System.Collections;
using System.Diagnostics;
using System.Linq;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model;
using Altzone.Scripts.Model.Poco.Player;
using UnityEngine;
using UnityEngine.Assertions;

namespace Altzone.Scripts
{
    /// <summary>
    /// Loads all services used by this game.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class ServiceBootLoader : MonoBehaviour
    {
        [SerializeField, Tooltip("Shows how long it took to load services")] private string _startupTime;

        public bool IsReady { get; private set; }

        private IEnumerator Start()
        {
            Debug.Log($"start");
            var startTime = Time.unscaledTime;
#if USE_LOCALIZER
            Localizer.LoadTranslations(Application.systemLanguage);
#endif
            var store = Storefront.Get();
            if (store.Version.VersionNumber != CreateDefaultModels.MasterStorageVersionNumber)
            {
                // Just re-create storage as we do not have anything else to do for now.
                store = Storefront.ResetStorage(CreateDefaultModels.MasterStorageVersionNumber);
                Assert.AreEqual(store.Version.VersionNumber, CreateDefaultModels.MasterStorageVersionNumber);
            }
            var gameConfig = GameConfig.Get();
            yield return StartCoroutine(CheckDataStoreDataAndState(store));
            yield return StartCoroutine(CheckPlayerDataAndState(store, gameConfig));
            CheckGameInfoDebugOnly(store);
            _startupTime = $"{Time.unscaledTime - startTime:0.000}";
            Debug.Log($"exit in {_startupTime}");
            IsReady = true;
        }

        private static IEnumerator CheckPlayerDataAndState(DataStore store, GameConfig gameConfig)
        {
            Debug.Log($"start");

            // Get current player.
            var playerSettings = gameConfig.PlayerSettings;
            var playerGuid = playerSettings.PlayerGuid;
            PlayerData playerData = null;
            var isCallbackDone = false;
            store.GetPlayerData(playerGuid, foundPlayerData =>
            {
                playerData = foundPlayerData;
                isCallbackDone = true;
            });
            yield return new WaitUntil(() => isCallbackDone);

            if (playerData == null || store.Version.PlayerDataVersion != CreateDefaultModels.PlayerDataVersion)
            {
                isCallbackDone = false;
                store.ForTest.GetAllCustomCharactersTest(customCharacters =>
                {
                    const string unknownCustomCharacterId = "0";
                    var currentCustomCharacterId = customCharacters.Count == 0 ? unknownCustomCharacterId : customCharacters[0].Id;
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
                        store.Version.PlayerDataVersion = CreateDefaultModels.PlayerDataVersion;
                        isCallbackDone = true;
                    });
                });
            }
            yield return new WaitUntil(() => isCallbackDone);
            Debug.Log($"PlayerData {playerData}");
            Assert.AreEqual(CreateDefaultModels.PlayerDataVersion, store.Version.PlayerDataVersion);

            Debug.Log($"exit");
        }

        private static IEnumerator CheckDataStoreDataAndState(DataStore store)
        {
            Debug.Log($"start");
            var isCallbackDone = true;

            #region Production data in this section

            if (store.Version.CharacterClassesVersion != CreateDefaultModels.CharacterClassesVersion)
            {
                isCallbackDone = false;
                // Replace default CharacterClass models.
                Debug.LogWarning(
                    $"Update CharacterClassesVersion {store.Version.CharacterClassesVersion} <- {CreateDefaultModels.CharacterClassesVersion}");
                Storefront.Set(CreateDefaultModels.CreateCharacterClasses(), success =>
                {
                    if (success)
                    {
                        store.Version.CharacterClassesVersion = CreateDefaultModels.CharacterClassesVersion;
                    }
                    isCallbackDone = true;
                });
            }
            yield return new WaitUntil(() => isCallbackDone);
            Assert.AreEqual(CreateDefaultModels.CharacterClassesVersion, store.Version.CharacterClassesVersion);

            if (store.Version.GameFurnitureVersion != CreateDefaultModels.GameFurnitureVersion)
            {
                isCallbackDone = false;
                // Replace default CharacterClass models.
                Debug.LogWarning($"Update GameFurniture {store.Version.GameFurnitureVersion} <- {CreateDefaultModels.GameFurnitureVersion}");
                Storefront.Set(CreateDefaultModels.CreateGameFurniture(), success =>
                {
                    if (success)
                    {
                        store.Version.GameFurnitureVersion = CreateDefaultModels.GameFurnitureVersion;
                    }
                    isCallbackDone = true;
                });
            }
            yield return new WaitUntil(() => isCallbackDone);
            Assert.AreEqual(CreateDefaultModels.GameFurnitureVersion, store.Version.GameFurnitureVersion);

            #endregion

            #region Development data in this section

            if (store.Version.ClanDataVersion != CreateDefaultModels.ClanDataVersion)
            {
                isCallbackDone = false;
                store.GetAllGameFurniture(furniture =>
                {
                    var clanData = CreateDefaultModels.CreateClanData("abba", furniture);
                    store.SaveClanData(clanData, updatedClanData => { Debug.Log($"Create clan {updatedClanData}"); });
                    store.Version.ClanDataVersion = CreateDefaultModels.ClanDataVersion;
                    isCallbackDone = true;
                });
            }
            yield return new WaitUntil(() => isCallbackDone);
            Assert.AreEqual(CreateDefaultModels.ClanDataVersion, store.Version.ClanDataVersion);

            if (store.Version.CustomCharactersVersion != CreateDefaultModels.CustomCharactersVersion)
            {
                isCallbackDone = false;
                // Replace default CustomCharacter models.
                Debug.LogWarning(
                    $"Update CustomCharactersVersion {store.Version.CustomCharactersVersion} <- {CreateDefaultModels.CustomCharactersVersion}");
                Storefront.Set(CreateDefaultModels.CreateCustomCharacters(), success =>
                {
                    if (success)
                    {
                        store.Version.CustomCharactersVersion = CreateDefaultModels.CustomCharactersVersion;
                    }
                    isCallbackDone = true;
                });
            }
            yield return new WaitUntil(() => isCallbackDone);
            Assert.AreEqual(CreateDefaultModels.CustomCharactersVersion, store.Version.CustomCharactersVersion);

            #endregion

            Debug.Log($"exit");
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void CheckGameInfoDebugOnly(DataStore store)
        {
            store.GetAllCharacterClasses(characterClasses =>
            {
                store.ForTest.GetAllCustomCharactersTest(customCharacters =>
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
                        if (playerPrefabs.GetPlayerPrefab(customCharacter.UnityKey) == null)
                        {
                            if (isCustomCharactersValid)
                            {
                                Debug.Log($"characterClasses {characterClasses.Count} ver {store.Version.CharacterClassesVersion}");
                                Debug.Log($"customCharacters {customCharacters.Count} ver {store.Version.CustomCharactersVersion}");
                            }
                            Debug.LogWarning($"customCharacter {customCharacter.Id} {customCharacter.Name} " +
                                             $"does not have PlayerPrefab {customCharacter.UnityKey}");
                            isCustomCharactersValid = false;
                        }
                    }
                    store.ForTest.GetAllBattleCharactersTest(battleCharacters =>
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
