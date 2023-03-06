using System.Collections;
using System.Diagnostics;
using System.Linq;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model;
using Altzone.Scripts.Model.Dto;
using Altzone.Scripts.Service.Audio;
using Altzone.Scripts.Service.LootLocker;
using Prg.Scripts.Common.Unity;
using Prg.Scripts.Common.Unity.Attributes;
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
        /// <summary>
        /// Development mode API Key suffix (simplified chinese) for <c>LootLocker</c> SDK API.
        /// </summary>
        private const string Prefix1 = "发展"; // Fāzhǎn

        /// <summary>
        /// Production mode API Key suffix (simplified chinese) for <c>LootLocker</c> SDK API.
        /// </summary>
        private const string Prefix2 = "生产"; // Shēngchǎn

        [SerializeField, ReadOnly] private bool _isLootLocker;
        [SerializeField, ReadOnly] private bool _isLoaded;

        public bool IsLootLocker => _isLootLocker;
        public bool IsLoaded => _isLoaded;

        private IEnumerator Start()
        {
            Debug.Log($"{name}");
            Localizer.LoadTranslations(Application.systemLanguage);
            AudioManager.Get();
            // Development vs production mode needs to be decided during build time!
            const bool isDevelopmentMode = true;
            StartLootLocker(isDevelopmentMode);
            // Parts of store can be initialized asynchronously and we start it now (if not running already).
            var store = Storefront.Get();
            yield return new WaitUntil(() => store.IsGameServerConnected);
            var task = store.GetPlayerDataModel(1);
            yield return new WaitUntil(() => task.IsCompleted);
            var playerDataModel = task.Result;
            if (playerDataModel == null)
            {
                // Create new player for us.
                playerDataModel = new PlayerDataModel(0, 0, "Pelaaja", 0);
                store.SavePlayerDataModel(playerDataModel);
                Debug.Log($"Create player {playerDataModel}");
            }
            else
            {
                Debug.Log($"Load player {playerDataModel}");
            }
            var gameConfig = GameConfig.Get();
            gameConfig.PlayerDataModel = playerDataModel;
            _isLoaded = true;
            ShowDebugGameInfo(this);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("FORCE_LOG")]
        private static void ShowDebugGameInfo(MonoBehaviour monoBehaviour)
        {
            monoBehaviour.StartCoroutine(CheckDebugGameInfo());

            IEnumerator CheckDebugGameInfo()
            {
                yield return null;
                var store = Storefront.Get();
                yield return new WaitUntil(() => store.IsInventoryConnected);
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
                    yield break;
                }
                // Dump all battle characters if something is wrong in storage.
                foreach (var battleCharacter in battleCharacters)
                {
                    Debug.Log($"battleCharacter {battleCharacter}");
                }
            }
        }

        [Conditional("USE_LOOTLOCKER")]
        private void StartLootLocker(bool isDevelopmentMode)
        {
            // We need player name and guid in order to start LootLocker.
            var gameConfig = GameConfig.Get();
            var playerDataCache = gameConfig.PlayerSettings;
            var playerDataModel = gameConfig.PlayerDataModel;
            if (string.IsNullOrWhiteSpace(playerDataModel.Name) || string.IsNullOrWhiteSpace(playerDataCache.PlayerGuid))
            {
                Debug.Log("Can not start LootLocker because player name and/or guid is missing");
                return;
            }
            var suffix = isDevelopmentMode ? Prefix1 : Prefix2;
            Debug.Log($"Start LootLocker IsRunning {LootLockerWrapper.IsRunning} suffix {suffix}");
            LootLockerWrapper.Start(isDevelopmentMode,
                () => Resources.Load<StringProperty>($"{nameof(StringProperty)}{suffix}")?.PropertyValue);
            _isLootLocker = true;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Helper class for UNITY Editor operations.
        /// </summary>
        public static class Support
        {
            public static StringProperty GetLootLockerResource() => _GetLootLockerResource();
        }

        private static StringProperty _GetLootLockerResource()
        {
            return Resources.Load<StringProperty>($"{nameof(StringProperty)}{Prefix1}");
        }
#endif
    }
}