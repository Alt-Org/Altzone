using System.Diagnostics;
using System.Linq;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco;
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
            // Parts of store can be initialized asynchronously and we start it now (if not running already).
            var store = Storefront.Get();
            var gameConfig = GameConfig.Get();
            var playerSettings = gameConfig.PlayerSettings;
            var playerGuid = playerSettings.PlayerGuid;
            var playerData = store.GetPlayerData(playerGuid);
            if (playerData == null)
            {
                // Create new player for us - currentCharacterModelId must be valid because it is not checked later.
                playerData = new PlayerData(1, 0, 1, "Player", 0, playerGuid);
                playerData = store.SavePlayerData(playerData);
                Debug.Log($"Create player {playerData}");
            }
            else
            {
                Debug.Log($"Load player {playerData}");
                Assert.AreEqual(playerGuid, playerData.UniqueIdentifier);
            }
            gameConfig.PlayerDataModel = playerData;
            ShowDebugGameInfo();
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("FORCE_LOG")]
        private static void ShowDebugGameInfo()
        {
            var store = Storefront.Get();
            var characterClassModels = store.GetAllCharacterClassModels();
            Debug.Log($"characterClasses {characterClassModels.Count}");
            var customCharacters = store.GetAllCustomCharacterModels();
            var playerPrefabs = GameConfig.Get().PlayerPrefabs;
            var isCustomCharactersValid = true;
            foreach (var customCharacter in customCharacters)
            {
                if (characterClassModels.All(x => x.Id != customCharacter.CharacterModelId))
                {
                    Debug.LogWarning($"customCharacter {customCharacter.Id} {customCharacter.Name} " +
                                     $"does not have CharacterModel {customCharacter.CharacterModelId}");
                    isCustomCharactersValid = false;
                }
                if (playerPrefabs.GetPlayerPrefab(customCharacter.PlayerPrefabId) == null)
                {
                    Debug.LogWarning($"customCharacter {customCharacter.Id} {customCharacter.Name} " +
                                     $"does not have PlayerPrefab {customCharacter.PlayerPrefabId}");
                    isCustomCharactersValid = false;
                }
            }
            Debug.Log($"customCharacters {customCharacters.Count}");
            var battleCharacters = store.GetAllBattleCharacters();
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
        }
    }
}